using Chess.Lib.Moves;

namespace Chess.Lib.Hardware.Pieces
{
	#region NoPiece

	internal record struct NoPiece(FileRank StartLocation, PieceType Type, Hue Side, int MoveCount, IBoard Board, ISquare Square) : IPiece, INoPiece
	{
		internal static readonly NoPiece Default = new NoPiece(FileRank.OffBoard, PieceType.None, Hue.Default, 0, Hardware.Board.Default, NoSquare.Default);

		IChessSquare IChessPiece.Square => Square;
		IChessBoard IChessPiece.Board => Board;
		bool IChessPiece.MovesDiagonals => false;
		bool IChessPiece.MovesFilesAndRanks => false;
		bool IChessPiece.IsCaptured => false;
		bool IChessPiece.CanMoveTo(IChessSquare square) => false;
		IChessMove IChessPiece.PreviousMove => NoMove.Default;
		FileRank IChessPiece.StartPosition => FileRank.OffBoard;
		void IPiece.ApplyState(PieceState state) { }
		void IPiece.Reset() { }
		int IPiece.PromotionIndex
		{
			get => -1;
			set { }
		}

		void IPiece.SetSquare(ISquare square) { }

		bool IPiece.CanMoveTo(ISquare square) => false;

		bool IPiece.Move(IChessMove move) => false;

		bool IPiece.CanCaptureTo(ISquare square) => false;

		IPiece IPiece.MakeCopyFor(IBoard forBoard) => Default;
	}

	#endregion

	#region NoPawn

	internal record struct NoPawn : IPawn, INoPiece
	{
		internal static readonly NoPawn Default = new NoPawn();

		public IEnumerable<ISquare> PromotionSquares() => Enumerable.Empty<ISquare>();
		public IBoard Board => Chess.Lib.Hardware.Board.Default;
		public ISquare Square => NoSquare.Default;
		public bool HasAnyMove => false;
		public PieceType Type => PieceType.Pawn;
		public Hue Side => Hue.Default;
		public int MoveCount => 0;
		public bool MovesDiagonals => false;
		public bool MovesFilesAndRanks => false;
		public bool IsCaptured => false;
		IChessBoard IChessPiece.Board => Board;
		IChessSquare IChessPiece.Square => Square;
		bool IChessPiece.CanMoveTo(IChessSquare square) => false;
		IChessMove IChessPiece.PreviousMove => NoMove.Default;
		FileRank IChessPiece.StartPosition => FileRank.OffBoard;
		void IPiece.Reset() { }
		void IPiece.ApplyState(PieceState state) { }
		int IPiece.PromotionIndex
		{
			get => -1;
			set { }
		}

		public bool CanCaptureTo(ISquare square) => false;

		public bool CanMoveTo(ISquare square) => false;

		public bool CanPromote() => false;

		public bool IsEnPassant(ISquare square, out IPawn captured)
		{
			captured = NoPawn.Default;
			return false;
		}

		public bool IsPromotion(ISquare square) => false;

		public IPiece MakeCopyFor(IBoard forBoard) => NoPiece.Default;

		public bool Move(IChessMove move) => false;

		public IPiece Promote(PieceType to, ISquare targetSquare) => NoPiece.Default;

		public void SetSquare(ISquare square) { }
	}

	#endregion

	#region NoKing

	internal record struct NoKing(PieceType Type, Hue Side, IBoard Board, ISquare Square) : IKing, INoPiece
	{
		internal static NoKing Default = new NoKing(PieceType.King, Hue.Default, Hardware.Board.Default, NoSquare.Default);

		bool IChessKing.IsMated => false;
		int IChessPiece.MoveCount => 0;
		IChessSquare IChessPiece.Square => Square;
		IChessBoard IChessPiece.Board => Board;
		bool IChessPiece.MovesDiagonals => false;
		bool IChessPiece.MovesFilesAndRanks => false;
		bool IChessPiece.IsCaptured => false;
		bool IChessPiece.CanMoveTo(IChessSquare square) => false;
		IChessMove IChessPiece.PreviousMove => NoMove.Default;
		FileRank IChessPiece.StartPosition => FileRank.OffBoard;
		void IPiece.Reset() { }
		void IPiece.ApplyState(PieceState state) { }
		int IPiece.PromotionIndex
		{
			get => -1;
			set { }
		}

		void IPiece.SetSquare(ISquare square) { }

		bool IPiece.CanMoveTo(ISquare square) => false;

		bool IPiece.Move(IChessMove move) => false;

		bool IPiece.CanCaptureTo(ISquare square) => false;

		IPiece IPiece.MakeCopyFor(IBoard forBoard) => Default;

		CastleMoveType IKing.CastleTypeOf(ISquare destination) => CastleMoveType.None;
		bool IKing.IsCastle(ISquare destination, bool isKingside, out ISquare rookSq, out ISquare rookDest)
		{
			rookSq = rookDest = NoSquare.Default;
			return false;
		}
		bool IChessKing.IsInCheck() => false;

		(bool ksPossible, bool qsPossible) IChessKing.IsFutureCastlePossible => (false, false);
	}

	#endregion
}
