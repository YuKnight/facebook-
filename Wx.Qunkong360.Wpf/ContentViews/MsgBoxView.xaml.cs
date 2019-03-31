using System.Windows;

namespace Wx.Qunkong360.Wpf.ContentViews
{
    /// <summary>
    /// MsgBox.xaml 的交互逻辑
    /// </summary>
    public partial class MsgBoxView
    {
        public MsgBoxView()
        {
            InitializeComponent();
            Left = (SystemParameters.PrimaryScreenWidth - 600) / 2;
            Top = SystemParameters.PrimaryScreenHeight * 0.84 - 60;
        }
    }
}
