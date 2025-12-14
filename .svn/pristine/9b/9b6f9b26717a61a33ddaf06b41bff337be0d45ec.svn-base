using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Sql.Lib.Services.Schemas
{
	public class Field
	{
		internal static Field Create(string name, string type, bool nullable, string dflt)
		{
			return new Field
			{
				Name = name,
				Type = type,
				Nullable = nullable,
				Default = dflt
			};
		}

		internal static Field FromMSSql(IDataReader rdr, List<Tuple<string, string>> keyInfo)
		{
			Field f = new Field
			{
				Name = (string)rdr[1],
				Type = (string)rdr[2],
				Nullable = rdr.GetBoolean(6)
			};
			if (f.Type.Contains("char"))
			{
				f.Type = string.Concat(f.Type, "(", rdr[3], ")");
			}
			foreach (var ki in keyInfo.Where(tt => tt.Item1 == f.Name))
			{
				switch (ki.Item2)
				{
					case "FOREIGN KEY": f.KeyType |= KeyType.Foreign; break;
					case "PRIMARY KEY": f.KeyType |= KeyType.Primary; break;
				}
			}
			return f;
		}

		internal Field(IDataReader rdr)
		{
			Name = (string)rdr["Field"];
			Type = (string)rdr["Type"];
			string n = (string)rdr["Null"];
			Nullable = n == "YES";
			string k = (string)rdr["Key"];
			switch (k)
			{
				case "PRI": KeyType = KeyType.Primary; break;
				case "MUL": KeyType = KeyType.Foreign; break;
				default: KeyType = KeyType.None; break;
			}
			Extra = (string)rdr["Extra"];
		}

		private Field() { }

		public string Name { get; private set; } = string.Empty;
		public string Type { get; private set; } = string.Empty;
		public bool Nullable { get; private set; }
		public KeyType KeyType { get; private set; }
		public bool IsPrimaryKey => KeyType.HasFlag(KeyType.Primary);
		public bool IsForeignKey => KeyType.HasFlag(KeyType.Foreign);
		public string Default { get; private set; } = string.Empty;
		public string Extra { get; private set; } = string.Empty;
		public bool HasForeignKeyDetails => (ForeignKeyDetails != null);
		public ForeignKey? ForeignKeyDetails { get; private set; }
		public bool IsAutoIncrement => string.Equals(Extra, "auto_increment");

		public Type SystemType
		{
			get
			{
				string type = Type.ToLower();
				int n = type.IndexOf('(');
				if (n > 0) type = type.Substring(0, n);
				switch (type)
				{
					case "int": return typeof(int);
					case "date":	// TODO: support DateOnly
					case "datetime": return typeof(DateTime);
					case "varchar":
					case "nvarchar":
					case "char":
					case "nchar":
					case "longtext":
					case "ntext":
					case "sysname":
					case "mediumtext":
					case "text": return typeof(string);
					case "tinyint": return typeof(bool);
					case "real":
					case "double": return typeof(double);
					case "money": return typeof(decimal);
					case "bigint": return typeof(long);
					case "smallint": return typeof(short);
					case "blob":
					case "longblob":
					case "image":
					case "varbinary": return typeof(byte[]);
					case "float": return typeof(float);
				}
				throw new DataException($"Unknown Type: {Type}");
			}
		}

		public string TypeAlias
		{
			get
			{
				string type = Type.ToLower();
				int n = type.IndexOf('(');
				if (n > 0) type = type.Substring(0, n);
				bool isUnsigned = Type.IndexOf("unsigned") > 0;
				switch (type)
				{
					case "int": return isUnsigned ? "uint" : "int";
					case "date":
					case "timestamp":
					case "datetime": return "DateTime";
					case "varchar":
					case "char":
					case "mediumtext":
					case "longtext":
					case "tinytext":
					case "text": return "string";
					case "tinyint": return "bool";
					case "double": return type;
					case "bigint": return isUnsigned ? "ulong" : "long";
					case "smallint": return "short";
					case "mediumblob":
					case "longblob":
					case "blob": return "byte[]";
					case "decimal": return "decimal";
					case "bit": return "sbyte";
					case "float": return "float";
				}
				throw new DataException($"Unknown Type: {Type}");
			}
		}

		public string PropertyName
		{
			get
			{
				return Table.PascalCased(Name);
			}
		}

		public override string ToString()
		{
			string snull = Nullable ? "YES" : "NO";
			string sdef = string.IsNullOrEmpty(Default) ? "NULL" : Default;
			return $"{Name}\t{Type}\t{snull}\t{KeyType}\t{sdef}\t{Extra}";
		}

		public override bool Equals(object? obj)
		{
			return obj is Field f &&
				string.Equals(Name, f.Name) &&
				string.Equals(Type, f.Type) &&
				Nullable == f.Nullable &&
				KeyType == f.KeyType &&
				string.Equals(Default, f.Default) &&
				string.Equals(Extra, f.Extra);
		}

		public override int GetHashCode()
		{
			return ToString().GetHashCode();
		}

		internal void SetForeignKey(string constraintName, string refTableName, string refColName)
		{
			KeyType |= KeyType.Foreign;
			ForeignKeyDetails = new ForeignKey(constraintName, refTableName, refColName);
		}
	}
}
