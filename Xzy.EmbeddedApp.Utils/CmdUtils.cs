using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Xzy.EmbeddedApp.Utils
{
    public static class CmdUtils
    {
        //private static Process proc = new Process();
        private static string str = "";
  
        /// <summary>
        /// 执行CMD语句
        /// </summary>
        /// <param name="cmd">要执行的CMD命令</param>
        public static (string, int) RunCmd(string cmd)
        {
            Process proc = new Process();

            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.FileName = "cmd.exe";
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardError = true;
            proc.StartInfo.RedirectStandardInput = true;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.Start();
            int id = proc.Id;
            proc.StandardInput.WriteLine(cmd);
            proc.StandardInput.WriteLine("exit");
            string outStr =  proc.StandardOutput.ReadToEnd();
            proc.Close();
            return (outStr, id);
        }

        public static void RunCmd2(object cmd)
        {
            Process proc = new Process();

            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.FileName = "cmd.exe";
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardError = true;
            proc.StartInfo.RedirectStandardInput = true;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.Start();
            int id = proc.Id;
            proc.StandardInput.WriteLine((string)cmd);
            proc.StandardInput.WriteLine("exit");
            string outStr = proc.StandardOutput.ReadToEnd();
            proc.Close();
        }

        /// <summary>
        /// 运行命令
        /// </summary>
        /// <param name="procInner"></param>
        /// <param name="cmd"></param>
        /// <param name="isReadResult">是否读取返回结果，默认：true，可不填写</param>
        /// <param name="isAutoClose">是否自动关闭进程，默认：true，可不填写</param>
        /// <returns></returns>
        public static (string, int) RunCmd(Process procInner, string cmd, bool isReadResult = true, bool isAutoClose = true)
        {
            Process proc = new Process();

            int id = -1;
            if (procInner == null)
            {
                procInner = proc;
                procInner.StartInfo.CreateNoWindow = true;
                procInner.StartInfo.FileName = "cmd.exe";
                procInner.StartInfo.UseShellExecute = false;
                procInner.StartInfo.RedirectStandardError = true;
                procInner.StartInfo.RedirectStandardInput = true;
                procInner.StartInfo.RedirectStandardOutput = true;
                procInner.Start();
                id = proc.Id;
                procInner.StandardInput.WriteLine(cmd);
                procInner.StandardInput.WriteLine("exit");
                procInner.StandardInput.Flush();
            }


            string outStr = "";
            if (isReadResult)
                outStr = proc.StandardOutput.ReadToEnd();
            if (isAutoClose)
                procInner.Close();
            return (outStr, id);
        }

        /// <summary>
        /// 运行命令-手机管理
        /// </summary>
        /// <param name="procInner"></param>
        /// <param name="cmd"></param>
        /// <param name="isReadResult">是否读取返回结果，默认：true，可不填写</param>
        /// <param name="isAutoClose">是否自动关闭进程，默认：true，可不填写</param>
        /// <returns></returns>
        public static async Task RunCmdManger(Process procInner, string cmd, bool isReadResult = true, bool isAutoClose = true)
        {
            Process proc = new Process();

            int id = -1;
            if (procInner == null)
            {
                procInner = proc;
                procInner.StartInfo.CreateNoWindow = true;
                procInner.StartInfo.FileName = "cmd.exe";
                procInner.StartInfo.UseShellExecute = false;
                procInner.StartInfo.RedirectStandardError = true;
                procInner.StartInfo.RedirectStandardInput = true;
                procInner.StartInfo.RedirectStandardOutput = true;
                procInner.Start();
                id = proc.Id;
                procInner.StandardInput.WriteLine(cmd);
                procInner.StandardInput.WriteLine("exit");
                procInner.StandardInput.Flush();
            }


            string outStr = "";
            if (isReadResult)
                await Task.Run(() =>
                {
                    Thread.Sleep(1000);
                    StreamOut(proc);

                    return outStr;
                });
            if (isAutoClose)
                procInner.Close();
        }

        public static void StreamOut(Process process)
        {
            str = process.StandardOutput.ReadToEnd();
        }

        public static string RunCmdGetPlainResult(string cmd)
        {
            return RunCmd(null, cmd).Item1.ToString();
        }

        /// <summary>
        /// 批量执行所有命令集合
        /// </summary>
        /// <param name="cmdList">要执行的命令集合</param>
        /// <returns></returns>
        public static Process RunCmdListSync(List<string> cmdList, bool isRetuen = false)
        {
            Process proc = new Process();

            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.FileName = "cmd.exe";
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardError = true;
            proc.StartInfo.RedirectStandardInput = true;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.Start();
            int id = proc.Id;
            Console.WriteLine("ProcID:" + id);
            //TODO 对于有等待输入的CMD，不能每次新建CMD，需要将要执行的命令依次发送即可
            foreach (string cmd in cmdList)
            {
                proc.StandardInput.WriteLine(cmd);
                proc.StandardInput.Flush();
                if (!cmd.Contains("adb -s") && isRetuen)
                {
                    string outStr = proc.StandardOutput.ReadToEnd();
                    proc.WaitForExit();
                }
                proc.StandardInput.WriteLine("exit");

            }



            return proc;


        }

        /// <summary>
        /// 批量执行所有命令集合
        /// </summary>
        /// <param name="cmdList">要执行的命令集合</param>
        /// <returns></returns>
        public static Process RunCmdListSync1(List<string> cmdList, bool isRetuen = false)
        {
            Process p = new Process();

            try
            {
                ProcessStartInfo info = new ProcessStartInfo();
                info.FileName = "cmd.exe";
                info.CreateNoWindow = true;
                info.RedirectStandardInput = true;
                info.UseShellExecute = false;
                info.RedirectStandardError = true;
                info.RedirectStandardInput = true;
                info.RedirectStandardOutput = true;

                p.StartInfo = info;
                p.Start();

                using (StreamWriter sw = p.StandardInput)
                {
                    if (sw.BaseStream.CanWrite)
                    {
                        foreach (string cmd in cmdList)
                        {
                            sw.WriteLine(cmd);
                            Thread.Sleep(2000);
                        }
                    }
                }

                p.Close();


            }
            catch (Exception e)
            {

                throw;
            }


            return p;


        }

        /// <summary>
        /// 批量执行所有命令集合
        /// </summary>
        /// <param name="cmdList">要执行的命令集合</param>
        /// <returns></returns>
        public static Process RunCmdListSync12(List<string> cmdList,int id,out int number, bool isRetuen = false)
        {
            Process p = new Process();

            try
            {
                ProcessStartInfo info = new ProcessStartInfo();
                info.FileName = "cmd.exe";
                info.CreateNoWindow = true;
                info.RedirectStandardInput = true;
                info.UseShellExecute = false;
                info.RedirectStandardError = true;
                info.RedirectStandardInput = true;
                info.RedirectStandardOutput = true;

                p.StartInfo = info;
                p.Start();

                using (StreamWriter sw = p.StandardInput)
                {
                    if (sw.BaseStream.CanWrite)
                    {
                        foreach (string cmd in cmdList)
                        {
                            Thread.Sleep(1500);
                            sw.WriteLine(cmd);
                        }
                        
                    }
                }

                using (StreamReader sr = p.StandardOutput)
                 {
                    string outstring = sr.ReadLine();
                    outstring = outstring + "\r\n";
                    int flag = 0;
                    while (!sr.EndOfStream)
                    {
                        outstring = sr.ReadLine();
                        if (outstring.IndexOf("com.facebook.katana") > 0)
                        {
                            flag = 1;
                            break;
                        }
                    }
                    if (flag == 1)
                    {
                        number = 1;

                    }
                    else
                    {
                        number = 0;
                        
                    }
                }
                 p.WaitForExit();
                p.Close();


            }
            catch (Exception e)
            {

                throw;
            }


            return p;


        }

        /// <summary>
        /// 打开软件并执行命令
        /// </summary>
        /// <param name="programName">软件路径加名称（.exe文件）</param>
        /// <param name="cmd">要执行的命令</param>
        public static void RunProgram(string programName, string cmd)
        {
            Process proc = new Process();
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.FileName = programName;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardError = true;
            proc.StartInfo.RedirectStandardInput = true;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.Start();
            if (cmd.Length != 0)
            {
                proc.StandardInput.WriteLine(cmd);
            }
            proc.Close();
        }
        /// <summary>
        /// 打开软件
        /// </summary>
        /// <param name="programName">软件路径加名称（.exe文件）</param>
        public static void RunProgram(string programName)
        {
            RunProgram(programName, "");
        }
    }
}
