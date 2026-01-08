using Org.BouncyCastle.Ocsp;
using System.Runtime.CompilerServices;

namespace Chess.Lib.Pgn
{
	/// <summary>
	/// Just a bunch of string constants relevant to PGN
	/// </summary>
	public static class PgnTags
	{
		/// <summary>
		/// This comparer sorts first required tags in order, then optional tags in alpha order
		/// </summary>
		public static readonly IComparer<string> TagComparer = new _TagComparer();

		public const int RequiredTagCount = 7;

		public const string Event = "Event";
		public const string Date = "Date";
		public const string Site = "Site";
		public const string Round = "Round";
		public const string Black = "Black";
		public const string White = "White";
		public const string Result = "Result";

		private static readonly string[] _reqTags = { Event, Site, Date, Round, White, Black, Result };

		public static bool IsRequired(string tagName) =>
			tagName == Event || tagName == Date || tagName == Site || tagName == Round || tagName == Black || tagName == White || tagName == Result;

		/// <summary>
		/// Returns an ordered list of required PGN tags.
		/// </summary>
		public static IEnumerable<string> Required => _reqTags;		

		internal static int IndexOf(string requiredTag) => _reqTags.IndexOf(requiredTag);

		public static class PlayerTags
		{
			public const string WhiteElo = "WhiteElo";
			public const string BlackElo = "BlackElo";
			public const string WhiteTitle = "WhiteTitle";
			public const string BlackTitle = "BlackTitle";
			public const string WhiteFideId = "WhiteFideId";
			public const string BlackFideId = "BlackFideId";
			public const string WhiteTeam = "WhiteTeam";
			public const string BlackTeam = "BlackTeam";
		}

		private class _TagComparer : Comparer<string>
		{
			public override int Compare(string? x, string? y)
			{
				if (x == null) return y == null ? 0 : -1;
				if (y == null) return 1;
				int ix = _reqTags.IndexOf(x), iy = _reqTags.IndexOf(y);
				switch (ix)
				{
					case < 0: return iy < 0 ? string.Compare(x, y) : 1;
					case >= 0: return iy < 0 ? -1 : iy > ix ? -1 : 1;
				}
			}
		}
	}
}
