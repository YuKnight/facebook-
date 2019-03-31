using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xzy.EmbeddedApp.Model
{
    public class RecordStage
    {
        /// <summary>
        /// 编号
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 手机索引
        /// </summary>
        public int PhoneNum { get; set; }

        /// <summary>
        /// 任务类型
        /// </summary>
        public int TypeId { get; set; }

        /// <summary>
        /// 上次操作记录
        /// </summary>
        public int LastNums { get; set; }
    }
}
