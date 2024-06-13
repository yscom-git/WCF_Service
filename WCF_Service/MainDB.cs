
using System;
using System.Configuration;
using System.Data;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Policy;

namespace WCF_Service
{
    class MainDB
    {
        static DB.ORACLE_DB _oracleDB;
        static bool m_DB_DEBUG = false;
        public static bool DB_DEBUG
        {
            get { return m_DB_DEBUG; }
        }
        private static bool GetBool(string val)
        {
            bool bRet = false;
            switch(val.ToUpper())
            {
                case "TRUE":
                case "T":
                case "Y":
                case "YES":
                    bRet = true;
                    break;
                    
            }
            return bRet;
        }
        public static DB.ORACLE_DB Instance
        {
            get
            {
                if (_oracleDB == null)
                {
                    _oracleDB = new DB.ORACLE_DB();
                    string svr = ConfigurationManager.AppSettings["DBNAME"].ToString();
                    string id = ConfigurationManager.AppSettings["DBUID"].ToString();
                    string pwd = ConfigurationManager.AppSettings["DBPWD"].ToString();
                    string service = ConfigurationManager.AppSettings["DBSERVICE"].ToString();           

                    m_DB_DEBUG = GetBool(ConfigurationManager.AppSettings["DB_DEBUG"].ToString());
                    if (string.IsNullOrEmpty(svr) ==false)
                    {
                        _oracleDB.Open(svr
                                , id
                                , pwd
                                , service);
                    }
                    else
                    {
                        throw new System.Exception("No Config File for DB");
                    }
                    
                }

                return _oracleDB;
            }
        }
        public static DataTable OpenXML(string path)
        {

            try
            {
                if (File.Exists(path))
                {
                    DataSet ds = new DataSet();
                    ds.ReadXml(path);
                    if (ds.Tables.Count > 0)
                    {
                        for (int row = 0; row < ds.Tables[0].Rows.Count; row++)
                        {
                            for (int col = 0; col < ds.Tables[0].Columns.Count; col++)
                            {
                                ds.Tables[0].Rows[row][col] = ds.Tables[0].Rows[row][col].ToString().Trim();
                            }
                        }
                    }
                    return ds.Tables[0];
                }
                return null;
            }
            catch
            {
                return null;

            }
        }
    }

}
