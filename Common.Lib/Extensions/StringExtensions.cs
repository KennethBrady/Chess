namespace Common.Lib.Extensions
{
	public static class StringExtensions
	{
		extension(string str)
		{
			public bool AreBalanced(char bracketChar, char endBracketChar, out int matchCount)
			{
				matchCount = 0;
				if (string.IsNullOrEmpty(str)) return false;
				int depth = 0;
				foreach (char c in str)
				{
					if (c == bracketChar) { depth++; matchCount++; }
					else
						if (c == endBracketChar) depth--;
					if (depth < 0) return false;
				}
				return depth == 0;
			}

			/// <summary>
			/// Convert a string to a primitive type or DateTime
			/// </summary>
			/// <param name="toType">The expected type represented by the string</param>
			/// <returns>
			/// The converted value boxed in an object, or null if either
			/// no conversion was found or the conversion failed.
			/// Also, note that the DateTime conversion is inexact,
			/// as the string does not have the full precision of the DateTime.
			/// </returns>
			public object? ConvertTo(Type toType)
			{
				object? ostr = null;
				switch (toType.Name)
				{
					case "Int32": if (int.TryParse(str, out int i)) ostr = i; break;
					case "UInt32": if (uint.TryParse(str, out uint u)) ostr = u; break;
					case "Int64": if (long.TryParse(str, out long l)) ostr = l; break;
					case "UInt64": if (ulong.TryParse(str, out ulong ul)) ostr = ul; break;
					case "Byte": if (byte.TryParse(str, out byte b)) ostr = b; break;
					case "SByte": if (sbyte.TryParse(str, out sbyte sb)) ostr = sb; break;
					case "Int16": if (short.TryParse(str, out short s)) ostr = s; break;
					case "UInt16": if (ushort.TryParse(str, out ushort us)) ostr = us; break;
					case "Single": if (float.TryParse(str, out float f)) ostr = f; break;
					case "Double": if (double.TryParse(str, out double d)) ostr = d; break;
					case "Decimal": if (decimal.TryParse(str, out decimal de)) ostr = de; break;
					case "Boolean": if (bool.TryParse(str, out bool bo)) ostr = bo; break;
					case "DateTime": if (DateTime.TryParse(str, out DateTime dt)) ostr = dt; break;
					case "String": ostr = str; break;
					case "Char": if (str.Length == 1) ostr = str[0]; break;
					case "Object": ostr = str; break;
				}
				return ostr;

			}

			public string SingleQuoteEscaped => str.Replace("'", "''");

			public string SingleQuoteUnescaped => str.Replace("''", "'");
		}
	}
}
