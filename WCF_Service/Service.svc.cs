using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Text;
using WCF_Service.DB;


namespace WCF_Service
{
    [System.ServiceModel.ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    public class Service : IService
    {
        public string GetTestString()
        {
            DataSet ds = ExecuteQuery("SELECT TO_CHAR(SYSDATE,'YYYY-MM-DD hh24:mi:ss') DT FROM DUAL");
            if(ds.Tables.Count>0 && ds.Tables[0].Rows.Count > 0 )
            {
                return ds.Tables[0].Rows[0]["DT"].ToString();
            }
            return "No Connection";
        }
       
        public DataSet GetTestDataTable()
        {
            return new DataSet();
        }

        public DataSet ExecuteQuery(string query, Dictionary<string, string> param = null)
        {
            string debID = "";
            try
            {
                debID=DBDebug.WriteLog(DBDebug.StatusEnum.Start, query, param);
                
                DataSet ds = new DataSet();
                ds.Tables.Add(MainDB.Instance.ExcuteQuery(query, param));

                DBDebug.WriteLog(DBDebug.StatusEnum.Complete, query, param, debID);
                return ds;
            }
            catch (Exception eLog)
            {
                System.Diagnostics.Debug.WriteLine(eLog.ToString());
                DBDebug.WriteLog(DBDebug.StatusEnum.Error, eLog.ToString(), param, debID);
                throw eLog;
            }
        }

        public int ExecuteNonQueryObject(string query, Dictionary<string, object> param = null)
        {
            try
            {
                return MainDB.Instance.ExcuteNonQuery(query, param);
            }
            catch (Exception eLog)
            {
                System.Diagnostics.Debug.WriteLine(eLog.ToString());
                throw eLog;
            }
        }

        public int ExecuteNonQuery(string query, Dictionary<string, string> param = null)
        {
            string debID = "";
            try
            {
                debID = DBDebug.WriteLog(DBDebug.StatusEnum.Start, query, param);

                int ret= MainDB.Instance.ExcuteNonQuery(query, param);
                DBDebug.WriteLog(DBDebug.StatusEnum.Complete, query, param, debID);
                return ret;
            }
            catch (Exception eLog)
            {
                System.Diagnostics.Debug.WriteLine(eLog.ToString());
                DBDebug.WriteLog(DBDebug.StatusEnum.Error, eLog.ToString(), param, debID);
                throw eLog;
            }

        }
    }
}
