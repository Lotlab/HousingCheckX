using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Threading;
using Lotlab.PluginCommon;

namespace HousingCheck
{
    public class PluginControlViewModel : PropertyNotifier
    {
        Config config;
        SimpleLogger logger { get; }
        readonly object salesLock = new object();
        public PluginControlViewModel(Config config, SimpleLogger logger)
        {
            this.config = config;
            this.logger = logger;

            BindingOperations.EnableCollectionSynchronization(Sales, salesLock);
        }
        public int UploadApiVersion
        {
            get => (int)config.UploadApiVersion;
            set
            {
                config.UploadApiVersion = (ApiVersion)value;
                OnPropertyChanged();
            }
        }
        public bool AutoUpload { get => config.AutoUpload; set { config.AutoUpload = value; OnPropertyChanged(); } }
        public bool EnableUploadSnapshot { get => config.EnableUploadSnapshot; set { config.EnableUploadSnapshot = value; OnPropertyChanged(); } }
        public string UploadUrl { get => config.UploadUrl; set { config.UploadUrl = value; OnPropertyChanged(); } }
        public string UploadToken { get => config.UploadToken; set { config.UploadToken = value; OnPropertyChanged(); } }
        public bool EnableTTS { get => config.EnableTTS; set { config.EnableTTS = value; OnPropertyChanged(); } }
        public bool EnableNotification { get => config.EnableNotification; set { config.EnableNotification = value; OnPropertyChanged(); } }
        public bool EnableNotifyHouseS { get => config.EnableNotifyHouseS; set { config.EnableNotifyHouseS = value; OnPropertyChanged(); } }
        public bool IgnoreEmpyreum { get => config.IgnoreEmpyreum; set { config.IgnoreEmpyreum = value; OnPropertyChanged(); } }
        public bool EnableNotifyCheck { get => config.EnableNotifyCheck; set { config.EnableNotifyCheck = value; OnPropertyChanged(); } }
        public string CheckNotifyAheadTime { get => config.CheckNotifyAheadTime.ToString(); set { config.CheckNotifyAheadTime = int.Parse(value); OnPropertyChanged(); } }
        public bool DebugEnabled { get => config.DebugEnabled; set { config.DebugEnabled = value; OnPropertyChanged(); logger.SetFilter(value ? LogLevel.DEBUG : LogLevel.INFO); OnPropertyChanged(nameof(DebugVisibility)); } }
        public bool DisableOpcodeCheck { get => config.DisableOpcodeCheck; set { config.DisableOpcodeCheck = value; OnPropertyChanged(); OnPropertyChanged(nameof(CustomOpcodeEditable)); OnPropertyChanged(nameof(UseCustomOpcodeEditable)); } }
        public bool UseCustomOpcode { get => config.UseCustomOpcode; set { config.UseCustomOpcode = value; OnPropertyChanged(); OnPropertyChanged(nameof(CustomOpcodeEditable)); } }
        public string CustomOpcodeWard { get => config.CustomOpcodeWard.ToString(); set { config.CustomOpcodeWard = int.Parse(value); OnPropertyChanged(); } }
        public string CustomOpcodeLand { get => config.CustomOpcodeLand.ToString(); set { config.CustomOpcodeLand = int.Parse(value); OnPropertyChanged(); } }

        public Visibility DebugVisibility => DebugEnabled ? Visibility.Visible : Visibility.Hidden;

        public bool CustomOpcodeEditable => UseCustomOpcode && !DisableOpcodeCheck;
        public bool UseCustomOpcodeEditable => !DisableOpcodeCheck;

        public ObservableCollection<LogItem> Logs => logger.ObserveLogs;

        public ObservableCollection<HousingOnSaleItem> Sales { get; private set; } = new ObservableCollection<HousingOnSaleItem>();

        public void UpdateSales(IEnumerable<HousingOnSaleItem> items)
        {
            lock (salesLock)
            {
                foreach (HousingOnSaleItem item in items)
                {
                    int listIndex;
                    if ((listIndex = Sales.IndexOf(item)) != -1)
                    {
                        Sales[listIndex].Update(item);
                    }
                    else
                    {
                        Sales.Add(item);
                    }
                }
            }
        }

        public void RemoveSales(IEnumerable<HousingOnSaleItem> items)
        {
            lock (salesLock)
            {
                foreach (var item in items)
                {
                    if (Sales.Contains(item))
                    {
                        Sales.Remove(item);
                    }
                }
            }
        }

        public event Action<string> OnInvoke;

        public void Invoke(string arg)
        {
            OnInvoke?.Invoke(arg);
        }
    }
}
