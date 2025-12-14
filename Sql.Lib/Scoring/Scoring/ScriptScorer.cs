using Sql.Lib.Parsing;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;

namespace Sql.Lib.Scoring
{
	public static class ScriptScorer
	{
		public static List<T> Score<T>(ISqlScorable scorable, ScoreItemGenerator<T> generator)
			where T : IScoreItem
		{
			if (scorable == null) throw new ArgumentNullException(nameof(scorable));
			if (generator == null) throw new ArgumentNullException(nameof(generator));
			if (scorable.RefResult == null) throw new ArgumentException($"{nameof(scorable)}.RefResult cannot be null.");
			ScoreItemGenerator<T> gen = generator;
			DataTable result = scorable.Result, refResult = scorable.RefResult;
			if (result == null)
			{
				result = refResult;
				gen = MockGenerator<T>.CreateMockGenerator(generator);
			}
			switch(scorable.Type)
			{
				case SqlType.DDL: return ScoreSchema(result, refResult, gen);
				case SqlType.DQL:
					int ofndx = SqlParser.OrderByFieldIndex(scorable.RefScript);
					return ScoreQuery(result, refResult, ofndx, gen);
				default: throw new NotSupportedException(scorable.Type.ToString());
			}
		}

		private static  List<T> ScoreSchema<T>(DataTable results, DataTable refResults, ScoreItemGenerator<T> generator)
			where T : IScoreItem
		{
			string fixInt(string dbType)
			{
				return RxInt.Match(dbType).Success ? "int" : dbType;
			}
			Dictionary<string, DataRow> resultFields = IndexRows(results, 0);
			List<T> items = new List<T>();
			items.Add(generator("# Fields", refResults.Rows.Count, results.Rows.Count, 2));
			T gen(string name, object expectedValue, object observedValue, double weight, bool ignoreCase = false)
			{
				var r = generator(name, expectedValue, observedValue, weight);
				r.IgnoreCase = ignoreCase;
				return r;
			}
			foreach(DataRow row in refResults.Rows)
			{
				string fieldName = ((string)row[0]).ToLower();
				if (!resultFields.ContainsKey(fieldName))
				{
					items.Add(gen("Missing Field", fieldName, string.Empty, 2));
					continue;
				}
				var refData = SchemaRowValues(row);
				var resData = SchemaRowValues(resultFields[fieldName]);
				items.Add(gen($"{fieldName} type", fixInt(refData.Item2), fixInt(resData.Item2), 1, true));
				items.Add(gen($"{fieldName} allow nulls", refData.Item3, resData.Item3, 1));
				items.Add(gen($"{fieldName} Key", refData.Item4, resData.Item4, 1));
				items.Add(gen($"{fieldName} Default", refData.Item5, resData.Item5, 1));
				items.Add(gen($"{fieldName} Extra", refData.Item6, resData.Item6, 1, true));
				resultFields.Remove(fieldName);
			}
			foreach(string fieldName in resultFields.Keys)
			{
				items.Add(gen($"Unexpected Field", string.Empty, fieldName, 1));
			}
			return items.Where(i => !IsDoubleNull(i)).ToList();
		}

		private static void NormalizeTypeNames(ref string rtype, ref string type)
		{
			if (rtype == "Int32" && type == "Int64") rtype = type;
			else
				if (rtype == "Int64" && type == "Int32") type = rtype;
		}

		private static List<T> ScoreQuery<T>(DataTable result, DataTable refResult, int orderByFieldIndex, 
			ScoreItemGenerator<T> generator)
			where T : IScoreItem
		{
			List<T> items = new List<T>();
			items.Add(generator("FieldCount", refResult.Columns.Count, result.Columns.Count, 2));
			for(int nc=0;nc<refResult.Columns.Count;++nc)
			{
				DataColumn rc = refResult.Columns[nc];
				if (nc < result.Columns.Count)
				{
					DataColumn c = result.Columns[nc];
					items.Add(generator($"FieldName{nc + 1}", rc.ColumnName, c.ColumnName, 1));
					string rtype = rc.DataType.Name, type = c.DataType.Name;
					NormalizeTypeNames(ref rtype, ref type);
					items.Add(generator($"FieldType{nc + 1}", rtype, type, 1));
				} else
				{
					items.Add(generator($"MissingField{nc + 1}", rc.ColumnName, string.Empty, 2));
				}
			}
			items.Add(generator("RecordCount", refResult.Rows.Count, result.Rows.Count, 2));
			bool sameRowCount = items.Last().IsCorrect;
			for(int nc=0;nc<refResult.Columns.Count;++nc)
			{
				if (nc >= result.Columns.Count) break;
				DataColumn dc = refResult.Columns[nc], uc = result.Columns[nc];
				List<object> refVals = ExtractFields(refResult, dc), vals = ExtractFields(result, uc);
				if (orderByFieldIndex == nc) items.Add(generator($"{dc.ColumnName}:Sorted", true, vals.SequenceEqual(refVals), 1));
				if (sameRowCount)
				{
					refVals.Sort();
					vals.Sort();
					items.Add(generator($"{dc.ColumnName}:Values", "AllFound", vals.SequenceEqual(refVals) ? "AllFound" : "NotFound", 1));
				} else
				{
					items.Add(generator($"{dc.ColumnName}:Values", "AllFound", "NotFound", 1));
				}
			}
			return items.Where(i => !IsDoubleNull(i)).ToList();
		}

		private static Dictionary<string, DataRow> IndexRows(DataTable table, int column)
		{
			Dictionary<string, DataRow> r = new Dictionary<string, DataRow>();
			foreach (DataRow row in table.Rows)
			{
				string key = ((string)row[column]).ToLower();
				r.Add(key, row);
			}
			return r;
		}

		private static Tuple<string, string, bool, string, string, string> SchemaRowValues(DataRow row)
		{
			return Tuple.Create(
				row[0] as string,
				row[1] as string,
				(bool)row[2],
				row[3] as string,
				row[4] as string,
				row[5] as string);
		}

		private static bool IsDoubleNull(IScoreItem item)
		{
			if (item.ExpectedValue is null && item.ObservedValue is null) return true;
			if (item.ExpectedValue is string && item.ObservedValue is string)
			{
				string ev = (string)item.ExpectedValue, eo = (string)item.ObservedValue;
				if (string.IsNullOrEmpty(ev) && string.IsNullOrEmpty(eo)) return true;
			}
			return false;
		}

		private static List<object> ExtractFields(DataTable table, DataColumn c)
		{
			List<object> r = new List<object>(table.Rows.Count);
			bool isInt = c.DataType.Name == "Int32";
			foreach (DataRow row in table.Rows)
			{
				object v = row[c];
				if (isInt)
				{
					int i = (int)v;
					v = (long)i;
				}
				r.Add(v);
			}
			return r;
		}

		private static readonly Regex RxInt = new Regex(@"(?<![a-z])int(?<len>\(\d+\))", RegexOptions.Compiled);

		private class MockGenerator<T> where T : IScoreItem
		{
			public static ScoreItemGenerator<T> CreateMockGenerator(ScoreItemGenerator<T> generator)
			{
				return new MockGenerator<T>(generator).Generate;
			}

			private ScoreItemGenerator<T> _baseGenerator;
			private MockGenerator(ScoreItemGenerator<T> gen)
			{
				_baseGenerator = gen;
			}

			private T Generate(string name, object expectedValue, object observedValue, double weight)
			{
				return _baseGenerator(name, expectedValue, null, weight);
			}
		}

	}
}
