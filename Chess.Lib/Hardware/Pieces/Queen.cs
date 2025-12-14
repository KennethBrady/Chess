namespace Chess.Lib.Hardware.Pieces
{
	internal interface IQueen : IPiece, IChessQueen;

	internal sealed record Queen(FileRank StartPosition, Hue Side, IBoard Board): Piece(StartPosition, PieceType.Queen, Side, Board), IQueen
	{
		public override bool CanMoveToImpl(ISquare toSquare)
		{
			if (!this.CanMoveToCore(toSquare)) return false;
			return Rook.CanMoveTo(Square,toSquare) || Bishop.CanMoveTo(Square,toSquare);
		}

		protected override IPiece CopyFor(IBoard forBoard) => new Queen(StartPosition, Side, forBoard);
	}
}
