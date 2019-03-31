using Xzy.EmbeddedApp.Model;

namespace Wx.Qunkong360.Wpf.Utils
{
    public class ConfigManager
    {
        public static readonly ConfigManager Instance = new ConfigManager();

        public ConfigModel Config { get; set; }

        private ConfigManager()
        {

        }
    }
}
