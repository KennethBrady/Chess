using Chess.Lib.Hardware;
using Chess.Lib.Hardware.Timing;
using Chess.Lib.Moves;
using Chess.Lib.Moves.Parsing;
using Common.Lib.Contracts;

namespace Chess.Lib.Games
{
	/// <summary>
	/// Represents a fixed, unplayable game.  Return Default when no actual game is available.
	/// </summary>
	internal struct NoGame : IGame, IInteractiveChessGame, INoGame
	{
		internal static readonly NoGame Default = new NoGame();
		public IMove LastMoveMade => NoMove.Default;
		public IBoard Board => Hardware.Board.Default;
		public IPlayer White => NoPlayer.Default;
		public IPlayer Black => NoPlayer.Default;
		public bool CanMakeMoves => false;
		public bool IsReadOnly => true;
		public IMoves Moves => NoMoves.Default;
		public IReadOnlyList<IMove> MoveList { get; }
		public IChessPlayer NextPlayer => NoPlayer.Default;
		IChessPlayer IChessGame.White => NoPlayer.Default;
		IReadOnlyChessPlayer IReadOnlyChessGame.White => NoPlayer.Default;
		IChessPlayer IChessGame.Black => NoPlayer.Default;
		IReadOnlyChessPlayer IReadOnlyChessGame.Black => NoPlayer.Default;
		IChessBoard IReadOnlyChessGame.Board => Hardware.Board.Default;
		IChessMove IReadOnlyChessGame.LastMoveMade => NoMove.Default;
		IChessMoves IReadOnlyChessGame.Moves => NoMoves.Default;
		public IChessgameState CurrentState => GameState.Empty;
		public IInteractiveChessGame Branch() => NoGame.Default;
		public IChessPlayer PlayerOf(Hue hue) => NoPlayer.Default;
		IReadOnlyChessPlayer IReadOnlyChessGame.PlayerOf(Hue hue) => NoPlayer.Default;
		public bool UndoLastMove() => false;
#pragma warning disable 00067   // Disable "never used" warnings                                   
		public event Handler<CompletedMove>? MoveCompleted;
		public event Handler<IChessMove>? MoveUndone;
		public event Handler<IChessgameState>? GameStateApplied;
		public event AsyncHandler<Promotion,Promotion>? PromotionRequest;
#pragma warning restore 00067
		public int ApplyMoves(IMoveParser parser) => 0;
		public int ApplyMoves(string moves, MoveFormat format = MoveFormat.Unknown) => 0;

		bool IInteractiveChessGame.AttachClock(ChessClockSetup clockSetup) => false;
		IChessClock IInteractiveChessGame.Clock => NullClock.Instance;
	}
}
