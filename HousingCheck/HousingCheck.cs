using System;
using System.IO;
using System.Windows.Forms;
using System.Collections;
using System.Linq;
using System.Text;
using Advanced_Combat_Tracker;
using FFXIV_ACT_Plugin.Common;
using System.Net;
using System.Threading;
using System.ComponentModel;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Newtonsoft.Json;
using Microsoft.Toolkit.Uwp.Notifications;
using System.Collections.Concurrent;

public static class Extensions
{
    public static T[] SubArray<T>(this T[] array, int offset, int length)
    {
        T[] result = new T[length];
        Array.Copy(array, offset, result, 0, length);
        return result;
    }
}

namespace HousingCheck
{
    public enum ApiVersion { V1, V2 }

    public class HousingCheck : IActPluginV1
    {
        /// <summary>
        /// 国服版本5.55 Opcode
        /// </summary>
        public const int OPCODE_WARD_INFO = 658;
        public const int OPCODE_LAND_INFO = 730;

        /// <summary>
        /// 房屋列表，用于和控件双向绑定
        /// </summary>
        public ObservableCollection<HousingOnSaleItem> HousingList = new ObservableCollection<HousingOnSaleItem>();

        /// <summary>
        /// 库啵，库啵啵？
        /// </summary>
        public BindingSource housingBindingSource;

        /// <summary>
        /// 插件对象
        /// </summary>
        FFXIV_ACT_Plugin.FFXIV_ACT_Plugin ffxivPlugin;

        List<(DateTime, string)> LogQueue = new List<(DateTime, string)>();

        /// <summary>
        /// 是否加载完成
        /// </summary>
        bool initialized = false;

        /// <summary>
        /// 有手动上报任务
        /// </summary>
        bool ManualUpload = false;

        /// <summary>
        /// 有自动上报任务
        /// </summary>
        bool HousingListUpdated = false;

        /// <summary>
        /// 快照自上次上报以来有更新
        /// </summary>
        bool SnapshotUpdated = false;

        /// <summary>
        /// 房屋详细信息自上次上报以来有更新
        /// </summary>
        bool LandInfoUpdated = false;

        /// <summary>
        /// 房区快照
        /// </summary>
        HousingSnapshotStorage SnapshotStorage = new HousingSnapshotStorage();

        /// <summary>
        /// 进行房区快照上报用的存储
        /// </summary>
        ConcurrentDictionary<Tuple<HouseArea, int>, HousingSlotSnapshot> WillUploadSnapshot = new ConcurrentDictionary<Tuple<HouseArea, int>, HousingSlotSnapshot>();

        /// <summary>
        /// 房屋详细信息存储
        /// </summary>
        LandInfoSignStorage LandInfoSignStorage = new LandInfoSignStorage();

        /// <summary>
        /// 用户上次操作的时间
        /// </summary>
        long LastOperateTime = 0;

        /// <summary>
        /// 无操作自动保存的时间
        /// </summary>
        long AutoSaveAfter = 20;

        /// <summary>
        /// 自动保存worker
        /// </summary>
        private BackgroundWorker AutoSaveThread;

        /// <summary>
        /// Log队列
        /// </summary>
        private BackgroundWorker LogQueueWorker;

        /// <summary>
        /// 定时任务Worker
        /// </summary>
        private BackgroundWorker TickWorker;

        /// <summary>
        /// 状态信息
        /// </summary>
        Label statusLabel;
        PluginControl control;

        private object GetFfxivPlugin()
        {
            ffxivPlugin = null;

            var plugins = ActGlobals.oFormActMain.ActPlugins;

            foreach (var plugin in plugins)
                if (plugin.pluginFile.Name.ToUpper().Contains("FFXIV_ACT_Plugin".ToUpper()) &&
                    plugin.lblPluginStatus.Text.ToUpper().Contains("FFXIV Plugin Started.".ToUpper()))
                    ffxivPlugin = (FFXIV_ACT_Plugin.FFXIV_ACT_Plugin)plugin.pluginObj;

            if (ffxivPlugin == null)
                throw new Exception("Could not find FFXIV plugin. Make sure that it is loaded before HousingCheck.");

            return ffxivPlugin;
        }

        void IActPluginV1.DeInitPlugin()
        {
            if (initialized)
            {
                ffxivPlugin.DataSubscription.NetworkReceived -= NetworkReceived;
                AutoSaveThread.CancelAsync();
                LogQueueWorker.CancelAsync();
                TickWorker.CancelAsync();
                control.SaveSettings();
                SaveHousingList();
                statusLabel.Text = "Exit :|";
            }
            else
            {
                statusLabel.Text = "Error :(";
            }
        }

        void IActPluginV1.InitPlugin(TabPage pluginScreenSpace, Label pluginStatusText)
        {
            statusLabel = pluginStatusText;
            GetFfxivPlugin();
            control = new PluginControl();
            pluginScreenSpace.Text = "房屋信息记录";
            housingBindingSource = new BindingSource { DataSource = HousingList };
            control.dataGridView1.DataSource = housingBindingSource;
            control.dataGridView1.UserDeletedRow += OnTableUpdated;
            foreach (DataGridViewColumn col in control.dataGridView1.Columns)
            {
                col.SortMode = DataGridViewColumnSortMode.Automatic;
            }

            control.Dock = DockStyle.Fill;
            pluginScreenSpace.Controls.Add(control);

            ffxivPlugin.DataSubscription.NetworkReceived += NetworkReceived;

            initialized = true;

            AutoSaveThread = new BackgroundWorker
            {
                WorkerSupportsCancellation = true
            };
            AutoSaveThread.DoWork += RunAutoUploadWorker;
            AutoSaveThread.RunWorkerAsync();

            LogQueueWorker = new BackgroundWorker
            {
                WorkerSupportsCancellation = true
            };
            LogQueueWorker.DoWork += RunLogQueueWorker;
            LogQueueWorker.RunWorkerAsync();

            TickWorker = new BackgroundWorker { WorkerSupportsCancellation = true };
            TickWorker.DoWork += TickDoWorker;
            TickWorker.RunWorkerAsync();

            statusLabel.Text = "Working :D";
            control.LoadSettings();
            control.buttonUploadOnce.Click += ButtonUploadOnce_Click;
            control.buttonCopyToClipboard.Click += ButtonCopyToClipboard_Click;
            control.buttonSaveToFile.Click += ButtonSaveToFile_Click;
            control.buttonNotifyTest.Click += ButtonNotifyTest_Click;
            control.buttonNotifyCheckTest.Click += ButtonNotifyCheckTest_Click;
            PrepareDir();
            //恢复上次列表
            LoadHousingList();
        }

        private void ButtonNotifyCheckTest_Click(object sender, EventArgs e)
        {
            NotifyCheckHouseAsnyc();
        }

        private void ButtonNotifyTest_Click(object sender, EventArgs e)
        {
            NotifyEmptyHouseAsync(new HousingOnSaleItem(HouseArea.海雾村, 1, 2, HouseSize.L, 10000000, false), false);
        }

        void NotifyEmptyHouseAsync(HousingOnSaleItem onSaleItem, bool exists)
        {
            new Action<HousingOnSaleItem, bool>((item, exist) => { NotifyEmptyHouse(item, exist); }).Invoke(onSaleItem, exists);
        }

        void NotifyEmptyHouse(HousingOnSaleItem onSaleItem, bool exists)
        {
            if (onSaleItem.Size == HouseSize.S && !control.EnableNotifyHouseS)
                return;

            if ((onSaleItem.Size == HouseSize.M || onSaleItem.Size == HouseSize.L) && !control.EnableNotifyHouseML)
                return;

            bool fallback = true;
            if (control.EnableTTS)
            {
                ActGlobals.oFormActMain.TTS(
                    string.Format("{0}{1}区{2}号{3}房",
                        HousingItem.GetHouseAreaShortStr(onSaleItem.Area),
                        onSaleItem.DisplaySlot,
                        onSaleItem.DisplayId,
                        onSaleItem.SizeStr
                    )
                );
                fallback = false;
            }
            if (control.EnableNotification)
            {
                var title = string.Format("{0} 第{1}区 {2}号 {3}房",
                        onSaleItem.AreaStr,
                        onSaleItem.DisplaySlot,
                        onSaleItem.DisplayId,
                        onSaleItem.SizeStr
                    );
                new ToastContentBuilder()
                    .AddText("新空房")
                    .AddText(title)
                    .Show();
                fallback = false;
            }
            if (fallback)
            {
                PlayAlert();
            }
        }

        /// <summary>
        /// 播放提示音
        /// </summary>
        void PlayAlert()
        {
            Console.Beep(3000, 1000);
        }

        void Log(string type, string message, bool important = false)
        {
            var time = (DateTime.Now).ToString("HH:mm:ss");
            var text = $"[{time}] [{type}] {message.Trim()}";
            control.BeginInvoke(new Action<string>((msg) =>
            {
                control.textBoxLog.AppendText(text + Environment.NewLine);
                control.textBoxLog.SelectionStart = control.textBoxLog.TextLength;
                control.textBoxLog.ScrollToCaret();
            }), text);

            //ActGlobals.oFormActMain.ParseRawLogLine(true, DateTime.Now, $"{text}");
            if (important)
            {
                var logText = $"00|{DateTime.Now.ToString("O")}|0|HousingCheck-{message}|";        //解析插件数据格式化
                LogQueue.Add((DateTime.Now, logText));
            }
        }

        void Log(string type, Exception ex, string msg = "")
        {
            Log(type, msg + ex.ToString(), false);
        }

        string HousingListToJson()
        {
            return JsonConvert.SerializeObject(
                    HousingList.Where(x => x.CurrentStatus).ToArray()
                );
        }

        void NetworkReceived(string connection, long epoch, byte[] message)
        {
            var opcode = BitConverter.ToUInt16(message, 18);
            if (message.Length == 2440)
            {
                if (opcode == control.OpcodeWard || control.DisableOpcodeCheck)
                {
                    WardInfoParser(message);
                }
                if (opcode != control.OpcodeWard && control.DebugEnabled)
                {
                    Log("Debug", "房屋列表Opcode不匹配！可能的Opcode为：" + opcode);
                }
            }
            if (message.Length == 312)
            {
                if (opcode == control.OpcodeLand || control.DisableOpcodeCheck)
                {
                    LandInfoParser(message);
                }
                if (opcode != control.OpcodeLand && control.DebugEnabled)
                {
                    Log("Debug", "房屋门牌Opcode不匹配！可能的Opcode为：" + opcode);
                }
            }
        }

        void WardInfoParser(byte[] message)
        {
            HousingSlotSnapshot snapshot;
            List<HousingOnSaleItem> updatedHousingList = new List<HousingOnSaleItem>();
            try
            {
                //解析数据包
                snapshot = new HousingSlotSnapshot(message);
                //存入存储
                SnapshotStorage.Insert(snapshot);
                WillUploadSnapshot[new Tuple<HouseArea, int>(snapshot.Area, snapshot.Slot)] = snapshot;
                SnapshotUpdated = true;

                //本区房屋列表
                var housingList = snapshot.HouseList;

                // 过滤本区房屋列表
                var oldOnSaleList = HousingList.Where(h => h.Area == snapshot.Area && h.Slot == snapshot.Slot);

                var removeList = new List<HousingOnSaleItem>();

                foreach (var a in housingList)
                {
                    HousingItem house = a.Value;
                    HousingOnSaleItem onSaleItem = new HousingOnSaleItem(house);
                    bool isExists = false;

                    //查找并更新原有房屋
                    var oldOnSaleItems = oldOnSaleList.Where(x => x.Id == house.Id);
                    foreach (var oldOnSaleItem in oldOnSaleItems)
                    {
                        updatedHousingList.Add(onSaleItem);
                        isExists = true;
                    }

                    if (isExists)
                    {
                        HousingListUpdated = true;
                    }

                    if (house.IsEmpty)
                    {
                        Log("Info", string.Format("{0} 第{1}区 {2}号 {3}房在售 当前价格: {4}",
                            onSaleItem.AreaStr, onSaleItem.DisplaySlot, onSaleItem.DisplayId,
                            onSaleItem.SizeStr, onSaleItem.Price), true);

                        NotifyEmptyHouseAsync(onSaleItem, isExists);
                        if (!isExists)
                        {
                            updatedHousingList.Add(onSaleItem);
                            HousingListUpdated = true;
                        }
                        else
                        {
                            Log("Info", "重复土地，已更新。");
                        }

                        var signInfo = new HousingLandInfoSign(snapshot.ServerId, house.Area, house.Slot, house.Id, snapshot.Time, house.Size);
                        LandInfoSignStorage.Add(signInfo);
                        LandInfoUpdated = true;

                    }
                    else if (isExists)
                    {
                        removeList.Add(onSaleItem);
                    }
                }

                LastOperateTime = new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds(); //更新上次操作的时间

                Log("Info", string.Format("{0} 第{1}区查询完成",
                    HousingItem.GetHouseAreaStr(snapshot.Area),
                    snapshot.Slot + 1), true);     //输出翻页日志

                //刷新UI
                control.BeginInvoke(new Action<List<HousingOnSaleItem>>(UpdateTable), updatedHousingList);
                control.BeginInvoke(new Action<IEnumerable<HousingOnSaleItem>>(removeTableItem), removeList);
            }
            catch (Exception ex)
            {
                Log("Error", ex, "查询房屋列表出错：");
                return;
            }
        }

        void LandInfoParser(byte[] message)
        {
            try
            {
                var info = new HousingLandInfoSign(message);
                LandInfoSignStorage.Add(info);
                LandInfoUpdated = true;

                LastOperateTime = new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds(); //更新上次操作的时间
            }
            catch (Exception e)
            {
                Log("Error", e, "查询房屋信息出错：");
            }
        }

        private void ButtonUploadOnce_Click(object sender, EventArgs e)
        {
            Log("Info", $"准备上报");
            ManualUpload = true;
        }

        private void ButtonCopyToClipboard_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(ListToString());
            Log("Info", $"复制成功");
        }

        private void PrepareDir()
        {
            string appdataPath = Path.Combine(Environment.CurrentDirectory, "AppData");
            if (!Directory.Exists(appdataPath)) Directory.CreateDirectory(appdataPath);
            string savingPath = Path.Combine(appdataPath, "HousingCheck");
            if (!Directory.Exists(savingPath)) Directory.CreateDirectory(savingPath);
            string snapshotPath = Path.Combine(savingPath, "snapshots");
            if (!Directory.Exists(snapshotPath)) Directory.CreateDirectory(snapshotPath);
        }

        private void SaveHousingList()
        {
            string savePath = Path.Combine(new string[] { Environment.CurrentDirectory, "AppData", "HousingCheck", "list.json" });

            StreamWriter writer = new StreamWriter(savePath, false, Encoding.UTF8);
            writer.Write(HousingListToJson());
            writer.Close();
            Log("Info", $"房屋列表已保存到{savePath}");
        }

        private void LoadHousingList()
        {
            string savePath = Path.Combine(new string[] { Environment.CurrentDirectory, "AppData", "HousingCheck", "list.json" });
            if (!File.Exists(savePath)) return;
            StreamReader reader = new StreamReader(savePath, Encoding.UTF8);
            string jsonStr = reader.ReadToEnd();
            reader.Close();
            try
            {
                var list = JsonConvert.DeserializeObject<HousingOnSaleItem[]>(jsonStr);
                foreach (var item in list)
                {
                    housingBindingSource.Add(item);
                }
                Log("Info", "已恢复上次保存的房屋列表");
            }
            catch (Exception ex)
            {
                Log("Error", ex, "恢复上次保存的房屋列表失败：");
            }
            reader.Close();
        }

        private void ButtonSaveToFile_Click(object sender, EventArgs e)
        {
            PrepareDir();
            string time = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string snapshotFilename = $"HousingCheck-{time}-Snapshot.csv";
            string landInfoFilename = $"HousingCheck-{time}-LandInfo.csv";
            string saveDir = Path.Combine(Environment.CurrentDirectory, "AppData", "HousingCheck", "snapshots");

            using (var writer = new StreamWriter(Path.Combine(saveDir, snapshotFilename), false, Encoding.UTF8))
            {
                SnapshotStorage.SaveCsv(writer);
            }

            if (LandInfoSignStorage.Count > 0)
            {
                using (var writer = new StreamWriter(Path.Combine(saveDir, landInfoFilename), false, Encoding.UTF8))
                {
                    LandInfoSignStorage.WriteCSV(writer);
                }
            }

            Log("Info", $"已保存到 {saveDir} 文件夹");
        }

        private string ListToString()
        {
            ArrayList area = new ArrayList(new string[] { "海雾村", "薰衣草苗圃", "高脚孤丘", "白银乡" });
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var line in HousingList)
            {
                if (!line.CurrentStatus)
                    continue;

                stringBuilder.Append($"{line.AreaStr} 第{line.DisplaySlot}区 {line.DisplayId}号{line.SizeStr}房在售，当前价格:{line.Price} {Environment.NewLine}");

                if (line.Area == HouseArea.海雾村 && area.IndexOf("海雾村") != -1)
                {
                    area.Remove("海雾村");
                }
                else if (line.Area == HouseArea.薰衣草苗圃 && area.IndexOf("薰衣草苗圃") != -1)
                {
                    area.Remove("薰衣草苗圃");
                }
                else if (line.Area == HouseArea.高脚孤丘 && area.IndexOf("高脚孤丘") != -1)
                {
                    area.Remove("高脚孤丘");
                }
                else if (line.Area == HouseArea.白银乡 && area.IndexOf("白银乡") != -1)
                {
                    area.Remove("白银乡");
                }
            }
            foreach (var line in area)
            {
                stringBuilder.Append($"{line} 无空房 {Environment.NewLine}");
            }

            return stringBuilder.ToString();
        }

        public void UpdateTable(List<HousingOnSaleItem> items)
        {
            foreach (HousingOnSaleItem item in items)
            {
                int listIndex;
                if ((listIndex = HousingList.IndexOf(item)) != -1)
                {
                    (housingBindingSource[listIndex] as HousingOnSaleItem).Update(item);
                }
                else
                {
                    housingBindingSource.Add(item);
                }
            }
            housingBindingSource.ResetBindings(false);
        }

        /// <summary>
        /// 删除表格中的内容
        /// </summary>
        /// <param name="items"></param>
        void removeTableItem(IEnumerable<HousingOnSaleItem> items)
        {
            foreach (var item in items)
            {
                if (HousingList.Contains(item))
                {
                    housingBindingSource.Remove(item);
                }
            }
            housingBindingSource.ResetBindings(false);
        }

        private void OnTableUpdated(object sender, DataGridViewRowEventArgs e)
        {
            HousingListUpdated = true;
            LastOperateTime = new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds(); //更新上次操作的时间
        }

        private void RunLogQueueWorker(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                if (LogQueueWorker.CancellationPending)
                {
                    break;
                }
                if (LogQueue.Count > 0)
                {
                    while (LogQueue.Count > 0)
                    {
                        var data = LogQueue.First();
                        LogQueue.RemoveAt(0);
                        ActGlobals.oFormActMain.ParseRawLogLine(false, data.Item1, data.Item2);
                    }
                }
                else
                {
                    Thread.Sleep(100);
                }
            }
        }

        private void RunAutoUploadWorker(object sender, DoWorkEventArgs e)
        {
            while (!AutoSaveThread.CancellationPending)
            {
                try
                {
                    long actionTime = LastOperateTime + AutoSaveAfter;
                    int snapshotCount = WillUploadSnapshot.Count;
                    bool autoUpload = control.upload;
                    bool uploadSnapshot = control.EnableUploadSnapshot;
                    ApiVersion apiVersion = control.UploadApiVersion;

                    if (ManualUpload)
                    {
                        Log("Debug", "手动开始上报");
                        UploadOnSaleList(apiVersion);
                        HousingListUpdated = false;
                        LandInfoUpdated = false;
                        // 手动上传在任何情况下都应当上传存储的数据
                        if (apiVersion == ApiVersion.V2 && uploadSnapshot)
                        {
                            if (snapshotCount > 0)
                                UploadSnapshot();

                            if (LandInfoSignStorage.UploadCount > 0)
                                UploadLandInfoSnapshot();
                        }
                        ManualUpload = false;
                        Log("Debug", "手动上报任务执行完毕");
                    }
                    else if (actionTime <= new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds())
                    {
                        if (HousingListUpdated)
                        {
                            //保存列表文件
                            SaveHousingList();
                            Log("Info", "房屋信息已保存");
                            if (autoUpload) UploadOnSaleList(apiVersion);

                            HousingListUpdated = false;
                        }

                        if (autoUpload)
                        {
                            if (apiVersion == ApiVersion.V2 && uploadSnapshot)
                            {
                                if (SnapshotUpdated) UploadSnapshot();
                                if (LandInfoUpdated) UploadLandInfoSnapshot();
                            }
                        }
                    }
                } 
                catch (Exception ex)
                {
                    Log("Error", ex, "执行定时任务时出现错误");
                }

                Thread.Sleep(500);
            }

            Log("Error", "上报线程退出！！！！");
        }

        private DateTime GetNextNotifyTime()
        {
            var nT = DateTime.Now.AddHours(1);
            return new DateTime(nT.Year, nT.Month, nT.Day, nT.Hour, 0, 0);
        }

        /// <summary>
        /// 弹出查房提示
        /// </summary>
        private void NotifyCheckHouse()
        {
            if (control.EnableTTS)
            {
                ActGlobals.oFormActMain.TTS("查房提醒：该查房了");
            }
            if (control.EnableNotification)
            {
                new ToastContentBuilder()
                    .AddText("查房提醒")
                    .AddText("该查房了")
                    .Show();
            }
        }

        /// <summary>
        /// 弹出查房提示
        /// </summary>
        private void NotifyCheckHouseAsnyc()
        {
            new Action(NotifyCheckHouse).Invoke();
        }

        private void TickDoWorker(object sender, DoWorkEventArgs e)
        {
            DateTime lastNotify = DateTime.Now.AddSeconds(-1);
            while (!TickWorker.CancellationPending)
            {
                DateTime nextNotify = GetNextNotifyTime();
                if (DateTime.Now > nextNotify.AddSeconds(-control.CheckNotifyAheadTime))
                {
                    if (lastNotify != nextNotify && control.EnableNotifyCheck)
                    {
                        NotifyCheckHouseAsnyc();
                        lastNotify = nextNotify;
                    }
                }
                Thread.Sleep(1000);
            }
        }

        private class CustomWebClient : WebClient
        {
            protected override WebRequest GetWebRequest(Uri uri)
            {
                WebRequest w = base.GetWebRequest(uri);
                w.Timeout = 30 * 1000; // 30s
                return w;
            }
        }

        System.Reflection.AssemblyName assemblyName => System.Reflection.Assembly.GetExecutingAssembly().GetName();

        public bool UploadData(string type, string postContent, string mime = "application/json")
        {
            var wb = new CustomWebClient();
            var token = control.UploadToken.Trim();
            if (token != "")
            {
                wb.Headers[HttpRequestHeader.Authorization] = "Token " + token;
            }
            wb.Headers[HttpRequestHeader.ContentType] = mime;
            wb.Headers.Add(HttpRequestHeader.UserAgent, assemblyName.Name + "/" + assemblyName.Version);

            string url;
            switch (control.UploadApiVersion)
            {
                case ApiVersion.V1:
                    url = control.UploadUrl;
                    break;
                case ApiVersion.V2:
                default:
                    url = control.UploadUrl.TrimEnd('/') + "/" + type;
                    break;
            }

            // 调试用，可以解决部分网页服务器不支持此功能的问题
            // var servicePoint = ServicePointManager.FindServicePoint(new Uri(url));
            // servicePoint.Expect100Continue = false;

            try
            {
                var response = wb.UploadData(url, "POST",
                    Encoding.UTF8.GetBytes(postContent)
                );
                string res = Encoding.UTF8.GetString(response);
                if (res.Length > 0)
                {
                    if (res[0] == '{')
                    {
                        var jsonRes = JsonConvert.DeserializeObject<Dictionary<string, string>>(res);
                        if (jsonRes["statusText"].ToLower() == "ok")
                        {
                            return true;
                        }
                        else
                        {
                            Log("Error", "上传出错：" + jsonRes["errorMessage"]);
                        }
                    }
                    else
                    {
                        if (res.ToLower() == "ok")
                        {
                            return true;
                        }
                        else
                        {
                            Log("Error", "上传出错：" + res);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log("Error", ex, "上传出错：");
            }
            return false;
        }

        private void UploadOnSaleList(ApiVersion apiVersion = ApiVersion.V2)
        {
            string postContent = "";
            string mime = "application/json";
            switch (apiVersion)
            {
                case ApiVersion.V2:
                    postContent = JsonConvert.SerializeObject(HousingList);
                    break;
                case ApiVersion.V1:
                    postContent = "text=" + WebUtility.UrlEncode(ListToString());
                    mime = "application/x-www-form-urlencoded";
                    break;
            }
            if (postContent.Length == 0)
            {
                Log("Error", "上报数据为空");
            }
            //Log("Debug", postContent);
            Log("Info", "正在上传空房列表");
            bool res = UploadData("info", postContent, mime);

            if (res)
            {
                Log("Info", "房屋列表上报成功");
            }
            else
            {
                Thread.Sleep(2000);
            }
        }

        private void UploadSnapshot()
        {
            try
            {
                List<HousingSlotSnapshotJSONObject> snapshotJSONObjects = new List<HousingSlotSnapshotJSONObject>();
                foreach (var snapshot in WillUploadSnapshot.Values)
                {
                    if (snapshot != null)
                    {
                        snapshotJSONObjects.Add(snapshot.ToJsonObject());
                    }
                }
                string json = JsonConvert.SerializeObject(snapshotJSONObjects);
                SnapshotUpdated = false;
                Log("Info", "正在上传房区快照");
                bool res = UploadData("info", json);
                if (res)
                {
                    Log("Info", "房区快照上报成功");
                }
                else
                {
                    Thread.Sleep(1000);
                }
            }
            catch (Exception ex)
            {
                Log("Error", ex, "房区快照上报出错：");
            }
            //Log("Info", $"上报消息给 {post_url}");
        }

        private void UploadLandInfoSnapshot()
        {
            try
            {
                var objs = LandInfoSignStorage.ToJsonObj();
                LandInfoUpdated = false;
                string json = JsonConvert.SerializeObject(objs);
                Log("Info", "正在上传房屋详细信息");
                bool res = UploadData("detail", json);
                if (res)
                {
                    Log("Info", "房屋详细信息上报成功");
                    // 标记过时数据以防止重复上报
                    LandInfoSignStorage.MarkOutdated(DateTime.Now);
                }
                else
                {
                    Thread.Sleep(1000);
                }
            }
            catch (Exception ex)
            {
                Log("Error", ex, "房屋详细信息上报出错：");
            }
        }
    }
}
