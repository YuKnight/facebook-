using System;
using System.Threading.Tasks;
using System.Windows;
using Wx.Qunkong360.Wpf.ContentViews;

namespace Wx.Qunkong360.Wpf.Utils
{
    public class MessageQueueManager
    {
        public static readonly MessageQueueManager Instance = new MessageQueueManager();


        public void AddInfo(string message)
        {
            Task.Run(async () =>
            {
                MsgBoxView msgBoxView = null;
                await Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    msgBoxView = new MsgBoxView()
                    {
                        DataContext = new MsgBoxViewModel(message, MessageType.Info)
                    };
                    msgBoxView.Show();
                }));

                await Task.Delay(3000);

                await Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    msgBoxView?.Close();
                }));

            });
        }

        public void AddWarning(string message)
        {
            Task.Run(async () =>
            {
                MsgBoxView msgBoxView = null;
                await Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    msgBoxView = new MsgBoxView()
                    {
                        DataContext = new MsgBoxViewModel(message, MessageType.Warning)
                    };
                    msgBoxView.Show();
                }));

                await Task.Delay(3000);

                await Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    msgBoxView?.Close();
                }));

            });
        }

        public void AddError(string message)
        {
            Task.Run(async () =>
            {
                MsgBoxView msgBoxView = null;
                await Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    msgBoxView = new MsgBoxView()
                    {
                        DataContext = new MsgBoxViewModel(message, MessageType.Error)
                    };
                    msgBoxView.Show();
                }));

                await Task.Delay(3000);

                await Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    msgBoxView?.Close();
                }));

            });
        }
    }
}
