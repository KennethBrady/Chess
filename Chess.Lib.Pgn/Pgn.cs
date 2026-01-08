using Chess.Lib.Games;
using Chess.Lib.Moves;
using Chess.Lib.Pgn.Parsing;
using System.Collections.Immutable;
using System.Text;

namespace Chess.Lib.Pgn
{

	public record PGN(ImmutableDictionary<string, string> Tags, string Moves)
	{
		private static readonly ImmutableDictionary<string, string> Required =
			ImmutableDictionary<string, string>.Empty.AddRange(PgnTags.Required.ToDictionary(t => t, t => string.Empty));

		public static PGN Empty = new PGN(ImmutableDictionary<string, string>.Empty, string.Empty);

		public static PGN EmptyTags = new PGN(Required, string.Empty);

		public static PGN? Parse(string pgn)
		{
			switch (PgnSourceParser.Parse(pgn))
			{
				case IPgnParseSuccess success:
					return new PGN(success.Import.Tags, success.Import.Moves);
				default: return null;
			}
		}

		private static ImmutableDictionary<string,string> FromPairs(IEnumerable<(string,string)> pairs)
		{
			ImmutableDictionary<string, string> r = ImmutableDictionary<string, string>.Empty;
			foreach (var pair in pairs) r = r.Add(pair.Item1, pair.Item2);
			return r;
		}
		public PGN(IEnumerable<(string,string)> tags, string moves): this(FromPairs(tags), moves) { }		

		public PGN(Dictionary<string,string> tags, string moves): this(ImmutableDictionary<string,string>.Empty.AddRange(tags), moves) { }

		public PGN(IEnumerable<KeyValuePair<string,string>> tags, string moves): this(ImmutableDictionary<string, string>.Empty.AddRange(tags), moves) { }

		private IEnumerable<KeyValuePair<string, string>> RequiredTags => Tags.Where(nvp => PgnTags.IsRequired(nvp.Key));
		private IEnumerable<KeyValuePair<string, string>> OptionalTags => Tags.Where(nvp => !PgnTags.IsRequired(nvp.Key));

		public override string ToString()
		{
			StringBuilder s = new StringBuilder();
			foreach(var nvp in RequiredTags.OrderBy(p => PgnTags.IndexOf(p.Key)))	// Required ordering
			{
				if (s.Length > 0) s.AppendLine();
				s.Append($"[{nvp.Key} \"{nvp.Value}\"]");
			}
			foreach(var nvp in OptionalTags.OrderBy(t => t.Key))	// Alphabetic ordering
			{
				s.AppendLine();
				s.Append($"[{nvp.Key} \"{nvp.Value}\"]");
			}
			s.AppendLine(); s.AppendLine();
			s.Append(FormatMoves(Moves));
			return s.ToString();
		}

		// Format moves into lines of <= 60 char, with a line-breaks at valid places
		private static string FormatMoves(string moves)
		{
			StringBuilder s = new StringBuilder(moves);
			int linePos = 0;
			for(int i=1;i<moves.Length;++i)
			{
				bool isNewMove = moves[i] == '.' && char.IsDigit(moves[i - 1]);
				if (isNewMove && (i - linePos) > 62)
				{
					moves.Insert(i - 2, Environment.NewLine);
					linePos = i;
				}
			}
			return s.ToString();
		}
	}

	public static class PgnGameExt
	{
		extension(PGN pgn)
		{
			public bool HasAllRequiredTags => PgnTags.Required.All(t => pgn.Tags.Any(tag => tag.Key == t && !string.IsNullOrEmpty(tag.Value)));
		}

		extension(IChessMoves moves)
		{
			/// <summary>
			/// Convert moves to a pgn-formatted string
			/// </summary>
			/// <returns></returns>
			public string ToPgn()
			{
				StringBuilder s = new();
				int moveNum = 0, lastLen = 0;
				foreach (var m in moves)
				{
					if (s.Length - lastLen >= 60)
					{
						s.Append(@"\r\n");
						lastLen = s.Length;
					}
					if (m.SerialNumber % 2 == 0)
					{
						if (s.Length > 0) s.Append(' ');
						s.Append($"{++moveNum}. ");
					}
					else s.Append(" ");
					s.Append(m.AlgebraicMove);
				}
				return s.ToString();
			}
		}

		extension(IPgnChessGame game)
		{
			public PGN ToPgn() => PGN.Empty with { Tags = PGN.Empty.Tags.AddRange(game.Source.Tags), Moves = game.Moves.ToPgn() };
		}

		extension(IChessGame game)
		{
			private ImmutableDictionary<string, string> Names =>
				ImmutableDictionary<string, string>.Empty.Add(PgnTags.White, game.White.Name).Add(PgnTags.Black, game.Black.Name).EnsureAllTags();

			public PGN ToPgn() => PGN.Empty with { Moves = game.Moves.ToPgn(), Tags = game.Names };

		}

		extension(ImmutableDictionary<string, string> tags)
		{
			internal ImmutableDictionary<string,string> EnsureAllTags()
			{
				ImmutableDictionary<string, string> r = tags;
				foreach(string tag in PgnTags.Required)
				{
					if (!r.ContainsKey(tag)) r = r.Add(tag, string.Empty);
				}
				return r;
			}
		}
	}
}
