using Cj.AppEmbeddedApp.DAL;
using System.Collections.Generic;
using Xzy.EmbeddedApp.Model;


namespace Cj.EmbeddedAPP.BLL
{
    public class AdminsBLL
    {
        private static AdminsDAL objAdminDAL = new AdminsDAL();
        /// <summary>
        /// 添加用户
        /// </summary>
        /// <param name="objadmins"></param>
        /// <returns></returns>
        public int AddAdmins(admins objadmins)
        {
            return objAdminDAL.AddAdmins(objadmins);
        } 

        /// <summary>
        /// 删除用户
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public int DelAdmins()
        {
            return objAdminDAL.DelAdmins();
        }

        /// <summary>
        /// 查询用户
        /// </summary>
        /// <param name="objadmin"></param>
        /// <returns></returns>
        public List<admins> GetAdmins()
        {
            return objAdminDAL.GetAdmins();
        }
    }
}
