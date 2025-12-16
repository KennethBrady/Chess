using System;
using System.Collections.Generic;
using System.Linq;

namespace Sql.Lib.Services
{
	/// <summary>
	/// Associate a C# type (record) with a database table
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
	public class DBTableAttribute : Attribute
	{
		private static string[] _emptyPaths = new string[0];
		private string[] _filePaths = _emptyPaths;
		private string _filePathFields = string.Empty;
		public DBTableAttribute(string tableName): this(tableName, string.Empty, string.Empty) { }

		public DBTableAttribute(string tableName, string fieldMapping): this(tableName, fieldMapping, string.Empty) { }

		public DBTableAttribute(string tableName, string fieldMapping, string filePathFields)
		{
			TableName = tableName ?? throw new ArgumentNullException(nameof(tableName));
			FieldMapping = fieldMapping;
			FilePathFields = filePathFields;
		}

		public string TableName { get; set; }
		public string? FieldMapping { get; set; } = string.Empty;

		/// <summary>
		/// Indicates that a field is a file-path.  This triggers replacement of the path charace '\' to be swapped with '/' for storage in MySql.
		/// </summary>
		public string FilePathFields
		{
			get => _filePathFields;
			set
			{
				_filePathFields = value;
				if (!string.IsNullOrEmpty(_filePathFields)) _filePaths = FilePathFields.Split(';');
			}
		}

		internal bool IsFilePathField(string fieldName) => _filePaths.Any(f => string.Equals(f, fieldName, StringComparison.OrdinalIgnoreCase));

		internal Dictionary<string,string> FieldMapper()
		{
			Dictionary<string, string> r = new();
			if (!string.IsNullOrEmpty(FieldMapping))
			{
				string[] maps = FieldMapping.Split(';');
				foreach(string map in maps)
				{
					string[] parts = map.Split(':');
					if (parts.Length == 2) r.Add(parts[0], parts[1]);
				}
			}
			return r;
		}

		internal static DBTableAttribute? FindAttribute(Type type)
		{
			var attrs = type.GetCustomAttributes(typeof(DBTableAttribute), false);
			if (attrs == null || attrs.Length == 0) return null;
			return (DBTableAttribute?)attrs[0];
		}
	}
}
