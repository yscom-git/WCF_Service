using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;


namespace WCF_Service
{
    [System.ServiceModel.ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    class Service : IService
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
            try
            {
                DataSet ds = new DataSet();
                ds.Tables.Add(MainDB.Instance.ExcuteQuery(query, param));
                return ds;
            }
            catch (Exception eLog)
            {
                System.Diagnostics.Debug.WriteLine(eLog.ToString());
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
    }
}
