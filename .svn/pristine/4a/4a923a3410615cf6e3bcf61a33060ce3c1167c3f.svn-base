using Chess.Lib.Hardware.Pieces;

namespace Chess.Lib.Hardware
{
	internal record struct PlacedPiece(int Index, PieceType PieceType, Hue Hue);

	public class BoardBuilder
	{
		private readonly Board _board;
		private readonly List<PlacedPiece> _piecePlacements = new();
		public BoardBuilder(bool populateStandard = false)
		{
			_board = new Board(false);
			if (populateStandard) _piecePlacements.AddRange(_board.Select(s => _board.DefaultsFor(s.File, s.Rank)).Where(p => p.PieceType != PieceType.None));
		}

		public bool SetPiece(File file, Rank rank, PieceType pieceType, Hue hue)
		{
			int ndx = Board.IndexOf(file, rank);
			if (_piecePlacements.Any(pp => pp.Index == ndx)) return false;
			_piecePlacements.Add(new PlacedPiece(ndx, pieceType, hue));
			return true;
		}

		public bool RemovePiece(File file, Rank rank)
		{
			int ndx = Board.IndexOf(file, rank);
			for (int i = 0; i < _piecePlacements.Count; i++)
			{
				if (_piecePlacements[i].Index == ndx)
				{
					_piecePlacements.RemoveAt(i);
					return true;
				}
			}
			return false;
		}

		public void Clear() => _piecePlacements.Clear();

		public int PieceCount => _piecePlacements.Count;

		public IChessBoard CreateBoard()
		{
			Board r = new Board(false);
			r.Build(_piecePlacements);
			return r;
		}
	}
}
