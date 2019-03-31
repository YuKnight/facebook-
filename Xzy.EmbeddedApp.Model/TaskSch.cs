namespace Xzy.EmbeddedApp.Model
{
    public class TaskSch
    {
        /// <summary>
        /// 编号
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 任务类型
        /// </summary>
        public int TypeId { get; set; }

        public string TypeDescripton { get; set; }


        /// <summary>
        /// 客户端标识
        /// </summary>
        public string Remotes { get; set; }

        /// <summary>
        /// 手机编号
        /// </summary>
        public int MobileIndex { get; set; }

        /// <summary>
        /// 参数主体json
        /// </summary>
        public string Bodys { get; set; }

        /// <summary>
        /// 执行状态
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// 执行结果
        /// </summary>
        public string ResultVal { get; set; }

        /// <summary>
        /// 滑动次数
        /// </summary>
        public int SlideNums { get; set; }

        /// <summary>
        /// 任务重复数
        /// </summary>
        public int RepeatNums { get; set; }

        /// <summary>
        /// 好友数量
        /// </summary>
        public int FriendNums { get; set; }

        /// <summary>
        /// 随机数-Min
        /// </summary>
        public int RandomMins { get; set; }

        /// <summary>
        /// 随机数-Max
        /// </summary>
        public int RandomMaxs { get; set; }

        /// <summary>
        /// 是否分步处理
        /// </summary>
        public int IsStep { get; set; }


        /// <summary>
        /// 是否全局任务
        /// </summary>
        public int IsWhole { get; set; }

        /// <summary>
        /// 输入内容
        /// </summary>
        public string InputName { get; set; }
        
        /// <summary>
        /// 创建时间
        /// </summary>
        public int Created { get; set; }
    }
}
