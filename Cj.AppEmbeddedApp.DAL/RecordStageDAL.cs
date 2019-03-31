using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using Xzy.EmbeddedApp.Model;
using Xzy.EmbeddedApp.Utils;

namespace Cj.AppEmbeddedApp.DAL
{
    public class RecordStageDAL
    {
        private const string strSql_select_recordbymobile = @"select * from recordstage where phonenum=@phonenum and typeid=@typeid";
        private const string strSql_insert_record = @"insert into recordstage (phonenum,typeid,lastnums) values (@phonenum,@typeid,@lastnums)";

        public IList<RecordStage> GetRecordstages(int mobileindex,int typeid)
        {
            List<RecordStage> list = new List<RecordStage>();
            MySqlParameter[] par = new MySqlParameter[2];
            par[0] = new MySqlParameter("@phonenum", MySqlDbType.Int32);
            par[0].Value = mobileindex;

            par[1] = new MySqlParameter("@typeid", MySqlDbType.Int32);
            par[1].Value = typeid;

            using (MySqlDataReader dr = MySqlHelpers.ExecuteReader(MySqlHelpers.ConnectionString, CommandType.Text, strSql_select_recordbymobile, par))
            {
                while (dr.Read())
                {
                    RecordStage record = new RecordStage();
                    record.Id = Int32.Parse(dr["id"].ToString());
                    record.PhoneNum = Int32.Parse(dr["phonenum"].ToString());
                    record.TypeId = Int32.Parse(dr["typeid"].ToString());
                    record.LastNums = Int32.Parse(dr["lastnums"].ToString());

                    list.Add(record);
                }
            }
            return list;
        }

        public int InsertRecord(RecordStage record)
        {
            int flag = 0;
            MySqlParameter[] par = new MySqlParameter[3];
            par[0] = new MySqlParameter("@phonenum", MySqlDbType.Int32);
            par[0].Value = record.PhoneNum;

            par[1] = new MySqlParameter("@typeid", MySqlDbType.Int32);
            par[1].Value = record.TypeId;

            par[2] = new MySqlParameter("@lastnums", MySqlDbType.Int32);
            par[2].Value = record.LastNums;

            try
            {
                flag = MySqlHelpers.ExecuteNonQuery(MySqlHelpers.ConnectionString, CommandType.Text
                    , strSql_insert_record, par);
            }
            catch (Exception ex)
            {
                LogUtils.Error($"{ex}");
                System.Diagnostics.EventLog.WriteEntry("Facebook", ex.ToString(), System.Diagnostics.EventLogEntryType.Error);
            }

            return flag;
        }
    }
}
