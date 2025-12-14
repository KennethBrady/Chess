using Sql.Lib.Services.Schemas;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Sql.Lib.Services
{
	internal class SqlTransactedService : SqlService, ITransactedService
	{
		internal SqlTransactedService(SqlService service, bool commitByDefault = true)
		{
			BaseService = service;
			CommitByDefault = commitByDefault;
			Connection = BaseService.CreateConnection();
			Transaction = Connection.BeginTransaction();
			Proxy = new ConnectionProxy(Connection, DatabaseName);
		}

		private SqlService BaseService { get; init; }
		private bool CommitByDefault { get; init; }
		private IDbConnection Connection { get; init; }
		private IDbTransaction Transaction { get; set; }
		public override string ConnectionString => BaseService.ConnectionString;
		public override string DatabaseName => BaseService.DatabaseName;
		public override List<string> LoadDatabaseNames() => BaseService.LoadDatabaseNames();
		public bool IsDisposed { get; private set; } = false;
		public bool IsTransactionCompleted { get; private set; }
		private ConnectionProxy Proxy { get; init; }

		#region ISchemaProvider

		public override Table LoadTableSchema(string tableName, bool ensureForeignKeys = false)
		{
			return BaseService.LoadTableSchema(tableName, ensureForeignKeys);
		}

		public override List<string> LoadTableNames()
		{
			CheckState();
			return BaseService.LoadTableNames();
		}

		#endregion

		#region IServiceExecution

		public override IDbConnection CreateConnection(bool open = true)
		{
			CheckState();
			return Proxy;
		}

		public override Task<int> ExecuteScriptAsync(string script, Action<ScriptCallback> callback, CancellationToken cancellationToken, int minTimeOut = 30)
		{
			CheckState();
			return BaseService.ExecuteScriptAsync(script, callback, cancellationToken, minTimeOut);
		}

		public override object? ExecuteScalar(string sqlCmd, int timeout = 30)
		{
			CheckState();
			return ExecuteScalar(sqlCmd, timeout, Connection, Transaction);
		}

		public override int ExecuteNonQuery(string sql, int timeOut = 30)
		{
			CheckState();
			return ExecuteNonQuery(sql, Connection, Transaction, timeOut);
		}

		public override void ExecuteCustomReader(string sql, Action<IDataReader> read, int timeOut = 30)
		{
			CheckState();
			ExecuteCustomReader(sql, read, Connection, Transaction, timeOut);
		}

		public override int ExecuteProcedure(string procedureName, params (string name, object value)[] parameters)
		{
			CheckState();
			return ExecuteProcedure(Connection, Transaction, procedureName, parameters);
		}

		public override int ExecuteStatements(IEnumerable<string> statements, bool useTransaction = true, int timeOut = 30)
		{
			CheckState();
			return ExecuteStatements(statements, Connection, Transaction, false);
		}

		#endregion

		#region IServiceAction 

		public override List<T> LoadAll<T>()
		{
			CheckState();

			return base.LoadAll<T>();
		}

		public override List<T> LoadSelect<T>(string selection, int timeOut = 30)
		{
			CheckState();
			return base.LoadSelect<T>(selection, timeOut);
		}

		public override List<T> LoadWhere<T>(string where, int timeOut = 30)
		{
			CheckState();
			return base.LoadWhere<T>(where, timeOut);
		}

		public override List<T> Insert<T>(IEnumerable<T> values, int timeOut = 30)
		{
			CheckState();
			return Insert(values, Connection, Transaction, timeOut);
		}

		public override long Update<T>(IEnumerable<T> values, int timeOut = 30)
		{
			CheckState();
			return base.Update(values, Connection, Transaction, timeOut);
		}

		public override int Add<T>(IEnumerable<T> values, int timeOut = 30)
		{
			CheckState();
			return Add(values, Connection, Transaction, timeOut);
		}

		#endregion

		public void Dispose()
		{
			if (IsDisposed) return;
			try
			{
				if (Transaction != null && !IsTransactionCompleted)
				{
					if (CommitByDefault) Transaction.Commit(); else Transaction.Rollback();
				}
				if (Connection != null && Connection.State == ConnectionState.Open) Connection.Close();
			}
			finally
			{
				IsDisposed = true;
			}
		}

		public void Commit()
		{
			if (IsTransactionCompleted) return;
			CheckState();
			Transaction.Commit();
			IsTransactionCompleted = true;
		}

		public void Rollback()
		{
			if (IsTransactionCompleted) return;
			CheckState();
			Transaction.Rollback();
			IsTransactionCompleted = true;
		}

		private void CheckState()
		{
			if (IsDisposed) throw new ObjectDisposedException(nameof(SqlTransactedService));
			if (IsTransactionCompleted) throw new InvalidOperationException($"Transaction has already been completed.");
		}

		private class ConnectionProxy : IDbConnection, IDisposable
		{
			internal ConnectionProxy(IDbConnection baseConnection, string dbName)
			{
				BaseConnection = baseConnection;
				Database = dbName;
			}
			private IDbConnection BaseConnection { get; init; }
#pragma warning disable 8767
			public string ConnectionString
			{
				get => BaseConnection.ConnectionString;
				set { }
			}
#pragma warning restore

			public int ConnectionTimeout { get; }
			public string Database { get; private init; }
			public ConnectionState State { get; }

			public IDbTransaction BeginTransaction()
			{
				throw new NotImplementedException();
			}

			public IDbTransaction BeginTransaction(IsolationLevel il)
			{
				throw new NotImplementedException();
			}

			public void ChangeDatabase(string databaseName)
			{
				throw new NotImplementedException();
			}

			public void Close()
			{

			}

			public IDbCommand CreateCommand() => BaseConnection.CreateCommand();

			public void Dispose()
			{

			}

			public void Open()
			{
				throw new NotImplementedException();
			}
		}
	}
}
