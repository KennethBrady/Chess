namespace Common.Lib.Extensions
{
	public static class StringEx
	{
		public static string SingleQuoteEscaped(this string s) => s.Replace("'", "\\'");

		public static bool AreBalanced(this string s, char bracketChar, char endBracketChar, out int matchCount)
		{
			matchCount = 0;
			if (string.IsNullOrEmpty(s)) return false;
			int depth = 0;
			foreach (char c in s)
			{
				if (c == bracketChar) { depth++; matchCount++; }
				else
					if (c == endBracketChar) depth--;
				if (depth < 0) return false;
			}
			return true;
		}
	}
}
