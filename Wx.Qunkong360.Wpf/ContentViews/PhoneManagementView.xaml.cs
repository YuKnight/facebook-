using System;
using System.Collections.Generic;
using System.Globalization;
using System.Resources;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WpfTreeView;
using Wx.Qunkong360.Wpf.Implementation;
using Wx.Qunkong360.Wpf.Utils;
using Wx.Qunkong360.Wpf.ViewModels;
using Xzy.EmbeddedApp.Model;
using Xzy.EmbeddedApp.Utils;
using CmdProcessLib;
using System.Reflection;
using System.Threading;
using System.IO;
using System.Linq;
using Cj.EmbeddedAPP.BLL;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Cj.EmbeddedApp.BLL;

namespace Wx.Qunkong360.Wpf.ContentViews
{
    /// <summary>
    /// PhoneManagementView.xaml 的交互逻辑
    /// </summary>
    public partial class PhoneManagementView : UserControl
    {
        ResourceManager resourceManager;
        CultureInfo cultureInfo;
        List<admins> gridLListSimulators = new List<admins>();
        private int _runningGroupIndex = 0;

        public int RunningGroupIndex
        {
            get { return _runningGroupIndex; }
            private set
            {
                _runningGroupIndex = 0;

                VmManager.Instance.RunningGroupIndex = 0;
            }
        }

        public PhoneManagementView()
        {
            InitializeComponent();

            this.Loaded += PhoneManagementView_Loaded;
        }

        private void PhoneManagementView_Loaded(object sender, RoutedEventArgs e)
        {
            resourceManager = new ResourceManager("Wx.Qunkong360.Wpf.Languages.Res", typeof(PhoneManagementView).Assembly);
            if (ConfigVals.Lang == 1)
            {
                cultureInfo = CultureInfo.CreateSpecificCulture("zh-cn");
                Res.Culture.Button = new Res_Zh_Button();
                ResA.CultureA.Button = new ResA_Zh_Button();
                ResD.CultureD.Button = new ResD_Zh_Button();
            }
            else if (ConfigVals.Lang == 2)
            {
                cultureInfo = CultureInfo.CreateSpecificCulture("en-us");
                Res.Culture.Button = new Res_En_Button();
                ResA.CultureA.Button = new ResA_En_Button();
                ResD.CultureD.Button = new ResD_En_Button();
            }
            lblPhoneManagement.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Phone_Management", SystemLanguageManager.Instance.CultureInfo);
            btnReconstruction.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Reconstruction", SystemLanguageManager.Instance.CultureInfo);

            lblPhoneManagement_MouseLeftButtonDown(null, null);
            dgPhoneManagement_MouseLeftButtonUp(null, null);
        }

        private void lblPhoneManagement_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            lblPhoneManagement.FontWeight = FontWeights.Bold;
            lblLoginManagement.FontWeight = FontWeights.Regular;
            //加载树
            InitRunningVmsTreeView(treeviewReconstructionSimulator);
        }

        /// <summary>
        /// 初始化树结构
        /// </summary>
        /// <param name="wpfTreeView"></param>
        private void InitRunningVmsTreeView(WpfTreeView.WpfTreeView wpfTreeView)
        {

            int runningGroupIndex = VmManager.Instance.RunningGroupIndex;

            if (runningGroupIndex == -1)
            {
                return;
            }

            int groupEndIndex = VmManager.Instance.VmIndexArray[runningGroupIndex, VmManager.Instance.Column - 1];
            int endNumber = groupEndIndex == -1 ? VmManager.Instance.MaxVmNumber : groupEndIndex + 1;


            //string firstLevelNodeText = $"第{runningGroupIndex + 1}组 {VmManager.Instance.VmIndexArray[runningGroupIndex, 0] + 1}-{endNumber}";
            string firstLevelNodeText = string.Format(SystemLanguageManager.Instance.ResourceManager.GetString("Group", SystemLanguageManager.Instance.CultureInfo), runningGroupIndex + 1, VmManager.Instance.VmIndexArray[runningGroupIndex, 0] + 1, endNumber);

            List<WpfTreeViewItem> wpfTreeViewItems = new List<WpfTreeViewItem>();

            WpfTreeViewItem topLevelNode = new WpfTreeViewItem()
            {
                Caption = firstLevelNodeText,
                Id = -1,
                IsExpanded = true,
            };

            wpfTreeViewItems.Add(topLevelNode);

            for (int i = 0; i < VmManager.Instance.Column; i++)
            {
                if (VmManager.Instance.VmIndexArray[runningGroupIndex, i] != -1)
                {
                    WpfTreeViewItem subNode = new WpfTreeViewItem()
                    {
                        Id = VmManager.Instance.VmIndexArray[runningGroupIndex, i] + 1,
                        Caption = string.Format(SystemLanguageManager.Instance.ResourceManager.GetString("Phone_Num", SystemLanguageManager.Instance.CultureInfo), VmManager.Instance.VmIndexArray[runningGroupIndex, i] + 1),
                        ParentId = -1,
                    };

                    wpfTreeViewItems.Add(subNode);
                }
            }

            wpfTreeView.SetItemsSourceData(wpfTreeViewItems, item => item.Caption, item => item.IsExpanded, item => item.Id, item => item.ParentId);
        }

        /// <summary>
        ///加载模拟器信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private  async void dgPhoneManagement_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Id.Header = resourceManager.GetString("ID", cultureInfo);
            IMEI.Header = resourceManager.GetString("IMEI", cultureInfo);
            phoneBrand.Header = resourceManager.GetString("Phone_Brand", cultureInfo);
            phoneType.Header = resourceManager.GetString("Phone_Type", cultureInfo);
            lblLoginManagement.Content = resourceManager.GetString("Login_Management", cultureInfo);
            AppOptViewModel _appOptViewModel = new AppOptViewModel();

            int index = 0;
            if (DeviceConnectionManager.Instance.Devices != null)
            {
                foreach (string deviceIP in DeviceConnectionManager.Instance.Devices)
                {
                    string cmdImei = string.Format("getprop phone.imei");
                    string cmdImsi = string.Format("getprop phone.imsi");
                    string cmdAndroidId = string.Format("getprop phone.androidid");
                    string cmdModel = string.Format("getprop ro.product.model");
                    string cmdBrand = string.Format("getprop ro.product.brand");
                    //每条命令执行后的等待时间，时间过短可能获取不到值，需要调整一个合适的阀值，默认50即可，如果获取不到可适当调大
                    ProcessHelper.WaitTime = 50;
                    string flag = @"root@x86:/ #";

                    ProcessHelper.RunResult runResult = ProcessHelper.RunAsContinueMode(ProcessUtils.LDPath + @"\adb", string.Format("-s {0} shell", deviceIP), new[] { cmdImei, cmdImsi, cmdAndroidId, cmdModel, cmdBrand,"exit" });

                    string imei = await ProcessHelper.FilterString(runResult.MoreOutputString[cmdImei], cmdImei, flag);
                    string imsi = await ProcessHelper.FilterString(runResult.MoreOutputString[cmdImsi], cmdImsi, flag);
                    string androidId = await ProcessHelper.FilterString(runResult.MoreOutputString[cmdAndroidId], cmdAndroidId, flag);
                    string model = await  ProcessHelper.FilterString(runResult.MoreOutputString[cmdModel], cmdModel, flag);
                    string brand = await ProcessHelper.FilterString(runResult.MoreOutputString[cmdBrand], cmdBrand, flag);
               
                _appOptViewModel.simulator.Add(new Simulator()
                    {
                        Id = index + 1,
                        Imei = imei,
                        Imsi = imsi,
                        PhoneBrand = brand,
                        PhoneType = model,
                        Androidid = androidId
                    });
                    index++;
                }
            }
            dgPhoneManagement.ItemsSource = _appOptViewModel.simulator;
        }

        /// <summary>
        /// 重建模拟器
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnReconstruction_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                //TODO 因为可能多选，需要先获取已选的模拟器，遍历
                var devicesList = from item in treeviewReconstructionSimulator.ItemsSourceData.FirstOrDefault().Children
                              where item.IsChecked

                              select new
                              {
                                  id = (int)item.Id - 1,
                                  DevicesIp = "127.0.0.1:55" + (55 + ((int)item.Id - 1) * 2)
                              };
                
                foreach (var p in devicesList)
                {                 
                    string deviceIP = p.DevicesIp;
                    string cmdWipeData = string.Format("wipe data");
                    string cmdReboot = string.Format("reboot");
                    //每条命令执行后的等待时间，时间过短可能获取不到值，需要调整一个合适的阀值，默认50即可，如果获取不到可适当调大
                    ProcessHelper.WaitTime = 50;
                    string flag = @"root@x86:/ #";

                    // 1、执行擦除命令
                    ProcessHelper.RunResult runResult = ProcessHelper.RunAsContinueMode(ProcessUtils.LDPath + @"\adb", string.Format("-s {0} shell", deviceIP), new[] { cmdWipeData, "exit" });

                    #region adb install方式安装apk
                    //命令原型：adb -s xxxxxx:xxxx install ./facebook.apk
                    //string apkPath = "xxx.apk";
                    string apkPath = $"{ Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\\AppDatas\\";

                    var files = Directory.GetFiles(apkPath, "*.apk");
                    //TODO 如果安装多个apk，循环处理，调用下面的ProcessHelper.RunAsContinueMode
                    // 2、安装apk
                    Task.Run(() =>
                    {
                        foreach (var file in files)
                        {
                            runResult = ProcessHelper.RunAsContinueMode(ProcessUtils.LDPath + @"\adb",
                            string.Format("-s {0} install {1}", deviceIP, file, new[] { "exit" }));
                            Thread.Sleep(4500);
                        }
                        #endregion
                        // 3、使用标准命令（standard）执行模式重启
                        ProcessHelper.RunAsStandardModel(ProcessUtils.LDPath + @"\adb", cmdReboot);
                        Thread.Sleep(2000);
                    });
                }
            });
        }
        
        #region 登录管理
        private void lblLoginManagement_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            lblLoginManagement.Content = resourceManager.GetString("Login_Management", cultureInfo);
            UserId.Header = resourceManager.GetString("User_Id", cultureInfo);
            Login.Header = resourceManager.GetString("Login", cultureInfo);
            Password.Header = resourceManager.GetString("Password", cultureInfo);
            Explain.Header = resourceManager.GetString("Explain", cultureInfo);
         

            if (!(sender is null))
            {
                lblPhoneManagement.FontWeight = FontWeights.Regular;
                lblLoginManagement.FontWeight = FontWeights.Bold;
            }
            Localize();
        }
        
        private void Localize()
        {
           AppOptViewModel _appOptViewModel = new AppOptViewModel();
           AdminsBLL objAdminBLL = new AdminsBLL();
           List<admins> objAdmin = objAdminBLL.GetAdmins();
        
            this.DataContext = _appOptViewModel;
            for (int i = 0; i < ConfigVals.MaxNums; i++)
            {
                if (objAdmin.Count > i)
                {
                    _appOptViewModel.admins.Add(new admins()
                    {
                        id = i + 1,
                        login_user = objAdmin[i].login_user,
                        login_pwd = objAdmin[i].login_pwd,
                        explain_detail = objAdmin[i].explain_detail
                    });
                }
                else
                {
                    _appOptViewModel.admins.Add(new admins()
                    {
                        id = i + 1
                    });
                }     
            }
            dgAccountManagement.ItemsSource = _appOptViewModel.admins;
        }

        /// <summary>
        /// 清缓存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClearCaching_Click(object sender, RoutedEventArgs e)
        {
            ClearMemory();
        }

        #region 强制清除Facebook缓存
        public void ClearMemory()
        {
            int id = dgAccountManagement.SelectedIndex;
            int number = 3;
            Task.Run(() =>
            {
                AdminsBLL objAdminBLL = new AdminsBLL();
                SystemConfigBLL bLL = new SystemConfigBLL();
                SystemConfigBLL objConfigs = new SystemConfigBLL();
                objAdminBLL.DelAdmins();//删除账号
                objConfigs.DelSystemConfig(id);//删除配置信息
                foreach (string deviceIP in DeviceConnectionManager.Instance.Devices)
                {
                    string connectDeviceCmd = string.Format("{0}/adb -s {1} shell", ProcessUtils.LDPath, deviceIP);
                    string cmdClear = string.Format("pm clear com.facebook.katana");
                    string cmdExit = string.Format("exit");
                    DeviceOperation deviceOperation = new DeviceOperation();
                    deviceOperation
                   .SetCmd(connectDeviceCmd)
                   .SetCmd(cmdClear)
                   .SetCmd(cmdExit)
                   .Run2(id,out number);
                }
            });
            MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("Eliminate_Success", SystemLanguageManager.Instance.CultureInfo));
        }
        #endregion

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        Thread th;
        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            int id = dgAccountManagement.SelectedIndex;
            int number = -1;
            int num = 555 + 2 * id;
            string port2 = "5" + num;
                
            SystemConfigBLL bLL = new SystemConfigBLL();
            Task.Run(() =>
            {
                List<string> devicesList = ProcessUtils.AdbDevices();
                foreach (string deviceIP in devicesList)
                {
                    if (deviceIP.IndexOf(port2) > 0)
                    {
                        string port = deviceIP.Remove(0, deviceIP.Length - 4);
                        string connectDeviceCmd = string.Format("{0}/adb -s {1} shell", ProcessUtils.LDPath, deviceIP);
                        string cmdPs = string.Format("ps");
                        string cmdExit = string.Format("exit");
                        DeviceOperation deviceOperation = new DeviceOperation();
                        deviceOperation
                       .SetCmd(connectDeviceCmd)
                       .SetCmd(cmdPs)
                       .SetCmd(cmdExit)
                       .Run2(id, out number);

                        switch (number)
                        {
                            case 0:
                                List<SystemConfig> model = bLL.GetSystemConfigList(id);
                                if (model.Count>0)
                                {
                                    model[0].state = 0;
                                    bLL.UpdateSystemConfig(model[0]);
                                }
                                else
                                {
                                    SystemConfig sc = new SystemConfig();
                                    sc.uid = id;
                                    sc.state = 0;
                                    bLL.AddSystemConfig(sc);
                                }
                                break;
                            case 1:
                                model = bLL.GetSystemConfigList(id);
                                if (model.Count > 0)
                                {
                                    model[0].state = 1;
                                    bLL.UpdateSystemConfig(model[0]);
                                }
                                else
                                {
                                    SystemConfig sc = new SystemConfig();
                                    sc.uid = id;
                                    sc.state = 0;
                                    bLL.AddSystemConfig(sc);
                                }
                                break;
                        }
                        //1表示已启动了0表示为没启动com.facebook.katana;判断模拟器是否启动facebook应用,有返回没启动

                        if (number == 0)
                        {
                            #region 启动Facebook
                            if (_runningGroupIndex == -1)
                            {
                                MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("Error_Please_Launch_A_Vm", SystemLanguageManager.Instance.CultureInfo));
                                return;
                            }
                            string packagename = "com.facebook.katana";
                            List<Phonenum> listPhon = new PhonenumBLL().SelectPhoneNumber();

                            for (int i = 0; i < VmManager.Instance.Column; i++)
                            {
                                int vmIndex = VmManager.Instance.VmIndexArray[_runningGroupIndex, i];
                                if (vmIndex != -1)
                                {
                                    ProcessUtils.AdbOpenApps(vmIndex, packagename);
                                }
                            }
                            break;
                            #endregion
                        }

                    }

                    LoginInfo li = new LoginInfo();
                    li.index = id;
                    li.port = port2;
                    if (th == null || !th.IsAlive)
                    {
                        th = new Thread(new  ParameterizedThreadStart(AutomaticLogin));
                        th.IsBackground = true;
                        th.Start(li);
                    }

                }
            });
    }

        private struct LoginInfo
        {
            public int index;
            public string port;
        }
        /// <summary>
        /// 传到指定模拟器编号启动登录
        /// </summary>
        private void AutomaticLogin(object login)
        {
            LoginInfo li = (LoginInfo)login;
            Task.Run(() =>
            {
                //获取所有设备连接信息
                List<string> devicesList = ProcessUtils.AdbDevices();
                gridLListSimulators.Clear();
                string txt = "";
                if (this.dgAccountManagement.Items[li.index] != null)
                {
                    if (((Xzy.EmbeddedApp.Model.admins)this.dgAccountManagement.Items[li.index]).login_pwd != null && ((Xzy.EmbeddedApp.Model.admins)this.dgAccountManagement.Items[li.index]).login_user != null)
                    {
                        if (((Xzy.EmbeddedApp.Model.admins)this.dgAccountManagement.Items[li.index]).login_pwd.Trim() != "" && ((Xzy.EmbeddedApp.Model.admins)this.dgAccountManagement.Items[li.index]).login_user.Trim() != "")
                        {
                            gridLListSimulators.Add(this.dgAccountManagement.Items[li.index] as Xzy.EmbeddedApp.Model.admins);
                        }
                        txt += ((Xzy.EmbeddedApp.Model.admins)this.dgAccountManagement.Items[li.index]).login_user.Trim() + "\r\n" +
                                ((Xzy.EmbeddedApp.Model.admins)this.dgAccountManagement.Items[li.index]).login_pwd + "\r\n";
                    }
                }

                if (((Xzy.EmbeddedApp.Model.admins)this.dgAccountManagement.Items[0]).login_user != null)
                {
                    string connectDeviceCmd = string.Format("{0}/adb -s {1} shell", ProcessUtils.LDPath, devicesList[li.index].ToString());
                    if (devicesList[li.index].IndexOf(li.port) > 0)
                    {
                        string cmdInputText = string.Format("input text {0}", gridLListSimulators[0].login_user);
                        string cmdInputTap = string.Format("input tap {0} {1}", 100, 200);
                        string cmdTab = "input keyevent 61";
                        string cmdPwd = string.Format("input text {0}", gridLListSimulators[0].login_pwd);
                        string cmdLogin = string.Format("input tap {0} {1}", 100, 300);
                        string cmdConfirm = string.Format("input tap {0} {1}", 245, 465);

                        StreamWriter sw = new StreamWriter(@"D:\" + li.index.ToString() + ".txt");
                        sw.WriteLine(cmdInputTap);
                        sw.WriteLine(cmdInputText);
                        sw.WriteLine(cmdInputTap);
                        sw.WriteLine(cmdTab);
                        sw.WriteLine(cmdPwd);
                        sw.WriteLine(cmdLogin);
                        sw.WriteLine(cmdConfirm);
                        sw.Close();

                        DeviceOperation deviceOperation = new DeviceOperation();
                        deviceOperation.RunText(connectDeviceCmd, @"D:\" + li.index.ToString() + ".txt");
                    }
                }
            });
        }
        #endregion

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            AdminsBLL objAdminBLL = new AdminsBLL();
            for (int i = 0; i < this.dgAccountManagement.Items.Count; i++)
            {
                    var a = this.dgAccountManagement.Items[i] as admins;
                    if (!a.login_pwd.IsNull() && !a.login_user.IsNull())
                    {
                        admins objAdmins = new admins()
                        {
                            id = a.id,
                            login_user = a.login_user,
                            login_pwd = a.login_pwd,
                            explain_detail = a.explain_detail,
                            addtime = DateTime.Now
                        };
                        objAdminBLL.AddAdmins(objAdmins);
                    }
            }               
            MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("Save_Success", SystemLanguageManager.Instance.CultureInfo));
        }
    }
    #region 保存中英文转换
    public static class ResA
    {
        public static ResCultureA CultureA = new ResCultureA();
    }

    public class ResCultureA : INotifyPropertyChanged
    {
        private IResA_Button _Button;
        public IResA_Button Button
        {
            get
            {
                return _Button;
            }
            set
            {
                _Button = value;
                OnPropertyChanged(nameof(Button));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
    }

    public interface IResA_Button
    {
        string btnSave { get; }
    }

    public class ResA_En_Button : IResA_Button
    {
        public string btnSave
        {
            get
            {
                return "Save";
            }
        }
    }

    public class ResA_Zh_Button : IResA_Button
    {
        public string btnSave
        {
            get
            {
                return "保存";
            }
        }
    }
    #endregion

    #region 清缓存中英文转换
    public static class Res
    {
        public static ResCulture Culture = new ResCulture();
    }

    public class ResCulture : INotifyPropertyChanged
    {
        private IRes_Button _Button;
        public IRes_Button Button
        {
            get
            {
                return _Button;
            }
            set
            {
                _Button = value;
                OnPropertyChanged(nameof(Button));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
    }

    public interface IRes_Button
    {
        string btnClearCaching { get; }
    }

    public class Res_En_Button : IRes_Button
    {
        public string btnClearCaching
        {
            get
            {
                return "ClearCaching";
            }
        }
    }

    public class Res_Zh_Button : IRes_Button
    {
        public string btnClearCaching
        {
            get
            {
                return "清缓存";
            }
        }
    }
    #endregion

    #region 登录中英文转换
    public static class ResD
    {
        public static ResCultureD CultureD = new ResCultureD();
    }

    public class ResCultureD : INotifyPropertyChanged
    {
        private IResD_Button _Button;
        public IResD_Button Button
        {
            get
            {
                return _Button;
            }
            set
            {
                _Button = value;
                OnPropertyChanged(nameof(Button));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
    }

    public interface IResD_Button
    {
        string btnLogin { get; }
    }

    public class ResD_En_Button : IResD_Button
    {
        public string btnLogin
        {
            get
            {
                return "Login";
            }
        }
    }

    public class ResD_Zh_Button : IResD_Button
    {
        public string btnLogin
        {
            get
            {
                return "登录";
            }
        }
    }
    #endregion
}


