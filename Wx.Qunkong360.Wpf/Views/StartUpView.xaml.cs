using Cj.EmbeddedApp.BLL;
using System;
using System.Globalization;
using Wx.Qunkong360.Wpf.Implementation;
using Xzy.EmbeddedApp.Model;
using Xzy.EmbeddedApp.Utils;
using System.Linq;
using System.Windows.Controls;
using Wx.Qunkong360.Wpf.Utils;

namespace Wx.Qunkong360.Wpf
{
    /// <summary>
    /// StartUpView.xaml 的交互逻辑
    /// </summary>
    public partial class StartUpView
    {
        public StartUpView()
        {
            InitializeComponent();

            bandInitData();
        }

        /// <summary>
        /// 初始化配置数据
        /// </summary>
        private void bandInitData()
        {
            ConfigsBLL bll = new ConfigsBLL();
            Configs configs = bll.GetAllData();
            if (configs != null)
            {
                var groupCapacityItem = cbGroupCapacity.Items.Cast<ComboBoxItem>().FirstOrDefault(item => item.Content.ToString() == configs.Groupnums.ToString());

                int groupCapacityIndex = cbGroupCapacity.Items.IndexOf(groupCapacityItem);

                cbGroupCapacity.SelectedIndex = groupCapacityIndex;



                var rowCapacityItem = cbRowCapacity.Items.Cast<ComboBoxItem>().FirstOrDefault(item => item.Content.ToString() == configs.Rownums.ToString());

                int rowCapacityIndex = cbRowCapacity.Items.IndexOf(rowCapacityItem);

                cbRowCapacity.SelectedIndex = rowCapacityIndex;


                if (configs.Lang == 2)
                {
                    rbEnglish.IsChecked = true;
                    ConfigVals.Lang = 2;
                }
                else
                {
                    rbChinese.IsChecked = true;
                    ConfigVals.Lang = 1;
                }
            }
            else
            {
                cbGroupCapacity.SelectedIndex = 0;
                cbRowCapacity.SelectedIndex = 0;
                rbChinese.IsChecked = true;
                ConfigVals.Lang = 1;
            }

            //初始化数据
            ConfigVals.GroupNums = Int32.Parse(cbGroupCapacity.SelectionBoxItem.ToString());
            ConfigVals.RowNums = Int32.Parse(cbRowCapacity.SelectionBoxItem.ToString());
            VmManager.Instance.Initialize(ConfigVals.MaxNums, ConfigVals.GroupNums);
        }

        private void SwitchLanguage()
        {
            if (rbChinese.IsChecked.HasValue && rbChinese.IsChecked.Value)
            {
                ConfigVals.Lang = 1;
                SystemLanguageManager.Instance.CultureInfo = CultureInfo.CreateSpecificCulture("zh-cn");
            }
            else if (rbEnglish.IsChecked.HasValue && rbEnglish.IsChecked.Value)
            {
                ConfigVals.Lang = 2;
                SystemLanguageManager.Instance.CultureInfo = CultureInfo.CreateSpecificCulture("en-us");
            }



            tbProductName.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Product_Name", SystemLanguageManager.Instance.CultureInfo);
            tbLanguage.Text = SystemLanguageManager.Instance.ResourceManager.GetString("System_Language", SystemLanguageManager.Instance.CultureInfo);

            lblGroupCapacity.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Group_Capacity", SystemLanguageManager.Instance.CultureInfo);

            lblRowCapacity.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Row_Capacity", SystemLanguageManager.Instance.CultureInfo);
            lblAuthorization.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Authorization_Status", SystemLanguageManager.Instance.CultureInfo);
            btnSavePaameters.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Save_Parameters", SystemLanguageManager.Instance.CultureInfo);
            btnLaunchGroupControl.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Launch_Group_Control", SystemLanguageManager.Instance.CultureInfo);
            btnExit.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Close_Group_Control", SystemLanguageManager.Instance.CultureInfo);
            rbChinese.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Chinese", SystemLanguageManager.Instance.CultureInfo);
            rbEnglish.Content = SystemLanguageManager.Instance.ResourceManager.GetString("English", SystemLanguageManager.Instance.CultureInfo);
            lblAuthorizedNumbers.Text = string.Format(SystemLanguageManager.Instance.ResourceManager.GetString("Authorized_Number", SystemLanguageManager.Instance.CultureInfo), ConfigVals.MaxNums);
            Title = SystemLanguageManager.Instance.ResourceManager.GetString("Product_Name", SystemLanguageManager.Instance.CultureInfo);
        }


        private void rbLanguage_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            SwitchLanguage();
        }

        private void btnSavePaameters_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            int groupCapacity = int.Parse(cbGroupCapacity.SelectionBoxItem.ToString());
            int rowCapacity = int.Parse(cbRowCapacity.SelectionBoxItem.ToString());

            VmManager.Instance.Initialize(ConfigVals.MaxNums, groupCapacity);

            ConfigsBLL bll = new ConfigsBLL();
            int lang = (rbEnglish.IsChecked == true) ? 2 : 1;
            int flag = bll.SaveConfigs(lang, groupCapacity, rowCapacity);
            if (flag > 0)
            {
                ConfigVals.Lang = lang;

                ConfigVals.GroupNums = groupCapacity;
                ConfigVals.RowNums = rowCapacity;
                MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("Save_Success", SystemLanguageManager.Instance.CultureInfo));
            }

        }

        private void btnLaunchGroupControl_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            MainView mainView = new MainView();
            mainView.Show();

            Close();
        }

        private void btnExit_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Close();
        }
    }
}
