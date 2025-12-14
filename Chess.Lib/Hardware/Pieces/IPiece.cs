using Chess.Lib.Moves;

namespace Chess.Lib.Hardware.Pieces
{
	/// <summary>
	/// Public interface representing a chess piece
	/// </summary>
	public interface IChessPiece
	{
		IChessBoard Board { get; }
		PieceType Type { get; }
		Hue Side { get; }
		int MoveCount { get; }
		IChessSquare Square { get; }
		bool MovesDiagonals { get; }
		bool MovesFilesAndRanks { get; }
		bool IsCaptured { get; }
		bool CanMoveTo(IChessSquare toSquare);
		IChessMove PreviousMove { get; }
		FileRank StartPosition { get; }
	}

	public interface IChessPawn : IChessPiece;
	public interface IChessRook : IChessPiece;
	public interface IChessKnight : IChessPiece;
	public interface IChessBishop : IChessPiece;
	public interface IChessQueen : IChessPiece;
	public interface IChessKing : IChessPiece
	{
		(bool ksPossible, bool qsPossible) IsFutureCastlePossible { get; }
		bool IsInCheck();
		bool IsMated { get; }
	}

	/// <summary>
	/// Internal interface providing access to Move and State management
	/// </summary>
	internal interface IPiece : IChessPiece
	{
		new IBoard Board { get; }
		bool Move(IChessMove move);
		new ISquare Square { get; }
		bool CanMoveTo(ISquare toSquare);
		bool CanCaptureTo(ISquare square); // TODO: verify en-passant and promotion behavior, then remove
		void SetSquare(ISquare square);
		IPiece MakeCopyFor(IBoard forBoard);
		void Reset();
		void ApplyState(PieceState state);
		int PromotionIndex { get; set; }
		void ApplyCastling(IMove castleMove, IKing king, IRook rook) { }
	}

	public interface INoPiece : IChessPiece;

}
