using Chess.Lib.Hardware.Pieces;

namespace Chess.Lib.Hardware
{
	public record struct PieceDef(PieceType Type, Hue Hue)
	{
		public static readonly PieceDef Default = new PieceDef(PieceType.None, Hue.Default);
		public static readonly PieceDef WhiteKing = new PieceDef(PieceType.King, Hue.White);
		public static readonly PieceDef BlackKing = new PieceDef(PieceType.King, Hue.Black);
		public static readonly PieceDef WhiteQueen = new PieceDef(PieceType.Queen, Hue.White);
		public static readonly PieceDef BlackQueen = new PieceDef(PieceType.Queen, Hue.Black);
		public static readonly PieceDef WhiteRook = new PieceDef(PieceType.Rook, Hue.White);
		public static readonly PieceDef BlackRook = new PieceDef(PieceType.Rook, Hue.Black);
		public static readonly PieceDef WhiteBishop = new PieceDef(PieceType.Bishop, Hue.White);
		public static readonly PieceDef BlackBishop = new PieceDef(PieceType.Bishop, Hue.Black);
		public static readonly PieceDef WhiteKnight = new PieceDef(PieceType.Knight, Hue.White);
		public static readonly PieceDef BlackKnight = new PieceDef(PieceType.Knight, Hue.Black);
		public static readonly PieceDef WhitePawn = new PieceDef(PieceType.Pawn, Hue.White);
		public static readonly PieceDef BlackPawn = new PieceDef(PieceType.Pawn, Hue.Black);
		public bool IsDefault => Type == PieceType.None || Hue == Hue.Default;

		public static IEnumerable<PieceDef> All
		{
			get
			{
				foreach(PieceType pt in PieceTypeExtensions.AllValid)
				{
					yield return new PieceDef(pt, Hue.White);
					yield return new PieceDef(pt, Hue.Black);
				}
			}
		}

	}

	internal record struct PlacedPiece(int Index, PieceType PieceType, Hue Hue);

	public class BoardBuilder
	{
		private Dictionary<Rank, PieceDef[]> _pieces = new();
		public BoardBuilder() 
		{
			foreach(Rank r in RFExtensions.AllRanks)
			{
				if (r == Rank.Offboard) continue;
				PieceDef[] row = new PieceDef[8];
				Array.Fill(row, PieceDef.Default);
				_pieces.Add(r, row);
			}
		}

		public bool SetPiece(FileRank position, PieceType type, Hue hue) => SetPiece(position.File, position.Rank, new PieceDef(type, hue));

		public bool SetPiece(File file, Rank rank, PieceType pieceType, Hue hue) => SetPiece(file, rank, new PieceDef(pieceType, hue));

		public bool SetPiece(File file, Rank rank, PieceDef piece)
		{
			if (file == File.Offboard || rank == Rank.Offboard) return false;
			_pieces[rank][(int)file] = piece;
			return true;
		}

		public void SetPieces(Rank rank, PieceDef[] pieces)
		{
			int max = Math.Min(8, pieces.Length);
			for (int i = 0; i < max; i++)
			{
				_pieces[rank][i] = pieces[i];
			}
		}

		public bool RemovePiece(File file, Rank rank)
		{
			if (file == File.Offboard || rank == Rank.Offboard) return false;
			PieceDef[] defs = _pieces[rank];
			defs[(int)file] = PieceDef.Default;
			return true;
		}

		public void Clear()
		{
			foreach(Rank r in RFExtensions.AllRanks)
			{
				if (r == Rank.Offboard) continue;
				Array.Fill(_pieces[r], PieceDef.Default);
			}
		}

		public int PieceCount => _pieces.Values.SelectMany(v => v.Where(p => !p.IsDefault)).Count();

		private IEnumerable<PlacedPiece> PlacedPieces()
		{
			for (int r = 0; r < 8; ++r)
			{
				Rank rank = (Rank)r;
				for (int f = 0; f < 8; ++f)
				{
					File file = (File)f;
					FileRank fr = new FileRank(file, rank);
					PieceDef pd = _pieces[rank][f];
					if (!pd.IsDefault) yield return new PlacedPiece(fr.ToSquareIndex, pd.Type, pd.Hue);
				}
			}
		}

		public IChessBoard CreateBoard()
		{
			Board b = new Board(PlacedPieces());
			return b;
		}
	}
}
