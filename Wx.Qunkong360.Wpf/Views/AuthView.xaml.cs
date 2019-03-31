using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wx.Qunkong360.Wpf.ContentViews;
using Wx.Qunkong360.Wpf.Utils;
using Xzy.EmbeddedApp.Model;
using Xzy.EmbeddedApp.Utils;
using Xzy.EmbeddedApp.WinForm.Utils;

namespace Wx.Qunkong360.Wpf
{
    /// <summary>
    /// AuthView.xaml 的交互逻辑
    /// </summary>
    public partial class AuthView
    {
        Timer _timer;
        int _triedTimes = 0;
        const int MaxTriedTimes = 5;

        public AuthView()
        {
            InitializeComponent();

            Title = SystemLanguageManager.Instance.ResourceManager.GetString("Authentication", SystemLanguageManager.Instance.CultureInfo);
            lblAuthTips.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Authenticating", SystemLanguageManager.Instance.CultureInfo);
            btnRetry.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Retry", SystemLanguageManager.Instance.CultureInfo);

            _timer = new Timer(Timer_Elapsed, null, Timeout.Infinite, Timeout.Infinite);
            _timer.Change(1000, Timeout.Infinite);
        }

        private async void Timer_Elapsed(object state)
        {
            Console.WriteLine("AuthLogin begins");

            await AuthLogin();

            Console.WriteLine("AuthLogin ends");

        }

        /// <summary>
        /// 启动后验证用户
        /// </summary>
        /// <returns></returns>
        public int AuthUser()
        {
            int flag = 100;
            //检查mysql端口
            bool ispoint = CheckPortisAvailable();
            if (!ispoint)
            {
                return -10;
            }
            //远程鉴权
            bool auth = AuthRemoteCode();
            if (!auth)
            {
                flag = -11;
            }
            return flag;
        }


        /// <summary>
        /// 集中验证
        /// </summary>
        /// <returns></returns>
        public async Task<bool> AuthLogin()
        {
            await CheckConfig();

            bool flag = false;

            int authflag = AuthUser();
            authflag = 100;
            if (authflag == 100)
            {
                await App.Current.Dispatcher.BeginInvoke(new Action(() =>
                 {
                     StartUpView startUpView = new StartUpView();
                     startUpView.Show();

                     Close();
                 }));
            }
            else
            {
                _triedTimes++;

                if (_triedTimes > MaxTriedTimes)
                {
                    _timer.Change(Timeout.Infinite, Timeout.Infinite);
                    await App.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        btnRetry.Visibility = System.Windows.Visibility.Visible;
                    }));
                }
                else
                {
                    _timer.Change(2000, Timeout.Infinite);
                }


                if (authflag == -10)
                {
                    await App.Current.Dispatcher.BeginInvoke(new Action(() =>
                     {
                         lblAuthTips.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Start_Database_Failure", SystemLanguageManager.Instance.CultureInfo);
                     }));

                }
                if (authflag == -11)
                {
                    await App.Current.Dispatcher.BeginInvoke(new Action(() =>
                     {
                         lblAuthTips.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Authentication_Failure", SystemLanguageManager.Instance.CultureInfo);
                     }));

                }
            }

            return flag;
        }



        /// <summary>
        /// 远程鉴权
        /// </summary>
        /// <returns></returns>
        public bool AuthRemoteCode()
        {
            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                lblAuthTips.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Authenticating", SystemLanguageManager.Instance.CultureInfo);
            }));

            bool authflag = false;
            var values = new List<KeyValuePair<string, string>>();

            string url = ConfigurationManager.AppSettings["AuthUrl"];//
            string ukey = ConfigManager.Instance.Config.UKey;//
            string macaddrss = SystemInfoUtils.GetMacAddress();
            string type = "facebook";

            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            long t = Convert.ToInt64(ts.TotalSeconds);

            string key = ConfigVals.AccessKey.Substring(8, 20);

            if (macaddrss == "" || key == "")
            {
                return authflag;
            }
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] result = Encoding.Default.GetBytes(ukey + macaddrss + type + t.ToString() + key);
            byte[] bytekey = md5.ComputeHash(result);
            string token = BitConverter.ToString(bytekey).Replace("-", "");

            /*values.Add(new KeyValuePair<string, string>("ukey", ukey));
            values.Add(new KeyValuePair<string, string>("macadd", macaddrss));
            values.Add(new KeyValuePair<string, string>("time", t.ToString()));
            values.Add(new KeyValuePair<string, string>("token", token));*/

            HttpClientHelp httpClient = new HttpClientHelp();
            var obj = new JObject() { { "ukey", ukey }, { "macadd", macaddrss }, { "type", type }, { "time", t.ToString() }, { "token", token.ToLower() } };

            authflag = httpClient.PostFunction(url, obj.ToString(Formatting.None), macaddrss, key,ukey);

            return authflag;
        }

        /// <summary>
        /// 检测端口
        /// </summary>
        /// 
        /// <returns></returns>
        public bool CheckPortisAvailable()
        {
            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                lblAuthTips.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Check_Database_Service", SystemLanguageManager.Instance.CultureInfo);
            }));

            bool connFlag = false;
            connFlag = MySqlHelpers.ConnectionTest(MySqlHelpers.ConnectionString);

            return connFlag;
        }



        private async void btnRetry_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            _triedTimes = 0;
            await App.Current.Dispatcher.BeginInvoke(new Action(() =>
             {
                 btnRetry.Visibility = System.Windows.Visibility.Collapsed;
                 lblAuthTips.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Please_Wait", SystemLanguageManager.Instance.CultureInfo);
             }));

            //await AuthLogin();
            _timer.Change(1000, Timeout.Infinite);
        }

        private async Task CheckConfig()
        {
            string config = $"{Directory.GetParent(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location))}\\config.txt";

            if (!File.Exists(config))
            {
                ConfigModel configModel = new ConfigModel()
                {
                    LDPath = "",
                    NoxPath = "",
                    UKey = "",
                };

                string json = JsonConvert.SerializeObject(configModel, Formatting.Indented);

                File.WriteAllText(config, json, Encoding.UTF8);

               await App.Current.Dispatcher.BeginInvoke(new Action(async () =>
               {
                   var dialog = new MessageDialogView()
                   {
                       Message = { Text = SystemLanguageManager.Instance.ResourceManager.GetString("Vm_Path_Not_Set", SystemLanguageManager.Instance.CultureInfo) }
                   };

                   await DialogHost.Show(dialog, "rootDialog");

                    //MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("Vm_Path_Not_Set", SystemLanguageManager.Instance.CultureInfo));
                    Close();
               }));

            }
            else
            {
                string json = File.ReadAllText(config);
                ConfigManager.Instance.Config = JsonConvert.DeserializeObject<ConfigModel>(json);
            }

        }

        private void MetroWindow_Closed(object sender, EventArgs e)
        {
            _timer.Dispose();
        }
    }
}
