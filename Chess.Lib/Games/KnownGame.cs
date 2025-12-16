using Chess.Lib.Hardware;
using Chess.Lib.Moves.Parsing;
using System.Collections.Immutable;

namespace Chess.Lib.Games
{
	internal class KnownGame : ChessGame, IKnownChessGame
	{
		internal static readonly KnownGame Empty = new KnownGame();

		internal KnownGame(): base(true)
		{
			Result = GameResult.Unknown;
		}

		private KnownGame(IBoard board): base(true, board) { }

		internal static KnownGame EmptyGameWithSetup(string fenSetup)
		{
			IBoard b = FEN.TryParse(fenSetup, out FEN f) ? new Board(f) : new Board();
			return new KnownGame(b);
		}

		public KnownGame(string moves): this(Parsers.TryParseMoves(moves)) { }

		public KnownGame(IParsedGame game, string whiteName = "", string blackName = ""): base((ChessGame)game.Game, whiteName, blackName)
		{
			if (game is IParsedGameSuccess s) Result = s.Result;
			if (game is IParsedGameFail f)
			{
				ParseError = f.Error.Error;
				UnparsedMoves = f.UnparsedMoves;
			}
		}

		internal KnownGame(string moves, string whiteName, string blackName, string fen = ""): 
			this(Parsers.TryParseMoves(moves), whiteName, blackName) { }

		internal KnownGame(AlgebraicMoves moves) : this(moves.Parse()) { }

		public override bool IsReadOnly => true;
		public bool IsEmpty => Moves.Count == 0;

		IReadOnlyChessPlayer IKnownChessGame.White => base.White;
		IReadOnlyChessPlayer IKnownChessGame.Black => base.Black;
		public GameResult Result { get; private init; } = GameResult.Unknown;
		public ParseErrorType ParseError { get; private init; }
		public ImmutableList<IParseableMove> UnparsedMoves { get; private init; } = ImmutableList<IParseableMove>.Empty;

	}
}
