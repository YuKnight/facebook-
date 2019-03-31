using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Xzy.EmbeddedApp.Model;
using System.Linq;
using System.Threading;

namespace Xzy.EmbeddedApp.Utils
{
    public static class ProcessUtils
    {
        public static string LDPath = "";
        /// <summary>
        /// 初始化雷电模拟器参数
        /// </summary>
        /// <param name="path"></param>
        /// <param name="simulator"></param>
        public static void Init(Simulator simulator)
        {
            string cmdCpu = $"--cpu {simulator.Cpu}";
            string cmdResolution = $"--resolution {simulator.Width},{simulator.Height},{simulator.Dpi}";
            string cmdMemory = $"--memory {simulator.Memory}";
            //string cmdImei = $"--imei {simulator.Imei}";
            string androidid = $"--androidid {simulator.Androidid}";
            string cmdStr = $"{LDPath}/dnconsole.exe modify --index 0 {cmdResolution} {cmdCpu} {cmdMemory} {androidid}";
            CmdUtils.RunCmd(cmdStr);
        }

        /// <summary>
        /// 初始化雷电模拟器参数
        /// </summary>
        /// <param name="path"></param>
        /// <param name="simulator"></param>
        public static void Init(int index, Simulator simulator)
        {
            string cmdCpu = $"--cpu {simulator.Cpu}";
            string cmdResolution = $"--resolution {simulator.Width},{simulator.Height},{simulator.Dpi}";
            string cmdMemory = $"--memory {simulator.Memory}";
            string cmdImei = $"--imei {simulator.Imei}";
            string androidid = $"--androidid {simulator.Androidid}";
            string cmdStr = $"{LDPath}/dnconsole.exe modify --index {index} {cmdResolution} {cmdCpu} {cmdMemory} {androidid}";
            CmdUtils.RunCmd(cmdStr);
        }

        /// <summary>
        /// 退出模拟器
        /// </summary>
        /// <param name="index"></param>
        public static void Quit(int index)
        {
            string cmdStr = $"{LDPath}/dnconsole.exe quit --index {index}";
            CmdUtils.RunCmd(cmdStr);
        }

        public static void QuitAll()
        {
            string cmdStr = $"{LDPath}/dnconsole.exe quitall";
            CmdUtils.RunCmd(cmdStr);
        }

        public static int Run(int index)
        {
            string cmdStr = $"{LDPath}/dnconsole.exe launch --index {index}";
            var cmd = CmdUtils.RunCmd(cmdStr);
            int id = cmd.Item2;
            return id;
            //Process.GetProcessById(cmd.Item2).MainWindowHandle;
        }

        /// <summary>
        /// 启动一个App
        /// </summary>
        /// <param name="index"></param>
        /// <param name="packagename"></param>
        /// <returns></returns>
        public static int AdbOpenApps(int index, string packagename)
        {
            string package = $"--packagename {packagename}";
            string cmdStr = $"{LDPath}/dnconsole.exe runapp --index {index} {package}";
            var cmd = CmdUtils.RunCmd(cmdStr);

            return 1;
        }

        /// <summary>
        /// 关闭一个App
        /// </summary>
        /// <param name="index"></param>
        /// <param name="packagename"></param>
        /// <returns></returns>
        public static int AdbCloseApps(int index, string packagename)
        {
            string package = $"--packagename {packagename}";
            string cmdStr = $"{LDPath}/dnconsole.exe killapp --index {index} {package}";
            var cmd = CmdUtils.RunCmd(cmdStr);
            return 1;
        }

        public static void AdbSortDevices()
        {
            string cmdStr = $"{LDPath}/dnconsole.exe sortWnd";
            var cmd = CmdUtils.RunCmd(cmdStr);
        }

        /// <summary>
        /// 安装apps
        /// </summary>
        /// <param name="index"></param>
        /// <param name="filename"></param>
        public static int AdbInstallApp(int index, string filename)
        {
            string files = $"--filename {filename} ";
            string cmdStr = $"{LDPath}/dnconsole.exe installapp --index {index} {files}";
          
            var cmd = CmdUtils.RunCmd(cmdStr);

            return 1;
        }

        /// <summary>
        /// 卸载app
        /// </summary>
        /// <param name="index"></param>
        /// <param name="packagename"></param>
        /// <returns></returns>
        public static int AdbUnInstallApp(int index, string packagename)
        {
            string packageApk = $"--packagename {packagename}";
            var Ftype = packageApk.Split('\\')[packageApk.Split('\\').Length - 1];

            string package=null;
            if (Ftype == "fb_facebook_153.0.0.53.88.apk")
            {
                package = $"com.facebook.katana";
            }
            else if (Ftype == "com.facebook.mlite_v40.0.0.25.156-123092092_Android-2.3.2.apk")
            {
                 package = $"com.facebook.mlite";
            }
            else if (Ftype == "app-debug-androidTest.apk")
            {
                 package = $"uitest.com.uitest.test";
            }
            else if (Ftype == "app-debug.apk")
            {
                 package = $"uitest.com.uitest";
            }

            string cmdStr = $"{LDPath}/dnconsole.exe uninstallapp --index {index} --packagename {package}";
            var cmd = CmdUtils.RunCmd(cmdStr);
            return 1;
        }

        public static int PushFileToVm(string mobileIdentifier, string sourceFilePath, string targetFilePath)
        {
            if (string.IsNullOrEmpty(mobileIdentifier))
            {
                return 0;
            }

            string cmd = $"{LDPath}/adb.exe -s {mobileIdentifier} push {sourceFilePath} {targetFilePath}";

            CmdUtils.RunCmd(cmd);

            return 1;
        }

        public static int DeleteFiles(string mobileIdentifier, string files)
        {
            if (string.IsNullOrEmpty(mobileIdentifier))
            {
                return 0;
            }

            string cmd = $"{LDPath}/adb.exe -s {mobileIdentifier} rm {files}";
            CmdUtils.RunCmd(cmd);

            return 1;
        }

        public static int DeleteAllPictures(string mobileIdentifier)
        {
            if (string.IsNullOrEmpty(mobileIdentifier))
            {
                return 0;
            }

            string pictures = "/sdcard/Pictures/*.*";
            string screenshots = "/sdcard/Pictures/Screenshots/*.*";
            string ads = "/sdcard/launcher/ad/*.*";

            string cmd1 = $"{LDPath}/adb.exe -s {mobileIdentifier} shell rm {pictures}";
            string cmd2 = $"{LDPath}/adb.exe -s {mobileIdentifier} shell rm {screenshots}";
            string cmd3 = $"{LDPath}/adb.exe -s {mobileIdentifier} shell rm {ads}";

            CmdUtils.RunCmd(cmd1);
            CmdUtils.RunCmd(cmd2);
            CmdUtils.RunCmd(cmd3);
            return 1;
        }

        /// <summary>
        /// 所以设备
        /// </summary>
        /// <param name="index"></param>
        /// <param name="devicesName"></param>
        /// <returns></returns>
        public static List<string> AdbDevices()
        {
            List<string> devicesAddress = new List<string>();

            string cmdStr = $"{LDPath}/adb devices ";
            string res = CmdUtils.RunCmdGetPlainResult(cmdStr);

            Regex reg = new Regex(@"((\d+\.){3}\d+)\:(\d+)");
            var matches = reg.Matches(res);
            foreach (var m in matches)
            {
                devicesAddress.Add(m.ToString());
            }
            return devicesAddress;
        }

        public static (string imei, string imsi) Regex (string cmdStr)
        {
            Regex reg = new Regex(@"(\[(phone.imsi|phone.imei)\]: \[(\d{15})\])");
            Match match = reg.Match(cmdStr);
            Match match2 = reg.Match(cmdStr);
            string Imei = match.Groups[3].ToString();
            string Imsi = match2.Groups[3].ToString();
            return (Imei, Imsi);
        }

        /// <summary>
        /// 手机型号
        /// </summary>
        /// <param name="index"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public static (String, int) AdbModel(int index, string model)
        {
            string cmdStr = $"{LDPath}/adb shell getprop ro.product.model {index} {model}";

            var cmd = CmdUtils.RunCmd(cmdStr);
            return cmd;
        }
        /// <summary>
        /// 手机品牌
        /// </summary>
        /// <param name="index"></param>
        /// <param name="brand"></param>
        /// <returns></returns>
        public static (string,int) AdbBrand(int index, string brand)
        {
            string cmdStr = $"{LDPath}/adb shell getprop ro.product.brand {index} {brand}";
            var cmd = CmdUtils.RunCmd(cmdStr);
            return cmd;
        }
        /// <summary>
        /// 手机分辨率
        /// </summary>
        /// <param name="index"></param>
        /// <param name="distinguish"></param>
        /// <returns></returns>
        public static (string,string) AdbDistinguish(int index, string distinguish)
        {

            string cmdStr = string.Format("{0}\\adb shell \"dumpsys window | grep mUnrestrictedScreen\"" , LDPath);
            var cmd = CmdUtils.RunCmd(cmdStr);
            //320x120
            Regex reg = new Regex(@"\d*x\d*");
            string res = CommonUtils.SplitCMDResult(cmd.Item1.ToString(), new char[] { '\r', '\n' }, 3);
            Match match = reg.Match(res);
            string screen = match.Groups[0].ToString();

            string[] screenArr = screen.Split('x');
            return (screenArr[0],screenArr[1]);
        }

        /// <summary>
        /// 获取Imei
        /// </summary>
        /// <param name="index"></param>
        /// <param name="Imei"></param>
        /// <returns></returns>
        public static (int, string) AdbGetprop(int index, string Imei)
        {
            string cmdStr = string.Format("{0}\\adb shell getprop",LDPath);
            var cmd = CmdUtils.RunCmd(cmdStr);
            //Imei
            Regex reg = new Regex(@"(0|[1-9][0-9]*)");
           // Regex indexReg = new Regex(@"([1-9][0-9]*){1,3}");
            string res = CommonUtils.SplitCMDResult(cmd.Item1.ToString(), new char[] { '\r', '\n' }, 91);
            string strInderx = CommonUtils.SplitCMDResult(cmd.Item1.ToString(), new char[] { '\r', '\n' }, 90);
            Match match = reg.Match(res);
            //Match inddexMatch = indexReg.Match(strInderx);
            //int indexMatch = Convert.ToInt32(inddexMatch.Groups[0].Value);
            string imei = match.Groups[0].ToString();
            return (1, imei);
        }

        /// <summary>
        /// 获取Androidid
        /// </summary>
        /// <param name="index"></param>
        /// <param name="Imei"></param>
        /// <returns></returns>
        public static (int, string) AdbAndroidid(int index, string android)
        {
            string cmdStr = string.Format("{0}\\adb shell getprop", LDPath);
            var cmd = CmdUtils.RunCmd(cmdStr);
            Regex reg = new Regex(@"(0|[1-9][0-9]*)");
            string res = CommonUtils.SplitCMDResult(cmd.Item1.ToString(), new char[] { '\r', '\n' }, 89);
            Match match = reg.Match(res);
            string androidid = match.Groups[0].ToString();
            return (1, androidid);
        }

        /// <summary>
        /// 获取Imsi
        /// </summary>
        /// <param name="index"></param>
        /// <param name="Imei"></param>
        /// <returns></returns>
        public static (int, string) AdbImsi(int index, string Imsi)
        {
            string cmdStr = string.Format("{0}\\adb shell getprop", LDPath);
            var cmd = CmdUtils.RunCmd(cmdStr);
            Regex reg = new Regex(@"(0|[1-9][0-9]*)");
            string res = CommonUtils.SplitCMDResult(cmd.Item1.ToString(), new char[] { '\r', '\n' }, 92);
            Match match = reg.Match(res);
            string imsi = match.Groups[0].ToString();
            return (1, imsi);
        }

        /// <summary>
        /// 手机物理密度
        /// </summary>
        /// <param name="index"></param>
        /// <param name="density"></param>
        /// <returns></returns>
        public static int AdbDensity(int index, string density)
        {
            string cmdStr = $"{LDPath}/adb shell wm density";
            var cmd = CmdUtils.RunCmd(cmdStr);
            return 1;
        }

        /// <summary>
        /// 模拟点击
        /// </summary>
        /// <param name="index">模拟器编号</param>
        /// <param name="X">坐标X</param>
        /// <param name="Y">坐标Y</param>
        /// <returns></returns>
        public static int AdbTapClick(int index,int X,int Y)
        {
            string cmd = $"{LDPath}/adb.exe -s 127.0.0.1:{5555 + index * 2} shell input tap {X} {Y}";
            CmdUtils.RunCmd(cmd);

            return 1;
        }

        /// <summary>
        /// 模拟输入内容
        /// </summary>
        /// <param name="index">模拟器编号</param>
        /// <param name="text">输入内容</param>
        /// <returns></returns>
        public static int AdbInputText(int index,string text)
        {
            string cmd = $"{LDPath}/dnconsole.exe action --index {index} --key call.input --value \"{text}\"";
            CmdUtils.RunCmd(cmd);

            return 1;
        }

        /// <summary>
        /// 模拟鼠标滑动
        /// </summary>
        /// <param name="index">模拟器编号</param>
        /// <param name="X1">滑动的起始坐标X</param>
        /// <param name="Y1">滑动的起始坐标Y</param>
        /// <param name="X2">滑动的停止坐标X</param>
        /// <param name="Y2">滑动的停止坐标Y</param>
        /// <returns></returns>
        public static int AdbSwipe(string mobileIdentifier,int X1,int Y1,int X2,int Y2)
        {
            string cmd = $"{LDPath}/adb.exe -s {mobileIdentifier} shell input swipe {X1} {Y1} {X2} {Y2}";
            CmdUtils.RunCmd(cmd);

            return 1;
        }

        public static int LaunchAndroidCommander(string device)
        {
            string cmd = $"{LDPath}/adb.exe -s {device} shell am instrument -w -r   -e debug false -e class 'test.InstrumentationTest#testFunction' uitest.com.uitest.test/android.support.test.runner.AndroidJUnitRunner";
            //string cmd = $"{LDPath}/adb.exe -s 127.0.0.1:{5555 + index * 2} shell am instrument -w -r   -e debug false -e class 'test.InstrumentationTest#testFunction' uitest.com.uitest.test/android.support.test.runner.AndroidJUnitRunner";
            //string cmd = $"{LDPath}/adb.exe -s emulator-{5554 + index * 2} shell am instrument -w -r   -e debug false -e class 'test.InstrumentationTest#testFunction' uitest.com.uitest.test/android.support.test.runner.AndroidJUnitRunner";
            CmdUtils.RunCmd(cmd);

            return 1;
        }

        public static string KillAdbServer()
        {
            string killServerCmd1 = $"{LDPath}/adb.exe kill-server";
            (string, int) killResult1 = CmdUtils.RunCmd(killServerCmd1);

            return killResult1.Item1;

            //Console.WriteLine(killResult1.Item1);

            //string killServerCmd2 = $"{LDPath}/adb.exe kill-server";
            //(string, int) killResult2 = CmdUtils.RunCmd(killServerCmd2);

            //Console.WriteLine(killResult2.Item1);

            //string killServerCmd3 = $"{LDPath}/adb.exe kill-server";
            //(string, int) killResult3 = CmdUtils.RunCmd(killServerCmd3);

            //Console.WriteLine(killResult3.Item1);
        }

        public static string StartAdbServer()
        {
            string startServerCmd = $"{LDPath}/adb.exe start-server";
            (string, int) startResult = CmdUtils.RunCmd(startServerCmd);

            return startResult.Item1;
            //Console.WriteLine(startResult.Item1);
        }

        public static string[] GetAttachedDevices()
        {
            Console.WriteLine("GetAttachedDevices begins...");
            string cmd = $"{ProcessUtils.LDPath}/adb.exe devices";

            (string, int) result = CmdUtils.RunCmd(cmd);

            string rawOutputData = result.Item1;

            string[] pieces = rawOutputData.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            var devices = from piece in pieces
                          where piece.EndsWith("device")
                          select piece.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries)[0];

            Console.WriteLine("GetAttachedDevices ends...");

            return devices.ToArray();
        }

        public static void ReconnectDevices()
        {
            KillAdbServer();
            StartAdbServer();
        }

        /// <summary>
        /// 静默安装apps
        /// </summary>
        /// <param name="index"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static int AdbSilentInstallation(int index,string filename)
        {
            try
            {
                    string cmd = $"{LDPath}/adb -s emulator-{5554 + index * 2} install -r {filename}";
                    CmdUtils.RunCmd(cmd);
                    Thread.Sleep(200);
                    return 1;
                
            }
            catch (Exception ex)
            {

                throw ex;
            }
           
        }

        /// <summary>
        /// 关闭服务
        /// </summary>
        /// <returns></returns>
        public static int AdbCloseService(int index)
        {
            string cmd = $"{LDPath}/adb -s emulator-{5554 + index * 2} kill-server";
            CmdUtils.RunCmd(cmd);

            return 1;
        }

        /// <summary>
        /// adb输入
        /// </summary>
        /// <param name="deviceIP"模拟器IP></param>
        /// <param name="text">输入的内容</param>
        /// <returns></returns>
        /// 
        //[Obsolete("已过时，可弃用")]
        //public static int AdbInputText(string deviceIP, string text)
        //{
        //    Process process = ConnectDeviceGetProcess(deviceIP);

        //    //执行具体操作
        //    string cmdStr = string.Format("input text {0}", text);
        //    var cmd = CmdUtils.RunCmd(process, cmdStr);


        //    // string package = $"\"{text}\"";
        //    //string cmdStr = $"{LDPath}/adb -s 127.0.0.1:{5555 + index * 2} shell";
        //    //var cmd = CmdUtils.RunCmd(cmdStr);
        //    //string cmdStrInput = $"{LDPath}/adb shell input text {text}";
        //    // var cmdInput= CmdUtils.RunCmd(cmdStrInput);
        //    return 1;
        //}

        /// <summary>
        /// 连接模拟器并获取指定模拟器的会话进程
        /// </summary>
        /// <param name="deviceIP"></param>
        /// <returns></returns>
        //public static Process ConnectDeviceGetProcess(string deviceIP)
        //{
        //    //连接模拟器
        //    string connectDeviceCmd = string.Format("{0}/adb -s {1} shell", LDPath, deviceIP);
        //    Process process = CmdUtils.RunCmdGetPlainResultSync(connectDeviceCmd);
        //    return process;
        //}

        /// <summary>
        /// Tab按键
        /// </summary>
        /// <param name="index"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        /// 
        //[Obsolete("已过时，可弃用")]
        //public static int AdbTab(int index, string text)
        //{
        //    // string text = $"KEYCODE_TAB";
        //    string cmdStr = $"{LDPath}/adb shell input keyevent KEYCODE_TAB";
        //    var cmd = CmdUtils.RunCmd(cmdStr);
        //    return 1;
        //}

        /// <summary>
        /// 模拟鼠标点击
        /// </summary>
        /// <param name="TapX"></param>
        /// <param name="TapY"></param>
        /// <returns></returns>
        /// 
        //[Obsolete("已过时，可弃用")]
        //public static int AdbMouseClick(string deviceIP, int TapX, int TapY)
        //{
        //    Process process = ConnectDeviceGetProcess(deviceIP);
        //    string strTap = $"{TapX} {TapY}";
        //    string cmdStr = $"input tap {strTap}";
        //    var cmd = CmdUtils.RunCmd(process, cmdStr);
        //    return 1;
        //}
    }
}