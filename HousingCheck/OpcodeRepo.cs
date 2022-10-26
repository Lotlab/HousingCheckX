using Lotlab.PluginCommon.FFXIV.Parser;
using Lotlab.PluginCommon.Updater;
using Lotlab.PluginCommon;
using System.Threading.Tasks;
using System;
using System.IO;
using System.Collections.Generic;

namespace HousingCheck
{
    class OpcodeRepo
    {
        SimpleLogger logger { get; }
        string localFilePath { get; }
        string remoteFileUrl { get; }

        public string LocalVersion { get; private set; }

        WizardOpcodeReader reader = new WizardOpcodeReader();

        Dictionary<ushort, string> opcodes = new Dictionary<ushort, string>();

        public OpcodeRepo(SimpleLogger logger, string local, string remote)
        {
            this.logger = logger;
            this.localFilePath = local;
            this.remoteFileUrl = remote;
        }

        public void SetOpcode(NetworkParser parser)
        {
            parser.ClearOpcodes();
            parser.SetOpcodes(opcodes);
        }

        /// <summary>
        /// 载入本地Opcode
        /// </summary>
        public void LoadLocalOpcode()
        {
            if (!File.Exists(localFilePath)) return;

            var content = File.ReadAllText(localFilePath);
            LocalVersion = getVersion(content);

            opcodes = reader.Parse(content);
        }

        public async Task GetOnlineOpcode()
        {
            var content = await HttpWrapper.GetAsync(remoteFileUrl);
            var version = getVersion(content);

            opcodes = reader.Parse(content);

            // write to file
            File.WriteAllText(localFilePath, content);
            LocalVersion = version;
        }

        private string getVersion(string content)
        {
            var lines = content.Split(new string[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (lines[0].StartsWith("// "))
                return lines[0].TrimStart(new char[] { '/', ' ' });

            return null;
        }
    }
}
