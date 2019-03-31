using System.Windows.Controls;
using Wx.Qunkong360.Wpf.Utils;

namespace Wx.Qunkong360.Wpf.ContentViews
{
    /// <summary>
    /// MessageDialogView.xaml 的交互逻辑
    /// </summary>
    public partial class MessageDialogView : UserControl
    {
        public MessageDialogView()
        {
            InitializeComponent();
            btn.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Accept", SystemLanguageManager.Instance.CultureInfo);
        }
    }
}
