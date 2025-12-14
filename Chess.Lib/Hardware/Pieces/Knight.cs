namespace Chess.Lib.Hardware.Pieces
{
	internal interface IKnight : IPiece, IChessKnight;

	internal sealed record Knight(FileRank StartPosition, Hue Side, IBoard Board) : Piece(StartPosition, PieceType.Knight, Side, Board), IKnight
	{
		public override bool CanMoveToImpl(ISquare toSquare)
		{
			if (!this.CanMoveToCore(toSquare)) return false;
			int dR = Math.Abs(Square.Rank - toSquare.Rank), dF = Math.Abs(Square.File - toSquare.File);
			if (dR == 1) return dF == 2;
			if (dR == 2) return dF == 1;
			return false;
		}

		protected override IPiece CopyFor(IBoard forBoard) => new Knight(StartPosition, Side, forBoard);
	}
}
