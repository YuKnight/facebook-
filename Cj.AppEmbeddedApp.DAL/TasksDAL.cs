using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using Xzy.EmbeddedApp.Model;
using Xzy.EmbeddedApp.Utils;

namespace Cj.AppEmbeddedApp.DAL
{
    public class TasksDAL
    {
        private const string strSql_insert_tasksign = @"insert into tasks (typeid,remotes,mobileindex,bodys,status,slidenums,repeatnums,friendnums,randommins,randommaxs,resultval,isstep,iswhole,
                                                        inputname,created) 
                                                        values (@typeid,@remotes,@mobileindex,@bodys,@status,@slidenums,@repeatnums,@friendnums,@randommins,@randommaxs, @resultval,@isstep,@iswhole,
                                                        @inputname,unix_timestamp())";

        private const string strSql_select_tasks_asc = @"select id,typeid,remotes,mobileindex,bodys,status,resultval,slidenums,repeatnums,friendnums,randommins,randommaxs,inputname,isstep,iswhole from tasks where (`status`=@status or @status='-1') order by id desc limit @limit";
        private const string strSql_select_tasks_desc = @"select id,typeid,remotes,mobileindex,bodys,status,resultval,slidenums,repeatnums,friendnums,randommins,randommaxs,inputname,isstep,iswhole from tasks where (`status`=@status or @status='-1') order by id desc limit @limit";
        private const string strSql_select_mobileindex_tasks_asc = @"select id,typeid,remotes,mobileindex,bodys,status,resultval,slidenums,repeatnums,friendnums,randommins,randommaxs,
                                                                    inputname,isstep,iswhole from tasks where (`status`=@status or @status='-1') and mobileindex=@mobileindex
                                                                    order by id desc limit @limit";


        private const string strSql_select_mobileindex_grouptasks = @"select mobileindex,count(*) as nums from tasks where `status`=@status group by mobileindex";

        private const string strSql_count_tasknums = @"select count(*) as countnums from tasks where `status`=@status and iswhole=@iswhole";
        private const string strSql_count_tasknums_mobileindex = @"select count(*) as countnums from tasks where `status`=@status and mobileindex=@mobileindex and iswhole=@iswhole";

        private const string strSql_update_updatetaskstatus = @"update tasks set `status`=@status where id=@id";

        private const string strSql_update_task_status_by_remotes = @"update tasks set `status`=@status where remotes=@remotes";

        private const string strSql_update_updatetaskres = @"update tasks set resultval=@resultval where id=@id";

        private const string strSql_delete_tasks = @"delete from tasks where id=@id or @id=-1";

        /// <summary>
        /// 删除任务 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int DeleteTasks(int id)
        {
            int flag = 0;
            MySqlParameter[] par = new MySqlParameter[1];
            par[0] = new MySqlParameter("@id", MySqlDbType.Int32);
            par[0].Value = id;

            try
            {
                flag = MySqlHelpers.ExecuteNonQuery(MySqlHelpers.ConnectionString, CommandType.Text,strSql_delete_tasks, par);
            }
            catch (Exception ex)
            {
                System.Diagnostics.EventLog.WriteEntry("WhatsApp", ex.ToString(), System.Diagnostics.EventLogEntryType.Error);
            }
            return flag;

        }

        /// <summary>
        /// 插入任务
        /// </summary>
        /// <param name="tasks"></param>
        /// <returns></returns>
        public int InsertTask (TaskSch tasks)
        {
            int flag = 0;
            MySqlParameter[] par = new MySqlParameter[14];
            par[0] = new MySqlParameter("@typeid", MySqlDbType.Int32);
            par[0].Value = tasks.TypeId;

            par[1] = new MySqlParameter("@remotes", MySqlDbType.Text);
            par[1].Value = tasks.Remotes;

            par[2] = new MySqlParameter("@mobileindex", MySqlDbType.Int32);
            par[2].Value = tasks.MobileIndex;

            par[3] = new MySqlParameter("@bodys", MySqlDbType.Text);
            par[3].Value = tasks.Bodys;

            par[4] = new MySqlParameter("@status", MySqlDbType.Text);
            par[4].Value = tasks.Status;

            par[5] = new MySqlParameter("@slidenums", MySqlDbType.Int32);
            par[5].Value = tasks.SlideNums;

            par[6] = new MySqlParameter("@repeatnums", MySqlDbType.Int32);
            par[6].Value = tasks.RepeatNums;

            par[7] = new MySqlParameter("@friendnums", MySqlDbType.Int32);
            par[7].Value = tasks.FriendNums;

            par[8] = new MySqlParameter("@randommins", MySqlDbType.Int32);
            par[8].Value = tasks.RandomMins;
            
            par[9] = new MySqlParameter("@randommaxs", MySqlDbType.Int32);
            par[9].Value = tasks.RandomMaxs;

            par[10] = new MySqlParameter("@resultval", MySqlDbType.Text);
            par[10].Value = tasks.ResultVal;

            par[11] = new MySqlParameter("@isstep", MySqlDbType.Int32);
            par[11].Value = tasks.IsStep;

            par[12] = new MySqlParameter("@iswhole", MySqlDbType.Int32);
            par[12].Value = tasks.IsWhole;

            par[13] = new MySqlParameter("@inputname", MySqlDbType.Text);
            par[13].Value = tasks.InputName;

            try
            {
                flag = MySqlHelpers.ExecuteNonQuery(MySqlHelpers.ConnectionString, CommandType.Text
                    , strSql_insert_tasksign, par);
            }
            catch (Exception ex)
            {
                LogUtils.Error($"{ex}");
                System.Diagnostics.EventLog.WriteEntry("WhatsApp", ex.ToString(), System.Diagnostics.EventLogEntryType.Error);
            }

            return flag;
        }
                
        /// <summary>
        /// 查询每个模拟器待处理任务数
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public IList<TaskSch> GetTasksForSendGroup(string status)
        {
            List<TaskSch> list = new List<TaskSch>();
            MySqlParameter[] par = new MySqlParameter[1];
            par[0] = new MySqlParameter("@status", MySqlDbType.Text);
            par[0].Value = status;

            using (MySqlDataReader dr = MySqlHelpers.ExecuteReader(MySqlHelpers.ConnectionString, CommandType.Text, strSql_select_mobileindex_grouptasks, par))
            {
                while (dr.Read())
                {
                    TaskSch task = new TaskSch();
                    task.MobileIndex = Int32.Parse(dr["mobileindex"].ToString());
                    task.Created = Int32.Parse(dr["nums"].ToString());

                    list.Add(task);
                }
            }

            return list;
        }

        /// <summary>
        /// 获取指定模拟器的任务列表
        /// </summary>
        /// <param name="status"></param>
        /// <param name="mobileindex"></param>
        /// <param name="orders"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public IList<TaskSch> GetTasksForSend(string status,int mobileindex, int orders, int limit = 10)
        {
            List<TaskSch> list = new List<TaskSch>(); //
            MySqlParameter[] par = new MySqlParameter[3];
            par[0] = new MySqlParameter("@status", MySqlDbType.Text);
            par[0].Value = status;

            par[1] = new MySqlParameter("@mobileindex", MySqlDbType.Int32);
            par[1].Value = mobileindex;

            par[2] = new MySqlParameter("@limit", MySqlDbType.Int32);
            par[2].Value = limit;

            using (MySqlDataReader dr = MySqlHelpers.ExecuteReader(MySqlHelpers.ConnectionString, CommandType.Text, strSql_select_mobileindex_tasks_asc, par))
            {
                while (dr.Read())
                {
                    TaskSch task = new TaskSch();
                    task.Id = Int32.Parse(dr["id"].ToString());
                    task.TypeId = Int32.Parse(dr["typeid"].ToString());
                    task.Remotes = dr["remotes"].ToString();
                    task.MobileIndex = Int32.Parse(dr["mobileindex"].ToString());
                    task.Bodys = dr["bodys"].ToString();
                    task.Status = dr["status"].ToString();
                    task.ResultVal = dr["resultval"].ToString();
                    task.SlideNums = Int32.Parse(dr["slidenums"].ToString());
                    task.FriendNums = Int32.Parse(dr["friendnums"].ToString());
                    task.RepeatNums = Int32.Parse(dr["repeatnums"].ToString());
                    task.RandomMins = Int32.Parse(dr["randommins"].ToString());
                    task.RandomMaxs = Int32.Parse(dr["randommaxs"].ToString());
                    task.IsStep = Int32.Parse(dr["isstep"].ToString());
                    task.IsWhole= Int32.Parse(dr["iswhole"].ToString());
                    task.InputName = dr["inputname"].ToString();

                    list.Add(task);
                }
            }

            return list;
        }

        /// <summary>
        /// 获取任务列表
        /// </summary>
        /// <param name="limit"></param>
        /// <returns></returns>
        public IList<TaskSch> GetTasksForSend(string status, int orders, int limit=100)
        {
            List<TaskSch> list = new List<TaskSch>();

            string sql_select = strSql_select_tasks_asc;
            if (orders == 2)
            {
                sql_select = strSql_select_tasks_desc;
            }

            MySqlParameter[] par = new MySqlParameter[2];
            par[0] = new MySqlParameter("@status", MySqlDbType.Text);
            par[0].Value = status;

            par[1] = new MySqlParameter("@limit", MySqlDbType.Int32);
            par[1].Value = limit;

            using (MySqlDataReader dr = MySqlHelpers.ExecuteReader(MySqlHelpers.ConnectionString, CommandType.Text, sql_select, par))
            {
                while(dr.Read())
                {
                    TaskSch task = new TaskSch();
                    task.Id = Int32.Parse(dr["id"].ToString());
                    task.TypeId = Int32.Parse(dr["typeid"].ToString());
                    task.Remotes = dr["remotes"].ToString();
                    task.MobileIndex = Int32.Parse(dr["mobileindex"].ToString());
                    task.Bodys = dr["bodys"].ToString();
                    task.Status = dr["status"].ToString();
                    task.ResultVal = dr["resultval"].ToString();
                    task.SlideNums = Int32.Parse(dr["slidenums"].ToString());
                    task.FriendNums = Int32.Parse(dr["friendnums"].ToString());
                    task.RepeatNums = Int32.Parse(dr["repeatnums"].ToString());
                    task.RandomMins = Int32.Parse(dr["randommins"].ToString());
                    task.RandomMaxs = Int32.Parse(dr["randommaxs"].ToString());
                    task.IsStep = Int32.Parse(dr["isstep"].ToString());
                    task.IsWhole = Int32.Parse(dr["iswhole"].ToString());
                    task.InputName = dr["inputname"].ToString();

                    list.Add(task);
                }
            }

            return list;
        }

        /// <summary>
        /// 统计任务数
        /// </summary>
        /// <returns></returns>
        public int CountByStatus(string status,int iswhole)
        {
            int resnum = 0;
            MySqlParameter[] par = new MySqlParameter[2];
            par[0] = new MySqlParameter("@status", MySqlDbType.Text);
            par[0].Value = status;

            par[1] = new MySqlParameter("@iswhole", MySqlDbType.Int32);
            par[1].Value = iswhole;

            using (MySqlDataReader dr = MySqlHelpers.ExecuteReader(MySqlHelpers.ConnectionString, CommandType.Text, strSql_count_tasknums, par))
            {
                if (dr.Read())
                {
                    resnum = Int32.Parse(dr["countnums"].ToString());
                }
            }

            return resnum;
        }

        /// <summary>
        /// 统计每个模拟器的任务数
        /// </summary>
        /// <param name="status"></param>
        /// <param name="mobileIndex"></param>
        /// <returns></returns>
        public int CountByStatus(string status,int mobileIndex, int iswhole)
        {
            int resnum = 0;
            MySqlParameter[] par = new MySqlParameter[3];
            par[0] = new MySqlParameter("@status", MySqlDbType.Text);
            par[0].Value = status;

            par[1] = new MySqlParameter("@mobileindex", MySqlDbType.Int32);
            par[1].Value = mobileIndex;

            par[2] = new MySqlParameter("@iswhole", MySqlDbType.Int32);
            par[2].Value = iswhole;

            using (MySqlDataReader dr = MySqlHelpers.ExecuteReader(MySqlHelpers.ConnectionString, CommandType.Text, strSql_count_tasknums_mobileindex, par))
            {
                if (dr.Read())
                {
                    resnum = Int32.Parse(dr["countnums"].ToString());
                }
            }

            return resnum;
        }

        /// <summary>
        /// 变更任务状态
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public int UpdateTaskStatus(int id,int status)
        {
            
            int nflag = 0;
            string statusval = "";
            switch(status)
            {
                case 1:
                    statusval = "submit ";
                    break;
                case -1:
                    statusval = "fail";
                    break;
                case 0:
                    statusval = "waiting";
                    break;
                default:
                    statusval = "fail";
                    break;
            }
            MySqlParameter[] par = new MySqlParameter[2];
            par[0] = new MySqlParameter("@id", MySqlDbType.Int32);
            par[0].Value = id;

            par[1] = new MySqlParameter("@status", MySqlDbType.Text);
            par[1].Value = statusval;

            nflag = MySqlHelpers.ExecuteNonQuery(MySqlHelpers.ConnectionString, CommandType.Text, strSql_update_updatetaskstatus, par);

            return nflag;    
        }

        public static int UpdateTaskStatusByRemotes(string remotes, int status)
        {
            int nflag = 0;
            string statusval = "";
            switch (status)
            {
                case 1:
                    statusval = "submit ";
                    break;
                case -1:
                    statusval = "fail";
                    break;
                case 0:
                    statusval = "waiting";
                    break;
                default:
                    statusval = "fail";
                    break;
            }
            MySqlParameter[] par = new MySqlParameter[2];
            par[0] = new MySqlParameter("@remotes", MySqlDbType.Text);
            par[0].Value = remotes;

            par[1] = new MySqlParameter("@status", MySqlDbType.Text);
            par[1].Value = statusval;

            nflag = MySqlHelpers.ExecuteNonQuery(MySqlHelpers.ConnectionString, CommandType.Text, strSql_update_task_status_by_remotes, par);

            return nflag;

        }

        /// <summary>
        /// 更新任务的执行结果
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public int UpdateTaskResult(int id, string status)
        {

            int nflag = 0;            
            MySqlParameter[] par = new MySqlParameter[2];
            par[0] = new MySqlParameter("@id", MySqlDbType.Int32);
            par[0].Value = id;

            par[1] = new MySqlParameter("@resultval", MySqlDbType.Text);
            par[1].Value = status;

            nflag = MySqlHelpers.ExecuteNonQuery(MySqlHelpers.ConnectionString, CommandType.Text, strSql_update_updatetaskres, par);

            return nflag;
        }
    }
}
