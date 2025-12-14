using Chess.Lib.Hardware;
using Chess.Lib.Moves;
using System.Collections.Immutable;

namespace Chess.Lib.Games
{
	public interface IChessgameState
	{
		IReadOnlyList<IChessMove> Moves { get; }
		IChessboardState BoardState { get; }
		IChessMove LastMove => BoardState.LastMove;
		int SerialNumber => Moves.Count == 0 ? -1 : Moves.Last().SerialNumber;
	}

	internal interface IGameState : IChessgameState
	{
		new ImmutableList<IMove> Moves { get; }
		new IBoardState BoardState { get; }
	}

	internal record struct GameState(ImmutableList<IMove> Moves, IBoardState BoardState) : IGameState
	{
		internal static readonly GameState Empty = new GameState(ImmutableList<IMove>.Empty, Hardware.BoardState.Empty);

		internal GameState(IGame game) : this(ImmutableList.Create<IMove>(game.Moves.PriorMoves.Cast<IMove>().ToArray()), game.Board.GetCurrentState()) { }

		IReadOnlyList<IChessMove> IChessgameState.Moves => Moves;
		IChessboardState IChessgameState.BoardState => BoardState;
	}
}
