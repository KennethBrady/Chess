
using Chess.Lib.Games;
using Chess.Lib.Hardware.Pieces;
using Chess.Lib.Moves;
using System.Text;

namespace Chess.Lib.Hardware
{
	public struct FEN
	{
		public const string FENStart = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
		public static readonly FEN Empty = new FEN();

		//private static Regex _rxFEN = new Regex(@"\s*^(((?:[rnbqkpRNBQKP1-8]+\/){7})[rnbqkpRNBQKP1-8]+)\s([b|w])\s([K|Q|k|q]{1,4})\s(-|[a-h][1-8])\s(\d+\s\d+)$", RegexOptions.Compiled);
		public static bool TryParse(string fen, out FEN result)
		{
			result = Empty;
			if (string.IsNullOrEmpty(fen)) return false;
			//if (!_rxFEN.IsMatch(fen)) return false;
			try
			{
				result = new FEN(fen);
				return true;
			}
			catch { }
			return false;
		}

		public FEN(string fen)
		{
			string[] parts = fen.Split(' ');
			if (parts.Length != 6) throw new ArgumentException("Invalid FEN string: " + fen);
			PiecePlacement = parts[0];
			switch (parts[1])
			{
				case "w": NextMoveColor = Hue.Light; break;
				case "b": NextMoveColor = Hue.Dark; break;
			}
			foreach (char c in parts[2])
			{
				switch (c)
				{
					case '-': break;
					case 'K': CanWhiteCastleKingside = true; break;
					case 'Q': CanWhiteCastleQueenside = true; break;
					case 'k': CanBlackCastleKingside = true; break;
					case 'q': CanBlackCastleQueenside = true; break;
				}
			}
			EnPassantTarget = FileRank.Parse(parts[3]);
			HalfMovesSinceLastCapture = int.Parse(parts[4]);
			FullMoveCount = int.Parse(parts[5]);
			// last two fields?

		}

		public FEN(IReadOnlyChessGame game)
		{
			PiecePlacement = game.Board.AsFEN();
			switch (game.LastMoveMade.MovedPiece.Side)
			{
				case Hue.Light: NextMoveColor = Hue.Dark; break;
				default: NextMoveColor = Hue.Light; break;
			}
			IChessKing wk = game.White.King, bk = game.Black.King;
			(CanWhiteCastleKingside, CanWhiteCastleQueenside) = wk.IsFutureCastlePossible;
			(CanBlackCastleKingside, CanBlackCastleQueenside) = bk.IsFutureCastlePossible;
			if (Pawn.InvitesEnpassant((IMove)game.LastMoveMade, out FileRank target)) EnPassantTarget = target;
			int nMoves = 0;
			foreach (IMove move in game.Moves.Reverse())
			{
				if (move.SerialNumber > game.LastMoveMade.SerialNumber) continue;
				if (move.IsCapture || move.MovedPiece is IPawn) break;
				nMoves++;
			}
			HalfMovesSinceLastCapture = nMoves;
			FullMoveCount = 1 + game.Moves.PriorMoves.Where(m => m.MovedPiece.Side == Hue.Dark).Count();
		}

		public bool IsEmpty => string.IsNullOrEmpty(PiecePlacement);
		public string PiecePlacement { get; set; } = string.Empty;
		public Hue? NextMoveColor { get; set; }
		public bool CanWhiteCastleKingside { get; set; }
		public bool CanWhiteCastleQueenside { get; set; }
		public bool CanBlackCastleKingside { get; set; }
		public bool CanBlackCastleQueenside { get; set; }
		public FileRank EnPassantTarget { get; set; } = FileRank.OffBoard;
		public int HalfMovesSinceLastCapture { get; set; }
		public int FullMoveCount { get; set; }

		public override string ToString()
		{
			StringBuilder s = new StringBuilder(PiecePlacement);
			void add(string rec) => s.Append(" ").Append(rec);
			if (NextMoveColor.HasValue)
			{
				switch (NextMoveColor.Value)
				{
					case Hue.Light: add("w"); break;
					case Hue.Dark: add("b"); break;
				}
			}
			else add("-");
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
