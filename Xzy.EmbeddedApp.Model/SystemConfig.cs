using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xzy.EmbeddedApp.Model
{
    public class SystemConfig
    {
        /// <summary>
        /// 序号
        /// </summary>
        public int id { get; set; }
        /// <summary>
        /// 模拟器id
        /// </summary>
        public int uid { get; set; }
        /// <summary>
        /// Facebook 启动状态，1表示已启动，0表示未启动
        /// </summary>
        public int state { get; set; }
    }
}
