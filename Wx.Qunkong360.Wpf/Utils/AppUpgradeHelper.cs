using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Wx.Qunkong360.Wpf.Implementation;
using Xzy.EmbeddedApp.Utils;

namespace Wx.Qunkong360.Wpf.Utils
{
    public class AppUpgradeHelper
    {
        private AppUpgradeHelper()
        {

        }

        public static readonly AppUpgradeHelper Instance = new AppUpgradeHelper();

        public async Task<string> InstallNewestApp()
        {
            try
            {
                await Task.Delay(2000);

                if (VmManager.Instance.RunningGroupIndex == -1)
                {

                    return SystemLanguageManager.Instance.ResourceManager.GetString("Error_Please_Launch_A_Vm", SystemLanguageManager.Instance.CultureInfo);
                }

                string appFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "UpgradeApp");

                if (!Directory.Exists(appFolder))
                {
                    Directory.CreateDirectory(appFolder);
                }

                string appFile = Path.Combine(appFolder, "app-release.apk");

                if (!File.Exists(appFile))
                {
                    return SystemLanguageManager.Instance.ResourceManager.GetString("No_Updated_App", SystemLanguageManager.Instance.CultureInfo);
                }

                for (int i = 0; i < VmManager.Instance.Column; i++)
                {
                    int vmIndex = VmManager.Instance.VmIndexArray[VmManager.Instance.RunningGroupIndex, i];

                    if (vmIndex != -1)
                    {
                        ProcessUtils.AdbInstallApp(vmIndex, appFile);
                        Thread.Sleep(200);
                    }
                }

                File.Delete(appFile);

                return SystemLanguageManager.Instance.ResourceManager.GetString("Finish_App_Update", SystemLanguageManager.Instance.CultureInfo);
            }
            catch (Exception ex)
            {
                LogUtils.Error($"{ex}");
                return SystemLanguageManager.Instance.ResourceManager.GetString("Failed_App_Update", SystemLanguageManager.Instance.CultureInfo);
            }
        }
    }
}
