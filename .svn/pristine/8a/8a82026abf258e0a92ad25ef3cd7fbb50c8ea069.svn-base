namespace Chess.Lib.Hardware.Pieces
{
	internal interface IBishop : IPiece, IChessBishop;

	internal sealed record Bishop(FileRank StartLocation, Hue Side, IBoard Board) : Piece(StartLocation, PieceType.Bishop, Side, Board), IBishop
	{
		public override bool MovesDiagonals => true;

		internal static bool AreSameDiagonal(ISquare square1, ISquare square2)
		{
			if (square1.Hue != square2.Hue) return false;
			int dF = Math.Abs(square1.File - square2.File), dR = Math.Abs(square1.Rank - square2.Rank);
			return dF == dR;
		}

		public override bool CanMoveToImpl(ISquare toSquare) => this.CanMoveToCore(toSquare) && CanMoveTo(Square, toSquare);

		internal static bool CanMoveTo(ISquare from, ISquare to)
		{
			if (!AreSameDiagonal(from, to)) return false;
			if (from.Board.DiagonalSquaresBetween(from, to).Any(s => s.HasPiece)) return false;
			return true;
		}

		protected override IPiece CopyFor(IBoard forBoard) => new Bishop(StartPosition, Side, forBoard);
	}
}
