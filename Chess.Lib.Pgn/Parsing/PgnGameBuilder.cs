using Chess.Lib.Games;
using Chess.Lib.Moves.Parsing;
using Chess.Lib.Pgn.DataModel;
using System.Collections.Immutable;

namespace Chess.Lib.Pgn.Parsing
{
	[Flags]
	public enum PgnImportStatus
	{
		None = 0,
		PlayersLocated = 0x0001,
		PgnMovesParsed = 0x0002,
		OpeningsMatched = 0x0004,
		UniquenessVerified = 0x008,
		Completed = PlayersLocated | PgnMovesParsed | OpeningsMatched | UniquenessVerified
	}

	/// <summary>
	/// PgnGameBuilder wraps an import and provides flags/ids that allow it to be prepared to insert into the database.
	/// See the ImportPGN sample for an example.
	/// </summary>
	public record PgnGameBuilder(GameImport Import, int OpeningId, ImmutableList<int> DupeGameIds,
		GameResult Result = GameResult.Unknown, PgnImportStatus Status = PgnImportStatus.None) : IPgnGame
	{
		public PgnGameBuilder(GameImport import) : this(import, Opening.NoOpening.Id, ImmutableList<int>.Empty, import.Result) { }

		public PgnGame ToGame(int sourceId)
		{
			PGNGameStatus status = Status.HasFlag(PgnImportStatus.PgnMovesParsed) ?
				Import.IsVariant ? PGNGameStatus.Variant : PGNGameStatus.Verified : PGNGameStatus.Broken;
			return new PgnGame(this, status, sourceId);
		}

		string IPgnGame.Moves => Import.Moves;
		string IPgnGame.FEN => Import.FEN;
		string IPgnGame.WhiteName => Import.WhiteName;
		string IPgnGame.BlackName => Import.BlackName;
		IReadOnlyDictionary<string, string> IPgnGame.Tags => Import.Tags;
	}

}
