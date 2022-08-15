using System;
using System.IO;
using System.Windows.Forms;
using System.Collections;
using System.Linq;
using System.Text;
using Advanced_Combat_Tracker;
using System.Net;
using System.Threading;
using System.ComponentModel;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Newtonsoft.Json;
using Microsoft.Toolkit.Uwp.Notifications;
using System.Collections.Concurrent;
using System.Windows.Forms.Integration;
using Lotlab.PluginCommon.FFXIV.Parser.Packets;
using Lotlab.PluginCommon.FFXIV.Parser;
using Lotlab.PluginCommon.FFXIV;
using Lotlab.PluginCommon;
using System.Runtime.InteropServices;

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
        /// 插件对象
        /// </summary>
        ACTPluginProxy ffxivPlugin;

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
        PluginControlWpf control;
        PluginControlViewModel vm;
        Config config;
        SimpleLoggerSync logger;
        NetworkParser parser = new NetworkParser();

        void IActPluginV1.DeInitPlugin()
        {
            if (initialized)
            {
                ffxivPlugin.DataSubscription.NetworkReceived -= NetworkReceived;
                ffxivPlugin.DataSubscription.NetworkSent -= NetworkSent;
                AutoSaveThread.CancelAsync();
                LogQueueWorker.CancelAsync();
                TickWorker.CancelAsync();
                config.SaveSettings();
                SaveHousingList();
                logger.Close();
                logger = null;
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

            var plugins = ActGlobals.oFormActMain.ActPlugins;
            foreach (var item in plugins)
            {
                if (item.pluginFile.Name.ToUpper().Contains("FFXIV_ACT_PLUGIN") && item.pluginObj != null)
                {
                    ffxivPlugin = new ACTPluginProxy(item.pluginObj);
                }
            }

            if (ffxivPlugin == null)
            {
                pluginStatusText.Text = "FFXIV Act Plugin is not loading.";
                return;
            }

            config = new Config();
            config.LoadSettings();

            logger = new SimpleLoggerSync(Path.Combine(DataDir, "app.log"));
            vm = new PluginControlViewModel(config, logger);

            parser.SetOpcode<HousingWardInfo>((ushort)config.OpcodeWard);
            parser.SetOpcode<LandInfoSign>((ushort)config.OpcodeLand);
            parser.SetOpcode<LandSaleInfo>((ushort)config.OpcodeSale);
            parser.SetOpcode<ClientTrigger>((ushort)config.OpcodeClientTrigger);

            control = new PluginControlWpf();
            control.DataContext = vm;
            pluginScreenSpace.Text = "房屋信息记录";
            var host = new ElementHost()
            {
                Dock = DockStyle.Fill,
                Child = control
            };

            pluginScreenSpace.Controls.Add(host);

            ffxivPlugin.DataSubscription.NetworkReceived += NetworkReceived;
            ffxivPlugin.DataSubscription.NetworkSent += NetworkSent;

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
            vm.OnInvoke += (arg) =>
            {
                switch (arg)
                {
                    case "UploadManaually":
                        ButtonUploadOnce_Click(null, null);
                        break;
                    case "CopyToClipboard":
                        ButtonCopyToClipboard_Click(null, null);
                        break;
                    case "SaveToFile":
                        ButtonSaveToFile_Click(null, null);
                        break;
                    case "TestNotification":
                        NotifyEmptyHouseAsync(new HousingOnSaleItem(HouseArea.海雾村, 1, 2, HouseSize.L, 10000000, false), false);
                        break;
                    default:
                        break;
                }
            };

            PrepareDir();
            //恢复上次列表
            LoadHousingList();
        }

        void NotifyEmptyHouseAsync(HousingOnSaleItem onSaleItem, bool exists)
        {
            new Action<HousingOnSaleItem, bool>((item, exist) => { NotifyEmptyHouse(item, exist); }).Invoke(onSaleItem, exists);
        }

        void NotifyEmptyHouse(HousingOnSaleItem onSaleItem, bool exists)
        {
            if (onSaleItem.Size == HouseSize.S && !config.EnableNotifyHouseS)
                return;

            if ((onSaleItem.Size == HouseSize.M || onSaleItem.Size == HouseSize.L) && !config.EnableNotifyHouseML)
                return;

            if (onSaleItem.Area == HouseArea.穹顶皓天 && config.IgnoreEmpyreum)
                return;

            if (config.EnableNotification)
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
            }
            if (config.EnableTTS)
            {
                ActGlobals.oFormActMain.TTS(
                    string.Format("{0}{1}区{2}号{3}房",
                        HousingItem.GetHouseAreaShortStr(onSaleItem.Area),
                        onSaleItem.DisplaySlot,
                        onSaleItem.DisplayId,
                        onSaleItem.SizeStr
                    )
                );
            }
        }

        void WriteActLog(string message)
        {
            var logText = $"00|{DateTime.Now.ToString("O")}|0|HousingCheck-{message}|";        //解析插件数据格式化
            LogQueue.Add((DateTime.Now, logText));
        }

        string HousingListToJson()
        {
            return JsonConvert.SerializeObject(
                vm.Sales.Where(x => x.CurrentStatus).ToArray()
            );
        }
        void NetworkSent(string connection, long epoch, byte[] message)
        {
            var packet = parser.ParsePacket(message);
            switch (packet)
            {
                case ClientTrigger trigger:
                    ClientTriggerParser(trigger);
                    break;
                default:
                    break;
            }
        }

        void NetworkReceived(string connection, long epoch, byte[] message)
        {
            var packet = parser.ParsePacket(message);
            switch (packet)
            {
                case HousingWardInfo ward:
                    WardInfoParser(ward);
                    break;
                case LandInfoSign land:
                    LandInfoParser(land);
                    break;
                case LandSaleInfo sale:
                    SaleInfoParser(sale);
                    break;
                default:
                    if (config.DebugEnabled)
                    {
                        var ipc = parser.ParseIPCHeader(message);
                        var guessOpcode = ipc.Value.type;
                        if (message.Length == Marshal.SizeOf<FFXIVIpcHousingWardInfo>())
                        {
                            logger.LogDebug("房屋列表Opcode不匹配！可能的Opcode为：" + guessOpcode);
                            if (config.DisableOpcodeCheck)
                                WardInfoParser(parser.ParseAsPacket<HousingWardInfo, FFXIVIpcHousingWardInfo>(message));
                        }
                        else if (message.Length == Marshal.SizeOf<FFXIVIpcLandInfoSign>())
                        {
                            logger.LogDebug("房屋门牌Opcode不匹配！可能的Opcode为：" + guessOpcode);
                            if (config.DisableOpcodeCheck)
                                LandInfoParser(parser.ParseAsPacket<LandInfoSign, FFXIVIpcLandInfoSign>(message));
                        }
                        else if (message.Length == Marshal.SizeOf<FFXIVIpcLandSaleInfo>())
                        {
                            logger.LogDebug("房屋销售信息Opcode不匹配！可能的Opcode为：" + guessOpcode);
                            if (config.DisableOpcodeCheck)
                                SaleInfoParser(parser.ParseAsPacket<LandSaleInfo, FFXIVIpcLandSaleInfo>(message));
                        }
                    }
                    break;
            }
        }

        void WardInfoParser(HousingWardInfo info)
        {
            HousingSlotSnapshot snapshot;
            List<HousingOnSaleItem> updatedHousingList = new List<HousingOnSaleItem>();
            try
            {
                //解析数据包
                snapshot = new HousingSlotSnapshot(info);

                if (snapshot.ServerId == 0)
                    return;
                 
                //存入存储
                SnapshotStorage.Insert(snapshot);
                WillUploadSnapshot[new Tuple<HouseArea, int>(snapshot.Area, snapshot.Slot)] = snapshot;
                SnapshotUpdated = true;

                //本区房屋列表
                var housingList = snapshot.HouseList;

                // 过滤本区房屋列表
                var oldOnSaleList = vm.Sales.Where(h => h.Area == snapshot.Area && h.Slot == snapshot.Slot);

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
                        var str = string.Format("{0} 第{1}区 {2}号 {3}房在售 当前价格: {4}",
                            onSaleItem.AreaStr, onSaleItem.DisplaySlot, onSaleItem.DisplayId,
                            onSaleItem.SizeStr, onSaleItem.Price);
                        logger.LogInfo(str);
                        WriteActLog(str);

                        NotifyEmptyHouseAsync(onSaleItem, isExists);
                        if (!isExists)
                        {
                            updatedHousingList.Add(onSaleItem);
                            HousingListUpdated = true;
                        }
                        else
                        {
                            logger.LogInfo("重复土地，已更新。");
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

                // 输出翻页日志
                var logStr = string.Format("{0} 第{1}区查询完成",
                    HousingItem.GetHouseAreaStr(snapshot.Area),
                    snapshot.Slot + 1);
                logger.LogInfo(logStr);
                WriteActLog(logStr);

                //刷新UI
                vm.UpdateSales(updatedHousingList);
                vm.RemoveSales(removeList);
            }
            catch (Exception ex)
            {
                logger.LogError("信息解析失败：" + ex.ToString());
            }
        }

        void LandInfoParser(LandInfoSign sign)
        {
            try
            {
                var info = new HousingLandInfoSign(sign);
                LandInfoSignStorage.Add(info);
                LandInfoUpdated = true;

                LastOperateTime = new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds(); //更新上次操作的时间
            }
            catch (Exception e)
            {
                logger.LogError("信息解析失败：" + e.ToString());
            }
        }

        void SaleInfoParser(LandSaleInfo sale)
        {
            logger.LogInfo(sale.ToString());
        }

        void ClientTriggerParser(ClientTrigger trigger)
        {
            if (trigger.Value.commandId != 0x0451) return;
            var req = parser.ParseAsPacket<ClientTriggerLandSaleRequest>(trigger.Value.data);
            logger.LogInfo(req.ToString());
        }

        private void ButtonUploadOnce_Click(object sender, EventArgs e)
        {
            logger.LogInfo($"准备上报");
            ManualUpload = true;
        }

        private void ButtonCopyToClipboard_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(ListToString());
            logger.LogInfo($"复制成功");
        }

        private void PrepareDir()
        {
            if (!Directory.Exists(SnapshotDir))
                Directory.CreateDirectory(SnapshotDir);
        }

        private void SaveHousingList()
        {
            string savePath = Path.Combine(DataDir, "list.json" );

            StreamWriter writer = new StreamWriter(savePath, false, Encoding.UTF8);
            writer.Write(HousingListToJson());
            writer.Close();
            logger.LogInfo($"房屋列表已保存到{savePath}");
        }

        private void LoadHousingList()
        {
            string savePath = Path.Combine(DataDir, "list.json");
            if (!File.Exists(savePath)) return;
            StreamReader reader = new StreamReader(savePath, Encoding.UTF8);
            string jsonStr = reader.ReadToEnd();
            reader.Close();
            try
            {
                var list = JsonConvert.DeserializeObject<HousingOnSaleItem[]>(jsonStr);
                vm.UpdateSales(list);
                logger.LogInfo("已恢复上次保存的房屋列表");
            }
            catch (Exception ex)
            {
                logger.LogError("恢复上次保存的房屋列表失败：" + ex.Message);
            }
            reader.Close();
        }

        string DataDir => Path.Combine(Environment.CurrentDirectory, "AppData", "HousingCheck");
        string SnapshotDir => Path.Combine(DataDir, "snapshots");

        private void ButtonSaveToFile_Click(object sender, EventArgs e)
        {
            PrepareDir();
            string time = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string snapshotFilename = $"HousingCheck-{time}-Snapshot.csv";
            string landInfoFilename = $"HousingCheck-{time}-LandInfo.csv";

            using (var writer = new StreamWriter(Path.Combine(SnapshotDir, snapshotFilename), false, Encoding.UTF8))
            {
                SnapshotStorage.SaveCsv(writer);
            }

            if (LandInfoSignStorage.Count > 0)
            {
                using (var writer = new StreamWriter(Path.Combine(SnapshotDir, landInfoFilename), false, Encoding.UTF8))
                {
                    LandInfoSignStorage.WriteCSV(writer);
                }
            }

            logger.LogInfo($"已保存到 {SnapshotDir} 文件夹");
        }

        private string ListToString()
        {
            byte area = 0;
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var line in vm.Sales)
            {
                if (!line.CurrentStatus)
                    continue;

                if (line.Area == HouseArea.穹顶皓天 && config.IgnoreEmpyreum)
                    continue;

                stringBuilder.Append($"{line.AreaStr} 第{line.DisplaySlot}区 {line.DisplayId}号{line.SizeStr}房在售，当前价格:{line.Price} {Environment.NewLine}");

                if (line.Area >= 0) area |= (byte)(1 << (int)line.Area);
            }
            for (int i = 1; i <= (int)HouseArea.穹顶皓天; i++)
            {
                if ((area & (1 << i)) == 0)
                {
                    stringBuilder.Append($"{HousingItem.GetHouseAreaStr((HouseArea)i)} 无空房 {Environment.NewLine}");
                }
            }

            return stringBuilder.ToString();
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
                    bool autoUpload = config.AutoUpload;
                    bool uploadSnapshot = config.EnableUploadSnapshot;
                    ApiVersion apiVersion = config.UploadApiVersion;

                    if (ManualUpload)
                    {
                        logger.LogDebug("手动开始上报");
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
                        logger.LogDebug("手动上报任务执行完毕");
                    }
                    else if (actionTime <= new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds())
                    {
                        if (HousingListUpdated)
                        {
                            //保存列表文件
                            SaveHousingList();
                            logger.LogInfo("房屋信息已保存");
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
                    logger.LogError("执行定时任务时出现错误: " + ex.Message);
                }

                Thread.Sleep(500);
            }

            logger.LogError("上报线程退出！！！！");
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
            if (config.EnableTTS)
            {
                ActGlobals.oFormActMain.TTS("查房提醒：该查房了");
            }
            if (config.EnableNotification)
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
                if (DateTime.Now > nextNotify.AddSeconds(-config.CheckNotifyAheadTime))
                {
                    if (lastNotify != nextNotify && config.EnableNotifyCheck)
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
                w.Timeout = 60 * 1000; // 60s
                return w;
            }
        }

        System.Reflection.AssemblyName assemblyName => System.Reflection.Assembly.GetExecutingAssembly().GetName();

        public bool UploadData(string type, string postContent, string mime = "application/json")
        {
            var wb = new CustomWebClient();
            var token = config.UploadToken.Trim();
            if (token != "")
            {
                wb.Headers[HttpRequestHeader.Authorization] = "Token " + token;
            }
            wb.Headers[HttpRequestHeader.ContentType] = mime;
            wb.Headers.Add(HttpRequestHeader.UserAgent, assemblyName.Name + "/" + assemblyName.Version);

            string url;
            switch (config.UploadApiVersion)
            {
                case ApiVersion.V1:
                    url = config.UploadUrl;
                    break;
                case ApiVersion.V2:
                default:
                    url = config.UploadUrl.TrimEnd('/') + "/" + type;
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
                            logger.LogError("上传出错：" + jsonRes["errorMessage"]);
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
                            logger.LogError("上传出错：" + res);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError("上传出错：" + ex.Message);
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
                    postContent = JsonConvert.SerializeObject(vm.Sales);
                    break;
                case ApiVersion.V1:
                    postContent = "text=" + WebUtility.UrlEncode(ListToString());
                    mime = "application/x-www-form-urlencoded";
                    break;
            }
            if (postContent.Length == 0)
            {
                logger.LogError("上报数据为空");
            }
            //Log("Debug", postContent);
            logger.LogInfo("正在上传房屋列表");
            bool res = UploadData("info", postContent, mime);

            if (res)
            {
                logger.LogInfo("房屋列表上报成功");
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
                logger.LogInfo("正在上传房区快照");
                bool res = UploadData("info", json);
                if (res)
                {
                    logger.LogInfo("房区快照上报成功");
                }
                else
                {
                    Thread.Sleep(1000);
                }
            }
            catch (Exception ex)
            {
                logger.LogError("房区快照上报出错：" + ex.Message);
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
                logger.LogInfo("正在上传房屋详细信息");
                bool res = UploadData("detail", json);
                if (res)
                {
                    logger.LogInfo("房屋详细信息上报成功");
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
                logger.LogError("房屋详细信息上报出错：" + ex.Message);
            }
        }
    }
}
