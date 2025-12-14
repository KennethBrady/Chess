using Chess.Lib.Games;
using Chess.Lib.Moves.Parsing;
using Chess.Lib.Pgn.Parsing;
using Chess.Lib.Pgn.Service;
using Sql.Lib.Services;

namespace Chess.Lib.Pgn.DataModel
{
	public enum ChessVariant { Classic, Chess960, Unkown };
	public enum PGNGameStatus : sbyte 
	{ 
		New = 0, 
		Verified = 1,			// Moves can be parsed
		Variant = 2,			// Variant game (Fisher Random)
		Broken = 3				// Moves not fully parseable
	};

	[DBTable(TableName)]
	public record PgnGame(int Id, int WhiteId, int BlackId, string Moves, PGNGameStatus Status, int SourceId, int SourceIndex, int SourcePos,
		DateTime EventDate, string Site, GameResult Result, int OpeningId) : IPgnGame
	{
		public const string TableName = "game";
		internal const string VariantTag = "Variant", FENTag = "FEN";
		public static readonly PgnGame Empty = new PgnGame(0, 0, 0, string.Empty, 0, 0, 0, 0, DateTime.MinValue, string.Empty, 0, 0);
		private readonly Lazy<IReadOnlyDictionary<string, string>> _tags = new(() => PgnGameService.LoadGameTags(Id));
		private readonly Lazy<PlayerPair> _players = new(() => PgnGameService.LoadPlayersFor(WhiteId, BlackId));

		internal PgnGame(PgnGameBuilder b, PGNGameStatus status, int sourceId):this(0, b.Import.White.Id, b.Import.Black.Id, b.Import.Moves, status,
			sourceId, b.Import.SourceInfo.Index, b.Import.SourceInfo.Position, b.Import.EventDate, b.Import.Site, b.Result, b.OpeningId)
		{
			_tags = new Lazy<IReadOnlyDictionary<string, string>>(b.Import.Tags);
		}

		public IReadOnlyDictionary<string,string> Tags => _tags.Value;
		public ChessVariant Variant
		{
			get
			{
				if (Tags.ContainsKey(VariantTag))
				{
					switch (Tags[VariantTag])
					{
						case "Chess960": return ChessVariant.Chess960;
						default: return ChessVariant.Unkown;
					}
				}
				return ChessVariant.Classic;
			}
		}

		public bool HasFEN => Variant == ChessVariant.Chess960 && _tags.Value.ContainsKey("FEN");
		public string FEN => HasFEN ? _tags.Value[FENTag] : string.Empty;

		public PgnPlayer White => _players.Value.White;
		public PgnPlayer Black => _players.Value.Black;
		public string WhiteName => Id > 0 ? Tags["White"] : string.Empty;
		public string BlackName => Id > 0 ? Tags["Black"] : string.Empty;
		public bool HasMoves => !string.IsNullOrEmpty(Moves);

		/// <summary>
		/// Retrieve this game as PGN.
		/// </summary>
		/// <returns>PGN string representing the game</returns>
		/// <remarks>TODO: Include comments</remarks>
		public string AsPgn() => PgnSourceParser.ExportPgn(Tags, Moves);
	}
}
