
using Chess.Lib.Games;
using Chess.Lib.Hardware.Pieces;
using Chess.Lib.Moves;
using System.Text;

namespace Chess.Lib.Hardware
{
	public record struct PiecePlacement(PieceDef Piece, FileRank Location);


	public struct FEN
	{
		/// <summary>
		/// FEN corresponding to a classical board with no moves played.
		/// </summary>
		public const string FENStart = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
		public static readonly FEN Empty = new FEN();

		private static readonly char[] _fenChars = { 'r','n', 'b', 'q', 'k', 'p', '/' };
		public static bool IsValidPiecePlacements(string s)
		{
			int n = s.Where(c => c == '/').Count();
			if (n != 7) return false;
			return s.All(c => char.IsDigit(c) || _fenChars.Contains(char.ToLower(c)));
		}

		public static FEN Parse(string fen)
		{
			string[] parts = fen.Split(' ');
			switch(parts.Length)
			{
				case 1:
					if (IsValidPiecePlacements(parts[0])) return new FEN
					{
						PiecePlacement = parts[0]
					};
					break;
				case 6:
					if (IsValidPiecePlacements(parts[0])) return new FEN(parts);					
					break;
			}
			return Empty;
		}

		public IChessBoard ToBoard() => new Board(this);

		private FEN(string[] parts)
		{
			PiecePlacement = parts[0];
			switch (parts[1])
			{
				case "w": NextMoveColor = Hue.White; break;
				case "b": NextMoveColor = Hue.Black; break;
			}
			foreach (char c in parts[2])
			{
				switch (c)
				{
					case '-': break;
					case 'K': WhiteCastling |= CastleMoveType.Kingside; break;
					case 'Q': WhiteCastling |= CastleMoveType.Queenside; break;
					case 'k': BlackCastling |= CastleMoveType.Kingside; break;
					case 'q': BlackCastling |= CastleMoveType.Queenside; break;
				}
			}
			EnPassantTarget = FileRank.Parse(parts[3]);
			HalfMovesSinceLastCapture = int.Parse(parts[4]);
			FullMoveCount = int.Parse(parts[5]);
		}

		public FEN(IChessGame game)
		{
			PiecePlacement = game.Board.FENPiecePlacements;
			switch (game.LastMoveMade.MovedPiece.Side)
			{
				case Hue.White: NextMoveColor = Hue.Black; break;
				default: NextMoveColor = Hue.White; break;
			}
			IChessKing wk = game.White.King, bk = game.Black.King;
			var c = wk.IsFutureCastlePossible;
			if (c.ksPossible) WhiteCastling |= CastleMoveType.Kingside;
			if (c.qsPossible) WhiteCastling |= CastleMoveType.Queenside;
			c = bk.IsFutureCastlePossible;
			if (c.ksPossible) BlackCastling |= CastleMoveType.Kingside;
			if (c.qsPossible) BlackCastling |= CastleMoveType.Queenside;
			if (Pawn.InvitesEnpassant((IMove)game.LastMoveMade, out FileRank target)) EnPassantTarget = target;
			int nMoves = 0;
			foreach (IMove move in game.Moves.Reverse())
			{
				if (move.SerialNumber > game.LastMoveMade.SerialNumber) continue;
				if (move.IsCapture || move.MovedPiece is IPawn) break;
				nMoves++;
			}
			HalfMovesSinceLastCapture = nMoves;
			FullMoveCount = 1 + game.Moves.PriorMoves.Where(m => m.MovedPiece.Side == Hue.Black).Count();
		}

		public bool IsEmpty => string.IsNullOrEmpty(PiecePlacement);
		public string PiecePlacement { get; private init; } = string.Empty;
		public Hue NextMoveColor { get; private init; } = Hue.Default;

		public CastleMoveType WhiteCastling { get; private init; } = CastleMoveType.None;
		public CastleMoveType BlackCastling { get; private init; } = CastleMoveType.None;

		public bool CanWhiteCastleKingside => WhiteCastling.HasFlag(CastleMoveType.Kingside);
		public bool CanWhiteCastleQueenside => WhiteCastling.HasFlag(CastleMoveType.Queenside);
		public bool CanBlackCastleKingside => BlackCastling.HasFlag(CastleMoveType.Kingside);
		public bool CanBlackCastleQueenside => BlackCastling.HasFlag(CastleMoveType.Queenside);
		public FileRank EnPassantTarget { get; private init; } = FileRank.OffBoard;
		public int HalfMovesSinceLastCapture { get; private init; }
		public int FullMoveCount { get; private init; }

		public IEnumerable<PiecePlacement> Pieces
		{
			get
			{
				string[] ranks = PiecePlacement.Split('/');
				if (ranks.Length != 8) yield break;
				Rank r = Rank.R8;
				foreach(string rank in ranks)
				{
					File f = File.A;
					foreach(char c in rank)
					{
						if (char.IsDigit(c)) f += int.Parse(c.ToString()); else
						{
							Hue h = char.IsUpper(c) ? Hue.White : Hue.Black;
							PieceType type = PieceTypeExtensions.Promotion(c);
							yield return new PiecePlacement(new PieceDef(type, h), new FileRank(f, r));
							f++;
						}
					}
					r--;
				}
			}
		}

		public override string ToString()
		{
			StringBuilder s = new StringBuilder(PiecePlacement);
			void add(string rec) => s.Append(" ").Append(rec);
			switch (NextMoveColor)
			{
				case Hue.White: add("w"); break;
				case Hue.Black: add("b"); break;
				default: add("-"); break;
			};
			s.Append(" ");
			if (CanWhiteCastleKingside) s.Append("K");
			if (CanWhiteCastleQueenside) s.Append("Q");
			if (CanBlackCastleKingside) s.Append("k");
			if (CanBlackCastleQueenside) s.Append("q");
			if (s[s.Length - 1] == ' ') s.Append("-");
			if (EnPassantTarget.IsOffBoard) add("-"); else add(EnPassantTarget.ToEngineMove);
			add(HalfMovesSinceLastCapture.ToString());
			add(FullMoveCount.ToString());
			return s.ToString();
		}

	}
}
