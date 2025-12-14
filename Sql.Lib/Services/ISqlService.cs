using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System.Threading;
using Sql.Lib.Services.Schemas;


namespace Sql.Lib.Services
{
	public interface ISchemaProvider
	{
		Table LoadTableSchema(string tableName, bool ensureForeignKeys = false);
		List<string> LoadTableNames();
		string ConnectionString { get; }
		string DatabaseName { get; }
		ITypeLoader CreateLoaderFor<T>();
		List<string> LoadDatabaseNames();
	}

	public interface IServiceExecution
	{
		IDbConnection CreateConnection(bool open = true);
		object? ExecuteScalar(string sqlCmd, int timeout = 30);
		void ExecuteCustomReader(string sqlCmd, Action<IDataReader> read, int timeout = 30);

		/// <summary>
		/// Execute an update, insert or delete statement.
		/// </summary>
		/// <param name="sql"></param>
		/// <param name="timeout"></param>
		/// <returns># rows affected</returns>
		int ExecuteNonQuery(string sql, int timeout = 30);
		int ExecuteStatements(IEnumerable<string> statements, bool useTransaction = true, int timeOut = 30);
	}

	public interface IServiceAction
	{
		List<T> LoadAll<T>() where T : class;
		List<T> LoadWhere<T>(string whereClause, int timeOut = 30) where T : class;
		List<T> Insert<T>(IEnumerable<T> values, int timeOut = 30) where T : class;
		T InsertOne<T>(T value, int timeOut = 30) where T : class;

		/// <summary>
		/// Performs the same as Insert, but does not return newly created values.  This can greatly improve performance for large inserts.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="values"></param>
		/// <param name="timeOut"></param>
		/// <returns># rows added</returns>
		int Add<T>(IEnumerable<T> values, int timeOut = 30) where T :class;

		/// <summary>
		/// Update values.  This overload generates insert statements for all data-bound fields.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="values"></param>
		/// <param name="timeOut"></param>
		/// <returns># rows updated</returns>
		long Update<T>(IEnumerable<T> values, int timeOut = 30) where T : class;
		bool UpdateOne<T>(T value) where T : class;
		/// <summary>
		/// This version of update generates sql statements only for changed fields.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="values"></param>
		/// <param name="timeOut"></param>
		/// <returns></returns>
		long Update<T>(IEnumerable<ValuePair<T>> values, int timeOut = 30) where T : class;
		int Delete<T>(IEnumerable<T> values) where T : class;
		int DeleteOne<T>(T value) where T : class;
		int ExecuteProcedure(string procedureName, params (string name, object value)[] parameters);
		/// <summary>
		/// Test whether script execution is supported.
		/// </summary>
		bool CanExecuteScript { get; }
		int ExecuteScript(string script, int timeOut = 30);
		Task<int> ExecuteScriptAsync(string script, Action<ScriptCallback> callback, CancellationToken cancellationToken, int minTimeOut = 30);
	}

	public interface ITransactedService : ISqlService, IDisposable
	{
		bool IsTransactionCompleted { get; }
		bool IsDisposed { get; }
		void Rollback();
		void Commit();
	}

	public interface IExtendedService
	{
		ITransactedService CreateTransactedService(bool commitByDefault = true);
	}

	public interface ISqlService : ISchemaProvider, IServiceExecution, IServiceAction, IExtendedService;

	public interface IAsyncSqlService
	{
		Task<List<T>> LoadAll<T>() where T : class;
		Task<List<T>> LoadWhere<T>(string whereClause) where T : class;
	}
}
