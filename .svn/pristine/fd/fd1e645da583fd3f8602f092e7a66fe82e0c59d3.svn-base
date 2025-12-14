using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Sql.Lib.Services.Schemas
{
	public class Table : IComparable<Table>
	{
		internal static string PascalCased(string dbIdentifier)
		{
			StringBuilder s = new StringBuilder();
			string[] parts = dbIdentifier.Split('_');
			foreach (string p in parts)
			{
				if (p.Length == 0) continue;
				s.Append(Char.ToUpper(p[0]));
				if (p.Length > 1) s.Append(p.Substring(1));
			}
			return s.ToString();
		}

		internal static string SingularOf(string name)
		{
			if (name.EndsWith("s"))
			{
				if (name.EndsWith("ies")) return name.Substring(0, name.Length - 3) + "y";
				if (name.EndsWith("ses")) return name.Substring(0, name.Length - 2);
				return name.Substring(0, name.Length - 1);
			}
			return name;
		}

		internal Table()
		{
			Fields = new List<Field>();
		}

		internal Table(string name) : this()
		{
			Name = name;
		}

		public string Name { get; internal set; } = string.Empty;
		public List<Field> Fields { get; internal set; }
		public string ClassName => PascalCased(Name);
		public string SingularClassName => SingularOf(ClassName);
		internal bool ForeignKeysApplied { get; set; }
		public int PrimaryKeyCount => Fields.Where(f => f.IsPrimaryKey).Count();
		public bool HasCompoundKey => PrimaryKeyCount > 1;
		public IEnumerable<Field> PrimaryKeys => Fields.Where(f => f.KeyType == KeyType.Primary);

		public string GenerateModelClassDefinition(string nameSpace, bool singularize = false, bool generateEqualsMethod = false,
			bool implementICopyFrom = false)
		{
			string className = singularize ? SingularOf(ClassName) : ClassName;
			string iCopyFrom = implementICopyFrom ? $": ICopyFrom<{className}>" : string.Empty;
			StringWriter sw = new StringWriter();
			IndentedTextWriter output = new IndentedTextWriter(sw, "\t");
			if (implementICopyFrom) output.WriteLine("using CommonTools.Lib.Contracts;");
			output.WriteLine("using CommonTools.Lib.SQL;");
			output.WriteLine("using System;");
			if (generateEqualsMethod) output.WriteLine("using System.Linq;");
			output.WriteLine();
			output.WriteLine($"namespace {nameSpace}");
			output.WriteLine("{");
			output.Indent += 1;
			output.WriteLine("[DataTable(TableName)]");
			output.WriteLine($"public partial class {className}{iCopyFrom}");
			output.WriteLine("{");
			output.Indent += 1;
			output.WriteLine($"public const string TableName = \"{Name}\";");
			output.WriteLine();
			output.WriteLine($"public {className}() {{ }}");
			output.WriteLine();
			if (implementICopyFrom)
			{
				output.WriteLine($"public {className}({className} o): this()");
				output.WriteLine("{");
				output.Indent++;
				output.WriteLine("CopyFrom(o);");
				output.Indent--;
				output.WriteLine("}");
				output.WriteLine();
			}
			output.WriteLine("#region Table Fields");
			output.WriteLine();
			int nField = 0;
			foreach (Field f in Fields)
			{
				string name = f.PropertyName;
				if (string.Equals(name, className)) name += "Data";
				string skey = string.Empty;
				if (f.KeyType == KeyType.Primary) skey = ", IsPrimaryKey = true";
				if (f.SystemType == typeof(DateTime)) skey += ", FormatString = \"yyyy-MM-dd hh:mm:ss.fff";
				if (0 < nField++) output.WriteLine();
				output.WriteLine($"[DataField(\"{f.Name}\"{skey})]");
				output.WriteLine($"public {f.TypeAlias} {name} {{ get; set; }}");
			}
			output.WriteLine();
			output.WriteLine("#endregion");
			if (implementICopyFrom)
			{
				output.WriteLine();
				ImplementICopyFrom(output, className);
			}
			if (generateEqualsMethod)
			{
				output.WriteLine();
				output.WriteLine("#region Equals/GetHashcode overrides");
				output.WriteLine();
				GenerateEqualsMethod(output, className);
				output.WriteLine();
				output.WriteLine("#endregion");
			}
			output.Indent -= 1;
			output.WriteLine("}");    // End class definition
			output.Indent -= 1;
			output.WriteLine("}");    // End namespace
			output.Flush();
			sw.Flush();
			return sw.ToString();
		}

		private void GenerateEqualsMethod(IndentedTextWriter output, string className)
		{
			char oname = char.ToLower(className[0]);
			if (oname == 'o') oname = 'v';
			output.WriteLine("public override bool Equals(object o)");
			output.WriteLine("{");
			output.Indent++;
			output.WriteLine($"if (!(o is {className} {oname})) return false;");
			output.Write("return ");
			int n = 0;
			foreach(Field f in Fields)
			{
				if (n++ > 0) output.WriteLine(" &&");
				string name = f.PropertyName;
				if (string.Equals(name, className)) name += "Data";
				switch(f.TypeAlias)
				{
					case "int":
					case "long":
					case "uint":
					case "ulong":
					case "bool":
					case "short":
					case "byte":
					case "sbyte": output.Write($"{name} == {oname}.{name}"); break;
					case "float":
					case "decimal":
					case "double":	output.Write($"DoubleComparer.AreEqual({name}, {oname}.{name})"); break;
					case "string":	output.Write($"string.Equals({name}, {oname}.{name})"); break;
					case "DateTime":	output.Write($"DateTime.Equals({name}, {oname}.{name})"); break;
					case "byte[]":	output.Write($"{name}.SequenceEqual({oname}.{name})"); break;
				}
			}
			output.WriteLine(";");
			output.Indent--;
			output.WriteLine("}");
			output.WriteLine();
			output.Write($"public override int GetHashCode() => ");
			Field? pk = Fields.FirstOrDefault(ff => ff.KeyType == KeyType.Primary);
			if (pk == null || pk.SystemType != typeof(int))
			{
				output.WriteLine($"base.GetHashCode();");
			} else
			{
				output.WriteLine($"{pk.PropertyName};");
			}
			output.WriteLine();
		}

		private void ImplementICopyFrom(IndentedTextWriter output, string className)
		{
			char oname = char.ToLower(className[0]);
			if (oname == 'o') oname = 'v';
			output.WriteLine($"public void CopyFrom({className} {oname})");
			output.WriteLine("{");
			output.Indent++;
			output.WriteLine($"if ({oname} != null)");
			output.WriteLine("{");
			output.Indent++;
			foreach(Field f in Fields)
			{
				string name = f.PropertyName;
				if (name == className) name += "Data";
				output.WriteLine($"{name} = {oname}.{name};");
			}
			output.Indent--;
			output.WriteLine("}");
			output.Indent--;
			output.WriteLine("}");
		}

		public string GenerateCreateTableStatement()
		{
			StringWriter sw = new StringWriter();
			IndentedTextWriter output = new IndentedTextWriter(sw, "\t");
			output.WriteLine($"create table {Name.Replace(" ", string.Empty)}(");
			output.Indent = 1;
			int nField = 0;
			bool compoundKey = Fields.Count(f => f.KeyType.HasFlag(KeyType.Primary)) > 1;
			foreach (Field f in Fields)
			{
				if (nField > 0) output.WriteLine();
				bool isPk = f.KeyType.HasFlag(KeyType.Primary), isFk = f.KeyType.HasFlag(KeyType.Foreign);
				output.Write($"{f.Name} {f.Type}");
				if (!isPk && !f.Nullable) output.Write(" NOT NULL");
				if (isPk)
				{
					if (!isFk && !compoundKey && f.Type == "int") output.Write(" AUTO_INCREMENT");
					if (!compoundKey) output.Write(" PRIMARY KEY");
				}
				if (isFk && !isPk && f.ForeignKeyDetails != null)
				{
					output.Write($" REFERENCES {f.ForeignKeyDetails.ReferencedTableName}({f.ForeignKeyDetails.ReferencedColumnName})");
				}
				if (++nField < Fields.Count) output.Write(',');
			}
			if (compoundKey)
			{
				output.WriteLine(",");
				string names = string.Join(",", Fields.Where(f => f.KeyType.HasFlag(KeyType.Primary)).Select(f => f.Name));
				output.WriteLine($"constraint PRIMARY KEY({names})");
			}
			output.WriteLine();
			output.Indent = 0;
			output.WriteLine(");");
			output.Flush();
			sw.Flush();
			return sw.ToString();
		}

		public override bool Equals(object? obj)
		{
			return obj is Table t &&
				string.Equals(Name, t.Name) &&
				Fields.OrderBy(f => f.Name).SequenceEqual(t.Fields.OrderBy(f => f.Name));
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		int IComparable<Table>.CompareTo(Table? other)
		{
			return string.Compare(Name, other?.Name);
		}

		public override string ToString()
		{
			return $"{Name} ({Fields.Count} fields)";
		}
	}
}
