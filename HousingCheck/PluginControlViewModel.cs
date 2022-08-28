using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using Lotlab.PluginCommon;

namespace HousingCheck
{
    public class PluginControlViewModel : PropertyNotifier
    {
        Config config { get; }
        SimpleLogger logger { get; }
        DataStorage storage { get; }

        public PluginControlViewModel(Config config, SimpleLogger logger, DataStorage storage)
        {
            this.config = config;
            this.logger = logger;
            this.storage = storage;
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
        public bool AutoUpdate { get => config.AutoUpdate; set { config.AutoUpdate = value; OnPropertyChanged(); } }
        public bool EnableTTS { get => config.EnableTTS; set { config.EnableTTS = value; OnPropertyChanged(); } }
        public bool EnableNotification { get => config.EnableNotification; set { config.EnableNotification = value; OnPropertyChanged(); } }
        public bool EnableNotifyHouseS { get => config.EnableNotifyHouseS; set { config.EnableNotifyHouseS = value; OnPropertyChanged(); } }
        public bool EnableNotifyHouseML { get => config.EnableNotifyHouseML; set { config.EnableNotifyHouseML = value; OnPropertyChanged(); } }
        public bool IgnoreEmpyreum { get => config.IgnoreEmpyreum; set { config.IgnoreEmpyreum = value; OnPropertyChanged(); } }
        public bool EnableNotifyCheck { get => config.EnableNotifyCheck; set { config.EnableNotifyCheck = value; OnPropertyChanged(); } }
        public string CheckNotifyAheadTime { get => config.CheckNotifyAheadTime.ToString(); set { config.CheckNotifyAheadTime = int.Parse(value); OnPropertyChanged(); } }
        public bool DebugEnabled { get => config.DebugEnabled; set { config.DebugEnabled = value; OnPropertyChanged(); logger.SetFilter(value ? LogLevel.DEBUG : LogLevel.INFO); OnPropertyChanged(nameof(DebugVisibility)); } }
        public bool EnableOpcodeGuess { get => config.EnableOpcodeGuess; set { config.EnableOpcodeGuess = value; OnPropertyChanged(); OnPropertyChanged(nameof(DisableOpcodeCheckEditable)); } }
        public bool DisableOpcodeCheck { get => config.DisableOpcodeCheck; set { config.DisableOpcodeCheck = value; OnPropertyChanged(); OnPropertyChanged(nameof(CustomOpcodeEditable)); OnPropertyChanged(nameof(UseCustomOpcodeEditable)); } }
        public bool UseCustomOpcode { get => config.UseCustomOpcode; set { config.UseCustomOpcode = value; OnPropertyChanged(); OnPropertyChanged(nameof(CustomOpcodeEditable)); } }
        public string CustomOpcodeWard { get => config.CustomOpcodeWard.ToString(); set { config.CustomOpcodeWard = int.Parse(value); OnPropertyChanged(); } }
        public string CustomOpcodeLand { get => config.CustomOpcodeLand.ToString(); set { config.CustomOpcodeLand = int.Parse(value); OnPropertyChanged(); } }
        public string CustomOpcodeSale { get => config.CustomOpcodeSale.ToString(); set { config.CustomOpcodeSale = int.Parse(value); OnPropertyChanged(); } }
        public string CustomOpcodeClientTrigger { get => config.CustomOpcodeClientTrigger.ToString(); set { config.CustomOpcodeClientTrigger = int.Parse(value); OnPropertyChanged(); } }

        public Visibility DebugVisibility => DebugEnabled ? Visibility.Visible : Visibility.Hidden;

        public bool CustomOpcodeEditable => UseCustomOpcode && !DisableOpcodeCheck;
        public bool UseCustomOpcodeEditable => !DisableOpcodeCheck;
        public bool DisableOpcodeCheckEditable => EnableOpcodeGuess;

        public ObservableCollection<LogItem> Logs => logger.ObserveLogs;

        public ObservableCollection<HousingOnSaleItem> Sales => storage.Sales;

        public SimpleCommand UploadManually { get; } = new SimpleCommand();
        public SimpleCommand CopyToClipboard { get; } = new SimpleCommand();
        public SimpleCommand SaveToFile { get; } = new SimpleCommand();
        public SimpleCommand TestNotification { get; } = new SimpleCommand();
        public SimpleCommand CheckUpdate { get; } = new SimpleCommand();
    }

    public class SimpleCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public event Action<object> OnExecute;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            OnExecute?.Invoke(parameter);
        }
    }
}
