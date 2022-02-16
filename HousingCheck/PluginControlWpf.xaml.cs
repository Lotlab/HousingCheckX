using Lotlab.PluginCommon;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HousingCheck
{
    /// <summary>
    /// PluginControlWpf.xaml 的交互逻辑
    /// </summary>
    public partial class PluginControlWpf : UserControl
    {
        public PluginControlWpf()
        {
            InitializeComponent();
        }

        private void UploadManaually(object sender, RoutedEventArgs e)
        {
            (DataContext as PluginControlViewModel)?.Invoke("UploadManaually");
        }
        private void CopyToClipboard(object sender, RoutedEventArgs e)
        {
            (DataContext as PluginControlViewModel)?.Invoke("CopyToClipboard");
        }
        private void SaveToFile(object sender, RoutedEventArgs e)
        {
            (DataContext as PluginControlViewModel)?.Invoke("SaveToFile");
        }
        private void TestNotification(object sender, RoutedEventArgs e)
        {
            (DataContext as PluginControlViewModel)?.Invoke("TestNotification");
        }
    }

    public static class AutoScrollBehavior
    {
        public static readonly DependencyProperty AutoScrollProperty =
            DependencyProperty.RegisterAttached("AutoScroll", typeof(bool), typeof(AutoScrollBehavior), new PropertyMetadata(false, AutoScrollPropertyChanged));


        public static void AutoScrollPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var scrollViewer = obj as ScrollViewer;
            if (scrollViewer != null && (bool)args.NewValue)
            {
                scrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;
                scrollViewer.ScrollToEnd();
            }
            else
            {
                scrollViewer.ScrollChanged -= ScrollViewer_ScrollChanged;
            }
        }

        private static void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            // Only scroll to bottom when the extent changed. Otherwise you can't scroll up
            if (e.ExtentHeightChange != 0)
            {
                var scrollViewer = sender as ScrollViewer;
                scrollViewer?.ScrollToBottom();
            }
        }

        public static bool GetAutoScroll(DependencyObject obj)
        {
            return (bool)obj.GetValue(AutoScrollProperty);
        }

        public static void SetAutoScroll(DependencyObject obj, bool value)
        {
            obj.SetValue(AutoScrollProperty, value);
        }
    }

    public class LogItemToStringValueConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is LogItem l)
            {
                return string.Format("[{0}] [{1}] {2}", l.Time.ToLongTimeString(), l.Level, l.Content);
            }
            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
