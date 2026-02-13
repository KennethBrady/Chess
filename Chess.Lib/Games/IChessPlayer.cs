using Chess.Lib.Hardware;
using Chess.Lib.Hardware.Pieces;
using Chess.Lib.Moves;
using Chess.Lib.Moves.Parsing;
using Common.Lib.Contracts;

namespace Chess.Lib.Games
{
	public record struct PlayerMove(IChessPlayer Player, IChessMove CompletedMove);

	/// <summary>
	/// Represents a chess player from a completed non-interactive game.
	/// </summary>
	public interface IReadOnlyChessPlayer
	{
		string Name { get; }
		Hue Side { get; }
		IEnumerable<IChessSquare> AvailableSquaresFor(IChessPiece piece);
		IReadOnlyList<IChessMove> CompletedMoves { get; }
		IChessBoard Board { get; }
		IChessGame Game { get; }
		IChessKing King { get; }
		bool IsReadOnly { get; }
		bool HasNextMove { get; }
		IEnumerable<IChessPiece> ActivePieces { get; }
		IEnumerable<IChessPiece> CapturedPieces { get; }
		event Handler<PlayerMove>? MoveMade;
		IChessMove LastMoveMade => CompletedMoves.Count == 0 ? NoMove.Default : CompletedMoves[0];
	}

	/// <summary>
	/// Represents an interactive chess player with the capability to make moves.
	/// </summary>
	public interface IChessPlayer : IReadOnlyChessPlayer
	{
		Task<IMoveAttempt> AttemptMove(IParseableMove move);
		Task<IMoveAttempt> AttemptMove(string move, MoveFormat format = MoveFormat.Engine);
		Task<IMoveAttempt> AttemptMove(MoveRequest moveRequest);
		bool CanUndo => !IsReadOnly && !HasNextMove && CompletedMoves.Count > 0;
		bool UndoLastMove();
		event Handler<bool>? CanMoveChanged;

	}

	/// <summary>
	/// Internal version of IChessPlayer
	/// </summary>
	internal interface IPlayer : IChessPlayer
	{
		new IGame Game { get; }
		new IBoard Board => Game.Board;
		new IKing King { get; }
		void RaiseCanMoveChanged();
	}

	public interface INoPlayer : IChessPlayer;
}
