using System;

namespace Xzy.EmbeddedApp.Utils
{
    public class CommonUtils
    {



        /// <summary>
        /// 分割CMD命令返回结果
        /// </summary>
        /// <param name="cmdResult">指令返回的结果</param>
        /// <param name="splitFlag">分割标志</param>
        /// <param name="index">目标结果索引</param>
        /// <returns></returns>
        public static string SplitCMDResult(string cmdResult, char[] splitFlag, int index)
        {
            string[] resArr = cmdResult.Split(splitFlag, StringSplitOptions.RemoveEmptyEntries);
            if (index < resArr.Length)
                return resArr[index];
            return "";
        }
    }
}
