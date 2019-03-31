using System.Collections.Generic;
using Xzy.EmbeddedApp.Utils;
using System.Threading;

namespace Wx.Qunkong360.Wpf.Utils
{
    public class DeviceOperation
    {
        private List<string> cmdList = new List<string>();



        public DeviceOperation SetCmd(string cmd)
        {
            cmdList.Add(cmd);
            return this;
        }

        ///<summary>
        ///执行文本命令
        ///</summary>
        public void RunText(string conAdbCmd, string path)
        {
            Thread t = new Thread(new ParameterizedThreadStart(CmdUtils.RunCmd2));
            //CmdUtils.RunCmd(conAdbCmd + "<" + path);
            t.Start(conAdbCmd + "<" + path);
        }

        /// <summary>
        /// 最后统一执行所有命令
        /// </summary>
        public void Run()
        {

            CmdUtils.RunCmdListSync1(cmdList);

        }

        /// <summary>
        /// 最后统一执行所有命令
        /// </summary>
        public void Run2(int id,out int number)
        {

            CmdUtils.RunCmdListSync12(cmdList,id,out number);
        }

        public List<string> Run(bool isReturn)
        {
            CmdUtils.RunCmdListSync(cmdList, isReturn);
            return null;
        }
    }
}
