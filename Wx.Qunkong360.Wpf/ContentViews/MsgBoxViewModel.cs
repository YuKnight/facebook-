using System.Windows.Media;
using Prism.Mvvm;
using Wx.Qunkong360.Wpf.Utils;

namespace Wx.Qunkong360.Wpf.ContentViews
{
    public class MsgBoxViewModel : BindableBase
    {
        public MsgBoxViewModel(string message, MessageType msgType)
        {
            Msg = message;
            switch (msgType)
            {

                case MessageType.Error:
                    var errorBorderString = ColorConverter.ConvertFromString("#ff8080");
                    if (errorBorderString != null)
                        MsgBorderBrush = new SolidColorBrush((Color)errorBorderString);

                    var errorBackgroundString = ColorConverter.ConvertFromString("#fff2f2");
                    if (errorBackgroundString != null)
                        MsgBackgroundBrush = new SolidColorBrush((Color)errorBackgroundString);
                    MsgImage = "../Images/msg_error.png";
                    break;

                case MessageType.Warning:
                    var warningBorderString = ColorConverter.ConvertFromString("#ffcc7f");
                    if (warningBorderString != null)
                        MsgBorderBrush = new SolidColorBrush((Color)warningBorderString);

                    var warningBackgroundString = ColorConverter.ConvertFromString("#ffffe5");
                    if (warningBackgroundString != null)
                        MsgBackgroundBrush = new SolidColorBrush((Color)warningBackgroundString);

                    MsgImage = "../Images/msg_warning.png";
                    break;
                case MessageType.Info:
                    //var themeDictionary =
                    //    Application.Current.Resources.MergedDictionaries.FirstOrDefault(
                    //        dictionary => dictionary.Source.OriginalString.EndsWith("Theme.xaml"));

                    //if (themeDictionary != null)
                    //{
                    //    var themeBrush = themeDictionary["ThemeBrush"];
                    //    var infoBorderBrush = (SolidColorBrush)themeBrush;
                    //    MsgBorderBrush = infoBorderBrush;
                    //}

                    var infoBackgroundString = ColorConverter.ConvertFromString("#e4f7f8");
                    if (infoBackgroundString != null)
                        MsgBackgroundBrush = new SolidColorBrush((Color)infoBackgroundString);

                    MsgImage = "../Images/msg_infomation.png";
                    break;
            }
        }

        private SolidColorBrush _msgBorderBrush;
        public SolidColorBrush MsgBorderBrush
        {
            get { return _msgBorderBrush; }
            set { SetProperty(ref _msgBorderBrush, value); }
        }

        private SolidColorBrush _msgBackgroundBrush;
        public SolidColorBrush MsgBackgroundBrush
        {
            get { return _msgBackgroundBrush; }
            set { SetProperty(ref _msgBackgroundBrush, value); }
        }

        private string _msgImage;
        public string MsgImage
        {
            get { return _msgImage; }
            set { SetProperty(ref _msgImage, value); }
        }

        private string _msg;
        public string Msg
        {
            get { return _msg; }
            set { SetProperty(ref _msg, value); }
        }

    }
}
