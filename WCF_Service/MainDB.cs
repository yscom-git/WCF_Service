
using System.Data;
using System.IO;

namespace WCF_Service
{
    class MainDB
    {
        static DB.ORACLE_DB _oracleDB;

        public static DB.ORACLE_DB Instance
        {
            get
            {
                if (_oracleDB == null)
                {
                    _oracleDB = new DB.ORACLE_DB();
                    DataTable dt = OpenXML(".\\HE_MES_Config.xml");
                    if(dt.Rows.Count>0)
                    {
                        _oracleDB.Open(dt.Rows[0]["DBNAME"].ToString()
                                , dt.Rows[0]["DBUID"].ToString()
                                , dt.Rows[0]["DBPWD"].ToString()
                                , dt.Rows[0]["DBSERVICE"].ToString());
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
