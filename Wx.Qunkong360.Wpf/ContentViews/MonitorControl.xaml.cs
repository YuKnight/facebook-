using System.Windows.Controls;

namespace Wx.Qunkong360.Wpf.ContentViews
{
    /// <summary>
    /// MonitorControl.xaml 的交互逻辑
    /// </summary>
    public partial class MonitorControl : UserControl
    {
        public MonitorControl()
        {
            InitializeComponent();

            //btnReconnect.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Reconnect", SystemLanguageManager.Instance.CultureInfo);
        }

        //private void btnReconnect_Click(object sender, RoutedEventArgs e)
        //{
        //    DeviceConnectionManager.Instance.ReconnectDevices();
        //}
    }
}
