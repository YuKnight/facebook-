using System.Windows;
using Wx.Qunkong360.Wpf.ContentViews;
using Wx.Qunkong360.Wpf.Utils;

namespace Wx.Qunkong360.Wpf.Views
{
    /// <summary>
    /// FacebookOperationView.xaml 的交互逻辑
    /// </summary>
    public partial class FacebookOperationView
    {
        public FacebookOperationView()
        {
            InitializeComponent();

            Title = SystemLanguageManager.Instance.ResourceManager.GetString("Facebook_Operation", SystemLanguageManager.Instance.CultureInfo);

            lblAddFriends.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Add_Friends", SystemLanguageManager.Instance.CultureInfo);
            lblMaintainAccounts.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Maintain_Accounts", SystemLanguageManager.Instance.CultureInfo);
            lblPostMoments.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Post_Moments", SystemLanguageManager.Instance.CultureInfo);
            lblGroupAndHomepage.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Group_Homepage", SystemLanguageManager.Instance.CultureInfo);
            lblTaskManagement.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Task_Management", SystemLanguageManager.Instance.CultureInfo);
            lblPhoneManagement.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Phone_Management", SystemLanguageManager.Instance.CultureInfo);
            lblClose.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Close", SystemLanguageManager.Instance.CultureInfo);

        }

        private void lbiAddFriends_Selected(object sender, RoutedEventArgs e)
        {
            gridContent.Children.Clear();
            gridContent.Children.Add(new AddFriendView());
        }

        private void lbiMaintainAccounts_Selected(object sender, RoutedEventArgs e)
        {
            gridContent.Children.Clear();
            gridContent.Children.Add(new MaintainAccountsView());
        }

        private void lbiPublishPost_Selected(object sender, RoutedEventArgs e)
        {
            gridContent.Children.Clear();
            gridContent.Children.Add(new PublishPostView());
        }

        private void lbiGroupAndHome_Selected(object sender, RoutedEventArgs e)
        {
            gridContent.Children.Clear();
            gridContent.Children.Add(new GroupAndHomepageView());
        }

        /// <summary>
        /// 任务管理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lbiTaskManagement_Selected(object sender, RoutedEventArgs e)
        {
            gridContent.Children.Clear();
            gridContent.Children.Add(new TaskManagerView());
        }
        /// <summary>
        /// 手机管理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lbiPhoneManagement_Selected(object sender, RoutedEventArgs e)
        {
            gridContent.Children.Clear();
            gridContent.Children.Add(new PhoneManagementView());
        }

        private void lbiClose_Selected(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
