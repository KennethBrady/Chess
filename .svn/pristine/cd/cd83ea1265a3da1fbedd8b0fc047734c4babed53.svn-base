namespace Chess.Lib.Hardware.Pieces
{
	internal interface IRook : IPiece, IChessRook { }

	internal sealed record Rook(FileRank StartPosition, Hue Side, IBoard Board) : Piece(StartPosition, PieceType.Rook, Side, Board), IRook
	{
		private ISquare _square = NoSquare.Default;
		public override bool MovesFilesAndRanks => true;

		public override bool CanMoveToImpl(ISquare toSquare)
		{
			if (!this.CanMoveToCore(toSquare)) return false;
			return CanMoveTo(Square, toSquare);
		}

		internal static bool CanMoveTo(ISquare from, ISquare to)
		{
			if (from.Rank == to.Rank)
			{
				var between = from.Board.FileSquaresBetween(from, to);
				if (between.Any(s => s.HasPiece)) return false;
				return true;
			}
			else
			if (from.File == to.File)
			{
				var between = from.Board.RankSquaresBetween(from, to);
				if (between.Any(s => s.HasPiece)) return false;
				return true;
			}
			return false;
		}

		protected override IPiece CopyFor(IBoard forBoard) => new Rook(StartPosition, Side, forBoard);

	}
}
