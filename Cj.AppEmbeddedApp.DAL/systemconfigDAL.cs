using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xzy.EmbeddedApp.Model;
using Xzy.EmbeddedApp.Utils;

namespace Cj.AppEmbeddedApp.DAL
{
    public  class SystemConfigDAL
    {
        /// <summary>
        /// 添加配置
        /// </summary>
        /// <param name="objSimulators"></param>
        /// <returns></returns>
        public int Add(SystemConfig objSystemConfig)
        {
           string sql = "insert into systemconfig (uid,state)";
            sql += " VALUES (@uid,@state)";

            MySqlParameter[] param = new MySqlParameter[]
            {
                new MySqlParameter("@uid",objSystemConfig.uid),
                new MySqlParameter("@state",objSystemConfig.state)
            };
            return MySqlHelpers.Update(sql.ToString(), param);
        }

        /// <summary>
        /// 查询配置
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public List<SystemConfig> GetSystemConfigList(int uid)
        {
            string sql = "select uid,state from systemconfig where uid="+uid;
            MySqlDataReader objReader = MySqlHelpers.GetReader(sql);
            List<SystemConfig> list = new List<SystemConfig>();
            while (objReader.Read())
            {
                list.Add(new SystemConfig()
                {
                    uid = Convert.ToInt32(objReader["uid"]),
                    state = Convert.ToInt32(objReader["state"])
                });
            };
            objReader.Close();
            return list;
        }

        /// <summary>
        /// 删除配置 
        /// </summary>
        /// <returns></returns>
        public int DelSystemConfig(int id)
        {
            int flag = 0;
            string sql = "delete from systemconfig";
            sql += " where id=@id";
            try
            {
                 flag = MySqlHelpers.UpdateSql(sql);
            }
            catch (Exception ex)
            {
                LogUtils.Error($"{ex}");
                System.Diagnostics.EventLog.WriteEntry(" ", ex.ToString(), System.Diagnostics.EventLogEntryType.Error);
            }
            return flag;
        }

        /// <summary>
        /// 修改配置
        /// </summary>
        /// <param name="objSystemConfig"></param>
        /// <returns></returns>
        public int UpdateSystemConfig(SystemConfig objSystemConfig)
        {
            int flag = 0;
            string sql = "update systemconfig set state=@state where id=@id";
            MySqlParameter[] param = new MySqlParameter[]
            {
                new MySqlParameter("@state",objSystemConfig.state),
                new MySqlParameter("@id",objSystemConfig.id)
            };
            try
            {
                flag = MySqlHelpers.Update(sql, param);
            }
            catch (Exception ex)
            {
                LogUtils.Error($"{ex}");
                System.Diagnostics.EventLog.WriteEntry(" ", ex.ToString(), System.Diagnostics.EventLogEntryType.Error);
            }
            return flag;
        }

    }
}
