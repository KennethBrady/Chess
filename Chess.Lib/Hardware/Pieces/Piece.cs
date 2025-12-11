using Chess.Lib.Moves;

namespace Chess.Lib.Hardware.Pieces
{
	#region Piece Interfaces

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
	}

	internal interface IPiece : IChessPiece
	{
		new IBoard Board { get; }
		bool Move(IChessMove move);
		new ISquare Square { get; }
		bool CanMoveTo(ISquare toSquare);
		bool CanCaptureTo(ISquare square); // TODO: verify en-passant and promotion behavior, then remove
		bool HasAnyMove { get; }
		void SetSquare(ISquare square);
		IPiece MakeCopyFor(IBoard forBoard);
		void Reset();
		void ApplyState(PieceState state);
		int PromotionIndex { get; set; }
		void ApplyCastling(IMove castleMove, IKing king, IRook rook) { }
	}

	public interface INoPiece : IChessPiece;

	#endregion

	internal record struct PieceState(IPiece Piece, PieceType Type, Hue Side, int SquareIndex, int MoveCount, int PromotionCount, IChessMove LastMove)
	{
		internal PieceState(IPiece piece) : this(piece, piece.Type, piece.Side, piece.Square.Index, piece.MoveCount, piece.PromotionIndex, piece.PreviousMove) { }
	}

	internal abstract record Piece(FileRank StartPosition, PieceType Type, Hue Side, IBoard Board) : IPiece
	{
		private int _moveCount = 0;
		private ISquare _square = NoSquare.Default;
		public ISquare Square
		{
			get => _square;
			private set => _square = value;
		}
		public int MoveCount
		{
			get => _moveCount;
			protected set => _moveCount = value;
		}
		public virtual bool MovesDiagonals => false;
		public virtual bool MovesFilesAndRanks => false;
		public bool IsCaptured => Square is NoSquare;
		IChessBoard IChessPiece.Board => Board;
		IChessSquare IChessPiece.Square => _square;

		public void SetSquare(ISquare square)
		{
			Square = square;
		}

		public virtual bool HasAnyMove => Board.Any(s => CanMoveTo((ISquare)s));
		public abstract bool CanMoveToImpl(ISquare square);
		public bool CanMoveTo(ISquare toSquare)
		{
			return CanMoveToImpl(toSquare) && !Board.WouldPutMyOwnKingInCheck(this, toSquare);
		}

		bool IChessPiece.CanMoveTo(IChessSquare toSquare) => CanMoveTo((ISquare)toSquare);
		public virtual bool CanCaptureTo(ISquare square) => square.HasPiece && square.Piece.Side != Side && CanMoveTo(square);
		public virtual bool Move(IChessMove move)
		{
			if (move.ToSquare is not ISquare to || Square is NoSquare) return false;
			((ISquare)move.FromSquare).SetPiece(NoPiece.Default);
			Square.SetPiece(NoPiece.Default);
			to.SetPiece(this);
			PreviousMove = move;
			_moveCount++;
			return true;
		}

		public IChessMove PreviousMove { get; private set; } = NoMove.Default;

		public override string ToString() => $"{Type.PGNChar()} {Square.Name}";

		public IPiece MakeCopyFor(IBoard forBoard)
		{
			IPiece r = CopyFor(forBoard);
			ISquare s = forBoard[Square.File, Square.Rank];
			s.SetPiece(r);
			r.SetSquare(s);
			return r;
		}

		protected abstract IPiece CopyFor(IBoard forBoard);

		public void Reset()
		{
			ISquare square = (ISquare)Board[StartPosition];
			square.SetPiece(this);
			_moveCount = 0;
			PreviousMove = NoMove.Default;
		}

		public void ApplyState(PieceState state)
		{
			_moveCount = state.MoveCount;
			ISquare square = (ISquare)Board[state.SquareIndex];
			square.SetPiece(this);
			SetSquare(square);
			PreviousMove = state.LastMove;
		}

		public void ApplyCastling(IMove move, IKing king, IRook rook)
		{
			Piece pk = (Piece)king, pr = (Piece)rook;
			pk.MoveCount++;
			pr.MoveCount++;
			pk.PreviousMove = move;
			pr.PreviousMove = move;
		}

		int IPiece.PromotionIndex { get; set; }

		//public override int GetHashCode() => StartPosition.ToSquareIndex;
	}

	internal class PieceTypeComparer : IComparer<IPiece>
	{
		internal static readonly PieceTypeComparer Default = new PieceTypeComparer();

		private PieceTypeComparer() { }

		public int Compare(IPiece? x, IPiece? y)
		{
			if (x is NoPiece || x is null) return -1;
			if (y is NoPiece || y is null) return 1;
			return Comparer<int>.Default.Compare((int)x.Type, (int)y.Type);
		}
	}


}
