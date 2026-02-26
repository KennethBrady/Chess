using Chess.Lib.Hardware.Pieces;

namespace Chess.Lib.Hardware
{
	//TODO: Detect stalement; 
	public class BoardInfo
	{
		private static readonly List<IPiece> _empty = new();
		private Dictionary<PieceDef, List<IPiece>> _pieces = PieceDef.All.ToDictionary(pd => pd, pd => new List<IPiece>());

		public BoardInfo(IChessBoard board)
		{
			IBoard = (IBoard)board;
			foreach (IPiece p in Board.ActivePieces)
			{
				if (p is not INoPiece) _pieces[p.Definition].Add(p);
			}
		}

		public int TotalPieceCount => _pieces.Values.Sum(v => v.Count);

		public bool HasBothKings => Exists(PieceDef.WhiteKing) && Exists(PieceDef.BlackKing);

		public bool IsStalemated
		{
			get
			{
				return false;	// TODO
			}
		}

		public bool Exists(PieceDef piece) => _pieces[piece].Count > 0;
		public int Count(PieceDef piece) => _pieces[piece].Count;

		public IChessBoard Board => IBoard;

		public IReadOnlyList<IChessPiece> this[PieceDef def] => _pieces.ContainsKey(def) ? _pieces[def] : _empty;

		private IBoard IBoard { get; init; }
		private IChessKing? WhiteKing => _pieces[PieceDef.WhiteKing].FirstOrDefault() as IChessKing;

		public bool IsMatePossible
		{
			get
			{
				if (!HasBothKings) return false;
				if (Exists(PieceDef.WhiteQueen) || Exists(PieceDef.BlackQueen) || Exists(PieceDef.WhiteRook) || Exists(PieceDef.BlackRook)) return true;
				if (Count(PieceDef.WhitePawn) > 0) return true;	// TODO: are pawns able to move?

				int wbc = _pieces[PieceDef.WhiteBishop].Count, bbc = _pieces[PieceDef.BlackBishop].Count,
					wkc = _pieces[PieceDef.WhiteKnight].Count, bkc = _pieces[PieceDef.BlackKnight].Count;
				if (wbc >= 2 || bbc >= 2 || wkc >=2 || bkc >= 2) return true;
				// TODO: consider pawns?
				if (wbc == 1) return wkc > 0;
				if (bbc == 1) return bkc > 0;
				return false;
			}
		}


	}
}
