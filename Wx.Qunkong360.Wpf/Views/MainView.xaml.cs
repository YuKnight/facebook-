using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using Wx.Qunkong360.Wpf.Implementation;
using Xzy.EmbeddedApp.Model;
using Xzy.EmbeddedApp.Utils;
using Panel = System.Windows.Forms.Panel;
using Point = System.Drawing.Point;
using Wx.Qunkong360.Wpf.Utils;
using Xzy.EmbeddedApp.WinForm.Socket;
using System.Windows.Media;
using Wx.Qunkong360.Wpf.Views;
using System.Threading.Tasks;
using Wx.Qunkong360.Wpf.Events;
using System.Windows.Input;
using Prism.Commands;
using Xzy.EmbeddedApp.WinForm.Tasks;

namespace Wx.Qunkong360.Wpf
{
    /// <summary>
    /// MainView.xaml 的交互逻辑
    /// </summary>
    public partial class MainView
    {
        public ICommand OpenTestViewCommand { get; set; }
        bool _bootable = true;
        private int _runningGroupIndex = -1;
        private IList<Button> _groupButtons = new List<Button>();

        public int RunningGroupIndex
        {
            get { return _runningGroupIndex; }
            private set
            {
                _runningGroupIndex = value;

                VmManager.Instance.RunningGroupIndex = value;

                SyncGroupStatus();
            }
        }

        private void SyncGroupStatus()
        {
            if (_runningGroupIndex == -1)
            {
                foreach (var btn in _groupButtons)
                {
                    btn.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Launch", SystemLanguageManager.Instance.CultureInfo);
                    btn.IsEnabled = true && _bootable;
                }
            }
            else
            {
                var runningGroupButton = _groupButtons.FirstOrDefault(btn => btn.Tag.ToString() == _runningGroupIndex.ToString());
                if (runningGroupButton != null)
                {
                    runningGroupButton.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Close", SystemLanguageManager.Instance.CultureInfo);
                }

                var notRunningGroupButtons = _groupButtons.Where(btn => btn.Tag.ToString() != _runningGroupIndex.ToString());

                foreach (var btn in notRunningGroupButtons)
                {
                    btn.IsEnabled = false;
                }
            }
        }

        public MainView()
        {
            InitializeComponent();

            OpenTestViewCommand = new DelegateCommand(() =>
            {
                ClickTestView clickTestView = new ClickTestView();
                clickTestView.Show();
            });

            DataContext = this;
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            EventAggregatorManager.Instance.EventAggregator.GetEvent<TaskUpdatedEvent>().Subscribe((taskProgress) =>
            {
                lblCurrentTaskValue.Text = taskProgress;
            }, Prism.Events.ThreadOption.UIThread, true);

            Assembly assembly = Assembly.GetExecutingAssembly();

            Type type = MethodBase.GetCurrentMethod().DeclaringType;
            string nspace = type.Namespace;

            string resourceName = nspace + ".Images.vmbackground.png";
            Stream stream = assembly.GetManifestResourceStream(resourceName);

            container.BackgroundImage = System.Drawing.Image.FromStream(stream);
            container.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;

            btnLaunchWhatsApp.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Launch_WhatsApp", SystemLanguageManager.Instance.CultureInfo);
            btnCloseWhatsApp.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Close_WhatsApp", SystemLanguageManager.Instance.CultureInfo);

            btnLaunchMessenger.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Launch_Messenger", SystemLanguageManager.Instance.CultureInfo);
            btnCloseMessenger.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Close_Messenger", SystemLanguageManager.Instance.CultureInfo);

            //btnSettings.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Settings", SystemLanguageManager.Instance.CultureInfo);
            btnInstallApk.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Install_Uninstall_APK", SystemLanguageManager.Instance.CultureInfo);
            btnOperation.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Facebook_Operation", SystemLanguageManager.Instance.CultureInfo);
            //btnOperation.Content = SystemLanguageManager.Instance.ResourceManager.GetString("WhatsApp_Operation", SystemLanguageManager.Instance.CultureInfo);
            btnCloseAll.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Turn_Off_All_Phones", SystemLanguageManager.Instance.CultureInfo);
            Title = SystemLanguageManager.Instance.ResourceManager.GetString("Product_Name", SystemLanguageManager.Instance.CultureInfo);

            btnMonitor.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Monitor", SystemLanguageManager.Instance.CultureInfo);
            lblCurrentTaskItem.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Current_Task", SystemLanguageManager.Instance.CultureInfo);

            ProcessUtils.LDPath = ConfigManager.Instance.Config?.LDPath;

            if (string.IsNullOrEmpty(ProcessUtils.LDPath))
            {
                _bootable = false;
                MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("Error_Vm_Path_Not_Set", SystemLanguageManager.Instance.CultureInfo));
            }

            if (!Directory.Exists(ProcessUtils.LDPath))
            {
                _bootable = false;
                MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("Error_Vm_Path_Not_Exist", SystemLanguageManager.Instance.CultureInfo));
            }

            InitVmGroups();

        }

        private void InitVmGroups()
        {
            for (int row = 0; row < VmManager.Instance.Row; row++)
            {
                Grid panel = new Grid();

                panel.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(120, GridUnitType.Pixel), });
                panel.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(0, GridUnitType.Auto), });

                int endIndex = VmManager.Instance.VmIndexArray[row, VmManager.Instance.Column - 1];
                int endNumber = endIndex == -1 ? VmManager.Instance.MaxVmNumber : endIndex + 1;

                TextBlock label = new TextBlock()
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    Text = string.Format(SystemLanguageManager.Instance.ResourceManager.GetString("Group", SystemLanguageManager.Instance.CultureInfo), row + 1, $"{VmManager.Instance.VmIndexArray[row, 0] + 1}-{ endNumber}"),
                    Foreground = new SolidColorBrush(Colors.White),
                };

                Button btn = new Button()
                {
                    Content = SystemLanguageManager.Instance.ResourceManager.GetString("Launch", SystemLanguageManager.Instance.CultureInfo),
                    IsEnabled = _bootable,
                    Tag = row,
                };

                btn.Click += Btn_Click;

                _groupButtons.Add(btn);

                panel.Children.Add(label);
                panel.Children.Add(btn);

                Grid.SetColumn(label, 0);
                Grid.SetColumn(btn, 1);

                listGroups.Items.Add(panel);
            }
        }

        /// <summary>
        /// 开启一组模拟手机
        /// </summary>
        public async void OpenGroupMobile(int groupIndex)
        {
            RunningGroupIndex = groupIndex;

            DeviceConnectionManager.Instance.LaunchAndroidTestOnTheBackground(5000);

            double tempRow = (double)VmManager.Instance.Column / (double)ConfigVals.RowNums;

            int _row = (int)Math.Ceiling(tempRow);

            int[,] insideGroupIndexArray = new int[_row, ConfigVals.RowNums];

            container.Controls.Clear();

            string ip = GetLocalIP();

            for (int i = 0; i < _row; i++)
            {
                for (int j = 0; j < ConfigVals.RowNums; j++)
                {
                    int insideGroupIndex = i * ConfigVals.RowNums + j;

                    if (insideGroupIndex > VmManager.Instance.Column - 1)
                    {
                        continue;
                    }

                    int vmIndex = VmManager.Instance.VmIndexArray[groupIndex, insideGroupIndex];

                    insideGroupIndexArray[i, j] = vmIndex;

                    if (vmIndex != -1)
                    {
                        Panel vmContainer = new Panel()
                        {
                            Width = 320,
                            Height = 480,
                            Location = new Point()
                            {
                                X = j * (320 + 3) + 3,
                                Y = i * (480 + 3) + 3,
                            }
                        };

                        container.Controls.Add(vmContainer);

                        string currtime = DateTime.Now.ToString("yyyyMMddHHmmssff");
                        string currindex = vmIndex.ToString().PadLeft(4, '0');

                        ProcessUtils.Init(vmIndex, new Simulator()
                        {
                            Cpu = 1,
                            Memory = 1024,
                            Width = 320,
                            Height = 480,
                            Dpi = 120,
                            //Imei = ip + currindex
                            //Imei = "auto",
                            Androidid = ip + currindex

                        });

                        ProcessUtils.Run(vmIndex);
                        //加入到对应关系集合中
                        VmManager.Instance.AddVmModel(vmIndex, new Abstract.VmModel() { Imei = ip + currindex, Index = vmIndex, AndroidId = ip });
                        var latestVmProcess = Process.GetProcessesByName("dnplayer").OrderByDescending(p => p.StartTime).FirstOrDefault();

                        if (latestVmProcess == null)
                        {
                            string error = SystemLanguageManager.Instance.ResourceManager.GetString("Error_No_Vm_Process", SystemLanguageManager.Instance.CultureInfo);
                            LogUtils.Error($"{error} vmIndex:{vmIndex}");
                            throw new Exception(error);
                        }

                        DateTime now1 = DateTime.Now;

                        while (latestVmProcess.MainWindowHandle == IntPtr.Zero)
                        {
                            if (DateTime.Now.Subtract(now1).TotalSeconds > 7)
                            {
                                string error = SystemLanguageManager.Instance.ResourceManager.GetString("Error_Main_Window_Handle_Timeout", SystemLanguageManager.Instance.CultureInfo);
                                LogUtils.Error(error);
                                //MessageDialogManager.ShowDialogAsync("轮询模拟器的主窗口句柄超时！");
                                break;
                            }
                        }

                        int setParentResult = WinAPIs.SetParent(latestVmProcess.MainWindowHandle, vmContainer.Handle);

                        if (setParentResult != 65552)
                        {
                            LogUtils.Error($"SetParent result:{setParentResult}, vmIndex:{vmIndex}");
                        }

                        int moveWindowResult = WinAPIs.MoveWindow(latestVmProcess.MainWindowHandle, 0, -35, 320, 515, true);

                        if (moveWindowResult != 1)
                        {
                            LogUtils.Error($"MoveWindow result:{moveWindowResult}, vmIndex:{vmIndex}");
                        }

                        //Log.Logger.Information($"VmIndex：{vmIndex}, SetParent Result：{setParentResult}, MoveWindow Result：{moveWindowResult}");

                        //btn.Text = "关闭";
                    }

                    await Task.Delay(500);
                }
            }
        }


        private async  void Btn_Click(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show("启动了模拟手机");
            Button btn = sender as Button;

            try
            {
                if (btn.Content.ToString() == SystemLanguageManager.Instance.ResourceManager.GetString("Launch", SystemLanguageManager.Instance.CultureInfo))
                {
                    int groupIndex = int.Parse(btn.Tag.ToString());

                    RunningGroupIndex = groupIndex;

                    DeviceConnectionManager.Instance.LaunchAndroidTestOnTheBackground(5000);

                    double tempRow = (double)VmManager.Instance.Column / (double)ConfigVals.RowNums;

                    int _row = (int)Math.Ceiling(tempRow);

                    int[,] insideGroupIndexArray = new int[_row, ConfigVals.RowNums];

                    container.Controls.Clear();

                    string ip = GetLocalIP();

                    for (int i = 0; i < _row; i++)
                    {
                        for (int j = 0; j < ConfigVals.RowNums; j++)
                        {
                            int insideGroupIndex = i * ConfigVals.RowNums + j;

                            if (insideGroupIndex > VmManager.Instance.Column - 1)
                            {
                                continue;
                            }

                            int vmIndex = VmManager.Instance.VmIndexArray[groupIndex, insideGroupIndex];

                            insideGroupIndexArray[i, j] = vmIndex;

                            if (vmIndex != -1)
                            {
                                Panel vmContainer = new Panel()
                                {
                                    Width = 320,
                                    Height = 480,
                                    Location = new Point()
                                    {
                                        X = j * (320 + 3) + 3,
                                        Y = i * (480 + 3) + 3,
                                    }
                                };

                                container.Controls.Add(vmContainer);

                                string currtime = DateTime.Now.ToString("yyyyMMddHHmmssff");
                                string currindex = vmIndex.ToString().PadLeft(4, '0');

                                ProcessUtils.Init(vmIndex, new Simulator()
                                {
                                    Cpu = 1,
                                    Memory = 1024,
                                    Width = 320,
                                    Height = 480,
                                    Dpi = 120,
                                    //Imei = ip + currindex
                                    //Imei = "auto",
                                    Androidid = ip + currindex

                                });

                                ProcessUtils.Run(vmIndex);
                                //加入到对应关系集合中
                                VmManager.Instance.AddVmModel(vmIndex, new Abstract.VmModel() { Imei = ip + currindex, Index = vmIndex, AndroidId = ip });
                                var latestVmProcess = Process.GetProcessesByName("dnplayer").OrderByDescending(p => p.StartTime).FirstOrDefault();

                                if (latestVmProcess == null)
                                {
                                    string error = SystemLanguageManager.Instance.ResourceManager.GetString("Error_No_Vm_Process", SystemLanguageManager.Instance.CultureInfo);
                                    LogUtils.Error($"{error} vmIndex:{vmIndex}");
                                    throw new Exception(error);
                                }

                                DateTime now1 = DateTime.Now;

                                while (latestVmProcess.MainWindowHandle == IntPtr.Zero)
                                {
                                    if (DateTime.Now.Subtract(now1).TotalSeconds > 7)
                                    {
                                        string error = SystemLanguageManager.Instance.ResourceManager.GetString("Error_Main_Window_Handle_Timeout", SystemLanguageManager.Instance.CultureInfo);
                                        LogUtils.Error(error);
                                        //MessageDialogManager.ShowDialogAsync("轮询模拟器的主窗口句柄超时！");
                                        break;
                                    }
                                }

                                int setParentResult = WinAPIs.SetParent(latestVmProcess.MainWindowHandle, vmContainer.Handle);

                                if (setParentResult != 65552)
                                {
                                    LogUtils.Error($"SetParent result:{setParentResult}, vmIndex:{vmIndex}");
                                }

                                int moveWindowResult = WinAPIs.MoveWindow(latestVmProcess.MainWindowHandle, 0, -35, 320, 515, true);

                                if (moveWindowResult != 1)
                                {
                                    LogUtils.Error($"MoveWindow result:{moveWindowResult}, vmIndex:{vmIndex}");
                                }

                                //Log.Logger.Information($"VmIndex：{vmIndex}, SetParent Result：{setParentResult}, MoveWindow Result：{moveWindowResult}");

                                //btn.Text = "关闭";
                            }

                            await Task.Delay(500);
                        }
                    }

                    //string installNewestAppLog = await AppUpgradeHelper.Instance.InstallNewestApp();
                    //LogUtils.Information(installNewestAppLog);
                }
                else
                {
                    btnCloseAll_Click(null, null);
                }
            }
            catch (Exception ex)
            {
                LogUtils.Error($"{ex}");

                MessageDialogManager.ShowDialogAsync(ex.Message);
            }
        }

        /// <summary>
        /// 获取本机IP
        /// </summary>
        /// <returns></returns>
        private string GetLocalIP()
        {
            IPAddress localIp = null;

            try
            {
                IPAddress[] ipArray;
                ipArray = Dns.GetHostAddresses(Dns.GetHostName());
                if (ipArray != null && ipArray.Length > 0)
                {
                    for (int i = 0; i < ipArray.Length; i++)
                    {
                        if (ipArray[i].AddressFamily == AddressFamily.InterNetwork)
                        {
                            localIp = ipArray[i];
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }

            if (localIp == null)
            {
                localIp = IPAddress.Parse("127.0.0.1");
            }
            string ip = localIp.ToString();
            string IpVals = "";
            if (ip != "")
            {
                string[] ips = ip.Split('.');
                for (int i = 0; i < ips.Length; i++)
                {
                    IpVals += ips[i].PadLeft(3, '0');
                }
            }
            return IpVals;
        }

        /// <summary>
        /// 启动facebook
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLaunchWhatsApp_Click(object sender, RoutedEventArgs e)
        {
            if (_runningGroupIndex == -1)
            {
                MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("Error_Please_Launch_A_Vm", SystemLanguageManager.Instance.CultureInfo));
                return;
            }

            string packagename = "com.facebook.katana";
            //List<Phonenum> listPhon = new PhonenumBLL().SelectPhoneNumber();

            for (int i = 0; i < VmManager.Instance.Column; i++)
            {
                int vmIndex = VmManager.Instance.VmIndexArray[_runningGroupIndex, i];
                if (vmIndex != -1)
                {
                    ProcessUtils.AdbOpenApps(vmIndex, packagename);
                }
            }

            //string installNewestAppLog = await AppUpgradeHelper.Instance.InstallNewestApp();
            //LogUtils.Information(installNewestAppLog);
        }
    
        private void btnCloseWhatsApp_Click(object sender, RoutedEventArgs e)
        {
            if (_runningGroupIndex == -1)
            {
                MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("Error_Please_Launch_A_Vm", SystemLanguageManager.Instance.CultureInfo));
                return;
            }

            string packagename = "com.facebook.katana";

            for (int i = 0; i < VmManager.Instance.Column; i++)
            {
                int vmIndex = VmManager.Instance.VmIndexArray[_runningGroupIndex, i];

                if (vmIndex != -1)
                {
                    ProcessUtils.AdbCloseApps(vmIndex, packagename);
                }
            }
        }

        /// <summary>
        /// 设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnInstallApk_Click(object sender, RoutedEventArgs e)
        {
            int GroupIndex = _runningGroupIndex;
            SimulatorView objSimulatorView = new SimulatorView(GroupIndex);
            objSimulatorView.ShowDialog();

            //string apkfile = $"{Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}/AppDatas/WhatsApp.apk";

            //if (_runningGroupIndex == -1)
            //{
            //    MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("Error_Please_Launch_A_Vm", SystemLanguageManager.Instance.CultureInfo));
            //    return;
            //}

            //for (int i = 0; i < VmManager.Instance.Column; i++)
            //{
            //    int vmIndex = VmManager.Instance.VmIndexArray[_runningGroupIndex, i];

            //    if (vmIndex != -1)
            //    {
            //        int id = ProcessUtils.AdbInstallApp(vmIndex, apkfile);
            //        Thread.Sleep(200);
            //    }
            //}

        }

        private void btnOperation_Click(object sender, RoutedEventArgs e)
        {
            FacebookOperationView facebookOperationView = new FacebookOperationView();

            try
            {
                facebookOperationView.ShowDialog();
            }
            catch (Exception ex)
            {
                facebookOperationView.Close();
            }

            //Simulator objSimlator = new Simulator()
            //{
            //    //Androidid= ProcessUtils.Init.,
            //    //Imei=
            //};
            //AppOptView appOptView = new AppOptView(AppOptViewModel);
            //appOptView.ShowDialog();
        }

        private void btnCloseAll_Click(object sender, RoutedEventArgs e)
        {
            RunningGroupIndex = -1;

            //DeviceConnectionManager.Instance.Reset();

            EventAggregatorManager.Instance.EventAggregator.GetEvent<VmClosedEvent>().Publish();

            //清除任务timer和标记
            TasksSchedule.timerstatus.Clear();
            if(!TasksSchedule.AllTimersKey.IsNull() && TasksSchedule.AllTimersKey.Count>0)
            {
                foreach(var item in TasksSchedule.AllTimersKey)
                {
                    item.Value.Dispose();
                }
            }
            TasksSchedule.AllTimersKey.Clear();

            ProcessUtils.QuitAll();
            container.Controls.Clear();
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ModemSocketClient.Stop();
            SocketServer.StopServer();
            ProcessUtils.QuitAll();
            Application.Current.Shutdown(0);
        }

        private void btnMonitor_Click(object sender, RoutedEventArgs e)
        {
            MonitorView.ShowMonitorView();
        }

        private void btnLaunchMessenger_Click(object sender, RoutedEventArgs e)
        {
            if (_runningGroupIndex == -1)
            {
                MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("Error_Please_Launch_A_Vm", SystemLanguageManager.Instance.CultureInfo));
                return;
            }

            string packagename = "com.facebook.mlite";
            //List<Phonenum> listPhon = new PhonenumBLL().SelectPhoneNumber();

            for (int i = 0; i < VmManager.Instance.Column; i++)
            {
                int vmIndex = VmManager.Instance.VmIndexArray[_runningGroupIndex, i];
                if (vmIndex != -1)
                {
                    ProcessUtils.AdbOpenApps(vmIndex, packagename);
                }
            }

            //string installNewestAppLog = await AppUpgradeHelper.Instance.InstallNewestApp();
            //LogUtils.Information(installNewestAppLog);

        }

        private void btnCloseMessenger_Click(object sender, RoutedEventArgs e)
        {
            if (_runningGroupIndex == -1)
            {
                MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("Error_Please_Launch_A_Vm", SystemLanguageManager.Instance.CultureInfo));
                return;
            }

            string packagename = "com.facebook.mlite";

            for (int i = 0; i < VmManager.Instance.Column; i++)
            {
                int vmIndex = VmManager.Instance.VmIndexArray[_runningGroupIndex, i];

                if (vmIndex != -1)
                {
                    ProcessUtils.AdbCloseApps(vmIndex, packagename);
                }
            }

        }
    }
}
