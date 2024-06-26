﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;

using System.Text;
using System.Data;
using System.IO;
using Oracle.ManagedDataAccess.Client;
using System.Threading;
using System.Collections;
using System.Reflection;


namespace WCF_Service.DB
{
    public partial class ORACLE_DB : Component, IDBBase
    {

        public event BackgroundRCV OnBackgroundRCV = null;

        public event BackgroundPR OnBackgroundPR = null;
        private const string CN_ORA_PORTABLE_PATH = @".\Oracle_Portable\";
        private string m_OutRefCurString = "OUT_CURSOR";
        private string m_DBSVR = "";
        private string m_DBUID = "";
        private string m_DBPWD = "";
        private string m_DBNM = "";
        private bool m_IsOciConnect = true;
        private COMM_DEF.DBOpenEnum m_DBOpenTY = COMM_DEF.DBOpenEnum.XML;

        private bool m_IsDBTrace = false;

        public bool IsDBTrace
        {
            get { return m_IsDBTrace; }
            set { m_IsDBTrace = value; }
        }
        

        public COMM_DEF.DBOpenEnum DBOpenTY
        {
            get
            {
                return m_DBOpenTY;
            }
            set
            {
                m_DBOpenTY = value;
            }
        }
        
        public bool IsAsynBusy
        {
            get { return m_backWorker == null ? false : m_backWorker.IsBusy; }
        }
       
        public bool AsynBusy(object key)
        {
            if(key == null)
            {
                return IsAsynBusy;
            }
            if(GetDicWorker(key)== null)
            {
                return false;
            }
            
            return GetDicWorker(key).IsBusy;
        }
        private BackgroundWorker GetDicWorker(object key)
        {
            if(m_dicBackWK.ContainsKey(key))
            {
                return m_dicBackWK[key];
            }
            return null;
        }
        public bool IsOciConnect
        {
            get { return m_IsOciConnect; }
        }
        public string DBSVR
        {
            get { return m_DBSVR; }
        }
        public string DBUID
        {
            get { return m_DBUID; }
        }
        public string DBPWD
        {
            get { return m_DBPWD; }
        }
        public string DBNM
        {
            get { return m_DBNM; }
        }
        public string OutRefCurString
        {
            get { return m_OutRefCurString; }
            set { m_OutRefCurString = value; }
        }
        public ORACLE_DB()
        {
            InitializeComponent();
        }

        public ORACLE_DB(IContainer container)
        {
            
            container.Add(this);

            InitializeComponent();
        }
        private Oracle.ManagedDataAccess.Client.OracleConnection m_Conn = null;
        public bool Open(string svr, string uid, string pwd, string dbnm)
        {
            try
            {
                
                m_DBSVR = svr;
                m_DBUID = uid;
                m_DBPWD = pwd;
                m_DBNM = dbnm;
                string[] spDBName = m_DBSVR.Split('.');
                if (spDBName.Length < 3)
                {
                    m_Conn = new OracleConnection(string.Format("Data Source={0};Persist Security Info=False;User ID={1};Password={2}", m_DBSVR, DBUID, DBPWD));
                    m_IsOciConnect = true;
                }
                else
                {
                    string[] svrs = m_DBSVR.Split(';');
                    if (svrs.Length > 1)
                    {
                        m_Conn = new OracleConnection(
                                        string.Format(
                                             @"Data Source=
                                        ( DESCRIPTION = 
                                            (FAILOVER = YES)
                                            ( ADDRESS_LIST = 
                                                ( ADDRESS =
                                                    ( PROTOCOL = TCP )
                                                    ( HOST = {0} )
                                                    ( PORT = 1521 ) ) 
                                                ( ADDRESS =
                                                    ( PROTOCOL = TCP )
                                                    ( HOST = {1} )
                                                    ( PORT = 1521 ) ) )
                                                ( CONNECT_DATA = 
                                                    ( SERVER = DEDICATED )
                                                    ( SERVICE_NAME = {2} ) ) )
                                        ; User Id= {3}; Password = {4};", svrs[0], svrs[1], DBNM, DBUID, DBPWD)
                                                                          );
                    }
                    else
                    {
                        m_Conn = new OracleConnection(
                                        string.Format(
                                            @"Data Source=
                                        ( DESCRIPTION = 
                                            ( ADDRESS_LIST = 
                                                ( ADDRESS =
                                                    ( PROTOCOL = TCP )
                                                    ( HOST = {0} )
                                                    ( PORT = 1521 ) ) )
                                                ( CONNECT_DATA = 
                                                    ( SERVER = DEDICATED )
                                                    ( SERVICE_NAME = {1} ) ) )
                                        ; User Id= {2}; Password = {3};", DBSVR, DBNM, DBUID, DBPWD)
                                                                            );

                    }
                    m_IsOciConnect = false;

                }
                m_Conn.Open();

                return true;

            }
            catch(Exception eLog)
            {
                return false;

            }
        }
        public bool Open(string path = "")
        {
            try
            {
                
                if (string.IsNullOrEmpty(path))
                {
                    path = COMM_DEF.CN_CONFIG_XML_PATH;
                }
                if (File.Exists(path))
                {
                    DataSet ds = new DataSet();
                    ds.ReadXml(path);
                    if (ds.Tables.Count > 0)
                    {
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            m_DBSVR = ds.Tables[0].Columns.Contains("DBNAME") ? ds.Tables[0].Rows[0]["DBNAME"].ToString() : "";
                            m_DBUID = ds.Tables[0].Columns.Contains("DBUID") ? ds.Tables[0].Rows[0]["DBUID"].ToString() : "";
                            m_DBPWD = ds.Tables[0].Columns.Contains("DBPWD") ? ds.Tables[0].Rows[0]["DBPWD"].ToString() : "";
                            m_DBNM = ds.Tables[0].Columns.Contains("DBSERVICE") ? ds.Tables[0].Rows[0]["DBSERVICE"].ToString() : "";
                            string[] spDBName = DBSVR.Split('.');
                            if (spDBName.Length < 3)
                            {
                                m_Conn = new OracleConnection(string.Format("Data Source={0};Persist Security Info=False;User ID={1};Password={2}", m_DBSVR, DBUID, DBPWD));
                                m_IsOciConnect = true;
                            }
                            else
                            {

                                string[] svrs = m_DBSVR.Split(';');
                                if (svrs.Length > 1)
                                {
                                    m_Conn = new OracleConnection(
                                                    string.Format(
                                                         @"Data Source=
                                        ( DESCRIPTION = 
                                            (FAILOVER = YES)
                                            ( ADDRESS_LIST = 
                                                ( ADDRESS =
                                                    ( PROTOCOL = TCP )
                                                    ( HOST = {0} )
                                                    ( PORT = 1521 ) ) 
                                                ( ADDRESS =
                                                    ( PROTOCOL = TCP )
                                                    ( HOST = {1} )
                                                    ( PORT = 1521 ) ) )
                                                ( CONNECT_DATA = 
                                                    ( SERVER = DEDICATED )
                                                    ( SERVICE_NAME = {2} ) ) )
                                        ; User Id= {3}; Password = {4};", svrs[0], svrs[1], DBNM, DBUID, DBPWD)
                                                                                      );
                                }
                                else
                                {
                                    m_Conn = new OracleConnection(
                                                    string.Format(
                                                        @"Data Source=
                                        ( DESCRIPTION = 
                                            ( ADDRESS_LIST = 
                                                ( ADDRESS =
                                                    ( PROTOCOL = TCP )
                                                    ( HOST = {0} )
                                                    ( PORT = 1521 ) ) )
                                                ( CONNECT_DATA = 
                                                    ( SERVER = DEDICATED )
                                                    ( SERVICE_NAME = {1} ) ) )
                                        ; User Id= {2}; Password = {3};", DBSVR, DBNM, DBUID, DBPWD)
                                                                                        );

                                }
                                m_IsOciConnect = false;

                            }


                            m_Conn.Open();

                            return true;
                        }
                    }
                }
                return false;
            }
            catch (Exception eLog)
            {
                System.Diagnostics.Debug.WriteLine("[" + System.Reflection.MethodBase.GetCurrentMethod().Name + "]" + eLog.Message);
               
                return false;

            }
        }
        public bool Close()
        {
            try
            {
                m_Conn.Close();
            }
            catch
            {
                return false;
            }
            return true;
        }
        public DataTable ExcuteQuery(string query, Dictionary<string, string> param = null)
        {

            try
            {
                if (m_Conn == null || m_Conn.State != ConnectionState.Open)
                {
                    if (!Open())
                    {
                        System.Diagnostics.Debug.WriteLine("[" + System.Reflection.MethodBase.GetCurrentMethod().Name + "]" + "DB is Closed.");
                        return new DataTable();
                    }
                }
                DateTime now = DateTime.Now;
                DataTable dt = new DataTable();
                Oracle.ManagedDataAccess.Client.OracleCommand cmd = new OracleCommand();
                cmd.CommandText = query;
                cmd.Connection = m_Conn;
                if ((query.Contains("PKG_") || query.Contains("SP_") || query.Contains("APG_") || query.Contains("ASP_") || query.Contains("ZPG_")))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (param != null)
                    {
                        foreach (KeyValuePair<string, string> pair in param)
                        {
                            OracleParameter sqlParam = new OracleParameter(pair.Key, pair.Value);
                            cmd.Parameters.Add(sqlParam);
                        }
                    }
                    OracleParameter outParam = new OracleParameter(OutRefCurString, Oracle.ManagedDataAccess.Client.OracleDbType.RefCursor, 0, ParameterDirection.Output);
                    cmd.Parameters.Add(outParam);
                }
                else
                {
                    cmd.CommandType = CommandType.Text;
                }

                OracleDataAdapter da = new OracleDataAdapter(cmd);

                da.Fill(dt);
                TimeSpan span = DateTime.Now - now;
                if (IsDBTrace)
                {
                    int cnt = 0;
                    if (dt != null)
                    {
                        cnt = dt.Rows.Count;
                    }
                    DB.COMM_DEF.WriteTxtLog(query + ":Delay[" + span.TotalMilliseconds.ToString() + "]" + ":Count[" + cnt.ToString() + "]");
                }
                return dt;

            }
            catch (Exception eLog)
            {                
                if (eLog.Message.Contains("ORA-03114") || eLog.Message.Contains("ORA-03113") || eLog.Message.Contains("ORA-12532") 
                    || eLog.Message.Contains("ORA-12571") || eLog.Message.Contains("ORA-04063") || eLog.Message.Contains("ORA-04068") 
                    || eLog.Message.Contains("ORA-03135") || eLog.Message.Contains("ORA-01092")
                    )
                {
                    try
                    {
                        Debug.WriteLine(eLog.Message.ToString());
                        m_Conn.Close();
                    }
                    catch
                    { }
                    return new DataTable();
                }
                else
                {
                    throw new Exception(eLog.Message);
                }
               
            }

        }
        public int ExcuteNonQueryList(string query, Dictionary<string, ArrayList> param)
        {
            OracleTransaction tran = null;
            try
            {
                if (m_Conn == null || m_Conn.State != ConnectionState.Open)
                {
                    if (!Open())
                    {
                        System.Diagnostics.Debug.WriteLine("[" + System.Reflection.MethodBase.GetCurrentMethod().Name + "]" + "DB is Closed.");
                        return -1;
                    }
                }
                DateTime now = DateTime.Now;
                tran = m_Conn.BeginTransaction();
                OracleCommand cmd = new OracleCommand();

                cmd.Connection = m_Conn;
                cmd.Transaction = tran;
                cmd.CommandText = query;
                cmd.BindByName = true;
                int idx = 0;
                if ((query.Contains("PKG_") || query.Contains("SP_") || query.Contains("APG_") || query.Contains("ASP_") || query.Contains("ZPG_")))
                {
                    
                    cmd.CommandType = CommandType.StoredProcedure;
                    foreach (KeyValuePair<string, ArrayList> pair in param)
                    {
                        if (idx == 0)
                        {
                            cmd.ArrayBindCount = pair.Value.Count;
                        }
                        OracleParameter sqlParam = new OracleParameter();
                        sqlParam.ParameterName = pair.Key;

                        sqlParam.OracleDbType = OracleDbType.Varchar2;
                        sqlParam.Value = pair.Value.ToArray();
                        sqlParam.Direction = ParameterDirection.Input;
                        cmd.Parameters.Add(sqlParam);
                        idx++;
                    }
                }
                else
                {
                    cmd.CommandType = CommandType.Text;
                    foreach (KeyValuePair<string, ArrayList> pair in param)
                    {
                        if (idx == 0)
                        {
                            cmd.ArrayBindCount = pair.Value.Count;
                        }
                        OracleParameter sqlParam = new OracleParameter();
                        sqlParam.ParameterName = pair.Key;

                        sqlParam.OracleDbType = OracleDbType.Varchar2;
                        sqlParam.Value = pair.Value.ToArray();
                        sqlParam.Direction = ParameterDirection.Input;
                        cmd.Parameters.Add(sqlParam);
                        idx++;
                    }
                }
                int nRet = cmd.ExecuteNonQuery();

                tran.Commit();
                TimeSpan span = DateTime.Now - now;
                if (IsDBTrace)
                {
                    DB.COMM_DEF.WriteTxtLog(query + ":Delay[" + span.TotalMilliseconds.ToString() + "]" + ":Count[" + nRet.ToString() + "]");
                }
                return nRet;

            }
            catch (Exception eLog)
            {
                if (tran != null)
                {
                    try
                    {
                        tran.Rollback();
                    }
                    catch
                    { }
                }
                if (eLog.Message.Contains("ORA-03114") || eLog.Message.Contains("ORA-03113") || eLog.Message.Contains("ORA-12532")
                    || eLog.Message.Contains("ORA-12571") || eLog.Message.Contains("ORA-04063") || eLog.Message.Contains("ORA-04068")
                    || eLog.Message.Contains("ORA-03135") || eLog.Message.Contains("ORA-01092"))
                {
                    try
                    {
                        m_Conn.Close();
                    }
                    catch
                    { }
                    return -1;
                }
                else
                {
                    throw new Exception(eLog.Message);                
                }
            }
        }
        public int ExcuteNonQuery(string query, Dictionary<string, object> param)
        {
            OracleTransaction tran = null;
            try
            {
                if (m_Conn == null || m_Conn.State != ConnectionState.Open)
                {
                    if (!Open())
                    {
                        System.Diagnostics.Debug.WriteLine("[" + System.Reflection.MethodBase.GetCurrentMethod().Name + "]" + "DB is Closed.");
                        return -1;
                    }
                }
                DateTime now = DateTime.Now;
                tran = m_Conn.BeginTransaction();
                
                OracleCommand cmd = new OracleCommand();
                cmd.CommandText = query;
                cmd.Connection = m_Conn;
                cmd.Transaction = tran;
                if ((query.Contains("PKG_") || query.Contains("SP_") || query.Contains("APG_") || query.Contains("ASP_") || query.Contains("ZPG_")))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    foreach (KeyValuePair<string, object> pair in param)
                    {
                        object objVal = pair.Value;
                        if (pair.Key.Contains("$") && pair.Value != null)
                        {
                            if (pair.Value is byte[])
                            {
                                Oracle.ManagedDataAccess.Types.OracleBlob blob = new Oracle.ManagedDataAccess.Types.OracleBlob(m_Conn);
                                blob.Erase();
                                blob.Write((byte[])pair.Value, 0, ((byte[])pair.Value).Length);
                                objVal = blob;
                            }
                        }

                        OracleParameter sqlParam = new OracleParameter(pair.Key, objVal);
                        if (pair.Key.Contains("$"))
                        {
                            sqlParam.OracleDbType = OracleDbType.Blob;
                        }
                        cmd.Parameters.Add(sqlParam);
                    }
                }
                else
                {
                    cmd.CommandType = CommandType.Text;
                }
                int nRet = cmd.ExecuteNonQuery();

                tran.Commit();
                TimeSpan span = DateTime.Now - now;
                if (IsDBTrace)
                {
                    DB.COMM_DEF.WriteTxtLog(query + ":Delay[" + span.TotalMilliseconds.ToString() + "]" + ":Count[" + nRet.ToString() + "]");
                }
                return nRet;

            }
            catch (Exception eLog)
            {
                if (tran != null)
                {
                    try
                    {
                        tran.Rollback();
                    }
                    catch
                    { }
                }
                if (eLog.Message.Contains("ORA-03114") || eLog.Message.Contains("ORA-03113") || eLog.Message.Contains("ORA-12532")
                    || eLog.Message.Contains("ORA-12571") || eLog.Message.Contains("ORA-04063") || eLog.Message.Contains("ORA-04068")
                    || eLog.Message.Contains("ORA-03135") || eLog.Message.Contains("ORA-01092"))
                {
                    try
                    {
                        m_Conn.Close();
                    }
                    catch
                    { }
                    return -1;
                }
                else
                {
                    throw new Exception(eLog.Message);
                }
            }
        }
        public int ExcuteNonQuery(string query, Dictionary<string, string> param = null)
        {
            Dictionary<string, object> dicRet = null;

            if (param != null)
            {
                dicRet = new Dictionary<string, object>();
                foreach (KeyValuePair<string, string> pa in param)
                {
                    dicRet.Add(pa.Key, (object)pa.Value);
                }
            }
            return ExcuteNonQuery(query, dicRet);
        }


        public bool IsOpen()
        {
            if (m_Conn.State == ConnectionState.Closed)
            {
                return false;
            }
            return true;
        }

       
        public int BulkInsert(string toTable, ref DataTable sendData, bool bAppend = true)
        {

            int nRet = -1;
            try
            {

                Dictionary<string, ArrayList> param = new Dictionary<string, ArrayList>();
                string cols = "";
                string vals = "";
                for (int row = 0; row < sendData.Rows.Count; row++)
                {
                    for (int col = 0; col < sendData.Columns.Count; col++)
                    {
                        if(row == 0)
                        {
                            if (col == 0)
                            {
                                cols += sendData.Columns[col].ColumnName;
                                vals += ":" + sendData.Columns[col].ColumnName;
                            }
                            else
                            {
                                cols += "," + sendData.Columns[col].ColumnName;
                                vals += ",:" + sendData.Columns[col].ColumnName;
                            }
                        }
                        string key = ":"+sendData.Columns[col].ColumnName;

                        if (param.ContainsKey(key) == false)
                        {
                            ArrayList arr = new ArrayList();
                            arr.Add(sendData.Rows[row][col]);
                            param.Add(key, arr);
                        }
                        else
                        {
                            param[key].Add(sendData.Rows[row][col]);
                        }
                    }
                }

                string query ="";
                query = string.Format("insert into {0}({1}) values({2})", toTable, cols, vals);
                ExcuteNonQueryList(query, param);
                
            }
            catch (Exception eLog)
            {

                Debug.WriteLine("[" + System.Reflection.MethodBase.GetCurrentMethod().Name + "]" + eLog.Message);
            }
           
            return nRet;
        }


        BackgroundWorker m_backWorker = null;
        Dictionary<object, BackgroundWorker> m_dicBackWK = new Dictionary<object, BackgroundWorker>();
        public void AsyncExcute(COMM_DEF.DBQueryTypeEnum qt, string query, Dictionary<string, string> param)
        {
            AsyncExcute(null, qt, query, param);
        }
        public void AsyncExcute(object sender, COMM_DEF.DBQueryTypeEnum qt, string query, Dictionary<string, string> param)
        {
            try
            {

                COMM_DEF.AsyncDBST rst = new COMM_DEF.AsyncDBST();
                rst.query = "";
                rst.param = new Dictionary<string, string>();
                rst.dt = new DataTable();
                if (sender == null)
                {
                    if (m_backWorker == null)
                    {
                        m_backWorker = new BackgroundWorker();
                        m_backWorker.DoWork += new DoWorkEventHandler(ExcuteBackground_DoWork);
                        m_backWorker.ProgressChanged += new ProgressChangedEventHandler(ExcuteBackground_ProgressChanged);
                        m_backWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(ExcuteBackground_RunWorkerCompleted);
                        m_backWorker.WorkerReportsProgress = true;
                        m_backWorker.WorkerSupportsCancellation = true;
                    }

                    if (!m_backWorker.IsBusy)
                    {
                        rst.param = param;
                        rst.query = query;
                        rst.qt = qt;
                        rst.sender = sender;
                        m_backWorker.RunWorkerAsync(rst);
                    }
                }
                else
                {

                    if (m_dicBackWK.ContainsKey(sender) == false)
                    {
                        BackgroundWorker backWorker = new BackgroundWorker();
                        backWorker = new BackgroundWorker();
                        backWorker.DoWork += new DoWorkEventHandler(ExcuteBackground_DoWork);
                        backWorker.ProgressChanged += new ProgressChangedEventHandler(ExcuteBackground_ProgressChanged);
                        backWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(ExcuteBackground_RunWorkerCompleted);
                        backWorker.WorkerReportsProgress = true;
                        backWorker.WorkerSupportsCancellation = true;
                        m_dicBackWK.Add(sender, backWorker);
                    }

                    if (m_dicBackWK.ContainsKey(sender))
                    {

                        if (m_dicBackWK[sender].IsBusy == false)
                        {
                            rst.param = param;
                            rst.query = query;
                            rst.qt = qt;
                            rst.sender = sender;
                            m_dicBackWK[sender].RunWorkerAsync(rst);

                        }
                    }
                }
            }
            catch (Exception eLog)
            {
                Debug.WriteLine("[" + System.Reflection.MethodBase.GetCurrentMethod().Name + "]" + eLog.Message);
            }

        }
        private void ExcuteBackground_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                COMM_DEF.AsyncDBST rst;
                if (e.Error != null)
                {

                }
                else if (e.Cancelled)
                {//취소함
                    //OnBackgroundRCV(rst.query, rst.param, rst.dt);
                   
                }
                else
                {
                    rst = (COMM_DEF.AsyncDBST)e.Result;
                    if (OnBackgroundRCV != null)
                    {
                        OnBackgroundRCV(rst.sender, rst.query, rst.param, rst.dt);
                    }
                }

            }
            catch (Exception eLog)
            {
                Debug.WriteLine("[" + System.Reflection.MethodBase.GetCurrentMethod().Name + "]" + eLog.Message);
            }

        }

        private void ExcuteBackground_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            try
            {
                if (OnBackgroundPR != null)
                {
                    OnBackgroundPR(sender, e);

                }
            }
            catch (Exception eLog)
            {
                Debug.WriteLine("[" + System.Reflection.MethodBase.GetCurrentMethod().Name + "]" + eLog.Message);
            }

        }

        private void ExcuteBackground_DoWork(object sender, DoWorkEventArgs e)
        {
           
            COMM_DEF.AsyncDBST rst = new COMM_DEF.AsyncDBST();
            rst = (COMM_DEF.AsyncDBST)e.Argument;
            try
            {


                if (!string.IsNullOrEmpty(rst.query))
                {
                    if (rst.qt == COMM_DEF.DBQueryTypeEnum.Get)
                    {
                        rst.dt = ExcuteQuery(rst.query, rst.param);
                    }
                    else if (rst.qt == COMM_DEF.DBQueryTypeEnum.Set)
                    {
                        ExcuteNonQuery(rst.query, rst.param);
                    }
                }
                e.Result = rst;
            }
            catch (Exception eLog)
            {
                Debug.WriteLine("[Query:" + rst.query + "]" + "[" + System.Reflection.MethodBase.GetCurrentMethod().Name + "]" + eLog.Message);
            }
            finally
            {

            }

        }


        
    }
}
