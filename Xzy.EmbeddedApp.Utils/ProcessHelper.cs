using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CmdProcessLib
{
    public class ProcessHelper
    {


        /// <summary>
        /// 读取数据的时候等待时间，等待时间过短时，可能导致读取不出正确的数据。
        /// </summary>
        public static int WaitTime = 50;

        /// <summary>
        /// 连续运行模式，支持打开某程序后，持续向其输入命令，直到结束。注意：如果命令执行后一直等待，请尝试最后的moreArgs最后一个命令为"exit"
        /// </summary>
        /// <param name="exePath">cmd、adb等所在路径，例如C:\test\adb.exe</param>
        /// <param name="args"></param>
        /// <param name="moreArgs">【可选值】默认null。设置启动应用程序时要使用的一组命令行自变量.</param>
        /// <returns></returns>
        public static RunResult RunAsContinueMode(string exePath, string args, string[] moreArgs =null)
        {
            var result = new RunResult();
            try
            {
                using (var p = GetProcess())
                {
                    p.StartInfo.FileName = exePath;
                    p.StartInfo.Arguments = args;
                    p.Start();

                    //先输出一个换行，以便将程序的第一行输出显示出来。
                    //如adb.exe，假如不调用此函数的话，第一行等待的shell@android:/ $必须等待下一个命令输入才会显示。
                    p.StandardInput.WriteLine();

                    result.OutputString = ReadStandardOutputLine(p);

                    result.MoreOutputString = new Dictionary<string, string>();
                    if (moreArgs != null)
                    {
                        for (int i = 0; i < moreArgs.Length; i++)
                        {
                            //p.StandardInput.WriteLine(moreArgs[i] + '\r');
                              p.StandardInput.WriteLine(moreArgs[i] );

                            //必须等待一定时间，让程序运行一会儿，马上读取会读出空的值。
                            Thread.Sleep(WaitTime);

                            result.MoreOutputString.Add(moreArgs[i], ReadStandardOutputLine(p));
                        }
                    }

                    
                    p.WaitForExit();

                    result.ExitCode = p.ExitCode;
                    result.Success = true;
                }
            }
            catch (Win32Exception ex)
            {
                result.Success = false;

                //System Error Codes (Windows)
                //http://msdn.microsoft.com/en-us/library/ms681382(v=vs.85).aspx
                result.OutputString = string.Format("{0}", ex.NativeErrorCode);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.OutputString = ex.ToString();
            }
            return result;
        }

        /// <summary>
        /// 标准运行模式
        /// </summary>
        /// <param name="exePath">设置要启动的应用程序所在路径</param>
        /// <param name="args">设置启动应用程序时要使用的一组命令行自变量</param>
        /// <returns></returns>
        public static RunResult RunAsStandardModel(string exePath, string args)
        {
            var result = new RunResult();
            try
            {
                using (var p = GetProcess())
                {
                    p.StartInfo.FileName = exePath;
                    p.StartInfo.Arguments = args;
                    p.Start();

                    //获取正常信息
                    if (p.StandardOutput.Peek() > -1)
                        result.OutputString = p.StandardOutput.ReadToEnd();

                    //获取错误信息
                    if (p.StandardError.Peek() > -1)
                        result.OutputString = p.StandardError.ReadToEnd();

                    // Do not wait for the child process to exit before
                    // reading to the end of its redirected stream.
                    // p.WaitForExit();
                    // Read the output stream first and then wait.
                    p.WaitForExit();

                    result.ExitCode = p.ExitCode;
                    result.Success = true;
                }
            }
            catch (Win32Exception ex)
            {
                result.Success = false;

                //System Error Codes (Windows)
                //http://msdn.microsoft.com/en-us/library/ms681382(v=vs.85).aspx
                result.OutputString = string.Format("{0}", ex.NativeErrorCode);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.OutputString = ex.ToString();
            }
            return result;
        }


        /// <summary>
        /// 获取Process实例
        /// </summary>
        /// <returns></returns>
        private static Process GetProcess()
        {
            var mProcess = new Process();

            mProcess.StartInfo.CreateNoWindow = true;
            mProcess.StartInfo.UseShellExecute = false;
            mProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

            mProcess.StartInfo.RedirectStandardInput = true;
            mProcess.StartInfo.RedirectStandardError = true;
            mProcess.StartInfo.RedirectStandardOutput = true;
            mProcess.StartInfo.StandardOutputEncoding = Encoding.UTF8;

            return mProcess;
        }

        /// <summary>
        /// 读取指定的Process的输出流
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        private static string ReadStandardOutputLine(Process p)
        {
            var tmp = new StringBuilder();

            //当下一次读取时，Peek可能为-1，但此时缓冲区其实是有数据的。正常的Read一次之后，Peek就又有效了。
            if (p.StandardOutput.Peek() == -1)
                tmp.Append((char)p.StandardOutput.Read());

            while (p.StandardOutput.Peek() > -1)
            {
                tmp.Append((char)p.StandardOutput.Read());
            }
            return tmp.ToString().Trim();
        }


        /// <summary>
        /// ProcessHelper运行结果返回对象
        /// </summary>
        public class RunResult
        {
            /// <summary>
            /// 当执行不成功时，OutputString会输出错误信息。
            /// </summary>
            public bool Success;
            public int ExitCode;
            public string OutputString;
            /// <summary>
            /// 调用RunAsContinueMode时，使用额外参数的顺序作为索引。</br>
            /// 如：调用ProcessHelper.RunAsContinueMode(AdbExePath, "shell", new[] { "su", "ls /data/data", "exit", "exit" });
            /// 果：MoreOutputString[0] = su执行后的结果字符串；MoreOutputString[1] = ls ...执行后的结果字符串；MoreOutputString[2] = exit执行后的结果字符串
            /// </summary>
            public Dictionary<string, string> MoreOutputString;

            public new string ToString()
            {
                var str = new StringBuilder();
                str.AppendFormat("Success:{0}\nExitCode:{1}\nOutputString:{2}\nMoreOutputString:\n", Success, ExitCode, OutputString);
                if (MoreOutputString != null)
                    foreach (var v in MoreOutputString)
                        str.AppendFormat("{0}:{1}\n", v.Key, v.Value.Replace("\r", "\\Ⓡ").Replace("\n", "\\Ⓝ"));
                return str.ToString();
            }
        }

        /// <summary>
        /// 过滤并截取字符串
        /// </summary>
        /// <param name="context">要过滤的字符串</param>
        /// <param name="startStr">截取的开始标识</param>
        /// <param name="endStr">截取的结束标识</param>
        /// <param name="isDelete_r_n">是否替换\r\n为空格</param>
        /// <returns></returns>
        public static async Task<string> FilterString(string context, string startStr, string endStr, bool isDelete_r_n = true)
        {
           if (isDelete_r_n)
            {
                context = context.Replace("\r", "").Replace("\n", "|");
            }
            string[] arr = context.Split('|');
            string str = "";
            await Task.Run(() =>
            {
                if (arr != null)
                {
                    try
                    {
                        str = arr[arr.Length - 2];
                    }
                    catch (Exception ex)
                    {
                        return ;
                    }
                }
            });

            return str;

        }
        public static async Task<string> FilterString2(string context, string startStr, string endStr, bool isDelete_r_n = true)
        {
            if (isDelete_r_n)
            {
                context = context.Replace("\r", "").Replace("\n", "|");
            }
            string[] arr = context.Split('|');
            string str = "";
            await Task.Run(() =>
            {
                if (arr != null)
                {
                    try
                    {
                        str = arr[arr.Length - 13];
                    }
                    catch (Exception ex)
                    {
                        return;
                    }
                }
            });

            return str;

        }

    }
}
