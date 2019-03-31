using System.Collections.Generic;
using Xzy.EmbeddedApp.Model;
using Cj.AppEmbeddedApp.DAL;

namespace Cj.EmbeddedAPP.BLL
{
    public class TasksBLL
    {
        /// <summary>
        /// 删除任务
        /// </summary>
        /// <returns></returns>
        public static int DeleteTasks(int id)
        {
            int flag = 0;
            TasksDAL dal = new TasksDAL();
            flag = dal.DeleteTasks(id);

            return flag;
        }

        public static int CreateTask(TaskSch tasks)
        {
            int flag = 0;
            TasksDAL dal = new TasksDAL();
            flag = dal.InsertTask(tasks);

            return flag;
        }        

        /// <summary>
        /// 获取执行的任务数
        /// </summary>
        /// <returns></returns>
        public static int CountByStatus(string status,int iswhole)
        {
            TasksDAL dal = new TasksDAL();
            int resnum = dal.CountByStatus(status, iswhole);

            return resnum;
        }

        /// <summary>
        /// 获取每个模拟器待发任务数
        /// </summary>
        /// <param name="status"></param>
        /// <param name="mobileindex"></param>
        /// <returns></returns>
        public static int CountByStatus(string status,int mobileindex, int iswhole)
        {
            TasksDAL dal = new TasksDAL();
            int resnum = dal.CountByStatus(status, mobileindex, iswhole);

            return resnum;
        }
        /// <summary>
        /// 查询模拟器待处理任务数
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public static List<TaskSch> GetTasksForSendGroup(string status = "waiting")
        {
            TasksDAL dal = new TasksDAL();
            List<TaskSch> list = (List<TaskSch>)dal.GetTasksForSendGroup(status);

            return list;
        }

        //根据模拟器获取待执行的任务列表
        public static List<TaskSch> GetTasksList(string status,int mobileindex, int orders, int limit = 10)
        {
            TasksDAL dal = new TasksDAL();
            List<TaskSch> list = (List<TaskSch>)dal.GetTasksForSend(status, mobileindex, orders, limit);

            return list;
        }

        public static List<TaskSch> GetTasksList(string status, int orders, int limit = 100)
        {
            TasksDAL dal = new TasksDAL();
            List<TaskSch> list = (List<TaskSch>)dal.GetTasksForSend(status, orders, limit);

            return list;
        }

        /// <summary>
        /// 变更任务状态
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public static int UpdateTaskStatus(int id, int status)
        {
            TasksDAL dal = new TasksDAL();
            int res = dal.UpdateTaskStatus(id, status);
            return res;
        }

        public static int UpdateTaskRes(int id ,string resval)
        {
            TasksDAL dal = new TasksDAL();
            int res = dal.UpdateTaskResult(id, resval);
            return res;
        }
    }

}
