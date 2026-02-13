using Chess.Lib.Games;
using Chess.Lib.Moves;
using Chess.Lib.Moves.Parsing;
using Chess.Lib.Pgn.Parsing;
using System.Collections.Immutable;
using System.Text;
using System.Text.RegularExpressions;

namespace Chess.Lib.Pgn
{

	public record PGN(ImmutableDictionary<string, string> Tags, string Moves) : IPgnGame
	{
		private static readonly ImmutableDictionary<string, string> Required =
			ImmutableDictionary<string, string>.Empty.AddRange(PgnTags.Required.ToDictionary(t => t, t => string.Empty));

		public static PGN Empty = new PGN(ImmutableDictionary<string, string>.Empty, string.Empty);

		public static PGN EmptyWithRequiredTags = new PGN(Required, string.Empty);

		public bool IsEmpty => Tags.Count == 0 && string.IsNullOrEmpty(Moves);

		public bool IsComplete => PgnTags.Required.All(t => Tags.ContainsKey(t)) && !string.IsNullOrEmpty(Moves);

		public static PGN Parse(string pgn) => ParsePartial(pgn);

		public static string ToPgn(IReadOnlyDictionary<string,string> tags, string moves) => new PGN(tags, moves).ToString();

		public PGN(IPgnGame game): this(ImmutableDictionary<string,string>.Empty.AddRange(game.Tags), game.Moves) { }

		private static ImmutableDictionary<string, string> FromPairs(IEnumerable<(string, string)> pairs)
		{
			ImmutableDictionary<string, string> r = ImmutableDictionary<string, string>.Empty;
			foreach (var pair in pairs) r = r.Add(pair.Item1, pair.Item2);
			return r;
		}
		public PGN(IEnumerable<(string, string)> tags, string moves) : this(FromPairs(tags), moves) { }

		public PGN(Dictionary<string, string> tags, string moves) : this(ImmutableDictionary<string, string>.Empty.AddRange(tags), moves) { }

		public PGN(IEnumerable<KeyValuePair<string, string>> tags, string moves) : this(ImmutableDictionary<string, string>.Empty.AddRange(tags), moves) { }

		private IEnumerable<KeyValuePair<string, string>> RequiredTags => Tags.Where(nvp => PgnTags.IsRequired(nvp.Key));
		private IEnumerable<KeyValuePair<string, string>> OptionalTags => Tags.Where(nvp => !PgnTags.IsRequired(nvp.Key));

		public string ToString(int maxLineLength = 50)
		{
			StringBuilder s = new StringBuilder();
			foreach (var nvp in RequiredTags.OrderBy(p => PgnTags.IndexOf(p.Key)))  // Required ordering
			{
				if (s.Length > 0) s.AppendLine();
				s.Append($"[{nvp.Key} \"{nvp.Value}\"]");
			}
			foreach (var nvp in OptionalTags.OrderBy(t => t.Key)) // Alphabetic ordering
			{
				s.AppendLine();
				s.Append($"[{nvp.Key} \"{nvp.Value}\"]");
			}
			if (s.Length > 0) s.AppendLine().AppendLine();
			s.Append(FormatMoves(Moves, maxLineLength));
			return s.ToString();
		}

		public override string ToString() => ToString(50);

		// Format moves into lines of <= 60 char, with a line-breaks at valid places
		private static Regex _rxAlgebraic = new Regex(@"\d+\.", RegexOptions.Compiled);
		internal static string FormatMoves(string moves, int maxLineLength = 50)
		{
			if (string.IsNullOrEmpty(moves)) return string.Empty;
			int lastLineBreak = 0;
			StringBuilder s = new StringBuilder();
			AlgebraicMoves ams = AlgebraicMoves.Create(moves);
			foreach(var am in ams)
			{
				bool isNewNumber = am.SerialNumber % 2 == 0;
				if (maxLineLength > 0 && isNewNumber && (s.Length - lastLineBreak) >= maxLineLength)
				{
					s.AppendLine();
					lastLineBreak = s.Length;
				}
				else if (s.Length > 0) s.Append(' ');
				if (isNewNumber) s.Append($"{am.GameMoveNumber}. ");
				s.Append(am.Move);
			}
			return s.ToString();
		}

		string IPgnGame.FEN => Tags.ContainsKey(PgnTags.FEN) ? Tags[PgnTags.FEN] : string.Empty;

		string IPgnGame.WhiteName => Tags.ContainsKey(PgnTags.White) ? Tags[PgnTags.White] : string.Empty;

		string IPgnGame.BlackName => Tags.ContainsKey(PgnTags.Black) ? Tags[PgnTags.Black] : string.Empty;

		IReadOnlyDictionary<string, string> IPgnGame.Tags => Tags;

		private static PGN ParsePartial(string pgn)
		{
			if (string.IsNullOrEmpty(pgn)) return Empty;
			Dictionary<string, string> hdrs = new();
			MatchCollection matches = PgnSourceParser._rxHeaders.Matches(pgn);
			foreach(Match m in matches)
			{
				if (m.Groups.Count > 2)
				{
					string key = m.Groups[1].Value.Trim(), val = m.Groups[2].Value.Trim();
					if (!hdrs.ContainsKey(key)) hdrs.Add(key, val);
				}
			}
			switch(matches.Count)
			{
				case 0:
					AlgebraicMoves ams = AlgebraicMoves.Create(pgn);
					return new PGN(hdrs, AlgebraicMoves.ToPgnMoves(ams.MoveList));
				default:
					Match m = matches.Last();
					int end = m.Index + m.Value.Length + 1;
					string moves = string.Empty;
					if (end < pgn.Length) moves = pgn.Substring(end, pgn.Length - end).Trim();
					foreach(Match match in matches)
					{
						string key = match.Groups[1].Value.Trim(), val = match.Groups[2].Value.Trim();
						if (!hdrs.ContainsKey(key)) hdrs.Add(key, val);
					}
					return new PGN(hdrs, moves);
			}
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
						s.Append(Environment.NewLine);
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
			private ImmutableDictionary<string, string> Names
			{
				get
				{
					var r = ImmutableDictionary<string, string>.Empty.Add(PgnTags.White, game.White.Name).Add(PgnTags.Black, game.Black.Name);
					if (game.LastMoveMade is not INoMove && game.LastMoveMade.IsCheckMate)
					{
						string res;
						switch(game.LastMoveMade.SerialNumber % 2)
						{
							case 0: res = PgnTags.ResultTags.WhiteWin; break;
							default: res = PgnTags.ResultTags.BlackWin; break;
						}
						r = r.SetItem(PgnTags.Result, res);
					}
					return r.EnsureAllTags();
				}
			}
				

			public PGN ToPgn()
			{
				if (game is IPgnChessGame pgnGame) return new PGN(pgnGame.Source);
				return PGN.Empty with { Moves = game.Moves.ToPgn(), Tags = game.Names };
			}

		}

		extension(ImmutableDictionary<string, string> tags)
		{
			internal ImmutableDictionary<string, string> EnsureAllTags()
			{
				ImmutableDictionary<string, string> r = tags;
				foreach (string tag in PgnTags.Required)
				{
					if (!r.ContainsKey(tag)) r = r.Add(tag, string.Empty);
				}
				return r;
			}
		}
	}
}
