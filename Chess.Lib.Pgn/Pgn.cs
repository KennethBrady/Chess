using Chess.Lib.Games;
using Chess.Lib.Moves;
using Chess.Lib.Pgn.Parsing;
using System.Collections.Immutable;
using System.Text;

namespace Chess.Lib.Pgn
{
	public record Pgn(ImmutableDictionary<string, string> Tags, string Moves)
	{
		private static readonly ImmutableDictionary<string, string> RequiredTags =
			ImmutableDictionary<string, string>.Empty.AddRange(PgnSourceParser.RequiredTags.ToDictionary(t => t, t => string.Empty));

		public static Pgn Empty = new Pgn(RequiredTags, string.Empty);

		private static ImmutableDictionary<string,string> FromPairs(IEnumerable<(string,string)> pairs)
		{
			ImmutableDictionary<string, string> r = ImmutableDictionary<string, string>.Empty;
			foreach (var pair in pairs) r = r.Add(pair.Item1, pair.Item2);
			return r;
		}
		public Pgn(IEnumerable<(string,string)> tags, string moves): this(FromPairs(tags), moves) { }		

		public override string ToString()
		{
			StringBuilder s = new StringBuilder();
			foreach(var nvp in Tags)
			{
				if (s.Length > 0) s.AppendLine();
				s.Append($"[{nvp.Key} \"{nvp.Value}\"]");
			}
			s.AppendLine(); s.AppendLine();
			s.Append(FormatMoves(Moves));
			return s.ToString();
		}

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
		extension(Pgn pgn)
		{
			public bool HasAllRequiredTags => PgnSourceParser.RequiredTags.All(t => pgn.Tags.Any(tag => tag.Key == t));
		}

		extension(IChessMoves moves)
		{
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
			public Pgn ToPgn() => Pgn.Empty with { Tags = Pgn.Empty.Tags.AddRange(game.Source.Tags), Moves = game.Moves.ToPgn() };
		}

		extension(IChessGame game)
		{
			private ImmutableDictionary<string, string> Names =>
				ImmutableDictionary<string, string>.Empty.Add(PgnSourceParser.WhiteTag, game.White.Name).Add(PgnSourceParser.BlackTag, game.Black.Name);

			public Pgn ToPgn() => Pgn.Empty with { Moves = game.Moves.ToPgn(), Tags = game.Names };

		}
	}
}
