using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.NetworkInformation;
using System.Web;
using System.Web.UI.WebControls;

namespace WCF_Service
{
    class DBDebug
    {
        public enum StatusEnum
        {
            Start,
            Processing,
            Error,
            Complete

        }
        private static string GetClientIP()
        {
            var clientIpAddress = System.Web.HttpContext.Current?.Request?.UserHostAddress;
            return clientIpAddress;
        }
        private static string GetParam(Dictionary<string, string> parameters)
        {
            string vals = "";
            foreach (KeyValuePair<string, string> pair in parameters)
            {
                vals += "["+pair.Key+":"+pair.Value+"]";
            }
            return vals;
        }
        public static string WriteLog(StatusEnum status, string query, Dictionary<string, string> param, string seqID="")
        {
            try
            {
                if(MainDB.DB_DEBUG == false)
                {
                    return "";
                }
                string who = GetClientIP();
                string paramStr = GetParam(param);
                if (status == StatusEnum.Start)
                {
                    string seq = "";
                    DataTable dt = MainDB.Instance.ExcuteQuery("MES.PKG_WCF_DEBUG.GET_NEXT_SEQ", null);
                    if(dt.Rows.Count>0)
                    {
                        seq = dt.Rows[0]["SEQID"].ToString();
                        
                        Dictionary<string, string> dbParam = new Dictionary<string, string>();
                        dbParam.Add("IN_SEQID", seq);
                        dbParam.Add("IN_WHO", who);
                        dbParam.Add("IN_QUERY", query);
                        dbParam.Add("IN_PARAM", paramStr);
                        dbParam.Add("IN_STATUS", "S");
                        MainDB.Instance.ExcuteNonQuery("MES.PKG_WCF_DEBUG.SET_LOG", dbParam);

                    }
                    return seq;
                }
                else if(status == StatusEnum.Complete && string.IsNullOrEmpty(seqID) == false)
                {
                    Dictionary<string, string> dbParam = new Dictionary<string, string>();
                    dbParam.Add("IN_SEQID", seqID);
                    dbParam.Add("IN_WHO", who);
                    dbParam.Add("IN_QUERY", query);
                    dbParam.Add("IN_PARAM", paramStr);
                    dbParam.Add("IN_STATUS", "C");
                    MainDB.Instance.ExcuteNonQuery("MES.PKG_WCF_DEBUG.SET_LOG", dbParam);
                }
                return "";
                
            }
            catch(Exception eLog)
            {

            }
            return "";
        }
        

    }
}