using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using Xzy.EmbeddedApp.Model;
using Xzy.EmbeddedApp.Utils;

namespace Cj.AppEmbeddedApp.DAL
{
    public class AdminsDAL
    {
        /// <summary>
        /// 删除账号
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public int DelAdmins()
        {
            int flag = 0;
            string sql = "delete from admins";
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
        /// 保存账号
        /// </summary>
        /// <param name="objadmins"></param>
        /// <returns></returns>
        public int AddAdmins(admins objadmins)
        {
            int flag = 0;
            string sql = " insert into admins (login_user,login_pwd,explain_detail,addtime)";
            sql += " VALUES (@login_user,@login_pwd,@explain_detail,@addtime)";

            MySqlParameter[] param = new MySqlParameter[]
            {
                new MySqlParameter("@login_user",objadmins.login_user),
                new MySqlParameter("@login_pwd",objadmins.login_pwd),
                new MySqlParameter("@explain_detail",objadmins.explain_detail),
                new MySqlParameter("@addtime",objadmins.addtime)
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
            return flag ;
        }

        /// <summary>
        /// 查询用户
        /// </summary>
        /// <param name="objadmin"></param>
        /// <returns></returns>
        public List<admins> GetAdmins()
        {
            string sql = "select id,login_user,login_pwd,explain_detail,addtime from admins where to_days(addtime) = to_days(now())";
            MySqlDataReader objReader = MySqlHelpers.GetReader(sql);
            List<admins> list = new List<admins>();
            while (objReader.Read())
            {
                list.Add(new admins()
                {
                    id=Convert.ToInt32(objReader["Id"]),
                    login_user=objReader["login_user"].ToString(),
                    login_pwd=objReader["login_pwd"].ToString(),
                    explain_detail=objReader["explain_detail"].ToString(),
                    addtime = Convert.ToDateTime(objReader["addtime"])
                });
            };
            objReader.Close();
            return list;
        }
    }
}
