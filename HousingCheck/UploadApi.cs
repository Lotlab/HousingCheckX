using System.Net;
using System;
using System.Text;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace HousingCheck
{
    public enum ApiVersion { V1, V2 }

    public class UploadApi
    {
        Config config { get; }

        Uri baseUrl
        {
            get
            {
                switch (Version)
                {
                    case ApiVersion.V1:
                        return new Uri(config.UploadUrl);
                    case ApiVersion.V2:
                        return new Uri(config.UploadUrl.TrimEnd('/'));
                    default:
                        return null;
                }
            }
        }
        string token => config.UploadToken;
        public ApiVersion Version => config.UploadApiVersion;

        bool doNotExpect100Continue { get; set; } = false;

        System.Reflection.AssemblyName assemblyName => System.Reflection.Assembly.GetExecutingAssembly().GetName();

        public UploadApi(Config config)
        {
            this.config = config;
        }

        WebClient NewWebClient()
        {
            var wb = new WebClient();
            if (!String.IsNullOrWhiteSpace(token)) wb.Headers[HttpRequestHeader.Authorization] = "Token " + token.Trim();
            wb.Headers.Add(HttpRequestHeader.UserAgent, assemblyName.Name + "/" + assemblyName.Version);

            return wb;
        }

        /// <summary>
        /// 判断服务器的返回值是否正确，若不正确则抛出 ServerResponseException 异常
        /// </summary>
        /// <param name="content"></param>
        void ParseResponse(byte[] response)
        {
            var content = Encoding.UTF8.GetString(response);
            try
            {
                var jsonRes = JsonConvert.DeserializeObject<Dictionary<string, string>>(content);
                if (jsonRes["statusText"].ToLower() == "ok") return;
                throw new ServerResponseException(jsonRes["errorMessage"]);
            }
            catch
            {
                if (content.ToLower() == "ok") return;
                throw new ServerResponseException(content);
            }
        }

        Uri GetFullUrl(string relative)
        {
            var fullUrl = new Uri(baseUrl, relative);
            if (doNotExpect100Continue)
            {
                var servicePoint = ServicePointManager.FindServicePoint(fullUrl);
                servicePoint.Expect100Continue = false;
            }
            return fullUrl;
        }

        void UploadRestfulJson(string path, string method, object data)
        {
            var fullUrl = GetFullUrl(path);
            var client = NewWebClient();
            client.Headers[HttpRequestHeader.ContentType] = "application/json";

            var resp = client.UploadData(fullUrl, method, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data)));
            ParseResponse(resp);
        }

        void UploadForm(string path, string method, IEnumerable<KeyValuePair<string, string>> data)
        {
            var fullUrl = GetFullUrl(path);
            var client = NewWebClient();
            client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";

            string content = "";
            foreach (var item in data)
            {
                content += WebUtility.UrlDecode(item.Key) + "=" + WebUtility.UrlEncode(item.Value);
                content += "&";
            }
            content = content.TrimEnd('&');

            var resp = client.UploadData(fullUrl, method, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data)));
            ParseResponse(resp);
        }

        public void UploadOnSaleListMsg(string message)
        {
            switch (Version)
            {
                case ApiVersion.V1:
                    UploadForm("", "POST", new Dictionary<string, string>() { { "text", message } });
                    break;
                case ApiVersion.V2:
                    throw new InvalidOperationException("Sale list is APIv1 method");
            }
        }

        public void UploadHouselList(IEnumerable<HousingSlotSnapshotJSONObject> houses)
        {
            switch (Version)
            {
                case ApiVersion.V1:
                    throw new InvalidOperationException("House list is APIv2 method");
                case ApiVersion.V2:
                    UploadRestfulJson("/info", "POST", houses);
                    break;
            }
        }

        public void UploadDetailList(IEnumerable<LandInfoSignBrief> details)
        {
            switch (Version)
            {
                case ApiVersion.V1:
                    throw new InvalidOperationException("Detail list is APIv2 method");
                case ApiVersion.V2:
                    UploadRestfulJson("/detail", "POST", details);
                    break;
            }
        }
    }

    class CustomWebClient : WebClient
    {
        protected override WebRequest GetWebRequest(Uri uri)
        {
            WebRequest w = base.GetWebRequest(uri);
            w.Timeout = 60 * 1000; // 60s
            return w;
        }
    }

    [System.Serializable]
    public class ServerResponseException : System.Exception
    {
        public ServerResponseException() { }
        public ServerResponseException(string message) : base(message) { }
        public ServerResponseException(string message, System.Exception inner) : base(message, inner) { }
        protected ServerResponseException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}