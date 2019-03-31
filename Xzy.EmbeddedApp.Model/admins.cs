using System;

namespace Xzy.EmbeddedApp.Model
{
    public class admins
    {
        /// <summary>
        /// 编号
        /// </summary>
        public int id { get; set; }
        /// <summary>
        /// 账号
        /// </summary>
        public string login_user { get; set; }
        /// <summary>
        /// 密码
        /// </summary>
        public string login_pwd { get; set; }
        /// <summary>
        /// 说明
        /// </summary>
        public string explain_detail { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime addtime { get; set; }
        /// <summary>
        /// 点击登录按钮匹配指定模拟器
        /// </summary>
        public int LoginDevicesIP { get; set; }

    }
}
