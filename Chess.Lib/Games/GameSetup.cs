using Chess.Lib.Hardware;
using Chess.Lib.Hardware.Timing;

namespace Chess.Lib.Games
{
	public enum GameBoardType { Classic, FischerRandom, Custom };

	public record struct GameBoard(GameBoardType Type, IChessBoard Board, Hue NextMove)
	{
		public static readonly GameBoard Default = new GameBoard(GameBoardType.Classic, NoBoard.Instance, Hue.Default);

		internal IBoard IBoard => (IBoard)Board;
	}


	public record struct GameSetup(string WhiteName, string BlackName, ChessClockSetup ClockSetup, GameBoard Board)
	{
		public static readonly GameSetup Default = new GameSetup(string.Empty, string.Empty, ChessClockSetup.Empty, GameBoard.Default);

		public static GameSetup Named(string whiteName, string blackName) => Default with { WhiteName = whiteName, BlackName = blackName };
	}
}
