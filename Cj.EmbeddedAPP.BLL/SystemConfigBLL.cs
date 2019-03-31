using Cj.AppEmbeddedApp.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xzy.EmbeddedApp.Model;

namespace Cj.EmbeddedAPP.BLL
{
   public class SystemConfigBLL
    {
        private static SystemConfigDAL objSystemConfigDAL = new SystemConfigDAL();
        /// <summary>
        /// 添加配置
        /// </summary>
        /// <param name="objSystemConfig"></param>
        /// <returns></returns>
        public int AddSystemConfig(SystemConfig objSystemConfig)
        {
            return objSystemConfigDAL.Add(objSystemConfig);
        }

        /// <summary>
        /// 查询配置 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="uid"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public List<SystemConfig> GetSystemConfigList(int uid)
        {
            return objSystemConfigDAL.GetSystemConfigList(uid);
        }

        /// <summary>
        /// 删除配置
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int DelSystemConfig(int id)
        {
            return objSystemConfigDAL.DelSystemConfig(id);
        }

        /// <summary>
        /// 更新配置
        /// </summary>
        /// <param name="objSystemConfig"></param>
        /// <returns></returns>
        public int UpdateSystemConfig(SystemConfig objSystemConfig)
        {
            return objSystemConfigDAL.UpdateSystemConfig(objSystemConfig);
        }
    }
}
