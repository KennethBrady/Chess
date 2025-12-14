using Chess.Lib.Moves;
using Chess.Lib.Moves.Parsing;

namespace Chess.Lib.Hardware.Pieces
{
	internal interface IPawn : IPiece, IChessPawn
	{
		bool IsPromotion(ISquare square);
		bool CanPromote();
		IPiece Promote(PieceType to, ISquare targetSquare);
		bool IsEnPassant(ISquare square, out IPawn captured);
		IEnumerable<ISquare> PromotionSquares();
	}

	internal sealed record Pawn(FileRank StartPosition, Hue Side, IBoard Board) : Piece(StartPosition, PieceType.Pawn, Side, Board), IPawn
	{
		public override bool CanCaptureTo(ISquare square)
		{
			if (IsEnPassant(square, out _)) return true;
			if (!base.CanCaptureTo(square)) return false;
			int rankDif = Math.Abs(square.Rank - Square.Rank), fileDif = Math.Abs(square.File - Square.File);
			return rankDif == 1 && fileDif == 1;
		}

		public override bool CanMoveToImpl(ISquare square)
		{
			switch (Side) // Don't move backward.
			{
				case Hue.Light: if (square.Rank <= Square.Rank) return false; break;
				case Hue.Dark: if (square.Rank >= Square.Rank) return false; break;
			}
			int rankDif = Math.Abs(square.Rank - Square.Rank), fileDif = Math.Abs(square.File - Square.File);
			if (rankDif <= 0 || rankDif > 2 || fileDif > 1) return false;
			if (rankDif == 2)
			{
				if (fileDif > 0) return false;      // double-push straight ahead
				if (MoveCount > 0) return false;    // no double-push after first move
				if (square.HasPiece) return false;  // no capture
				Rank rOver = Side == Hue.Light ? Square.Rank + 1 : Square.Rank - 1;
				ISquare over = Board[Square.File, rOver];
				if (over.HasPiece) return false;    // Cannot jump
			}
			if (IsEnPassant(square, out _)) return true;
			if (square.File == Square.File) // simple move:
			{
				if (square.HasPiece) return false;
				return true;
			}
			else // capture
			{
				if (fileDif != 1) return false;
				return square.HasPiece && square.Piece.Side != Side;
			}
		}

		public override bool Move(IChessMove move)
		{
			ISquare to = (ISquare)move.ToSquare;
			if (IsEnPassant(to, out IPawn ep))
			{
				ep.Square.SetPiece(NoPiece.Default);
				ep.SetSquare(NoSquare.Default);
				Me.SetSquare((ISquare)move.ToSquare);
				to.SetPiece(this);
				((ISquare)move.FromSquare).SetPiece(NoPiece.Default);
				return true;
			}
			return base.Move(move);
		}

		public bool IsEnPassant(IMoveParseSuccess move) => IsEnPassant(move, out _);

		public bool IsEnPassant(IMoveParseSuccess move, out IPawn captured) =>
			IsEnPassant((ISquare)move.ToSquare, out captured);

		public bool IsEnPassant(ISquare square, out IPawn captured)
		{
			captured = NoPawn.Default;
			int fileDiff = Square.File - square.File, rankDiff = Square.Rank - square.Rank;
			if (Math.Abs(fileDiff) != 1) return false;
			Rank capRank = Rank.R1;
			switch (Side)
			{
				case Hue.Light:
					if (Square.Rank != Rank.R5) return false;
					if (square.Rank != Rank.R6) return false;
					capRank = Rank.R5;
					break;
				case Hue.Dark:
					if (Square.Rank != Rank.R4) return false;
					if (square.Rank != Rank.R3) return false;
					capRank = Rank.R4;
					break;
			}
			ISquare capSq = Board[square.File, capRank];
			if (capSq.Piece is not IPawn pcap) return false;
			if (pcap.Side == Side) return false;
			if (pcap.MoveCount != 1) return false;
			if (pcap.PreviousMove != Board.LastMove) return false;
			captured = pcap;
			return true;
		}

		public Rank PromotionRank
		{
			get
			{
				switch (Square.Rank)
				{
					case Rank.R2: return Rank.R1;
					case Rank.R7: return Rank.R8;
					default: return Rank.Offboard;
				}
			}
		}

		public IEnumerable<ISquare> PromotionSquares()
		{
			IEnumerable<ISquare> squaresFor(Rank onRank)
			{
				int ifMin = Math.Max((int)Square.File - 1, 0), ifMax = Math.Min((int)Square.File + 1, 7);
				for (int i = ifMin; i <= ifMax; i++)
				{
					ISquare tgt = Board[(File)i, onRank];
					switch (tgt.Piece)
					{
						case NoPiece: yield return tgt; break;
						case IPiece piece:
							if (piece.Side == Side) break;
							int fDiff = Math.Abs(i - (int)Square.File);
							if (fDiff == 1) yield return tgt;
							break;
					}
				}
			}
			switch (Square.Rank)
			{
				case Rank.R2: return squaresFor(Rank.R1);
				case Rank.R7: return squaresFor(Rank.R8);
				default: return Enumerable.Empty<ISquare>();
			}
		}

		public bool CanPromote()
		{
			return PromotionSquares().Any(s => CanMoveTo(s));
		}

		public bool IsPromotion(ISquare square) => PromotionSquares().Any(s => s.Index ==  square.Index);

		public IPiece Promote(PieceType to, ISquare targetSquare)
		{
			if (!CanPromote()) return NoPiece.Default;
			IPiece r;
			FileRank pos = new FileRank(targetSquare.File, targetSquare.Rank);
			switch (to)
			{
				case PieceType.Queen: r = new Queen(pos, Side, Board); break;
				case PieceType.Rook: r = new Rook(pos, Side, Board); break;
				case PieceType.Bishop: r = new Bishop(pos, Side, Board); break;
				case PieceType.Knight: r = new Knight(pos, Side, Board); break;
				default: return NoPiece.Default;
			}
			r.SetSquare(targetSquare);
			targetSquare.SetPiece(r);
			Square.SetPiece(NoPiece.Default);
			Me.SetSquare(NoSquare.Default);
			return r;
		}

		internal bool CanMoveEnPassant(IPiece lastMovedPiece, out IChessSquare? toSquare)
		{
			toSquare = null;
			if (lastMovedPiece == null || lastMovedPiece.Type != PieceType.Pawn) return false;
			if (lastMovedPiece.MoveCount != 1) return false;
			int df = lastMovedPiece.Square.File - Square.File;
			if (Math.Abs(df) != 1) return false;
			File f = lastMovedPiece.Square.File;
			Rank r = lastMovedPiece.Square.Rank;
			if (lastMovedPiece.Side == Hue.Light) r++; else r--;
			ISquare s = Board[f, r];
			if (!s.HasPiece) toSquare = s;
			return !s.HasPiece;
		}

		protected override IPiece CopyFor(IBoard forBoard) => new Pawn(StartPosition, Side, forBoard);

		internal static bool InvitesEnpassant(IMove lastMove, out FileRank target)
		{
			target = FileRank.OffBoard;
			if (lastMove.MovedPiece is not IPawn p) return false;
			if (p.MoveCount > 1) return false;
			int fDif = Math.Abs(lastMove.RankChange);
			if (fDif != 2) return false;
			if (fDif == 2)
				switch (p.Square.Rank)
				{
					case Rank.R4: target = new FileRank(p.Square.File, Rank.R3); break;
					case Rank.R5: target = new FileRank(p.Square.File, Rank.R6); break;
					default: return false;
				}
			return true;
		}
	}
}
