using Chess.Lib.Hardware.Pieces;
using Chess.Lib.Moves;
using System.Collections.Immutable;

namespace Chess.Lib.Hardware
{
	internal record struct PieceAndState(IPiece Piece, PieceState State)
	{
		internal PieceAndState(IPiece piece) : this(piece, new PieceState(piece)) { }
	}

	public interface IChessboardState
	{
		int SerialNumber { get; }
		IChessMove LastMove { get; }
		IReadOnlyList<IChessPiece> Promotions { get; }
		IReadOnlyList<IChessPiece> Removed { get; }
		string FEN { get; }
	}

	internal interface IBoardState : IChessboardState
	{
		new IMove LastMove { get; }
		ImmutableList<PieceAndState> PieceStates { get; }
		new ImmutableList<IPiece> Promotions { get; }
		new ImmutableList<IPiece> Removed { get; }
	}

	internal record struct BoardState(int SerialNumber, IMove LastMove, string FEN, ImmutableList<PieceAndState> PieceStates,
		ImmutableList<IPiece> Promotions, ImmutableList<IPiece> Removed) : IBoardState
	{
		internal static readonly BoardState Empty = new BoardState(-1, NoMove.Default, string.Empty, ImmutableList<PieceAndState>.Empty,
			ImmutableList<IPiece>.Empty, ImmutableList<IPiece>.Empty);

		IChessMove IChessboardState.LastMove => LastMove;
		IReadOnlyList<IChessPiece> IChessboardState.Promotions => Promotions;
		IReadOnlyList<IChessPiece> IChessboardState.Removed => Removed;

		private static ImmutableList<PieceAndState> CreateStates(IBoard board)
		{
			return ImmutableList<PieceAndState>.Empty.AddRange(board.ActivePieces.Select(p => new PieceAndState(p)).ToArray());
		}

		internal BoardState(IBoard board) : this(board.LastMove.SerialNumber, board.LastMove, board.AsFEN(), CreateStates(board),
			board.Promotions, board.RemovedPieces) { }

		internal static int CountRepetitions(IEnumerable<IBoardState> states)
		{
			string prev = string.Empty;
			int n = 0;
			foreach (IBoardState state in states.OrderBy(s => s.FEN))
			{
				if (string.Equals(prev, state.FEN)) n++; else prev = state.FEN;
			}
			return n / 2;
		}
	}

	
}
