using System;
using System.Text;
using System.Xml;
using System.IO;
using Advanced_Combat_Tracker;

namespace HousingCheck
{
    public class Config
    {
        /// <summary>
        /// 国服 Opcode
        /// </summary>
        public const int OPCODE_WARD_INFO = 882;
        public const int OPCODE_LAND_INFO = 998;
        public const int OPCODE_SALE_INFO = 429;
        public const int OPCODE_CLIENT_TRIGGER = 402;

        /// <summary>
        /// 上报API版本
        /// </summary>
        public ApiVersion UploadApiVersion { get; set; }
        /// <summary>
        /// 自动上报
        /// </summary>
        public bool AutoUpload { get; set; }
        /// <summary>
        /// 是否上报快照
        /// </summary>
        public bool EnableUploadSnapshot { get; set; }
        /// <summary>
        /// 上报地址
        /// </summary>
        public string UploadUrl { get; set; }
        /// <summary>
        /// 上报Token
        /// </summary>
        public string UploadToken { get; set; }

        /// <summary>
        /// 是否启用TTS通知
        /// </summary>
        public bool EnableTTS { get; set; }
        /// <summary>
        /// 是否启用操作系统通知
        /// </summary>
        public bool EnableNotification { get; set; }
        /// <summary>
        /// 启用S房通知提醒
        /// </summary>
        public bool EnableNotifyHouseS { get; set; }
        /// <summary>
        /// 启用ML房通知提醒
        /// </summary>
        public bool EnableNotifyHouseML { get; set; }

        /// <summary>
        /// 忽略穹顶皓天
        /// </summary>
        /// <remarks>
        /// 不发送提醒消息，也不在剪贴板中显示
        /// </remarks>
        public bool IgnoreEmpyreum { get; set; }

        /// <summary>
        /// 启用整点查房提醒
        /// </summary>
        public bool EnableNotifyCheck { get; set; }

        /// <summary>
        /// 查房提醒提前时间（秒）
        /// </summary>
        public int CheckNotifyAheadTime { get; set; }

        /// <summary>
        /// 调试模式是否开启
        /// </summary>
        public bool DebugEnabled { get; set; }
        /// <summary>
        /// 是否勾上禁用Opcode检测
        /// </summary>
        public bool DisableOpcodeCheck { get; set; }
        /// <summary>
        /// 是否勾上使用自定义Opcode
        /// </summary>
        public bool UseCustomOpcode { get; set; }
        /// <summary>
        /// 自定义房屋列表Opcode
        /// </summary>
        public int CustomOpcodeWard { get; set; }
        /// <summary>
        /// 自定义房屋门牌Opcode
        /// </summary>
        public int CustomOpcodeLand { get; set; }

        /// <summary>
        /// 自定义房屋售卖Opcode
        /// </summary>
        public int CustomOpcodeSale { get; set; }

        /// <summary>
        /// 房屋列表Opcode
        /// </summary>
        public int OpcodeWard => (UseCustomOpcode && !DisableOpcodeCheck) ? CustomOpcodeWard : OPCODE_WARD_INFO;
        /// <summary>
        /// 房屋门牌Opcode
        /// </summary>
        public int OpcodeLand => (UseCustomOpcode && !DisableOpcodeCheck) ? CustomOpcodeLand : OPCODE_LAND_INFO;
        /// <summary>
        /// 房屋售卖信息Opcode
        /// </summary>
        public int OpcodeSale => (UseCustomOpcode && !DisableOpcodeCheck) ? CustomOpcodeSale : OPCODE_SALE_INFO;

        public int OpcodeClientTrigger => OPCODE_CLIENT_TRIGGER;

        private static readonly string SettingsFile = Path.Combine(ActGlobals.oFormActMain.AppDataFolder.FullName, "Config\\HousingCheck.config.xml");

        public void LoadSettings()
        {
            if (File.Exists(SettingsFile))
            {
                try
                {
                    XmlDocument xdo = new XmlDocument();
                    xdo.Load(SettingsFile);
                    XmlNode head = xdo.SelectSingleNode("Config");
                    UploadUrl = head?.SelectSingleNode("UploadURL")?.InnerText;
                    UploadApiVersion = (ApiVersion)int.Parse(head?.SelectSingleNode("UploadApiVersion")?.InnerText);
                    UploadToken = head?.SelectSingleNode("UploadToken")?.InnerText;
                    AutoUpload = bool.Parse(head?.SelectSingleNode("AutoUpload")?.InnerText ?? "false");
                    //checkBoxML.Checked = bool.Parse(head?.SelectSingleNode("UploadMLOnly")?.InnerText ?? "true");
                    EnableUploadSnapshot = bool.Parse(head?.SelectSingleNode("UploadSnapshot")?.InnerText ?? "true");
                    EnableTTS = bool.Parse(head?.SelectSingleNode("TTSNotify")?.InnerText ?? "false");
                    EnableNotification = bool.Parse(head?.SelectSingleNode("ShellNotify")?.InnerText ?? "false");
                    EnableNotifyHouseML = bool.Parse(head?.SelectSingleNode("NotifyHouseML")?.InnerText ?? "true");
                    EnableNotifyHouseS = bool.Parse(head?.SelectSingleNode("NotifyHouseS")?.InnerText ?? "false");
                    IgnoreEmpyreum = bool.Parse(head?.SelectSingleNode("IgnoreEmpyreum")?.InnerText ?? "true");
                    EnableNotifyCheck = bool.Parse(head?.SelectSingleNode("NotifyCheck")?.InnerText ?? "false");
                    CheckNotifyAheadTime = int.Parse(head?.SelectSingleNode("NotifyCheckAhead")?.InnerText ?? "120");

                    DebugEnabled = bool.Parse(head?.SelectSingleNode("Debug")?.InnerText ?? "false");
                    DisableOpcodeCheck = bool.Parse(head?.SelectSingleNode("DisableOpcodeCheck")?.InnerText ?? "false");
                    UseCustomOpcode = bool.Parse(head?.SelectSingleNode("UseCustomOpcode")?.InnerText ?? "false");

                    CustomOpcodeWard = int.Parse(head?.SelectSingleNode("CustomOpcodeWard")?.InnerText ?? OPCODE_WARD_INFO.ToString());
                    CustomOpcodeLand = int.Parse(head?.SelectSingleNode("CustomOpcodeLand")?.InnerText ?? OPCODE_LAND_INFO.ToString());
                    CustomOpcodeSale = int.Parse(head?.SelectSingleNode("CustomOpcodeSale")?.InnerText ?? OPCODE_SALE_INFO.ToString());
                }
                catch (Exception e)
                {
                    ActGlobals.oFormActMain.NotificationAdd("读取配置文件出错", e.ToString());
                    // File.Delete(SettingsFile);
                }
            }
        }

        public void SaveSettings()
        {
            FileStream fs = new FileStream(SettingsFile, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            XmlTextWriter xWriter = new XmlTextWriter(fs, Encoding.UTF8) { Formatting = Formatting.Indented, Indentation = 1, IndentChar = '\t' };
            xWriter.WriteStartDocument(true);
            xWriter.WriteStartElement("Config");    // <Config>
            xWriter.WriteElementString("UploadURL", UploadUrl);
            xWriter.WriteElementString("UploadApiVersion", ((int)UploadApiVersion).ToString());
            xWriter.WriteElementString("UploadToken", UploadToken);
            xWriter.WriteElementString("AutoUpload", AutoUpload.ToString());
            //xWriter.WriteElementString("UploadMLOnly", checkBoxML.Checked.ToString());
            xWriter.WriteElementString("UploadSnapshot", EnableUploadSnapshot.ToString());
            xWriter.WriteElementString("TTSNotify", EnableTTS.ToString());
            xWriter.WriteElementString("ShellNotify", EnableNotification.ToString());
            xWriter.WriteElementString("NotifyHouseS", EnableNotifyHouseS.ToString());
            xWriter.WriteElementString("NotifyHouseML", EnableNotifyHouseML.ToString());
            xWriter.WriteElementString("IgnoreEmpyreum", IgnoreEmpyreum.ToString());
            xWriter.WriteElementString("NotifyCheck", EnableNotifyCheck.ToString());
            xWriter.WriteElementString("NotifyCheckAhead", CheckNotifyAheadTime.ToString());

            xWriter.WriteElementString("Debug", DebugEnabled.ToString());
            xWriter.WriteElementString("DisableOpcodeCheck", DisableOpcodeCheck.ToString());
            xWriter.WriteElementString("UseCustomOpcode", UseCustomOpcode.ToString());
            xWriter.WriteElementString("CustomOpcodeWard", CustomOpcodeWard.ToString());
            xWriter.WriteElementString("CustomOpcodeLand", CustomOpcodeLand.ToString());
            xWriter.WriteElementString("CustomOpcodeSale", CustomOpcodeSale.ToString());

            xWriter.WriteEndElement();              // </Config>
            xWriter.WriteEndDocument();             // Tie up loose ends (shouldn't be any)
            xWriter.Flush();                        // Flush the file buffer to disk
            xWriter.Close();
        }

    }
}
