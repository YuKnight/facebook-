using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Path = System.IO.Path;
using Wx.Qunkong360.Wpf.Implementation;
using Xzy.EmbeddedApp.Utils;
using System.IO;
using Wx.Qunkong360.Wpf.Utils;
using System;
using System.Windows.Data;
using System.Threading.Tasks;
using Xzy.EmbeddedApp.Model;
using Cj.EmbeddedAPP.BLL;
using System.Collections.Generic;
using CmdProcessLib;

namespace Wx.Qunkong360.Wpf.Views
{
    /// <summary>
    /// SimulatorView.xaml 的交互逻辑
    /// </summary>
    public partial class SimulatorView
    {
        public int appState = 0;//-1卸载0安装
        private int _runningGroupIndex;
        public int RunningGroupIndex
        {
            get { return _runningGroupIndex; }
            private set
            {
                _runningGroupIndex = value;

                VmManager.Instance.RunningGroupIndex = value;

            }
        }
        public SimulatorView(int runningGroupIndex)
        {
            InitializeComponent();
            _runningGroupIndex += runningGroupIndex;

            Title = SystemLanguageManager.Instance.ResourceManager.GetString("Facebook_Operation", SystemLanguageManager.Instance.CultureInfo);

            GetFileName();
        }
        public void GetFileName()
        {
            string apkfile = $"{Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\\AppDatas\\";
            if (!Directory.Exists(apkfile))
            {
                Directory.CreateDirectory(apkfile);
            }

            var files = Directory.GetFiles(apkfile, "*.apk");
            int decId = 0;
            int Heightapp = 0;
            int Len2 = 0;
            double WidthappLen =10;
            foreach (var fullPath in files)
            {
                string filename = System.IO.Path.GetFileName(fullPath);//文件名
                                                                       //tring strWidth =System.IO.Path.GetFileName(fullPath.Substring(fullPath.LastIndexOf(@"\")));

                InstallButton btn = new InstallButton
                {
                    Name = "Button" + decId++,
                    Content = SystemLanguageManager.Instance.ResourceManager.GetString("Install", SystemLanguageManager.Instance.CultureInfo) + "  " + filename,//按钮标题
                    Height = 45,//按钮高度
                    //Width = (this.Width - 100) / 4,//按钮宽度
                    HorizontalAlignment = HorizontalAlignment.Left,
                    // Margin = new Thickness(WidthappLen + Len2++ * 10, Heightapp, 0, 0),//在界面上按钮的位置
                    Margin = new Thickness(WidthappLen),
                    VerticalAlignment = VerticalAlignment.Top,
                    Visibility = Visibility.Visible,
                    FileName = filename,
                    Padding = new Thickness(5, 2, 5, 2),
                 };
                //WidthappLen += btn.Width;//下一个按钮递增的距离
                if (decId % 3== 0)
                {//每行3个按钮
                    Heightapp += 100;//每行按钮之间的高度
                    WidthappLen = 10;//每行按钮第一个按钮离左边窗口的距离
                    Len2 = 0;//下一个按钮递增的距离
                }
                if (_runningGroupIndex != -1)
                {
                    if (appState != -1)
                    {
                        btn.Click += new RoutedEventHandler(btnInstall_Click);
                        appState = 0;
                    }
                    else
                    {
                        btn.Click += new RoutedEventHandler(btnUninstall_Click);
                        appState = -1;
                    }
                }
                applist.Children.Add(btn);
            }

        }
        private void btnLaunchWhatsApp_Click(object sender, RoutedEventArgs e)
        {
            if (_runningGroupIndex == -1)
            {
                MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("Error_Please_Launch_A_Vm", SystemLanguageManager.Instance.CultureInfo));
                return;
            }

            string packagename = "com.whatsapp";

            for (int i = 0; i < VmManager.Instance.Column; i++)
            {
                int vmIndex = VmManager.Instance.VmIndexArray[_runningGroupIndex, i];

                if (vmIndex != -1)
                {
                    ProcessUtils.AdbOpenApps(vmIndex, packagename);
                }
            }
        }

        private void btnInstall_Click(object sender, RoutedEventArgs e)
        {
            pb.Visibility = Visibility.Visible;
                Thread.Sleep(100);
                string apkfile = $"{Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\\AppDatas\\" + FilterSpecial(DelChinese(((Button)sender).Content.ToString()).Trim()).Trim();

                if (_runningGroupIndex == -1)
                {
                    MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("Error_Please_Launch_A_Vm", SystemLanguageManager.Instance.CultureInfo));
                    return;
                }
          
                for (int i = 0; i < VmManager.Instance.Column; i++)
                {
                    int vmIndex = VmManager.Instance.VmIndexArray[_runningGroupIndex, i];

                    if (vmIndex != -1)
                {
                    Task.Run(() =>
                    {
                        //ProcessUtils.AdbCloseService(vmIndex);//关闭服务
                        //int id = ProcessUtils.AdbSilentInstallation(vmIndex, apkfile);
                        int id = ProcessUtils.AdbInstallApp(vmIndex, apkfile);
                        Thread.Sleep(200);
                    });
                }
                }
       
            ((Button)e.Source).Click -= btnInstall_Click;
                ((Button)e.Source).Click += btnUninstall_Click;
                ((Button)e.Source).Content = SystemLanguageManager.Instance.ResourceManager.GetString("Uninstall", SystemLanguageManager.Instance.CultureInfo) + " " + ((InstallButton)e.Source).FileName;
            btnProgress_Click(sender, e);
        }

        #region Filtering Chinese and English strings
        /// <summary>
        /// Delete the Chinese in the string
        /// </summary>
        public static string DelChinese(string str)
        {
            string retValue = str;
            if (System.Text.RegularExpressions.Regex.IsMatch(str, @"[\u4e00-\u9fa5]"))
            {
                retValue = string.Empty;
                var strsStrings = str.ToCharArray();
                for (int index = 0; index < strsStrings.Length; index++)
                {
                    if (strsStrings[index] >= 0x4e00 && strsStrings[index] <= 0x9fa5)
                    {
                        continue;
                    }
                    retValue += strsStrings[index];
                }
            }
            return retValue;
        }

        /// <summary>
        ///Filter special characters
        /// If the string is empty, return directly.
        /// </summary>
        /// <param name="str"> String that needs to be filtered</param>
        /// <returns>Filtered string</returns>
        public static string FilterSpecial(string str)
        {
            if (str == "")
            {
                return str;
            }
            else
            {
                str = str.Replace("Install", "");
                str = str.Replace("Uninstall", "");
                return str;
            }
        }

        #endregion
        private void btnUninstall_Click(object sender, RoutedEventArgs e)
        {
            string apkfile = $"{Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\\AppDatas\\" + FilterSpecial(DelChinese(((Button)sender).Content.ToString()).Trim()).Trim();

            if (_runningGroupIndex == -1)
            {
                MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("Error_Please_Launch_A_Vm", SystemLanguageManager.Instance.CultureInfo));
                return;
            }

            for (int i = 0; i < VmManager.Instance.Column; i++)
            {
                int vmIndex = VmManager.Instance.VmIndexArray[_runningGroupIndex, i];

                if (vmIndex != -1)
                {
                    int id = ProcessUtils.AdbUnInstallApp(vmIndex, apkfile);
                    Thread.Sleep(200);
                }
            }

            ((Button)e.Source).Click += btnInstall_Click;
            ((Button)e.Source).Click -= btnUninstall_Click;
            ((Button)e.Source).Content = SystemLanguageManager.Instance.ResourceManager.GetString("Install", SystemLanguageManager.Instance.CultureInfo) + " " + ((InstallButton)e.Source).FileName;

        }

        #region ProgressBar
        private void btnProgress_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i <= 1000; i++)
            {
                double value = i * 100 / 1000;
                pb.Dispatcher.Invoke(new Action<System.Windows.DependencyProperty, object>(pb.SetValue), System.Windows.Threading.DispatcherPriority.Background, ProgressBar.ValueProperty, value);
            }
            System.Threading.Thread.Sleep(4000);
            pb.Visibility = Visibility.Collapsed;
        }
        #endregion

        //private async void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        //{
        //    //string deviceIP = "127.0.0.1:5555";
        //    //string cmdPs = string.Format("pm list packages");
        //    //ProcessHelper.WaitTime = 50;
        //    //string flag = @"root@x86:/ #";
        //    //ProcessHelper.RunResult runResult = ProcessHelper.RunAsContinueMode(ProcessUtils.LDPath + @"\adb", string.Format("-s {0} shell", deviceIP), new[] { cmdPs,"exit"});
        //    //string Ps = await ProcessHelper.FilterString2(runResult.MoreOutputString["exit"], cmdPs, flag);

        //    //if (Ps == "package:com.facebook.katana")
        //    //{
        //    //    appState = 0;
        //    //    GetFileName();
        //    //}
        //    //else
        //    //{
        //    //    GetFileName();
        //    //}
        //}
    }
}
