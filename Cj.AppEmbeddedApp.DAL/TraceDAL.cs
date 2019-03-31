using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using Xzy.EmbeddedApp.Model;
using Xzy.EmbeddedApp.Utils;

namespace Cj.AppEmbeddedApp.DAL
{
    public static class TraceDAL
    {
        private const string insert_trace_sql = "insert into trace (mobileindex, typeid, position) values (@mobileindex, @typeid, @position)";
        private const string select_trace_sql = "select mobileindex, typeid, position from trace where mobileindex=@mobileindex and typeid=@typeid";
        private const string get_trace_position_sql = "select position from trace where mobileindex=@mobileindex and typeid=@typeid";
        private const string update_trace_sql = "update trace set `position`=@position where mobileindex=@mobileindex and typeid=@typeid";
        private const string count_trace_sql = "select count(*) as countnums from trace where mobileindex=@mobileindex and typeid=@typeid";

        public static int InsertTaskTrace(TaskTrace taskTrace)
        {
            int insertResult = 0;

            MySqlParameter[] parameters = new MySqlParameter[3];

            parameters[0] = new MySqlParameter("@mobileindex", MySqlDbType.Int32);
            parameters[0].Value = taskTrace.MobileIndex;

            parameters[1] = new MySqlParameter("@typeid", MySqlDbType.Int32);
            parameters[1].Value = taskTrace.TypeId;

            parameters[2] = new MySqlParameter("@position", MySqlDbType.Int32);
            parameters[2].Value = taskTrace.Position;

            try
            {
                insertResult = MySqlHelpers.ExecuteNonQuery(MySqlHelpers.ConnectionString, CommandType.Text, insert_trace_sql, parameters);
            }
            catch (Exception ex)
            {
                Console.WriteLine("InsertTaskTrace EXCEPTION");

                LogUtils.Error($"{ex}");

            }

            return insertResult;
        }

        public static int CountTaskTrace(TaskTrace taskTrace)
        {
            int countResult = 0;

            MySqlParameter[] parameters = new MySqlParameter[2];

            parameters[0] = new MySqlParameter("@mobileindex", MySqlDbType.Int32);
            parameters[0].Value = taskTrace.MobileIndex;

            parameters[1] = new MySqlParameter("@typeid", MySqlDbType.Int32);
            parameters[1].Value = taskTrace.TypeId;

            try
            {
                countResult =int.Parse(MySqlHelpers.ExecuteScalar(MySqlHelpers.ConnectionString, CommandType.Text, count_trace_sql, parameters).ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine("CountTaskTrace EXCEPTION");

                LogUtils.Error($"{ex}");

            }

            return countResult;
        }

        public static int UpdateTaskTrace(TaskTrace taskTrace)
        {
            int updateResult = 0;

            MySqlParameter[] parameters = new MySqlParameter[3];

            parameters[0] = new MySqlParameter("@mobileindex", MySqlDbType.Int32);
            parameters[0].Value = taskTrace.MobileIndex;

            parameters[1] = new MySqlParameter("@typeid", MySqlDbType.Int32);
            parameters[1].Value = taskTrace.TypeId;

            parameters[2] = new MySqlParameter("@position", MySqlDbType.Int32);
            parameters[2].Value = taskTrace.Position;

            try
            {
                updateResult = MySqlHelpers.ExecuteNonQuery(MySqlHelpers.ConnectionString, CommandType.Text, update_trace_sql, parameters);
            }
            catch (Exception ex)
            {
                Console.WriteLine("UpdateTaskTrace EXCEPTION");
                LogUtils.Error($"{ex}");

            }

            return updateResult;

        }

        public static int GetTaskTracePosition(TaskTrace taskTrace)
        {
            int position = 0;

            MySqlParameter[] parameters = new MySqlParameter[2];

            parameters[0] = new MySqlParameter("@mobileindex", MySqlDbType.Int32);
            parameters[0].Value = taskTrace.MobileIndex;

            parameters[1] = new MySqlParameter("@typeid", MySqlDbType.Int32);
            parameters[1].Value = taskTrace.TypeId;

            try
            {
                position = (int)MySqlHelpers.ExecuteScalar(MySqlHelpers.ConnectionString, CommandType.Text, get_trace_position_sql, parameters);
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetTaskTracePosition EXCEPTION");

                LogUtils.Error($"{ex}");

            }

            return position;

        }
    }
}
