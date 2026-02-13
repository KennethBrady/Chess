using Chess.Lib.Hardware;
using Chess.Lib.Hardware.Pieces;
using Chess.Lib.Hardware.Timing;
using Chess.Lib.Moves;
using Chess.Lib.Moves.Parsing;
using Common.Lib.Contracts;
using System.Collections.Immutable;

namespace Chess.Lib.Games
{
	/// <summary>
	/// Represents a completed, non-modifiable chess game
	/// </summary>
	public interface IReadOnlyChessGame
	{
		IChessBoard Board { get; }
		IReadOnlyChessPlayer White { get; }
		IReadOnlyChessPlayer Black { get; }
		IReadOnlyChessPlayer PlayerOf(Hue hue);
		IChessMove LastMoveMade { get; }
		event Handler<CompletedMove>? MoveCompleted;
		IChessMoves Moves { get; }

		/// <summary>
		/// Create an interactive game based on the current game
		/// </summary>
		IInteractiveChessGame Branch();
		IChessgameState CurrentState { get; }

		/// <summary>
		/// This event is raised upon setting Moves.CurrentMove.
		/// </summary>
		event Handler<IChessgameState>? GameStateApplied;
	}

	public interface IKnownChessGame : IChessGame
	{
		new IReadOnlyChessPlayer White { get; }
		new IReadOnlyChessPlayer Black { get; }
		GameResult Result { get; }
		bool IsEmpty { get; }
		ParseErrorType ParseError { get; }
		ImmutableList<IParseableMove> UnparsedMoves { get; }
	}

	public interface IPgnChessGame : IKnownChessGame
	{
		IPgnGame Source { get; }
	}

	public interface IChessGame : IReadOnlyChessGame
	{
		new IChessPlayer White { get; }
		new IChessPlayer Black { get; }
		IChessPlayer NextPlayer { get; }
		new IChessPlayer PlayerOf(Hue hue);
	}

	public interface IInteractiveChessGame : IChessGame
	{
		//Do these belong here?  Need something like IGameBuilder?
		int ApplyMoves(IMoveParser parser);
		int ApplyMoves(string moves, MoveFormat format = MoveFormat.Unknown);

		IChessClock Clock { get; }

		bool AttachClock(ChessClockSetup clockSetup);
		bool UndoLastMove();
		event Handler<IChessMove>? MoveUndone;
		event AsyncHandler<Promotion,Promotion>? PromotionRequest;
	}

	internal interface IGame : IChessGame
	{
		new IMove LastMoveMade { get; }
		new IBoard Board { get; }
		new IPlayer White { get; }
		new IPlayer Black { get; }
		bool CanMakeMoves { get; }
		bool IsReadOnly { get; }
		new IMoves Moves { get; }
		IReadOnlyList<IMove> MoveList { get; }
	}

	internal interface IPromotingGame
	{
		Task<PieceType> RequestPromotion(Hue forPlayer, ISquare onSquare);
	}

	public interface INoGame : IChessGame;
}
