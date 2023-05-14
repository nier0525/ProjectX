using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using System.Text;

namespace Core
{
    public static class ProcedureResult
    {
        public const int FAIL = 0;
        public const int SUCCESS = 1;
    }

    public class MSSQLConnection : IDisposable
    {
        private MSSQLManager        _manager    = null;
        private SqlConnection       _connection = null;
        private SqlCommand          _command    = null;

        private DataRowCollection   _rows       = null;
        private DataRow             _data       = null;
        private int                 _rowIndex   = 0;

        public MSSQLConnection(string config, MSSQLManager manager)
        {
            _connection = new SqlConnection(config);
            _manager    = manager;
        }

        public void Dispose()
        {
            _manager.ReleaseDB(this);
        }

        public bool Open()
        {
            try
            {
                _connection.Open();
            }
            catch (SqlException exception)
            {
                NetworkLogger.Write($"SQL Connection Open Failed.. Error {exception.ErrorCode}");
                return false;
            }
            return true;
        }

        public void Close()
        {
            try
            {
                _command = null;
                _rows = null;
                _rowIndex = 0;
                _data = null;

                _connection.Close();
            }
            catch (SqlException exception)
            {
                NetworkLogger.Write($"SQL Connection Close Failed.. Error {exception.ErrorCode}");
            }
        }

        public bool NewProcedure(string procedureName)
        {
            if (null == _connection)
                return false;

            try
            {
                _command = new SqlCommand(procedureName, _connection);
                _command.CommandType = CommandType.StoredProcedure;
            }
            catch (SqlException exception)
            {
                NetworkLogger.Write($"SQL Procedure Failed.. Error {exception.ErrorCode}");
                return false;
            }
            return true;
        }

        public bool TryProcedure()
        {
            if (null == _command)
                return false;

            try
            {
                _command.ExecuteNonQuery();
            }
            catch (SqlException exception)
            {
                NetworkLogger.Write($"SQL Procedure Failed.. Error {exception.ErrorCode}");
                return false;
            }
            return true;
        }

        public bool GetProcedure()
        {
            if (null == _command)
                return false;

            try
            {
                var data    = new SqlDataAdapter(_command);
                var result  = new DataSet();

                data.Fill(result);
                if (null != result && null != result.Tables && 0 != result.Tables.Count)
                    _rows = result.Tables[0].Rows;
                return true;
            }
            catch (SqlException exception)
            {
                NetworkLogger.Write($"SQL Procedure Failed.. Error {exception.ErrorCode}");
                return false;
            }
        }

        public bool Fetch()
        {
            if (null == _rows)
                return false;
            if (_rows.Count == _rowIndex)
                return false;

            _data = _rows[_rowIndex++];
            return true;
        }

        public T GetParam<T>(string paramName)
        {
            if (null == _data)
                throw new ArgumentNullException("Invalid Data");
            return (T)_data[paramName];
        }

        public void GetParam<T>(string paramName, ref T value)
        {
            if (null == _data)
                throw new ArgumentNullException("Invalid Data");
            value = (T)_data[paramName];
        }

        public string GetParamString(string paramName)
        {
            if (null == _data)
                throw new ArgumentNullException("Invalid Data");
            return _data[paramName].ToString();
        }

        public void GetParamString(string paramName, ref string value)
        {
            if (null == _data)
                throw new ArgumentNullException("Invalid Data");
            value = _data[paramName].ToString();
        }

        public bool AddParam(string paramName, object data)
        {
            if (null == _command)
                return false;

            SqlParameter param = null;
            
            var type = data.GetType();

            if (true == type.Equals(typeof(float)))
                param = new SqlParameter(paramName, SqlDbType.Float);
            else if (sizeof(byte) == Marshal.SizeOf(data))
                param = new SqlParameter(paramName, SqlDbType.TinyInt);
            else if (sizeof(short) == Marshal.SizeOf(data))
                param = new SqlParameter(paramName, SqlDbType.SmallInt);
            else if (sizeof(int) == Marshal.SizeOf(data))
                param = new SqlParameter(paramName, SqlDbType.Int);
            else if (sizeof(long) == Marshal.SizeOf(data))
                param = new SqlParameter(paramName, SqlDbType.BigInt);

            if (null == param)
                return false;

            param.Value = data;
            _command.Parameters.Add(param);
            return true;
        }

        public bool AddParamToString(string paramName, string data)
        {
            if (null == _command)
                return false;

            var type = data.GetType();
            if (false == type.Equals(typeof(string)))
                return false;

            var param = new SqlParameter(paramName, SqlDbType.VarChar);
            param.Value = data;

            _command.Parameters.Add(param);
            return true;
        }

        public bool AddParamToWString(string paramName, string data)
        {
            if (null == _command)
                return false;

            var type = data.GetType();
            if (false == type.Equals(typeof(string)))
                return false;

            var param = new SqlParameter(paramName, SqlDbType.NVarChar);
            param.Value = data;

            _command.Parameters.Add(param);
            return true;
        }        

        public bool AddOutput(string paramName, object data)
        {
            if (false == AddParam(paramName, data))
                return false;

            _command.Parameters[paramName].Direction = ParameterDirection.Output;
            return true;
        }

        public bool AddOutputString(string paramName, string data)
        {
            if (false == AddParamToString(paramName, data))
                return false;

            _command.Parameters[paramName].Direction = ParameterDirection.Output;
            return true;
        }

        public bool AddOutputWString(string paramName, string data)
        {
            if (false == AddParamToWString(paramName, data))
                return false;

            _command.Parameters[paramName].Direction = ParameterDirection.Output;
            return true;
        }

        public T GetOutput<T>(string paramName)
        {
            if (null == _command)
                return default(T);

            if (false == _command.Parameters.Contains(paramName))
                return default(T);

            return (T)_command.Parameters[paramName].Value;            
        }

        public bool AddReturnValue()
        {
            if (null == _command)
                return false;

            _command.Parameters.Add("@nReturn", SqlDbType.VarChar).Direction = ParameterDirection.ReturnValue;
            return true;
        }

        public int GetReturnValue()
        {
            if (null == _command)
                return 0;

            if (false == _command.Parameters.Contains("@nReturn"))
                return 0;

            return (int)_command.Parameters["@nReturn"].Value;
        }
    }

    public class MSSQLManager
    {
        private Queue<MSSQLConnection> _connections = new Queue<MSSQLConnection>();
        private string _config;

        public void Initialize(string hostName, string database, string id, string pw, int poolCount)
        {
            _config = $"Server={hostName};Database={database};Uid={id};Pwd={pw}";
            
            lock (_connections)
            {
                for (int i = 0; i < poolCount; ++i)
                    _connections.Enqueue(new MSSQLConnection(_config, this));
            }
        }

        public MSSQLConnection GetDB()
        {
            MSSQLConnection connection = null;

            lock (_connections)
            {
                if (0 == _connections.Count)
                    connection = new MSSQLConnection(_config, this);
                else
                    connection = _connections.Dequeue();
            }

            if (false == connection.Open())
                return null;
            
            return connection;
        }

        public void ReleaseDB(MSSQLConnection connection)
        {
            if (null == connection)
                return;

            connection.Close();
            
            lock (_connections)
                _connections.Enqueue(connection);            
        }
    }
}
