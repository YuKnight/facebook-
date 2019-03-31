using System.Windows;
using Wx.Qunkong360.Wpf.Utils;

namespace Wx.Qunkong360.Wpf.ContentViews
{
    /// <summary>
    /// CustomMessageDialog.xaml 的交互逻辑
    /// </summary>
    public partial class CustomMessageDialog
    {
        public CustomMessageDialog()
        {
            InitializeComponent();
            Title = SystemLanguageManager.Instance.ResourceManager.GetString("Message_Prompt", SystemLanguageManager.Instance.CultureInfo);
            btnConfirm.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Accept", SystemLanguageManager.Instance.CultureInfo);
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
