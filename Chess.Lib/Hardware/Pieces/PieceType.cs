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

	#region PieceTypeExtensions

	internal static class PieceTypeExtensions
	{
		private static readonly char[] _promos = { 'r', 'n', 'b', 'q' };
		public static bool IsPromotion(char c) => _promos.Contains(c);
		public static char? PromotionChar(PieceType piece)
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

		public static char PieceCharacter(this IPiece p) => PieceCharacter(p.Type, p.Side);

		public static char PGNChar(this PieceType pt)
		{
			switch (pt)
			{
				case PieceType.Knight: return 'N';
				default: return pt.ToString()[0];
			}
		}

		internal static bool HasAnyMove(this IPiece piece) =>
			piece.Board.Any(square => piece.CanMoveTo((ISquare)square));

		internal static bool CanMoveToCore(this IPiece piece, ISquare square)
		{
			if (piece.Square.Index == square.Index) return false;
			if (square.Piece is not NoPiece && square.Piece.Side == piece.Side) return false;
			return true;
		}

		internal static bool MoveToCore(this IPiece piece, IParseSuccess move)
		{
			if (!(move.ToSquare is ISquare to) || piece.Square is NoSquare) return false;
			((ISquare)move.FromSquare).SetPiece(NoPiece.Default);
			piece.Square.SetPiece(NoPiece.Default);
			to.SetPiece(piece);
			return true;
		}

		internal static IEnumerable<ISquare> SquaresBetween(this IPiece piece, ISquare toSquare)
		{
			return piece.Board.SquaresBetween(piece.Square, toSquare);
		}

		internal static string Name(this IPiece piece) => $"{PieceCharacter(piece)} {piece.Square.Name}";
	}

	#endregion
}
