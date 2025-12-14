using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Sql.Lib.Services
{
	public interface ISqlClause
	{
		bool IsEmpty { get; }
		string AsSql { get; }
		string AsSqlNegated { get; }
	}

	public interface ISqlWhereClause : ISqlClause
	{
		string AsSqlWithoutWhere { get; }
		string AsSqlNegatedWithoutWhere { get; }
	}

	public interface ISqlInList : ISqlWhereClause
	{
		string FieldName { get; }
		string InList { get; }
	}

	public static class SqlClauses
	{
		public static readonly ISqlWhereClause Empty = new SqlInList(string.Empty, string.Empty);

		public static ISqlInList WhereInList(string fieldName, IEnumerable<int> keys)
		{
			return WhereInList<int>(fieldName, keys, false);
		}

		public static ISqlInList WhereInList(string fieldName, params int[] keys)
		{
			return WhereInList(fieldName, keys, false);
		}

		public static ISqlInList WhereInList(string fieldName, IEnumerable<string> values, bool useQuotes = true)
		{
			return WhereInList<string>(fieldName, values, useQuotes);
		}

		private static ISqlInList WhereInList<T>(string fieldName, IEnumerable<T> values, bool useQuote)
		{
			StringBuilder s = new StringBuilder();
			foreach (T t in values)
			{
				if (t == null) continue;
				string? sval = t.ToString();
				if (useQuote) sval = String.Concat("'", sval, "'");
				if (s.Length > 0) s.Append(",");
				s.Append(sval);
			}
			return new SqlInList(fieldName, s.ToString());
		}

		public static ISqlWhereClause Where(params string[] conditions) => new ConditionalWhereClause(conditions, "and");
		
		public static ISqlWhereClause Where(string fieldName, IEnumerable<int> values, params string[] conditions) =>
			new InListWithConditions((ISqlInList)WhereInList<int>(fieldName, values, false), new ConditionalWhereClause(conditions, "and"));

		/// <summary>
		/// Format a date for MySql/MariaDB
		/// </summary>
		/// <param name="date"></param>
		/// <returns></returns>
		public static string FormatDate(DateTime date) => $"'{date:yyyy-MM-dd hh:mm:ss}'";

		public static ISqlWhereClause And(ISqlWhereClause cl1, ISqlWhereClause cl2)
		{
			if (cl1.IsEmpty && cl2.IsEmpty) return SqlInList.Empty;
			if (cl1.IsEmpty) return cl2;
			if(cl2.IsEmpty) return cl1;
			return new ConditionalWhereClause([cl1.AsSqlWithoutWhere, cl2.AsSqlWithoutWhere], "and");
		}
	}

	[DebuggerDisplay("{AsSql}")]
	internal record struct InListWithConditions(ISqlInList InList, ISqlWhereClause Conditions) : ISqlWhereClause
	{
		public string AsSqlWithoutWhere => $"{Conditions.AsSqlWithoutWhere} and {InList.AsSqlWithoutWhere}";
		public string AsSqlNegatedWithoutWhere => $"{Conditions.AsSqlNegatedWithoutWhere} and {InList.AsSqlNegatedWithoutWhere}";
		public bool IsEmpty => InList.IsEmpty || Conditions.IsEmpty;
		public string AsSql => $"where {AsSqlWithoutWhere}";
		public string AsSqlNegated => $"where {AsSqlNegatedWithoutWhere}";
	}

	[DebuggerDisplay("{AsSql}")]
	internal record struct ConditionalWhereClause(string[] Conditions, string Connector) : ISqlWhereClause
	{
		public string AsSqlWithoutWhere => string.Join($" {Connector} ", Conditions);
		public string AsSqlNegatedWithoutWhere => $"not ({AsSqlWithoutWhere})";
		public bool IsEmpty => Conditions.Length == 0;
		public string AsSql => $"where {AsSqlWithoutWhere}";
		public string AsSqlNegated => $"where {AsSqlNegatedWithoutWhere}";
	}

	[DebuggerDisplay("{AsSql}")]
	internal record struct SqlInList(string FieldName, string InList) : ISqlInList
	{
		internal static readonly SqlInList Empty = new SqlInList(string.Empty, string.Empty);
		public bool IsEmpty => string.IsNullOrEmpty(InList);
		public string AsSqlWithoutWhere => $"{FieldName} in ({InList})";
		public string AsSqlNegatedWithoutWhere => $"{FieldName} not in ({InList})";
		public string AsSql => $"where {FieldName} in ({InList})";
		public string AsSqlNegated => $"where {FieldName} not in ({InList})";
		public static implicit operator string(SqlInList c) => c.AsSql;
	}
}
