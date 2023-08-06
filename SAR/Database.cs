using MySqlConnector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Windows.Forms;

namespace SAR
{

    public abstract class AbstractDatabaseTable<M> : IDB where M : class, IDB<M>, IAbstractModel, new()
    {
        public DbConnection Connection { get; set; }
        protected DbCommand Command;
        protected DbTransaction? Transaction;
        public bool IsConnected { get => Connection.State.Equals(ConnectionState.Open); }
        M _model { get; } = new();
        public RecordSource<M> Source { get; private set; } = new RecordSource<M>();
        public IRecordSource DataSource { get => Source; }
        public Type ModelType { get => _model.GetType(); }

        public AbstractDatabaseTable()=> EstablishConnection();

        abstract protected void EstablishConnection();

        public void OpenConnection() => Connection.Open();
        public void CloseConnection() => Connection.Close();

        public void Reconnect()
        {
            if (IsConnected) CloseConnection();
            EstablishConnection();
            OpenConnection();
        }

        public bool IsConnectionStateGood()
        {
            if (Connection == null) return false;
            switch (Connection.State)
            {
                case ConnectionState.Open:
                    return true;
                case ConnectionState.Closed:
                case ConnectionState.Broken:
                    return false;
                default: return false;
            }
        }

        protected IEnumerable<M> FetchData(DbDataReader reader)
        {
            while (reader.Read()) yield return _model.GetRecord(reader);
        }

        public void Select()
        {
            NewCommand(_model.SQLStatements(SQLType.SELECT),Connection);
            Source = new(FetchData(Command.ExecuteReader()).AsIRecordSource<M>());
            Source.Origin = ToString();
            Command.Dispose();
        }

        public Task GetTable()
        {
            Connection.Open();
            Select();
            Connection.Close();
            return Task.CompletedTask;
        }

        public IEnumerable<Task> SetForeignKeys() =>
        Source.Select(async item => await item.SetForeignKeys());

        public int Update(object? _record) =>
        AlterRecord((M?)_record, "update", SQLType.UPDATE);

        public int Delete(object? _record) =>
        AlterRecord((M?)_record, "delete", SQLType.DELETE);

        public int Insert(object? _record) =>
        AlterRecord((M?)_record, "insert", SQLType.INSERT);

        protected long LastInsertedID()
        {
            if (!IsConnectionStateGood()) throw new Exception("Connection is not open or broken;");
            NewCommand("SELECT LAST_INSERT_ID();", Connection);
            object? val = Command.ExecuteScalar();
            Command.Dispose();
            if (val is not null) return (long)(UInt64)val;
            return 0;
        }

        protected abstract void NewCommand(string sqlstatement,DbConnection connection);

        protected int AlterRecord(M? _record, string exception, SQLType _sqlType)
        {
            if (_record == null) throw new ArgumentNullException($"Cannot {exception} a null record.");
            if (!IsConnectionStateGood()) throw new Exception("Connection is not open or broken;");
            NewCommand(_record.SQLStatements(_sqlType), (MySqlConnection)Connection);
            _record.Params(Command.Parameters);
            var rows = Command.ExecuteNonQuery();
            Command.Parameters.Clear();
            Command.Dispose();
            switch (_sqlType)
            {
                case SQLType.INSERT:
                    _record.SetPrimaryKey(LastInsertedID());
                    Source.AddRecord(_record);
                    break;
                case SQLType.DELETE:
                    Source.DeleteRecord(_record);
                    break;
                case SQLType.UPDATE:
                    Source.UpdateRecord(_record);
                    break;
            }
            _record.IsDirty = false;
            return rows;
        }

        public int RunStatement(string sql)
        {
            NewCommand(sql,Connection);
            int rows = Command.ExecuteNonQuery();
            Command.Dispose();
            return rows;
        }

        public void StartTransaction()
        {
            OpenConnection();
            RunStatement("SET autocommit = 0;");
            Transaction = Connection.BeginTransaction();
            NewCommand();
            Command.Connection = Connection;
        }

        protected abstract void NewCommand();

        public object? InsertTransaction(object? _record) =>
        RunTransaction((M?)_record, "insert", SQLType.INSERT);

        public object? UpdateTransaction(object? _record) =>
        RunTransaction((M?)_record, "update", SQLType.UPDATE);

        public object? DeleteTransaction(object? _record) =>
        RunTransaction((M?)_record, "delete", SQLType.DELETE);

        M? RunTransaction(M? _record, string exception, SQLType _sqlType)
        {
            if (_record == null) throw new ArgumentNullException($"Cannot {exception} a null record.");
            if (!IsConnectionStateGood()) throw new Exception("Connection is not open or broken;");
            if (Command == null) throw new Exception("Command is null, StartTransaction() might not have been called.");
            Command.CommandText = _record?.SQLStatements(_sqlType);
            Command.Transaction = Transaction;
            _record?.Params(Command.Parameters);
            Command.ExecuteScalar();
            Command.Parameters.Clear();
            Command.Dispose();
            _record!.IsDirty = false;
            if (_sqlType.Equals(SQLType.INSERT))
            {
                _record?.SetPrimaryKey(LastInsertedID());
            }
            return _record;
        }

        public void CommitTransaction()
        {
            Transaction?.Commit();
            Command?.Dispose();
            RunStatement("SET autocommit = 1;");
            CloseConnection();
        }

        public void RollBack()
        {
            Transaction?.Rollback();
            Command?.Dispose();
            RunStatement("SET autocommit = 1;");
            CloseConnection();
        }

        public override string ToString() => $"Database<{_model.GetType().Name}>";
    }

    public class MySQLDatabaseTable<M> : AbstractDatabaseTable<M> where M : class, IDB<M>, IAbstractModel, new()
    {
        protected override void EstablishConnection()
        {
            if (string.IsNullOrEmpty(Sys.DatabaseManager.ConnectionString)) throw new NotImplementedException("Connection String is empty");
            Connection = new MySqlConnection(Sys.DatabaseManager.ConnectionString);
        }

        protected override void NewCommand(string sqlstatement, DbConnection connection)=>
        Command = new MySqlCommand(sqlstatement, (MySqlConnection)connection);

        protected override void NewCommand()=>
        Command = new MySqlCommand();
    }

    public class SQLiteTable<M> : AbstractDatabaseTable<M> where M : class, IDB<M>, IAbstractModel, new()
    {
        public static string Version = "3";
        //@".\DB\MyDb.db";;

        protected override void EstablishConnection() 
        {
          if (string.IsNullOrEmpty(Sys.DatabaseManager.ConnectionString)) throw new NotImplementedException("Connection String is empty");
          Connection = new SQLiteConnection($"Data Source={Sys.DatabaseManager.ConnectionString};Version={Version};");
        }

        protected override void NewCommand(string sqlstatement, DbConnection connection) =>
        Command = new SQLiteCommand(sqlstatement, (SQLiteConnection)connection);

        protected override void NewCommand() =>
        Command = new SQLiteCommand();
    }

    public interface IDB
    {
        public DbConnection Connection { get; set; }
        public Type ModelType { get; }
        public Task GetTable();
        public void Select();
        public int Insert(object? _record);
        public int Update(object? _record);
        public int Delete(object? _record);
        public int RunStatement(string sql);
        public void OpenConnection();
        public void CloseConnection();
        public object? InsertTransaction(object? _record);
        public object? UpdateTransaction(object? _record);
        public object? DeleteTransaction(object? _record);
        public IRecordSource DataSource { get; }
        public IEnumerable<Task> SetForeignKeys();
    }


}
