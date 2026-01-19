using Chess.Lib.Hardware;
using Chess.Lib.Hardware.Pieces;
using Chess.Lib.Moves;
using Chess.Lib.Moves.Parsing;

namespace Chess.Lib.Games
{
	public static class GameFactory
	{
		public static IKnownChessGame CreateKnown(string moves, string whiteName = "", string blackName = "", string fenSetup = "") 
			=> new KnownGame(moves, whiteName, blackName, fenSetup);

		public static IInteractiveChessGame CreateInteractive(string whiteName = "", string blackName = "") => new InteractiveGame(whiteName, blackName);

		public static IInteractiveChessGame CreateInteractive(GameStartDefinition gameDefinition) => new InteractiveGame(gameDefinition);
		
		public static IPgnChessGame CreatePgn(IPgnGame game) => new KnownPgnGame(game);

		public static IChessBoard CreateBoard(bool populatePieces = true) => new Board(populatePieces);

		public static INoSquare NoSquare => Hardware.NoSquare.Default;
		public static IChessPiece NoPiece => Hardware.Pieces.NoPiece.Default;
		public static IChessBoard EmptyBoard => Board.Default;
		public static IChessGame NoGame => Games.NoGame.Default;
		public static IChessMove NoMove => Moves.NoMove.Default;
		public static IChessPlayer NoPlayer => Games.NoPlayer.Default;
		public static IKnownChessGame EmptyKnownGame => KnownGame.Empty;
		public static IParseableMove NoParseableMove => NotParseable.Default;
	}
}
