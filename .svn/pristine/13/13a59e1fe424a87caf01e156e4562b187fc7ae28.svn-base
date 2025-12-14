namespace Chess.Lib.Hardware
{
	public struct NeighborSquares
	{
		internal static NeighborSquares Empty = new NeighborSquares();

		internal NeighborSquares(ISquare center)
		{
			CenterSquare = center;
			NextRank = (Center.Rank == Rank.R8) ? NoSquare.Default : CenterSquare.Board[Center.File, Center.Rank + 1];
			PrevRank = (Center.Rank == Rank.R1) ? NoSquare.Default : Center.Board[Center.File, Center.Rank - 1];
			NextFile = (Center.File == File.H) ? NoSquare.Default : Center.Board[Center.File + 1, Center.Rank];
			PrevFile = (Center.File == File.A) ? NoSquare.Default : Center.Board[Center.File - 1, Center.Rank];
			if (Center.Rank != Rank.R8)
			{
				DiagUL = Center.File == File.A ? NoSquare.Default : Center.Board[Center.File - 1, Center.Rank + 1];
				DiagUR = Center.File == File.H ? NoSquare.Default : Center.Board[Center.File + 1, Center.Rank + 1];
			}
			if (Center.Rank != Rank.R1)
			{
				DiagBL = Center.File == File.A ? NoSquare.Default : Center.Board[Center.File - 1, Center.Rank - 1];
				DiagBR = Center.File == File.H ? NoSquare.Default : Center.Board[Center.File + 1, Center.Rank - 1];
			}
		}

		internal ISquare CenterSquare { get; private init; } = NoSquare.Default;
		public IChessSquare Center => CenterSquare;
		public IChessSquare NextRank { get; private init; } = NoSquare.Default;
		public IChessSquare PrevRank { get; private init; } = NoSquare.Default;
		public IChessSquare NextFile { get; private init; } = NoSquare.Default;
		public IChessSquare PrevFile { get; private init; } = NoSquare.Default;
		public IChessSquare DiagUL { get; private init; } = NoSquare.Default;
		public IChessSquare DiagUR { get; private init; } = NoSquare.Default;
		public IChessSquare DiagBL { get; private init; } = NoSquare.Default;
		public IChessSquare DiagBR { get; private init; } = NoSquare.Default;

		public IEnumerable<IChessSquare> All(bool omitNulls = true) => AllSquares().Where(s => !omitNulls || s != null);

		private IEnumerable<ISquare> AllSquares()
		{
			yield return (ISquare)Center;
			if (NextRank is not NoSquare) yield return (ISquare)NextRank;
			if (DiagUL is not NoSquare) yield return (ISquare)DiagUL;
			if (NextFile is not NoSquare) yield return (ISquare)NextFile;
			if (DiagBL is not NoSquare) yield return (ISquare)DiagBL;
			if (PrevRank is not NoSquare) yield return (ISquare)PrevRank;
			if (NextFile is not NoSquare) yield return (ISquare)NextFile;
			if (DiagBR is not NoSquare) yield return (ISquare)DiagBR;
			if (PrevFile is not NoSquare) yield return (ISquare)PrevFile;
			if (DiagUR is not NoSquare) yield return (ISquare)DiagUR;
		}
	}
}
