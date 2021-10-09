using System;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using Advanced_Combat_Tracker;
using System.Collections.Generic;

namespace HousingCheck
{
    public partial class PluginControl : UserControl
    {
        public ApiVersion UploadApiVersion
        {
            get {
                return (selectApiVersion.SelectedItem == null) ? ApiVersion.V2 : (ApiVersion)selectApiVersion.SelectedValue;
            }
            set { }
        }

        public bool EnableUploadSnapshot {
            get
            {
                return checkBoxUploadSnapshot.Checked;
            }
            set
            {
                checkBoxUploadSnapshot.Checked = value;
            }
        }

        public string UploadUrl
        {
            get
            {
                return textBoxUpload.Text;
            }
            set
            {
                textBoxUpload.Text = value;
            }
        }

        public string UploadToken
        {
            get
            {
                return textBoxUploadToken.Text;
            }
            set
            {
                textBoxUploadToken.Text = value;
            }
        }

        /// <summary>
        /// 是否启用TTS通知
        /// </summary>
        public bool EnableTTS => checkboxTTS.Checked;
        /// <summary>
        /// 是否启用操作系统通知
        /// </summary>
        public bool EnableNotification => checkBoxNotification.Checked;
        /// <summary>
        /// 启用S房通知提醒
        /// </summary>
        public bool EnableNotifyHouseS => checkBoxNotifyS.Checked;
        /// <summary>
        /// 启用ML房通知提醒
        /// </summary>
        /// <remarks>
        /// 为防止错过提示，此选项永远为True
        /// </remarks>
        public bool EnableNotifyHouseML => true;

        /// <summary>
        /// 启用整点查房提醒
        /// </summary>
        public bool EnableNotifyCheck => checkBoxNotifyCheck.Checked;

        /// <summary>
        /// 查房提醒提前时间（秒）
        /// </summary>
        public int CheckNotifyAheadTime => (int)numericUpDownNotifyCheck.Value;

        private static readonly string SettingsFile = Path.Combine(ActGlobals.oFormActMain.AppDataFolder.FullName, "Config\\HousingCheck.config.xml");
        public bool upload;
        public Dictionary<string, ApiVersion> apiVersionList = new Dictionary<string, ApiVersion>();
        public PluginControl()
        {
            InitializeComponent();

            //API版本选择
            apiVersionList.Add("V1", ApiVersion.V1);
            apiVersionList.Add("V2", ApiVersion.V2);
            selectApiVersion.DataSource = new BindingSource(apiVersionList, null);
            selectApiVersion.DisplayMember = "Key";
            selectApiVersion.ValueMember = "Value";
            selectApiVersion.SelectedIndex = 1;
        }

        private void checkBoxUpload_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;
            upload = checkBox.Checked;

            this.textBoxUpload.ReadOnly = !upload;
            this.textBoxUpload.Enabled = upload;

            this.textBoxUploadToken.ReadOnly = !upload;
            this.textBoxUploadToken.Enabled = upload;

            //this.checkBoxML.Enabled = upload;
            this.checkBoxUploadSnapshot.Enabled = upload;
        }

        public void LoadSettings()
        {
            if (File.Exists(SettingsFile))
            {
                try
                {
                    XmlDocument xdo = new XmlDocument();
                    xdo.Load(SettingsFile);
                    XmlNode head = xdo.SelectSingleNode("Config");
                    textBoxUpload.Text = head?.SelectSingleNode("UploadURL")?.InnerText;
                    selectApiVersion.SelectedIndex = int.Parse(head?.SelectSingleNode("UploadApiVersion")?.InnerText);
                    textBoxUploadToken.Text = head?.SelectSingleNode("UploadToken")?.InnerText;
                    checkBoxUpload.Checked = bool.Parse(head?.SelectSingleNode("AutoUpload")?.InnerText ?? "false");
                    //checkBoxML.Checked = bool.Parse(head?.SelectSingleNode("UploadMLOnly")?.InnerText ?? "true");
                    checkBoxUploadSnapshot.Checked = bool.Parse(head?.SelectSingleNode("UploadSnapshot")?.InnerText ?? "true");
                    checkboxTTS.Checked = bool.Parse(head?.SelectSingleNode("TTSNotify")?.InnerText ?? "false");
                    checkBoxNotification.Checked = bool.Parse(head?.SelectSingleNode("ShellNotify")?.InnerText ?? "false");
                    checkBoxNotifyML.Checked = true;
                    checkBoxNotifyS.Checked = bool.Parse(head?.SelectSingleNode("NotifyHouseS")?.InnerText ?? "false");
                    checkBoxNotifyCheck.Checked = bool.Parse(head?.SelectSingleNode("NotifyCheck")?.InnerText ?? "false");
                    numericUpDownNotifyCheck.Value = int.Parse(head?.SelectSingleNode("NotifyCheckAhead")?.InnerText ?? "120");
                }
                catch (Exception)
                {
                    File.Delete(SettingsFile);
                }
            }
        }

        public void SaveSettings()
        {
            FileStream fs = new FileStream(SettingsFile, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            XmlTextWriter xWriter = new XmlTextWriter(fs, Encoding.UTF8) { Formatting = Formatting.Indented, Indentation = 1, IndentChar = '\t' };
            xWriter.WriteStartDocument(true);
            xWriter.WriteStartElement("Config");    // <Config>
            xWriter.WriteElementString("UploadURL", textBoxUpload.Text);
            xWriter.WriteElementString("UploadApiVersion", selectApiVersion.SelectedIndex.ToString());
            xWriter.WriteElementString("UploadToken", textBoxUploadToken.Text);
            xWriter.WriteElementString("AutoUpload", checkBoxUpload.Checked.ToString());
            //xWriter.WriteElementString("UploadMLOnly", checkBoxML.Checked.ToString());
            xWriter.WriteElementString("UploadSnapshot", checkBoxUploadSnapshot.Checked.ToString());
            xWriter.WriteElementString("TTSNotify", checkboxTTS.Checked.ToString());
            xWriter.WriteElementString("ShellNotify", checkBoxNotification.Checked.ToString());
            xWriter.WriteElementString("NotifyHouseS", checkBoxNotifyS.Checked.ToString());
            xWriter.WriteElementString("NotifyCheck", checkBoxNotifyCheck.Checked.ToString());
            xWriter.WriteElementString("NotifyCheckAhead", numericUpDownNotifyCheck.Value.ToString());
            xWriter.WriteEndElement();              // </Config>
            xWriter.WriteEndDocument();             // Tie up loose ends (shouldn't be any)
            xWriter.Flush();                        // Flush the file buffer to disk
            xWriter.Close();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void textBoxUpload_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
