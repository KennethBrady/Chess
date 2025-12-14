using MySql.Data.MySqlClient;
using Sql.Lib.Services.Schemas;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Sql.Lib.Services
{
	/// <summary>
	/// Implementation of SqlService for MySql / MariaDB
	/// </summary>
	public class MySqlService : SqlService
	{
		private readonly string _connString;
		private Lazy<AsyncImpl> _asyncImpl;
		public MySqlService(string connectionString)
		{
			_connString = connectionString;
			_asyncImpl = new Lazy<AsyncImpl>(() => new AsyncImpl(this));
		}

		public override string ConnectionString => _connString;
		public override IDbConnection CreateConnection(bool open = true)
		{
			MySqlConnection conn = new MySqlConnection(ConnectionString);
			if (open) conn.Open();
			return conn;
		}
		public IAsyncSqlService Async => _asyncImpl.Value;

		private static readonly Regex _dbRx = new Regex(@"[dD]atabase=(\w+)", RegexOptions.Compiled);
		public override string DatabaseName
		{
			get
			{
				Match match = _dbRx.Match(ConnectionString);
				if (!match.Success) throw new Exception($"Invalid connection string (database undefined): {ConnectionString}");
				return match.Groups[1].Value;
			}
		}

		private static string[] _systemDbs = { "sys", "mysql" };
		public override List<string> LoadDatabaseNames()
		{
			List<string> r = new();
			ExecuteCustomReader("show databases", rdr =>
			{
				string dbname = rdr.GetString(0);
				if (!dbname.EndsWith("_schema") && !_systemDbs.Contains(dbname)) r.Add(dbname);
			});
			return r;
		}

		public override Table LoadTableSchema(string tableName, bool ensureForeignKeys = false)
		{
			Table table = new Table { Name = tableName };
			using (var conn = CreateConnection())
			{
				var cmd = conn.CreateCommand();
				cmd.CommandText = $"describe {tableName}";
				using (var rdr = cmd.ExecuteReader())
				{
					while (rdr.Read())
					{
						table.Fields.Add(new Field(rdr));
					}
				}
				if (ensureForeignKeys)
				{
					table.ForeignKeysApplied = true;
					string sql = $"select column_name, constraint_name, referenced_table_name, referenced_column_name from information_schema.key_column_usage where table_schema='{DatabaseName}' and table_name='{tableName}' and referenced_column_name is not null";
					cmd = conn.CreateCommand();
					cmd.CommandText = sql;
					using (var rdr = cmd.ExecuteReader())
					{
						while (rdr.Read())
						{
							string fieldName = rdr.GetString(0);
							Field? f = table.Fields.FirstOrDefault(ff => ff.Name == fieldName);
							if (f != null)
							{
								f.SetForeignKey(rdr.GetString(1), rdr.GetString(2), rdr.GetString(3));
							}
						}
					}
				}
			}
			table.Fields.Sort((fx, fy) => string.Compare(fx.Name, fy.Name));
			return table;

		}

		public override List<string> LoadTableNames()
		{
			// NOTE:  Does not work with information_schema
			List<string> r = new List<string>();
			ExecuteCustomReader("show full tables", (rdr) =>
			{
				// Exclude views:
				string type = rdr.GetString(1);
				if (type == "BASE TABLE") r.Add(rdr.GetString(0));
			});
			return r;
		}

		public override bool CanExecuteScript => true;

		public override int ExecuteScript(string script, int minTimeout = 30)
		{
			using MySqlConnection conn = new MySqlConnection(TimeOutCS(minTimeout));
			MySqlScript mySqlScript = new MySqlScript(conn, script);
			return mySqlScript.Execute();
		}

		public override Task<int> ExecuteScriptAsync(string script, Action<ScriptCallback> callback, CancellationToken cancellationToken, int minTimeOut = 30)
		{
			using MySqlConnection conn = new MySqlConnection(TimeOutCS(minTimeOut));
			MySqlScript mySqlScript = new MySqlScript(conn, script);
			DateTime now = DateTime.Now, prev = now;
			if (callback != null)
			{
				mySqlScript.StatementExecuted += (o, e) =>
				{
					TimeSpan tx = DateTime.Now - prev;
					callback.Invoke(new ScriptCallback(tx, e.StatementText));
					prev = DateTime.Now;
				};
			}
			return mySqlScript.ExecuteAsync(cancellationToken);

		}

		private class AsyncImpl : IAsyncSqlService
		{
			internal AsyncImpl(MySqlService service)
			{
				Service = service;
			}

			private MySqlService Service { get; init; }

			public Task<List<T>> LoadAll<T>() where T : class => Task<List<T>>.Factory.StartNew(Service.LoadAll<T>);

			public Task<List<T>> LoadWhere<T>(string whereClause) where T : class
			{
				List<T> load() => Service.LoadWhere<T>(whereClause);
				return Task<List<T>>.Factory.StartNew(load);
			}

		}
	}
}
