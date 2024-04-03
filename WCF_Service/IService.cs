using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.ServiceModel;


namespace WCF_Service
{
    [ServiceContract]
    interface IService
    {
        [OperationContract]
        string GetTestString();

        [OperationContract]
        DataSet GetTestDataTable();

        [OperationContract]
        DataSet ExecuteQuery(string query, Dictionary<string, string> param = null);

        [OperationContract]
        int ExecuteNonQueryObject(string query, Dictionary<string, object> param = null);

        [OperationContract]
        int ExecuteNonQuery(string query, Dictionary<string, string> param = null);

    }
}
