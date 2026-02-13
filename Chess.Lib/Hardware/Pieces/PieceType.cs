using Chess.Lib.Moves.Parsing;

namespace Chess.Lib.Hardware.Pieces
{
	public enum PieceType
	{
		None = 0,
		Pawn = 10,
		Rook = 50,
		Knight = 30,
		Bishop = 35,
		Queen = 90,
		King = 25
	};

	internal static class PieceTypeExtensions
	{
		private static readonly char[] _promos = { 'r', 'n', 'b', 'q' };
		internal static bool IsPromotion(char c) => _promos.Contains(c);
		internal static char? PromotionChar(PieceType piece)
		{
			switch (piece)
			{
				case PieceType.Rook: return 'r';
				case PieceType.Knight: return 'n';
				case PieceType.Bishop: return 'b';
				case PieceType.Queen: return 'q';
			}
			return null;
		}

		public static PieceType Promotion(char c, bool allowPawnAndKing = true)
		{
			switch (char.ToLower(c))
			{
				case 'r': return PieceType.Rook;
				case 'n': return PieceType.Knight;
				case 'b': return PieceType.Bishop;
				case 'q': return PieceType.Queen;
				case 'k': return allowPawnAndKing ? PieceType.King : PieceType.None;
				case 'p': return allowPawnAndKing ? PieceType.Pawn : PieceType.None;
				default: return PieceType.None;
			}
		}

		public static PieceType? GetPromotion(string moves, int moveStartPosition)
		{
			if (string.IsNullOrEmpty(moves) || (moves.Length < moveStartPosition + 5)) return null;
			switch (moves[moveStartPosition + 4])
			{
				case 'r':
				case 'n':
				case 'q': return Promotion(moves[moveStartPosition + 5], false);
				case 'b': break;
				default: return null;
			}
			// b for Bishop, but might be the File of the next move:
			if (moves.Length < moveStartPosition + 6) return null;
			char c = moves[moveStartPosition + 5];
			if (char.IsDigit(c)) return null;
			return PieceType.Bishop;
		}

		public static char PieceCharacter(PieceType type, Hue hue)
		{
			char r = ' ';
			switch (type)
			{
				case PieceType.Rook: r = 'r'; break;
				case PieceType.Knight: r = 'n'; break;
				case PieceType.Bishop: r = 'b'; break;
				case PieceType.Queen: r = 'q'; break;
				case PieceType.King: r = 'k'; break;
				case PieceType.Pawn: r = 'p'; break;
			}
			return (hue == Hue.Dark) ? r : Char.ToUpper(r);
		}

		extension(IPiece p)
		{
			internal char PieceCharacter() => PieceCharacter(p.Type, p.Side);
			internal bool HasAnyMove() => p.Board.Any(s => p.CanMoveTo(s));
			internal bool CanMoveToCore(ISquare square)
			{
				if (p.Square.Index == square.Index) return false;
				if (square.Piece is not NoPiece && square.Piece.Side == p.Side) return false;
				return true;
			}
			internal bool MoveToCore(IMoveParseSuccess move)
			{
				if (!(move.ToSquare is ISquare to) || p.Square is NoSquare) return false;
				((ISquare)move.FromSquare).SetPiece(NoPiece.Default);
				p.Square.SetPiece(NoPiece.Default);
				to.SetPiece(p);
				return true;
			}
			internal IEnumerable<ISquare> SquaresBetween(ISquare toSquare) => p.Board.SquaresBetween(p.Square, toSquare);
			internal string Name() => $"{PieceCharacter(p)} {p.Square.Name}";
		}

		public static char PGNChar(this PieceType pt)
		{
			switch (pt)
			{
				case PieceType.Knight: return 'N';
				default: return pt.ToString()[0];
			}
		}

		extension(PieceType pieceType)
		{
			internal bool IsPromotionTarget
			{
				get
				{
					switch (pieceType)
					{
						case PieceType.Knight:
						case PieceType.Bishop:
						case PieceType.Rook:
						case PieceType.Queen: return true;
						default: return false;
					}
				}
			}
		}

	}

}
