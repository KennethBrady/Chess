using CommonTools.Lib.SQL.Schemas;
using CommonTools.Lib.SQL.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sql.Lib
{
	internal static class SqlScriptService
	{
		private static Random _random = new Random();
		private const string _connectionBase = "server=localhost;user id=root;password=abcd;database=";
		private const string TEMPDB = "tempdb";
		private const string _connectionString = "server=localhost;user id=root;password=abcd;database=information_schema";
		private static readonly MySqlService _service = new MySqlService(_connectionString);

		internal static ISqlService ServiceFor(string databaseName)
		{
			return new MySqlService(_connectionBase + databaseName);
		}

		internal static ITempDb CreateTempDbService()
		{
			List<string> names = DatabaseNames();
			string name = TEMPDB + _random.Next(1, 10000);
			while (names.Contains(name)) name = TEMPDB + _random.Next(1, 10000);
			_service.ExecuteNonQuery($"create database {name}");
			ISqlService tmp = new MySqlService(_connectionBase + name);
			return new TempDbService(name, tmp);
		}

		private static List<string> DatabaseNames()
		{
			List<string> r = new List<string>();
			_service.ExecuteCustomReader("show databases", (rdr) =>
			{
				r.Add(rdr.GetString(0));
			});
			return r;
		}

		private class TempDbService : ITempDb
		{
			private string _dbName;
			private ISqlService _service;
			internal TempDbService(string dbName, ISqlService service)
			{
				_dbName = dbName;
				_service = service;
			}

			void ITempDb.ExecuteSchemas(IEnumerable<string> ddls)
			{
				_service.ExecuteStatements(ddls, false);
			}

			void ITempDb.ExecuteSchema(string ddl)
			{
				_service.ExecuteNonQuery(ddl);
			}

			Table ITempDb.GetTableSchema(string tableName)
			{
				return _service.LoadTableSchema(tableName, true);
			}

			List<Table> ITempDb.GetTableSchema()
			{
				return _service.LoadTableNames().Select(n => _service.LoadTableSchema(n, true)).ToList();
			}

			void IDisposable.Dispose()
			{
				_service.ExecuteNonQuery($"drop database {_dbName}");
			}
		}

		internal interface ITempDb : IDisposable
		{
			void ExecuteSchemas(IEnumerable<string> ddls);
			void ExecuteSchema(string ddl);
			Table GetTableSchema(string tableName);
			List<Table> GetTableSchema();
		}
	}
}