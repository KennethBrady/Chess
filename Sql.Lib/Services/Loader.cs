using Common.Lib.Extensions;
using Sql.Lib.Services.Schemas;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Sql.Lib.Services
{
	public interface ITypeLoader
	{
		Type ForType { get; }
		string TableName { get; }
		Table SchemaTable { get; }

		/// <summary>
		/// Construct and populate an instance of T based on known schema and values read from the reader.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="rdr"></param>
		/// <returns>A new and populated instance of T</returns>
		T CreateAndPopulate<T>(IDataReader rdr);
	}

	internal class Loader : ITypeLoader
	{
		private static readonly Dictionary<Type, Loader> _cache = new();
		private static readonly object _lock = new object();

		internal static Loader For(SqlService service, Type forType)
		{
			lock (_lock)  // might be invoked in multi-threaded environment.
			{
				if (_cache.ContainsKey(forType)) return _cache[forType];
				DBTableAttribute? attr = DBTableAttribute.FindAttribute(forType);
				if (attr == null) throw new MissingAttributeException(forType);
				if (_cache.ContainsKey(forType)) return _cache[forType];
				var table = service.LoadTableSchema(attr.TableName, true);
				var r = new Loader(forType, attr, table, attr.FieldMapper());
				_cache.Add(forType, r);
				return r;
			}
		}

		[DebuggerDisplay("{Field.Name} => {Property.Name}")]
		private record struct PropertyBinding(Field Field, PropertyInfo Property, ParameterInfo ConstructorParameter);
		private record struct ConstPrams(ConstructorInfo Constructor, ParameterInfo[] Params);
		private List<PropertyBinding> _bindings;
		private Loader(Type forType, DBTableAttribute attr, Table schemaTable, Dictionary<string, string> fieldToPropertyMap)
		{
			ForType = forType;
			DBTableAttr = attr;
			SchemaTable = schemaTable;
			ParameterInfo[]? parameters = null;
			var cps = forType.GetConstructors().Select(c => new ConstPrams(c, c.GetParameters())).Where(c => c.Params.Length <= SchemaTable.Fields.Count)
				.OrderByDescending(c => c.Params.Length).ToList();
			if (cps.Count == 0) throw new ArgumentException($"Type {forType.Name} has no constructor matching the table fields.");

			bool matchesFields(ConstPrams constPrams)
			{
				foreach (var p in constPrams.Params)
				{
					var field = SchemaTable.Fields.FirstOrDefault(ff => string.Equals(ff.Name, p.Name, StringComparison.OrdinalIgnoreCase));
					if (field == null)
					{
						KeyValuePair<string, string>? fff = fieldToPropertyMap.FirstOrDefault(nvp => string.Equals(nvp.Value, p.Name, StringComparison.OrdinalIgnoreCase));
						if (fff == null) return false;
					}
				}
				return true;
			}
			ConstPrams? best = cps.FirstOrDefault(cp => matchesFields(cp));
			if (best == null) throw new ArgumentException($"Type {forType.Name} has no constructor matching the table fields.");
			(Constructor, parameters) = best.Value;
			_bindings = new List<PropertyBinding>(parameters.Length);
			foreach (PropertyInfo prop in forType.GetProperties())
			{
				ParameterInfo? pi = parameters.FirstOrDefault(p => string.Equals(p.Name, prop.Name, StringComparison.OrdinalIgnoreCase));
				if (pi is null) continue; // non-constructor property.  Ignore				
				var field = SchemaTable.Fields.FirstOrDefault(f => string.Equals(f.Name, prop.Name, StringComparison.OrdinalIgnoreCase));
				if (field is null)
				{
					foreach (var key in fieldToPropertyMap.Keys)
					{
						var f = SchemaTable.Fields.FirstOrDefault(ff => string.Equals(ff.Name, key, StringComparison.OrdinalIgnoreCase));
						if (f != null && fieldToPropertyMap[key] == prop.Name)
						{
							field = f;
							break;
						}
					}
				}
				if (field is null) throw new UnmatchedFieldException(prop);
				if (field.KeyType == KeyType.Primary) PrimaryKeyName = field.Name;
				_bindings.Add(new(field, prop, pi));
			}
		}

		private DBTableAttribute DBTableAttr { get; init; }
		public Table SchemaTable { get; private init; }
		public string TableName => SchemaTable.Name;
		public Type ForType { get; private init; }
		private ConstructorInfo Constructor { get; init; }
		internal string? PrimaryKeyName { get; private init; }
		internal int PrimaryKeyCount => SchemaTable.PrimaryKeyCount;
		internal bool HasCompoundKey => SchemaTable.HasCompoundKey;
		internal bool IsKeyAutoIncrement
		{
			get
			{
				Field? pk = SchemaTable.Fields.FirstOrDefault(f => f.IsPrimaryKey);
				if (pk == null) return false;
				return pk.IsAutoIncrement;
			}
		}

		internal object? PrimaryKeyFor(object value)
		{
			if (value is null) return null;
			if (value.GetType() != ForType) throw new WrongTypeException(ForType, value.GetType());
			List<object> keys = new();
			foreach (PropertyBinding b in _bindings)
			{
				if (b.Field.IsPrimaryKey)
				{
					object? v = b.Property.GetValue(value);
					if (v != null) keys.Add(v);
				}
			}
			switch (keys.Count)
			{
				case 0: return null;
				case 1: return keys[0];
				default: return keys.ToArray();
			}
		}

		public T CreateAndPopulate<T>(IDataReader reader)
		{
			if (typeof(T) != ForType) throw new InvalidOperationException("Wrong type");
			return (T)CreateAndPopulate(reader);
		}

		internal object CreateAndPopulate(IDataReader reader)
		{
			//object[] prams = new object[ConstructorParameters.Length];
			Dictionary<string, object?> dParams = new(SchemaTable.Fields.Count);
			foreach (var b in _bindings)
			{
				object? val = reader[b.Field.Name];
				if (DBTableAttr.IsFilePathField(b.Field.Name) && val is string sval) val = sval.Replace("/", "\\");
				if (b.Field.SystemType == typeof(bool) && b.Property.PropertyType == typeof(bool) && val is sbyte sb) val = (sb == 0) ? false : true;
				if (val is DBNull) val = null;
				if (b.Property.PropertyType.IsEnum)
				{
					string name = b.Property.PropertyType.GetEnumUnderlyingType().Name;
					// special case for ulong:
					if (name == "UInt64") val = Convert.ChangeType(val, b.Property.PropertyType.GetEnumUnderlyingType());
				}
				dParams.Add(b.Field.Name, val);
			}
			return Constructor.Invoke(dParams.Values.ToArray());
		}

		internal ISqlStatement CreateInsertStatement(object value)
		{
			if (!value.GetType().IsAssignableTo(ForType)) throw new WrongTypeException(ForType, value.GetType());
			StringBuilder s = new($"insert into {TableName} (");
			List<string?> values = new();
			int n = 0;
			string quoted(string s) => $"`{s}`";
			foreach (var b in _bindings)
			{
				if (b.Field.IsPrimaryKey && b.Field.IsAutoIncrement) continue;
				string fieldName = b.Field.Name;
				if (n++ > 0) s.Append(", ");
				s.Append(quoted(fieldName));
				object? val = Insertable(b.Property.GetValue(value), fieldName);
				if (val is Enum e)
				{
					switch (val.GetType().GetEnumUnderlyingType().Name)
					{
						case "Int32": val = (int)val; break;
						case "Int64": val = (long)val; break;
						case "UInt64": val = (ulong)val; break;
						case "SByte": val = (sbyte)val; break;
						case "Byte": val = (byte)val; break;
					}
				}
				values.Add(val?.ToString());
			}
			if (n == 0) return NoStatement.Default;
			n = 0;
			s.Append(") values (");
			foreach (string? sval in values)
			{
				if (n++ > 0) s.Append(", ");
				s.Append(sval);
			}
			s.Append(")");
			return new SqlStatement(s.ToString());
		}

		private object Insertable(object? orig, string fieldName)
		{
			if (orig is null) return "null";
			else
			{
				if (orig is bool bval) return bval ? 1 : 0;
				else
				if (orig is byte[] bvals) return $"x'{BitConverter.ToString(bvals).Replace("-", string.Empty)}'";
				else
				if (orig is string sval)
				{
					if (DBTableAttr.IsFilePathField(fieldName)) sval = sval.Replace("\\", "/");
					if (sval.Contains('\'')) sval = sval.SingleQuoteEscaped;
					return $"'{sval}'";
				}
				else
				if (orig is DateOnly doo) return $"'{doo:yyyy-MM-dd}'";
				else
				if (orig is DateTime dt) return $"'{dt:yyyy-MM-dd HH:mm:ss.ffffff}'";
				return orig;
			}
		}

		internal string PrimaryKeyWhereClause(object value)
		{
			StringBuilder where = new StringBuilder();
			foreach (var b in _bindings)
			{
				if (b.Field.IsPrimaryKey)
				{
					if (where.Length > 0) where.Append(" and ");
					object? pVal = b.Property.GetValue(value);
					if (pVal is null) where.Append($"{b.Field.Name} is null"); else where.Append($"{b.Field.Name}={Insertable(pVal, b.Field.Name)}");
				}
			}
			return where.Insert(0, "where ").ToString();
		}

		internal ISqlStatement CreateUpdateStatement(object? value)
		{
			if (value == null) throw new ArgumentNullException(nameof(value));
			if (value.GetType() != ForType && !ForType.IsAssignableFrom(value.GetType())) throw new WrongTypeException(ForType, value.GetType());
			StringBuilder updates = new StringBuilder();
			foreach (var b in _bindings)
			{
				string fieldName = b.Field.Name;
				object? propVal = Insertable(b.Property.GetValue(value), fieldName);
				if (b.Property.PropertyType.IsEnum)
				{
					switch (Enum.GetUnderlyingType(b.Property.PropertyType).Name)
					{
						case "Int32": propVal = (int)propVal; break;
						case "UInt32": propVal = (uint)propVal; break;
						case "Int64": propVal = (long)propVal; break;
						case "UInt64": propVal = (ulong)propVal; break;
					}
				}
				if (b.Field.IsPrimaryKey) continue;
				if (updates.Length > 0) updates.Append(", ");
				updates.Append($"{fieldName}={propVal}");
			}
			if (updates.Length == 0) return NoStatement.Default;
			updates.Insert(0, $"update {TableName} set ").Append(" ").Append(PrimaryKeyWhereClause(value));
			return new SqlStatement(updates.ToString());
		}

		internal ISqlStatement CreateUpdateStatement<T>(ValuePair<T> pair) where T : class => CreateUpdateStatement(pair.NewValue, pair.OriginalValue);

		internal ISqlStatement CreateUpdateStatement(object value, object originalValue)
		{
			if (value.GetType() != ForType) throw new WrongTypeException(ForType, value.GetType());
			StringBuilder updates = new();
			foreach (var b in _bindings)
			{
				if (b.Field.KeyType == KeyType.Primary) continue;
				string fieldName = b.Field.Name;
				object? propVal = b.Property.GetValue(value), cmpVal = b.Property.GetValue(originalValue);
				if (Equals(propVal, cmpVal)) continue;
				if (updates.Length > 0) updates.Append(", ");
				updates.Append($"{fieldName}={Insertable(propVal, fieldName)}");
			}
			if (updates.Length == 0) return NoStatement.Default;
			updates.Insert(0, $"update {TableName} set ").Append(PrimaryKeyWhereClause(value));
			return new SqlStatement(updates.ToString());
		}

		internal ISqlStatement CreateDeleteStatement(object? value)
		{
			if (value == null) throw new ArgumentNullException(nameof(value));
			if (!value.GetType().IsAssignableTo(ForType)) throw new WrongTypeException(ForType, value.GetType());
			string where = PrimaryKeyWhereClause(value);
			if (string.IsNullOrEmpty(where)) return NoStatement.Default;  // another option is to build where clause based on all fields
			return new SqlStatement($"delete from {TableName} {where}");
		}
	}
}
