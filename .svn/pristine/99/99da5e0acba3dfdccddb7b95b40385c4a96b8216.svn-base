using CommonTools.Lib.SQL.Schemas;
using CommonTools.Lib.SQL.Services;
using Sql.Lib.Parsing;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Sql.Lib.Scoring
{
	public static class SqlRunner
	{
		private const string NOSCRIPT = "No script provided.";

		public static Task<int> RunCorrelatedAsync(IEnumerable<ISqlScoreExportable> sqls)
		{
			if (!ValidateSqls(sqls)) return Task<int>.Factory.StartNew(() => 0);
			return Task<int>.Factory.StartNew(() => RunCorrelated(sqls));
		} 

		public static int RunCorrelated(IEnumerable<ISqlScoreExportable> sqls)
		{
			if (!ValidateSqls(sqls)) return 0;
			SqlType type = sqls.First().Type;
			switch(type)
			{
				case SqlType.DDL: return RunSchemas(sqls);
				case SqlType.DQL:	return RunQueries(sqls);
			}
			throw new NotSupportedException($"SqlType {type} is not supported.");
		}

		public static Task<int> RunUncorrelatedAsync(IEnumerable<ParseResult> scripts, IEnumerable<ISqlScoreExportable> schemas)
		{
			return Task<int>.Factory.StartNew(() => RunUncorrelated(scripts, schemas));
		}

		public static Task<List<Table>> RunSchemaAsync(IEnumerable<string> ddlStatements)
		{
			return Task<List<Table>>.Factory.StartNew(() => RunSchema(ddlStatements));
		}

		private static List<Table> RunSchema(IEnumerable<string> ddlStatements)
		{
			using (var tmpDb = SqlScriptService.CreateTempDbService())
			{
				tmpDb.ExecuteSchemas(ddlStatements);
				return tmpDb.GetTableSchema();
			}
		}

		private static int RunUncorrelated(IEnumerable<ParseResult> scripts, IEnumerable<ISqlScoreExportable> schemas)
		{
			if (scripts == null) throw new ArgumentNullException(nameof(scripts));
			if (scripts.Count() == 0) throw new ArgumentException("No scripts to run.");
			if (!scripts.All(s => s.Type == SqlType.DDL)) throw new ArgumentException($"{nameof(RunUncorrelated)} can only run DDL statements");
			if (!ValidateSqls(schemas)) return 0;
			Dictionary<string, string> errorsByTableName = new Dictionary<string, string>();
			void addError(string tableName, string err)
			{
				if (!errorsByTableName.ContainsKey(tableName)) errorsByTableName.Add(tableName, err);
				else
				{
					errorsByTableName[tableName] = string.Concat(errorsByTableName[tableName], Environment.NewLine, err);
				}
			}
			int nSuccess = 0;
			using (var tmpDb = SqlScriptService.CreateTempDbService())
			{
				const string OTHER = "OTHER";
				foreach(var pr in scripts)
				{
					string tableName = OTHER;
					CreateParseResult cpr = pr as CreateParseResult;
					if (cpr != null)
					{
						if (cpr.TargetType == "table") tableName = cpr.TargetName;
					}
					try
					{
						tmpDb.ExecuteSchema(pr.FullScript);
					}
					catch(Exception ex)
					{
						addError(tableName, ex.Message);
					}
				}
				var refService = SqlScriptService.ServiceFor(schemas.First().ReferenceDb);
				foreach(var schema in schemas)
				{
					SqlParser.TryParseCreateTable(schema.RefScript, out string tableName);
					DataTable refTable = schema.RefResult, userTable = null;
					if (refTable == null) refTable = CreateSchemaTable(refService.LoadTableSchema(tableName, true));
					try
					{
						userTable = CreateSchemaTable(tmpDb.GetTableSchema(tableName));
						nSuccess++;
					}
					catch(Exception ex)
					{
						addError(tableName, ex.Message);
					}
					string errMsg = null;
					if (errorsByTableName.ContainsKey(tableName)) errMsg = errorsByTableName[tableName];
					schema.SetResult(userTable, refTable, errMsg);
				}
			}
			return nSuccess;
		}

		private static bool ValidateSqls(IEnumerable<ISqlScoreExportable> sqls)
		{
			if (sqls == null) throw new ArgumentNullException(nameof(sqls));
			if (sqls.Count() == 0) return false;
			SqlType type = sqls.First().Type;
			if (!sqls.All(s => s.Type == type)) throw new ArgumentException("All scripts must be the same SqlType.");
			string refDb = sqls.First().ReferenceDb;
			if (!sqls.All(s => s.ReferenceDb == refDb)) throw new ArgumentException("All scripts must use the same reference database.");
			if (!sqls.All(s => !string.IsNullOrEmpty(s.RefScript))) throw new ArgumentException("All reference scripts must be set.");
			return true;
		}

		private static int RunQueries(IEnumerable<ISqlScoreExportable> queries)
		{
			ISqlService service = SqlScriptService.ServiceFor(queries.First().ReferenceDb);
			foreach(var q in queries)
			{
				DataTable refTable = q.RefResult, userTable = null;
				string errMsg = null;
				if (refTable == null) refTable = QueryResult(q.RefScript, service);
				try
				{
					if (string.IsNullOrEmpty(q.Script)) errMsg = NOSCRIPT;
						else userTable = QueryResult(q.Script, service);
				}
				catch(Exception ex)
				{
					errMsg = ex.Message;
				}
				q.SetResult(userTable, refTable, errMsg);
			}
			return 0;
		}

		private static int RunSchemas(IEnumerable<ISqlScoreExportable> schemas)
		{
			var refService = SqlScriptService.ServiceFor(schemas.First().ReferenceDb);
			int nSuccess = 0;
			using (var tmpDb = SqlScriptService.CreateTempDbService())
			{
				foreach (var schema in schemas)
				{
					SqlParser.TryParseCreateTable(schema.RefScript, out string tableName);
					DataTable refTable = schema.RefResult, userTable = null;
					if (refTable == null)
					{
						refTable = CreateSchemaTable(refService.LoadTableSchema(tableName, true));
					}
					string errMsg = null;
					if (string.IsNullOrEmpty(schema.Script)) errMsg = NOSCRIPT; else
					{
						try
						{
							tmpDb.ExecuteSchema(schema.Script);
							userTable = CreateSchemaTable(tmpDb.GetTableSchema(tableName));
							nSuccess++;
						}
						catch (Exception ex)
						{
							errMsg = ex.Message;
						}
					}
					schema.SetResult(userTable, refTable, errMsg);
				}
			}
			return nSuccess;
		}

		public static DataTable QueryResult(string qry, ISqlService service)
		{
			DataSet dataSet = new DataSet();
			using (var adapter = service.CreateAdapter(qry))
			{
				adapter.Fill(dataSet);
			}
			return dataSet.Tables[0];
		}

		private static DataTable CreateSchemaTable(Table schema)
		{
			DataTable r = new DataTable(schema.Name);
			r.Columns.Add(new DataColumn("Field Name", typeof(string)));
			r.Columns.Add(new DataColumn("Type", typeof(string)));
			r.Columns.Add(new DataColumn("Nullable", typeof(bool)));
			r.Columns.Add(new DataColumn("KeyType", typeof(string)));
			r.Columns.Add(new DataColumn("Default", typeof(string)));
			r.Columns.Add(new DataColumn("Extra", typeof(string)));
			foreach (Field f in schema.Fields)
			{
				string keyStr = (f.KeyType == KeyType.None) ? string.Empty : f.KeyType.ToString();
				object[] values = new object[6];
				values[0] = f.Name;
				values[1] = f.Type;
				values[2] = f.Nullable;
				values[3] = keyStr;
				values[4] = f.Default;
				values[5] = f.Extra;
				if (string.IsNullOrEmpty(f.Extra) && f.ForeignKeyDetails != null)
				{
					values[5] = f.ForeignKeyDetails.ToString();
				}
				r.Rows.Add(values);
			}
			r.AcceptChanges();
			return r;
		}

	}
}
