using System;
using System.Collections.Generic;

using System.Text;
using System.ComponentModel;
using System.Data;

namespace WCF_Service.DB
{
    public class COMM_DEF
    {
        private const string CN_LOCAL_LOG_DIR = @".\DB_LOG";

        public const string CN_CONFIG_XML_PATH = @".\HE_MES_Config.xml";
        public enum DBOpenEnum
        {
            XML
            ,Args
        }
        public enum DBQueryTypeEnum
        {
            Get
            , Set
        }
        public struct AsyncDBST
        {
            public string query;
            public Dictionary<string, string> param;
            public DataTable dt;
            public DBQueryTypeEnum qt;
            public object sender;
        }
        public static bool WriteTxtLog(string log, string writeDIR = CN_LOCAL_LOG_DIR)
        {
            bool bRet = true;
            try
            {

                if (!System.IO.Directory.Exists(writeDIR))
                {
                    System.IO.Directory.CreateDirectory(writeDIR);
                }
                string tagetFile = writeDIR + "\\" 
                        +System.DateTime.Today.ToString("yyyyMMdd")+ ".txt";

                using (System.IO.StreamWriter sw = new System.IO.StreamWriter(tagetFile, true))
                {
                    sw.WriteLine(DateTime.Now + "\t" + log);
                }


            }
            catch (Exception eLog)
            {
                System.Diagnostics.Debug.WriteLine("[" + System.Reflection.MethodBase.GetCurrentMethod().Name + "]" + eLog.Message);
                bRet = false;
            }

            return bRet;
        }
    }
}
