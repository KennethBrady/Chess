using CommonTools.Lib.SQL.Schemas;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Sql.Lib.Scoring
{
	public class TableMatch
	{
		public static TableMatch MissingTableMatch(Table referenceTable)
		{
			return new TableMatch($"Table {referenceTable.Name} is missing.")
			{
				ReferenceTable = referenceTable
			};
		}

		private List<string> _errors;
		public TableMatch(Table referenceTable, Table studentTable)
		{
			ReferenceTable = referenceTable ?? throw new ArgumentNullException(nameof(referenceTable));
			StudentTable = studentTable ?? throw new ArgumentNullException(nameof(studentTable));
			_errors = new List<string>();
			CompareTables();
		}

		private TableMatch(string message)
		{
			_errors = new List<string>();
			_errors.Add(message);
		}

		public Table ReferenceTable { get; private set; }
		public Table StudentTable { get; private set; }
		public bool HasErrors => _errors.Count > 0;
		public ReadOnlyCollection<string> Errors => _errors.AsReadOnly();

		private void CompareTables()
		{
			if (!string.Equals(ReferenceTable.Name, StudentTable.Name, StringComparison.OrdinalIgnoreCase)) _errors.Add("Table names do not match.");
			if (ReferenceTable.Fields.Count != StudentTable.Fields.Count) _errors.Add("Tables have different field counts.");
			int nFields = Math.Min(ReferenceTable.Fields.Count, StudentTable.Fields.Count);
			for(int i=0;i<nFields;++i)
			{
				Field fRef = ReferenceTable.Fields[i], fStud = StudentTable.Fields[i];
				if (!string.Equals(fRef.Name, fStud.Name, StringComparison.OrdinalIgnoreCase)) _errors.Add($"Field {fRef.Name} does not match.");
				if (fRef.Nullable != fStud.Nullable) _errors.Add($"{fRef.Name} nullability is incorrect.");
				if (fRef.Type.StartsWith("int"))
				{
					if (!fStud.Type.StartsWith("int")) _errors.Add($"{fRef.Name} type mismatch ({fRef.Type}/{fStud.Type})");
				} else
				{
					if (fRef.Type != fStud.Type) _errors.Add($"{fRef.Name} type mismatch: {fRef.Type}/{fStud.Type}");
				}
				if (fRef.KeyType != fStud.KeyType) _errors.Add($"Field KeyType Mismatch: {fRef.KeyType}/{fStud.KeyType}");
			}
		}
	}
}
