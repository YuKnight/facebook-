using Wx.Qunkong360.Wpf.ContentViews;

namespace Wx.Qunkong360.Wpf.Utils
{
    public class MessageDialogManager
    {
        public static void ShowDialogAsync(string msg, bool isModeDialog = false)
        {
            //var dialog = new MessageDialogView()
            //{
            //    Message = { Text = msg },
            //};

            CustomMessageDialog customMessageDialog = new CustomMessageDialog()
            {
                Message = { Text = msg },
            };

            //await DialogHost.Show(dialog, "rootDialog");

            if (isModeDialog)
            {
                customMessageDialog.ShowDialog();
            }
            else
            {
                customMessageDialog.Show();
            }
        }
    }
}
