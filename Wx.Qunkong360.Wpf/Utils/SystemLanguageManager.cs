using Cj.EmbeddedApp.BLL;
using System.Globalization;
using System.Resources;
using Xzy.EmbeddedApp.Model;
using Xzy.EmbeddedApp.Utils;

namespace Wx.Qunkong360.Wpf.Utils
{
    public class SystemLanguageManager
    {
        private static readonly object SyncRoot = new object();

        private static SystemLanguageManager _instance;
        public static SystemLanguageManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (SyncRoot)
                    {
                        if (_instance == null)
                        {
                            _instance = new SystemLanguageManager();
                        }
                    }
                }

                return _instance;
            }
        }

        private SystemLanguageManager()
        {
            ResourceManager = new ResourceManager("Wx.Qunkong360.Wpf.Languages.Res", typeof(SystemLanguageManager).Assembly);

            SetupCultureInfo();
        }

        public void SetupCultureInfo()
        {
            ConfigsBLL bll = new ConfigsBLL();
            Configs configs = bll.GetAllData();

            if (configs.Lang == 1)
            {
                CultureInfo = CultureInfo.CreateSpecificCulture("zh-cn");
            }
            else if (configs.Lang == 2)
            {
                CultureInfo = CultureInfo.CreateSpecificCulture("en-us");
            }
        }

        public ResourceManager ResourceManager { get; set; }
        public CultureInfo CultureInfo { get; set; }

        public string GetAnotherLanguageString(string key)
        {
            if (ConfigVals.Lang == 1)
            {
                var resourceManager = new ResourceManager("Wx.Qunkong360.Wpf.Languages.Res", typeof(SystemLanguageManager).Assembly);
                var cultureInfo = CultureInfo.CreateSpecificCulture("en-us");

                return resourceManager.GetString(key, cultureInfo);
            }
            else
            {
                var resourceManager = new ResourceManager("Wx.Qunkong360.Wpf.Languages.Res", typeof(SystemLanguageManager).Assembly);
                var cultureInfo = CultureInfo.CreateSpecificCulture("zh-cn");

                return resourceManager.GetString(key, cultureInfo);
            }
        }
    }
}
