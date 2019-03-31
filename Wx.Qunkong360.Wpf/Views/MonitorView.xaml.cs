using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using Wx.Qunkong360.Wpf.ContentViews;
using Wx.Qunkong360.Wpf.Events;
using Wx.Qunkong360.Wpf.Implementation;
using Wx.Qunkong360.Wpf.Utils;
using Xzy.EmbeddedApp.WinForm.Socket;

namespace Wx.Qunkong360.Wpf.Views
{
    /// <summary>
    /// MonitorView.xaml 的交互逻辑
    /// </summary>
    public partial class MonitorView
    {
        private static MonitorView _monitorView = null;

        private static readonly object SyncRoot = new object();

        private static bool _hasRegisteredEvents = false;

        private static List<MonitorControl> _monitors = new List<MonitorControl>();

        private static void RegisterEvents()
        {

            EventAggregatorManager.Instance.EventAggregator.GetEvent<NewDeviceAttachedEvent>().Subscribe(() =>
            {
                if (_monitorView == null)
                {
                    return;
                }

                _monitorView.lblBindingPhoneNumsValue.Text = $"{SocketServer.AllConnectionKey.Values.Count}";
                _monitorView.lblConnectedPhoneNumsValue.Text = $"{DeviceConnectionManager.Instance.Devices.Length}";

                if (_monitors.Count == 0)
                {
                    InitializeMonitorControls();
                }

                if (VmManager.Instance.RunningGroupIndex != -1)
                {
                    for (int i = 0; i < VmManager.Instance.Column; i++)
                    {
                        int index = VmManager.Instance.VmIndexArray[VmManager.Instance.RunningGroupIndex, i];

                        if (index != -1)
                        {
                            _monitors[i].lblPhoneId.Text = $"{ index + 1}";
                            _monitors[i].lblPhoneNickName.Text = DeviceConnectionManager.Instance.GetDeviceNameByMobileIndex(index);
                        }
                    }
                }

            }, ThreadOption.UIThread, true);


            EventAggregatorManager.Instance.EventAggregator.GetEvent<NewSocketConnectedEvent>().Subscribe((mobileIndex) =>
            {
                if (_monitorView == null)
                {
                    return;
                }

                _monitorView.lblBindingPhoneNumsValue.Text = $"{SocketServer.AllConnectionKey.Values.Count}";
                _monitorView.lblConnectedPhoneNumsValue.Text = $"{DeviceConnectionManager.Instance.Devices.Length}";

                if (_monitors.Count == 0)
                {
                    InitializeMonitorControls();
                }

                var monitor = _monitors.FirstOrDefault(monitorControl => monitorControl.lblPhoneId.Text == $"{mobileIndex + 1}");
                if (monitor!=null)
                {
                    monitor.imgConnectionStatus.Source = new BitmapImage(new Uri("../Images/红.png", UriKind.RelativeOrAbsolute));
                }
                //monitor.lblConnectionStatus.Background = new SolidColorBrush(Colors.Green);

            }, ThreadOption.UIThread, true);


            EventAggregatorManager.Instance.EventAggregator.GetEvent<VmClosedEvent>().Subscribe(() =>
            {
                _monitors.Clear();
                if (_monitorView == null)
                {
                    return;
                }

                _monitorView.lblBindingPhoneNumsValue.Text = $"{SocketServer.AllConnectionKey.Values.Count}";
                _monitorView.lblConnectedPhoneNumsValue.Text = $"{DeviceConnectionManager.Instance.Devices.Length}";

                int count = _monitorView.monitorContainer.Children.Count;
                _monitorView.monitorContainer.Children.RemoveRange(0, count);

                _monitorView.monitorContainer.Children.Clear();
            }, ThreadOption.UIThread, true);

            _hasRegisteredEvents = true;
        }

        public static void ShowMonitorView()
        {
            lock (SyncRoot)
            {
                if (!_hasRegisteredEvents)
                {
                    RegisterEvents();
                }

                if (_monitorView == null)
                {
                    Console.WriteLine("monitor view is null, go create a new one.\r\n");

                    _monitorView = new MonitorView();
                    _monitorView.Show();
                }
                else if (_monitorView.IsLoaded)
                {
                    Console.WriteLine("monitor view exists, clear it up and go create a new one.\r\n");

                    _monitorView.Close();
                    _monitorView = null;
                    _monitorView = new MonitorView();
                    _monitorView.Show();
                }
                else
                {
                    Console.WriteLine("monitor view is closed, go create a new one.\r\n");

                    _monitorView = null;
                    _monitorView = new MonitorView();
                    _monitorView.Show();
                }
            }
        }

        public static void WriteLog(int mobileIndex, string log)
        {
            var monitor = _monitors.FirstOrDefault(m => m.lblPhoneId.Text == $"{mobileIndex + 1}");

            if (monitor != null)
            {
                monitor.tbLog.Text += (log + "\r\n");
            }
        }

        public static void WriteThreadLog(int mobileIndex, string log)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() => {

                var monitor = _monitors.FirstOrDefault(m => m.lblPhoneId.Text == $"{mobileIndex + 1}");

                if (monitor != null)
                {
                   monitor.tbLog.Text += (log + "\r\n");
                }
            }));

            /*var monitor = _monitors.FirstOrDefault(m => m.lblPhoneId.Text == $"{mobileIndex + 1}");

            if (monitor != null)
            {                
                monitor.tbLog.Dispatcher.BeginInvoke(new Action(() => {

                    monitor.tbLog.Text += (log + "\r\n");
                }));
                //monitor.tbLog.Text += (log + "\r\n");
            }*/
        }

        public static void WriteThreadErrorLog(int mobileIndex, string log)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() => {

                var monitor = _monitors.FirstOrDefault(m => m.lblPhoneId.Text == $"{mobileIndex + 1}");
                string errortxt = SystemLanguageManager.Instance.ResourceManager.GetString("Facebook_OpError", SystemLanguageManager.Instance.CultureInfo);
                if (monitor != null)
                {
                    monitor.tbLog.Text += (log + "" + errortxt + "\r\n");
                }
            }));

            
        }

        public static void ClearLog()
        {
            _monitors.ForEach((m) =>
            {
                m.tbLog.Clear();
            });
        }

        private MonitorView()
        {
            InitializeComponent();
            Closing += MonitorView_Closing;
            Loaded += MonitorView_Loaded;
            Title = SystemLanguageManager.Instance.ResourceManager.GetString("Monitor", SystemLanguageManager.Instance.CultureInfo);
        }

        private void MonitorView_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            int count = monitorContainer.Children.Count;
            monitorContainer.Children.RemoveRange(0, count);
            _monitorView = null;
        }

        private void MonitorView_Loaded(object sender, RoutedEventArgs e)
        {
            //btnReconnect.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Reconnect", SystemLanguageManager.Instance.CultureInfo);
            lblBindingPhoneNumsItem.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Binding_Phone_Num", SystemLanguageManager.Instance.CultureInfo);
            lblConnectedPhoneNumsItem.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Connected_Phone_Num", SystemLanguageManager.Instance.CultureInfo);
            btnDetailsOrOverview.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Details", SystemLanguageManager.Instance.CultureInfo);

            lblBindingPhoneNumsValue.Text = "0";
            lblConnectedPhoneNumsValue.Text = "0";

            InitializeMonitorControls();
        }

        private static void InitializeMonitorControls()
        {
            if (VmManager.Instance.RunningGroupIndex != -1)
            {
                _monitorView.lblBindingPhoneNumsValue.Text = $"{SocketServer.AllConnectionKey.Values.Count}";
                _monitorView.lblConnectedPhoneNumsValue.Text = $"{DeviceConnectionManager.Instance.Devices.Length}";

                int RunningVmCount = VmManager.Instance.GetRunningCount();

                bool useCachedMonitors = false;
                if (_monitors.Count == RunningVmCount)
                {
                    useCachedMonitors = true;
                }
                else
                {
                    _monitors.Clear();
                    _monitorView.monitorContainer.Children.Clear();
                }

                for (int i = 0; i < VmManager.Instance.Column; i++)
                {
                    int index = VmManager.Instance.VmIndexArray[VmManager.Instance.RunningGroupIndex, i];

                    if (index != -1)
                    {
                        bool connected = SocketServer.AllConnectionKey.Values.Any(session => session.MobileIndex == index);


                        if (useCachedMonitors)
                        {
                            //_monitors[i].lblConnectionStatus.Background = connected ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red);
                            _monitors[i].imgConnectionStatus.Source = connected ? new BitmapImage(new Uri("../Images/红.png", UriKind.RelativeOrAbsolute)) : new BitmapImage(new Uri("../Images/灰.png", UriKind.RelativeOrAbsolute));
                            _monitors[i].lblPhoneId.Text = $"{index + 1}";
                            _monitors[i].lblPhoneNickName.Text = DeviceConnectionManager.Instance.GetDeviceNameByMobileIndex(index);

                            if (_monitors[i].Parent == null)
                            {
                                _monitorView.monitorContainer.Children.Add(_monitors[i]);
                            }
                        }
                        else
                        {
                            MonitorControl monitorControl = new MonitorControl()
                            {
                                //lblConnectionStatus = { Background = connected ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red) },
                                imgConnectionStatus = { Source= connected ? new BitmapImage(new Uri("../Images/红.png", UriKind.RelativeOrAbsolute)) : new BitmapImage(new Uri("../Images/灰.png", UriKind.RelativeOrAbsolute)) },
                                lblPhoneId = { Text = $"{index + 1}" },
                                lblPhoneNickName = { Text = DeviceConnectionManager.Instance.GetDeviceNameByMobileIndex(index), },
                            };

                            _monitors.Add(monitorControl);
                            _monitorView.monitorContainer.Children.Add(monitorControl);
                        }
                    }
                }
            }
        }

        private void btnDetailsOrOverview_Click(object sender, RoutedEventArgs e)
        {
            if (btnDetailsOrOverview.Content.ToString() == SystemLanguageManager.Instance.ResourceManager.GetString("Details", SystemLanguageManager.Instance.CultureInfo))
            {
                // show details

                for (int i = 0; i < _monitors.Count; i++)
                {
                    _monitors[i].tbLog.Visibility = Visibility.Visible;
                }

                btnDetailsOrOverview.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Overview", SystemLanguageManager.Instance.CultureInfo);
            }
            else if (btnDetailsOrOverview.Content.ToString() == SystemLanguageManager.Instance.ResourceManager.GetString("Overview", SystemLanguageManager.Instance.CultureInfo))
            {
                // show overview

                for (int i = 0; i < _monitors.Count; i++)
                {
                    _monitors[i].tbLog.Visibility = Visibility.Collapsed;
                }

                btnDetailsOrOverview.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Details", SystemLanguageManager.Instance.CultureInfo);
            }

        }

        //private void btnReconnect_Click(object sender, RoutedEventArgs e)
        //{
        //    if (VmManager.Instance.RunningGroupIndex == -1)
        //    {
        //        MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("Error_Please_Launch_A_Vm", SystemLanguageManager.Instance.CultureInfo));

        //        return;
        //    }

        //    _monitorView.lblReconnectTips.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Reconnecting", SystemLanguageManager.Instance.CultureInfo);

        //    _monitorView.lblConnectedPhoneNumsValue.Text = "0";
        //    _monitorView.lblBindingPhoneNumsValue.Text = "0";

        //    _monitors.Clear();
        //    _monitorView.monitorContainer.Children.Clear();

        //    DeviceConnectionManager.Instance.ReconnectDevices();
        //}
    }
}
