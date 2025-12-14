using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.Lib.Extensions;
using Sql.Lib.Services.Schemas;

namespace Sql.Lib.Services
{
	public record struct ScriptCallback(TimeSpan TimeTaken, string SqlStatement);

	public abstract class SqlService : ISqlService
	{
		protected SqlService() { }

		public abstract string ConnectionString { get; }
		public abstract string DatabaseName { get; }

		#region ISchemaProvider 

		public abstract Table LoadTableSchema(string tableName, bool ensureForeignKeys = false);

		public abstract List<string> LoadTableNames();

		public ITypeLoader CreateLoaderFor<T>() => CreateLoader(typeof(T));

		public abstract List<string> LoadDatabaseNames();

		#endregion

		#region IServiceExecution

		public abstract IDbConnection CreateConnection(bool open = true);

		public virtual object? ExecuteScalar(string sqlCmd, int timeout = 30)
		{
			using (IDbConnection conn = CreateConnection()) return ExecuteScalar(sqlCmd, timeout, conn, null);
		}

		public virtual int ExecuteNonQuery(string sql, int timeOut = 30)
		{
			using (IDbConnection conn = CreateConnection()) return ExecuteNonQuery(sql, conn, null, timeOut);
		}

		public virtual int ExecuteStatements(IEnumerable<string> statements, bool useTransaction = true, int timeOut = 30)
		{
			if (statements == null || statements.Count() == 0) return 0;
			using (IDbConnection conn = CreateConnection()) return ExecuteStatements(statements, conn, null, useTransaction, timeOut);
		}

		public virtual int ExecuteProcedure(string procedureName, params (string name, object value)[] parameters)
		{
			using (IDbConnection conn = CreateConnection()) return ExecuteProcedure(conn, null, procedureName, parameters);
		}

		public virtual void ExecuteCustomReader(string sql, Action<IDataReader> read, int timeOut = 30)
		{
			using (IDbConnection conn = CreateConnection(true)) ExecuteCustomReader(sql, read, conn, null, timeOut);
		}

		public virtual bool CanExecuteScript => false;

		public virtual int ExecuteScript(string script, int timeOut = 30)
		{
			throw new NotSupportedException();
		}

		public virtual Task<int> ExecuteScriptAsyn(string script, Action<ScriptCallback> callback, CancellationToken cancellationToken, int minTimeOut = 30)
		{
			throw new NotSupportedException();
		}

		#endregion

		#region IServiceAction

		public virtual List<T> LoadAll<T>() where T : class
		{
			Type forType = typeof(T);
			Loader loader = CreateLoader(forType);
			List<T> r = new();
			using (var conn = CreateConnection(true))
			{
				var cmd = conn.CreateCommand();
				cmd.CommandText = $"select * from {loader.TableName}";
				using (var rdr = cmd.ExecuteReader())
				{
					while (rdr.Read()) r.Add((T)loader.CreateAndPopulate(rdr));
				}
			}
			return r;
		}

		public virtual List<T> LoadWhere<T>(string where, int timeOut = 30) where T : class
		{
			if (string.IsNullOrEmpty(where)) throw new ArgumentException("where clause cannot be empty.");
			Type forType = typeof(T);
			Loader loader = CreateLoader(forType);
			where = where.Trim().ToLower();
			if (!where.StartsWith("where ")) where = "where " + where;
			List<T> r = new();
			using (var conn = CreateConnection())
			{
				var cmd = conn.CreateCommand();
				cmd.CommandTimeout = timeOut;
				cmd.CommandText = $"select * from {loader.TableName} {where}";
				using (var rdr = cmd.ExecuteReader())
				{
					while (rdr.Read()) r.Add((T)loader.CreateAndPopulate(rdr));
				}
			}
			return r;
		}

		public virtual List<T> LoadSelect<T>(string selection, int timeOut = 30)
		{
			Type forType = typeof(T);
			Loader loader = CreateLoader(forType);
			List<T> r = new();
			using (var conn = CreateConnection())
			{
				var cmd = conn.CreateCommand();
				cmd.CommandTimeout = timeOut;
				cmd.CommandText = selection;
				cmd.CommandType = CommandType.Text;
				using (var rdr = cmd.ExecuteReader())
				{
					while (rdr.Read()) r.Add((T)loader.CreateAndPopulate(rdr));
				}
			}
			return r;
		}

		public T InsertOne<T>(T value, int timeOut = 30) where T : class => Insert(value.Yield(), timeOut).First();

		public virtual int Add<T>(IEnumerable<T> values, int timeOut = 30) where T : class
		{
			using var conn = CreateConnection();
			IDbTransaction tx = conn.BeginTransaction();
			try
			{
				var ret = Add(values, conn, tx, timeOut);
				tx.Commit();
				return ret;
			}
			catch
			{
				tx.Rollback();
				throw;
			}
		}

		public virtual List<T> Insert<T>(IEnumerable<T> values, int timeOut = 30) where T : class
		{
			using var conn = CreateConnection();
			IDbTransaction tx = conn.BeginTransaction();
			try
			{
				var ret = Insert(values, conn, tx, timeOut);
				tx.Commit();
				return ret;
			}
			catch
			{
				tx.Rollback();
				throw;
			}
		}

		public bool UpdateOne<T>(T value) where T : class => Update(value.Yield()) == 1;

		public virtual long Update<T>(IEnumerable<T> values, int timeOut = 30) where T : class
		{
			Loader loader = CreateLoader(typeof(T));
			List<string> statements = ApplySqlOnly(values.Select(v => loader.CreateUpdateStatement(v))).ToList();
			if (statements.Count == 0) return 0;
			return ExecuteStatements(statements);
		}

		public long Update<T>(IEnumerable<ValuePair<T>> pairs, int timeOut = 30) where T : class
		{
			Loader loader = CreateLoader(typeof(T));
			List<string> statements = ApplySqlOnly(pairs.Select(p => loader.CreateUpdateStatement(p))).ToList();
			if (statements.Count == 0) return 0;
			return ExecuteStatements(statements, true, timeOut);
		}

		public int DeleteOne<T>(T value) where T : class => Delete(value.Yield());

		public virtual int Delete<T>(IEnumerable<T> values) where T : class
		{
			Loader loader = CreateLoader(typeof(T));
			List<string> statements = ApplySqlOnly(values.Select(v => loader.CreateDeleteStatement(v))).ToList();
			if (statements.Count == 0) return 0;
			return ExecuteStatements(statements);
		}

		public ITransactedService CreateTransactedService(bool commitByDefault = true)
			=> new SqlTransactedService(this, commitByDefault);

		#endregion

		#region Scripts

		protected string TimeOutCS(int minTmOut) => $"{ConnectionString};default command timeout={minTmOut}";

		public abstract Task<int> ExecuteScriptAsync(string script, Action<ScriptCallback> callback, CancellationToken cancellationToken, int minTimeOut = 30);

		public Task<int> ExecuteScriptAsync(string script, Action<ScriptCallback> callback, int minTimeOut = 30)
		{
			CancellationToken token = new CancellationToken(false);
			return ExecuteScriptAsync(script, callback, token, minTimeOut);
		}

		public Task<int> ExecuteScriptFileAsync(string fpath, Action<ScriptCallback> callback, int minTimeOut = 30)
		{
			CancellationToken token = new CancellationToken(false);
			return ExecuteScriptFileAsync(fpath, callback, token, minTimeOut);
		}

		public Task<int> ExecuteScriptFileAsync(string fpth, Action<ScriptCallback> callback, CancellationToken cancellationToken, int minTimeOut = 30)
		{
			return ExecuteScriptAsync(File.ReadAllText(fpth), callback, cancellationToken, minTimeOut);
		}

		#endregion

		private static IEnumerable<string> ApplySqlOnly(IEnumerable<ISqlStatement> statements) =>
			statements.Where(s => s is SqlStatement ss).Cast<SqlStatement>().Select(ss => ss.Sql);

		internal Loader CreateLoader(Type forType) => Loader.For(this, forType);

		#region With Transactions

		protected object? ExecuteScalar(string sqlCommand, int timeout, IDbConnection connection, IDbTransaction? transaction)
		{
			IDbCommand cmd = connection.CreateCommand();
			cmd.Transaction = transaction;
			cmd.CommandText = sqlCommand;
			cmd.CommandType = CommandType.Text;
			cmd.CommandTimeout = timeout;
			return cmd.ExecuteScalar();
		}

		protected int ExecuteNonQuery(string sql, IDbConnection connection, IDbTransaction? transaction, int timeOut = 0)
		{
			IDbCommand cmd = connection.CreateCommand();
			cmd.Transaction = transaction;
			cmd.CommandText = sql;
			cmd.CommandType = CommandType.Text;
			if (timeOut > 0) cmd.CommandTimeout = timeOut;
			return cmd.ExecuteNonQuery();
		}

		protected int ExecuteStatements(IEnumerable<string> statements, IDbConnection connection, IDbTransaction? transaction,
			bool commitAndRollback, int timeOut = 30)
		{
			int rowsAffected = 0, n = 0;
			if (commitAndRollback && transaction == null) transaction = connection.BeginTransaction();
			foreach (string sql in statements)
			{
				n++;
				IDbCommand cmd = connection.CreateCommand();
				cmd.Transaction = transaction;
				cmd.CommandText = sql;
				cmd.CommandType = CommandType.Text;
				cmd.CommandTimeout = timeOut;
				try
				{
					rowsAffected += cmd.ExecuteNonQuery();
				}
				catch (Exception e)
				{
					string msg = $"SQL failed at index {n} with command: {cmd.CommandText}";
					if (commitAndRollback) transaction?.Rollback();  // Roll back the transaction
					throw new DataException(msg, e);
				}
			}
			if (commitAndRollback) transaction?.Commit();
			return rowsAffected;
		}

		protected void ExecuteCustomReader(string sql, Action<IDataReader> read, IDbConnection connection, IDbTransaction? transaction, int timeOut)
		{
			using (IDbConnection conn = CreateConnection(true))
			{
				IDbCommand cmd = conn.CreateCommand();
				cmd.CommandText = sql;
				cmd.CommandType = CommandType.Text;
				cmd.CommandTimeout = timeOut;
				cmd.Transaction = transaction;
				using (var rdr = cmd.ExecuteReader())
				{
					while (rdr.Read()) read(rdr);
				}
			}
		}

		protected int ExecuteProcedure(IDbConnection connection, IDbTransaction? transaction, string procedureName, params (string name, object value)[] parameters)
		{
			var cmd = connection.CreateCommand();
			cmd.CommandText = procedureName;
			cmd.CommandType = CommandType.StoredProcedure;
			cmd.Transaction = transaction;
			foreach (var p in parameters)
			{
				var pram = cmd.CreateParameter();
				pram.ParameterName = p.name;
				pram.Value = p.value;
				pram.Direction = ParameterDirection.Input;
				cmd.Parameters.Add(pram);
			}
			return cmd.ExecuteNonQuery();
		}

		protected int Add<T>(IEnumerable<T> values, IDbConnection connection, IDbTransaction transaction, int timeOut = 30)
		{
			if (values == null) return 0;
			Loader loader = CreateLoader(typeof(T));
			if (loader.PrimaryKeyCount == 0) throw new ArgumentException("Insertion requires a primary key.");
			List<string> statements = new();
			int nAdded = 0;
			foreach(T item in values)
			{
				if (item is null) continue;
				string statement = string.Empty;
				switch(loader.CreateInsertStatement(item))
				{
					case NoStatement: continue;
					case SqlStatement ss: statement = ss.Sql; break;
				}
				IDbCommand cmd = connection.CreateCommand();
				cmd.CommandText = statement;
				cmd.CommandType = CommandType.Text;
				cmd.Transaction = transaction;
				cmd.CommandTimeout = timeOut;
				if (cmd.ExecuteNonQuery() == 1) nAdded++;
			}
			return nAdded;
		}

		protected List<T> Insert<T>(IEnumerable<T> values, IDbConnection connection, IDbTransaction transaction, int timeOut = 30) where T : class
		{
			Loader loader = CreateLoader(typeof(T));
			if (loader.PrimaryKeyCount == 0) throw new ArgumentException("Insertion requires a primary key.");
			List<T> r = new();
			if (values == null) return r;
			List<string> statements = new();
			foreach (T item in values)
			{
				if (item is null) continue;
				string statement = string.Empty;
				switch (loader.CreateInsertStatement(item))
				{
					case NoStatement: continue;
					case SqlStatement ss: statement = ss.Sql; break;
				}
				IDbCommand cmd = connection.CreateCommand();
				cmd.CommandText = statement;
				cmd.CommandType = CommandType.Text;
				cmd.Transaction = transaction;
				cmd.CommandTimeout = timeOut;
				string where = string.Empty;
				if (cmd.ExecuteNonQuery() == 1) // 1 row inserted
				{
					if (loader.HasCompoundKey || !loader.IsKeyAutoIncrement) where = loader.PrimaryKeyWhereClause(item);
					else
					{
						cmd = connection.CreateCommand();
						cmd.CommandType = CommandType.Text;
						cmd.CommandText = "select LAST_INSERT_ID()";
						cmd.Transaction = transaction;	
						object? oid = cmd.ExecuteScalar();
						int id = 0;
						switch (oid)
						{
							case long l: id = (int)l; break;
							case ulong ul: id = (int)ul; break;
							case null: throw new Exception("Unable to acquire LAST_INSERT_iD");
						}
						where = $"where {loader.PrimaryKeyName}={id}";
					}
					cmd = connection.CreateCommand();
					cmd.Transaction = transaction;
					cmd.CommandText = $"select * from {loader.TableName} {where}";
					cmd.CommandType = CommandType.Text;
					using var rdr = cmd.ExecuteReader();
					if (rdr.Read()) r.Add((T)loader.CreateAndPopulate(rdr)); else throw new Exception($"Unable to load newly inserted value.");
				}
				else throw new Exception("Value not inserted");
			}
			return r;
		}

		protected int Update<T>(IEnumerable<T> values, IDbConnection connection, IDbTransaction transaction, int timeOut = 30)
		{
			Loader loader = CreateLoader(typeof(T));
			List<string> statements = ApplySqlOnly(values.Select(v => loader.CreateUpdateStatement(v))).ToList();
			if (statements.Count == 0) return 0;
			return ExecuteStatements(statements, connection, transaction, false, timeOut);
		}

		protected int Delete<T>(IEnumerable<T> values, IDbConnection connection, IDbTransaction transaction, int timeOut = 30)
		{
			Loader loader = CreateLoader(typeof(T));
			List<string> statements = ApplySqlOnly(values.Select(v => loader.CreateDeleteStatement(v))).ToList();
			if (statements.Count == 0) return 0;
			return ExecuteStatements(statements, connection, transaction, true, timeOut);
		}

		#endregion

	}
}