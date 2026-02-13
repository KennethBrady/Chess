using Chess.Lib.Moves;
using Common.Lib.Contracts;
using System.Diagnostics;

namespace Chess.Lib.Hardware.Pieces
{
	/// <summary>
	/// Internal interface for King.  Provides access to Castling queries.
	/// </summary>
	internal interface IKing : IPiece, IChessKing
	{
		bool IsCastle(ISquare destination, bool isKingside, out ISquare rookSquare, out ISquare rookDest);
		bool IsCastle(ISquare destination, bool isKingside) => IsCastle(destination, isKingside, out _, out _);
		CastleMoveType CastleTypeOf(ISquare destination);
	}

	internal record struct KingState(Hue Side, bool IsChecked, bool IsMated)
	{
		internal static KingState DefaultFor(Hue side) => new KingState(side, false, false);
	}

	internal sealed record King(FileRank StartPosition, Hue Side, IBoard Board) : Piece(StartPosition, PieceType.King, Side, Board), IKing
	{
		internal enum KingStatus { OK, InCheck, Checkmated, Stalemated };

		public override bool CanMoveToImpl(ISquare toSquare)
		{
			if (!this.CanMoveToCore(toSquare)) return false;
			using var d = new ActionDisposer(() => IsQueryingStatus = false);
			IsQueryingStatus = true;
			if (toSquare.Piece is not NoPiece && toSquare.Piece.Side == Side) return false;	// Same as CanMoveToCore.  Delete?
			if (IsCastle(toSquare, true, out _, out _) || IsCastle(toSquare, false, out _, out _)) return true;
			int dR = Math.Abs(Square.Rank - toSquare.Rank), dF = Math.Abs(Square.File - toSquare.File);
			if (dR > 1 || dF > 1) return false;
			// Temporarily move king to square, and see if it can be captured:
			ISquare current = Square;
			IPiece pieceOnSquare = toSquare.Piece;
			toSquare.SetPiece(this);
			current.SetPiece(NoPiece.Default);
			using var undoMove = new ActionDisposer(() =>
			{
				current.SetPiece(this);
				toSquare.SetPiece(pieceOnSquare);
			});
			foreach (IPiece piece in Board.ActivePieces)
			{
				if (piece.Side == Side) continue;
				// Is this necessary?
				if (piece is King k && k.IsQueryingStatus) continue; // prevents re-entry / infinite loop / stack overflow
				if (ReferenceEquals(piece, toSquare.Piece)) continue;
				// TODO: if toSquare has a piece of opposite side, temporarily remove it, simulating a capture.
				if (piece.CanMoveTo(toSquare)) return false; // moving into check
			}
			return true;
		}

		public override bool Move(IChessMove move)
		{
			if (move.IsCastle && IsCastle((ISquare)move.ToSquare, move.IsKingsideCastle, out ISquare rookSq, out ISquare rookDest) &&
				rookSq.Piece is IRook rook)
			{
				((IMove)move).Castle = Castle.Empty with { Type = move.Castle.Type, RookOrigin = rookSq, MovedRook = rook, RookDestination = rookDest };
				ISquare to = (ISquare)move.ToSquare, prev = Square;
				bool kingDestIsRookSq = Board.IsVariantBoard && rookSq.Index == move.ToSquare.Index;
				if (Square.Index != to.Index)
				{
					to.SetPiece(this);
					prev.SetPiece(NoPiece.Default);
				}
				if (rookSq.Index != rookDest.Index)
				{
					rookDest.SetPiece(rook);
					if (!kingDestIsRookSq) rookSq.SetPiece(NoPiece.Default);
				}
				Me.ApplyCastling((IMove)move, this, rook);
				return true;
			}
			else return base.Move(move);
		}

		private bool IsQueryingStatus { get; set; }

		CastleMoveType IKing.CastleTypeOf(ISquare destination)
		{
			if (MoveCount > 0) return CastleMoveType.None;
			if (Square.Rank != destination.Rank) return CastleMoveType.None;
			if (Square.File != File.E && !Board.IsVariantBoard) return CastleMoveType.None;
			if (IsCastle(destination, true, out _, out _)) return CastleMoveType.Kingside;
			if (IsCastle(destination, false, out _, out _)) return CastleMoveType.Queenside;
			return CastleMoveType.None;
		}

		public bool IsCastle(ISquare toSquare, bool isKingSide, out ISquare rookSq, out ISquare rookDest)
		{
			rookSq = rookDest = NoSquare.Default;
			if (MoveCount > 0) return false;
			if (Square.Rank != toSquare.Rank) return false;
			if (Square.File != File.E && !Board.IsVariantBoard) return false;
			Rank toRank = Square.Rank;
			File toFile = isKingSide ? File.G : File.C;
			if (Board[toFile, toRank].Index == toSquare.Index)
			{
				toFile = isKingSide ? File.F : File.D;
				IPiece? rook = null;
				if (Board.IsVariantBoard)
				{
					if (isKingSide)
					{
						for (File f = File.H; f >= File.A; f--)
						{
							ISquare sq = Board[f, toRank];
							if (sq.Piece is IRook r)
							{
								rook = r;
								break;
							}
						}
					}
					else
					{
						for (File f = File.A; f <= File.H; f++)
						{
							ISquare sq = Board[f, toRank];
							if (sq.Piece is IRook r)
							{
								rook = r;
								break;
							}
						}
					}
				}
				else
				{
					File fr = isKingSide ? File.H : File.A;
					ISquare rsq = Board[fr, toRank];
					if (rsq.Piece is IRook r) rook = r;
				}
				if (rook == null) return false;
				rookSq = rook.Square;
				rookDest = Board[toFile, toRank];
				foreach (ISquare s in Board.FileSquaresBetween(Square, toSquare))
				{
					if (s.HasPiece && !ReferenceEquals(s.Piece, rook)) return false;
					foreach (IPiece p in Board.ActivePieces.Where(pp => pp.Side != Side))
					{
						if (p.CanMoveTo(s)) return false; // Pawn??  p.CanCaptureTo returns false if s has no piece.
					}
				}
				return true;
			}
			return false;
		}

		private bool CanResolveCheck()
		{
			// TODO: examine moves to capture attacker and block check.
			IKing me = this;
			ISquare mySq = Square;
			if (Board.KingSquares(Square).Any(s => CanMoveTo(s))) return true;
			List<IPiece> attackers = Board.ActivePieces.Where(p => p.Side != me.Side && p.CanMoveTo(mySq)).ToList();
			switch (attackers.Count)
			{
				case 0: throw new UnreachableException("King is in check with no attackers??");
				case 1:
					IPiece attacker = attackers[0];
					if (Board.PiecesTargeting(attacker.Square, Side).Count() > 0) return true;
					//if (Board.ActivePieces.Where(p => p.Side == me.Side).Any(p => p.CanMoveTo(attacker.Square))) return true;
					// No pieces.  Can check be blocked?
					if (attacker.Type == PieceType.Knight) return false;
					// 'Brute Force' search for resolving moves:
					List<ISquare> between = Board.SquaresBetween(attacker.Square, mySq).ToList();
					foreach (IPiece p in Board.ActivePieces.Where(p => p.Side == me.Side))
					{
						if (between.Any(s => p.CanMoveTo(s))) return true;
					}
					break;
				default: foreach (ISquare s in Board.KingSquares(mySq)) if (me.CanMoveTo(s)) return true; break;
			}
			return false;
		}

		//TODO: Convert to property
		public bool IsInCheck()
		{
			IPiece? p = Board.ActivePieces.Where(p => p.Side != Side && p.CanCaptureTo(Square)).FirstOrDefault();
			return p != null;
		}

		public KingState CurrentState()
		{
			IPiece? p = Board.ActivePieces.Where(p => p.Side != Side && p.CanCaptureTo(Square)).FirstOrDefault();
			return new KingState(Side, p != null, p != null && IsMated);
		}

		public bool IsMated
		{
			get
			{
				if (!IsInCheck()) return false;
				return !CanResolveCheck();
			}
		}

		public (bool ksPossible, bool qsPossible) IsFutureCastlePossible
		{
			get
			{
				bool ks = false, qs = false;
				if (MoveCount > 0) return (ks, qs);
				foreach (IPiece p in Board.ActivePieces)
				{
					if (p.Side == Side && p is IRook r)
					{
						switch ((StartPosition.Rank, r.StartPosition.File))
						{
							case (Rank.R8, File.A):
							case (Rank.R1, File.A): qs = r.MoveCount == 0; break;
							case (Rank.R8, File.H):
							case (Rank.R1, File.H): ks = r.MoveCount == 0; break;
						}
					}
				}
				return (ks, qs);
			}
		}

		internal KingStatus GetStatus()
		{
			using var d = new ActionDisposer(() => IsQueryingStatus = false);
			IsQueryingStatus = true;
			bool inCheck = IsInCheck();
			int kingMoveCount = Board.KingSquares(Square).Where(s => CanMoveTo(s)).Count();
			if (inCheck)
			{
				return CanResolveCheck() ? KingStatus.InCheck : KingStatus.Checkmated;
			}
			if (kingMoveCount == 0)
			{
				if (Board.ActivePieces.Where(p => p.Side == Side && p.Type != PieceType.King).Any(p => p.HasAnyMove())) return KingStatus.OK;
				return KingStatus.Stalemated;
			}
			return KingStatus.OK;
		}

		internal IKing OtherKing
		{
			get
			{
				IPiece me = this;
				foreach (IPiece p in Board.ActivePieces)
				{
					if (p.Type == PieceType.King && p.Side != Side) return (IKing)p;
				}
				// unreachable
				throw new Exception("Unreachable code");
			}
		}

		private bool HasMoves()
		{
			//Board.KingSquares(_square).Any(s => CanMoveTo(s));
			IKing k = this;
			foreach (ISquare s in Board.KingSquares(Square))
			{
				if (k.CanMoveTo(s)) return true;
			}
			return false;
		}

		protected override IPiece CopyFor(IBoard forBoard) => new King(StartPosition, Side, forBoard);

		private new IKing Me => (IKing)base.Me;
	}
}
