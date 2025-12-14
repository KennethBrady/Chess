using Chess.Lib.Games;
using Chess.Lib.Moves.Parsing;
using Chess.Lib.Pgn.DataModel;
using System.Collections.Immutable;

namespace Chess.Lib.Pgn.Parsing
{
	public record struct SourceInfo(string Name, int Position, int Index)
	{
		internal static readonly SourceInfo Empty = new SourceInfo(string.Empty, 0, 1);
		public bool IsEmpty => string.IsNullOrEmpty(Name);
	}

	public record struct GameImport(SourceInfo SourceInfo, string Pgn, DateTime EventDate, PgnPlayer White, PgnPlayer Black,
		string Moves, GameResult Result, bool HasUnexpectedMoves, ImmutableDictionary<string, string> Tags) : IPgnGame
	{
		public string Site => Tags["Site"];
		public bool IsVariant => Tags.ContainsKey("Variant") && Tags.ContainsKey("FEN");

		public string FEN => Tags.ContainsKey("FEN") ? Tags["FEN"] : string.Empty;
		public string WhiteName => Tags["White"];

		public string BlackName => Tags["Black"];

		IReadOnlyDictionary<string, string> IPgnGame.Tags => Tags;

		public string FindTag(string tagKey, string defaultValue = "") => Tags.ContainsKey(tagKey) ? Tags[tagKey] : defaultValue;
	}

}
