using MySql.Data.MySqlClient;
using System;
using System.Data;

namespace RegisterDiscoveryService.DAO
{
    public class MySQLHelper
    {
        public string config = Config.MySqlConnection;
        public MySQLHelper() { }

        #region  执行简单SQL语句
        public int ExecuteSql(string SQLString)
        {
            if (string.IsNullOrEmpty(SQLString)) return 0;

            using (MySqlConnection connection = new MySqlConnection(config))
            {
                using (MySqlCommand cmd = new MySqlCommand(SQLString, connection))
                {
                    try
                    {
                        connection.Open();
                        int rows = cmd.ExecuteNonQuery();
                        return rows;
                    }
                    catch (MySql.Data.MySqlClient.MySqlException e)
                    {
                        connection.Close();
                        throw e;
                    }
                }
            }
        }
       
        public int ExecuteSql(string SQLString, string content)
        {
            if (string.IsNullOrEmpty(SQLString) && content.Length == 0) return 0;

            using (MySqlConnection connection = new MySqlConnection(this.config))
            {
                var cmd = new MySqlCommand(SQLString, connection);
                var myParameter = new MySql.Data.MySqlClient.MySqlParameter("@content", SqlDbType.NText);
                myParameter.Value = content;
                cmd.Parameters.Add(myParameter);
                try
                {
                    connection.Open();
                    int rows = cmd.ExecuteNonQuery();
                    return rows;
                }
                catch (MySql.Data.MySqlClient.MySqlException e) { throw e; }
                finally
                {
                    cmd.Dispose();
                    connection.Close();
                }
            }
        }

        public object GetSingle(string SQLString)
        {
            if (string.IsNullOrEmpty(SQLString)) return 0;

            using (MySqlConnection connection = new MySqlConnection(this.config))
            {
                using (MySqlCommand cmd = new MySqlCommand(SQLString, connection))
                {
                    try
                    {
                        connection.Open();
                        var obj = cmd.ExecuteScalar();
                        if ((object.Equals(obj, null)) || (object.Equals(obj, System.DBNull.Value))) return null;
                        return obj;
                    }
                    catch (MySql.Data.MySqlClient.MySqlException e)
                    {
                        connection.Close();
                        throw e;
                    }
                }
            }
        }
        public object GetSingle(string SQLString, int Times)
        {
            if (string.IsNullOrEmpty(SQLString)) return 0;

            using (MySqlConnection connection = new MySqlConnection(this.config))
            {
                using (MySqlCommand cmd = new MySqlCommand(SQLString, connection))
                {
                    try
                    {
                        connection.Open();
                        cmd.CommandTimeout = Times;
                        var obj = cmd.ExecuteScalar();
                        if ((object.Equals(obj, null)) || (object.Equals(obj, System.DBNull.Value))) return null;
                        return obj;
                    }
                    catch (MySql.Data.MySqlClient.MySqlException e)
                    {
                        connection.Close();
                        throw e;
                    }
                }
            }
        }
        public MySqlDataReader ExecuteReader(string strSQL)
        {
            if (string.IsNullOrEmpty(strSQL)) return null;

            var connection = new MySqlConnection(this.config);
            var cmd = new MySqlCommand(strSQL, connection);
            try
            {
                connection.Open();
                var myReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                return myReader;
            }
            catch (MySql.Data.MySqlClient.MySqlException e) { throw e; }
        }
        public DataSet Query(string SQLString)
        {
            if (string.IsNullOrEmpty(SQLString)) return null;

            using (MySqlConnection connection = new MySqlConnection(this.config))
            {
                var ds = new DataSet();
                try
                {
                    connection.Open();
                    var command = new MySqlDataAdapter(SQLString, connection);
                    command.Fill(ds, "ds");
                }
                catch (MySql.Data.MySqlClient.MySqlException ex) { throw new Exception(ex.Message); }
                return ds;
            }
        }
        public DataSet Query(string SQLString, int Times)
        {
            if (string.IsNullOrEmpty(SQLString)) return null;

            using (MySqlConnection connection = new MySqlConnection(this.config))
            {
                var ds = new DataSet();
                try
                {
                    connection.Open();
                    var command = new MySqlDataAdapter(SQLString, connection);
                    command.SelectCommand.CommandTimeout = Times;
                    command.Fill(ds, "ds");
                }
                catch (MySql.Data.MySqlClient.MySqlException ex) { throw new Exception(ex.Message); }
                return ds;
            }
        }
        #endregion

        #region 执行带参数的SQL语句
        public int ExecuteSql(string SQLString, params MySqlParameter[] cmdParms)
        {
            if (string.IsNullOrEmpty(SQLString) && (cmdParms == null || cmdParms.Length == 0)) return 0;

            using (MySqlConnection connection = new MySqlConnection(this.config))
            {
                using (MySqlCommand cmd = new MySqlCommand())
                {
                    try
                    {
                        PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                        int rows = cmd.ExecuteNonQuery();
                        cmd.Parameters.Clear();
                        return rows;
                    }
                    catch (MySql.Data.MySqlClient.MySqlException e)
                    {
                        //G.LogError(string.Format("sql: {0},Error: {1}", SQLString, e));
                        throw e;
                    }
                }
            }
        }
        
        public object GetSingle(string SQLString, params MySqlParameter[] cmdParms)
        {
            if (string.IsNullOrEmpty(SQLString) && (cmdParms == null || cmdParms.Length == 0)) return 0;

            using (MySqlConnection connection = new MySqlConnection(this.config))
            {
                using (MySqlCommand cmd = new MySqlCommand())
                {
                    try
                    {
                        PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                        object obj = cmd.ExecuteScalar();
                        cmd.Parameters.Clear();
                        if ((object.Equals(obj, null)) || (object.Equals(obj, System.DBNull.Value))) return null;
                        return obj;
                    }
                    catch (MySql.Data.MySqlClient.MySqlException e)
                    {
                        //G.LogError(string.Format("sql: {0},Error: {1}", SQLString, e));
                        throw e;
                    }
                }
            }
        }
        public MySqlDataReader ExecuteReader(string SQLString, params MySqlParameter[] cmdParms)
        {
            if (string.IsNullOrEmpty(SQLString) && (cmdParms == null || cmdParms.Length == 0)) return null;

            var connection = new MySqlConnection(this.config);
            var cmd = new MySqlCommand();
            try
            {
                PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                var myReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                cmd.Parameters.Clear();
                return myReader;
            }
            catch (MySql.Data.MySqlClient.MySqlException e) { throw e; }
        }
        public DataSet Query(string SQLString, params MySqlParameter[] cmdParms)
        {
            if (string.IsNullOrEmpty(SQLString) && (cmdParms == null || cmdParms.Length == 0)) return null;

            using (MySqlConnection connection = new MySqlConnection(this.config))
            {
                var cmd = new MySqlCommand();
                PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                using (MySqlDataAdapter da = new MySqlDataAdapter(cmd))
                {
                    var ds = new DataSet();
                    try
                    {
                        da.Fill(ds, "ds");
                        cmd.Parameters.Clear();
                    }
                    catch (MySql.Data.MySqlClient.MySqlException ex)
                    {
                        //g.LogError(string.Format("sql: {0},Error: {1}", SQLString, ex));
                        throw new Exception(ex.Message);
                    }
                    return ds;
                }
            }
        }
        private void PrepareCommand(MySqlCommand cmd, MySqlConnection conn, MySqlTransaction trans, string cmdText, MySqlParameter[] cmdParms)
        {
            if (conn.State != ConnectionState.Open) conn.Open();
            cmd.Connection = conn;
            cmd.CommandText = cmdText;
            if (trans != null) cmd.Transaction = trans;
            cmd.CommandType = CommandType.Text;
            if (cmdParms != null)
            {
                foreach (MySqlParameter parameter in cmdParms)
                {
                    if ((parameter.Direction == ParameterDirection.InputOutput || parameter.Direction == ParameterDirection.Input) &&
                        (parameter.Value == null)) { parameter.Value = DBNull.Value; }
                    cmd.Parameters.Add(parameter);
                }
            }
        }
        #endregion

        #region 参数处理
        public void AddParam(MySqlCommand cmd, string name, object value, MySqlDbType dbType, ParameterDirection direction = ParameterDirection.Input, int size = 0)
        {
            cmd.Parameters.Add(name, dbType);
            cmd.Parameters[name].Value = value;
            cmd.Parameters[name].Direction = direction;
            if (size > 0) cmd.Parameters[name].Size = size;
        }
        public MySqlParameter CreateParam(string name, object value, MySqlDbType dbType)
        {
            var param = new MySqlParameter(name, dbType);
            param.Value = value;
            return param;
        }
        #endregion
  
     
    }
}