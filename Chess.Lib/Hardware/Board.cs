using Chess.Lib.Games;
using Chess.Lib.Hardware.Pieces;
using Chess.Lib.Moves;
using Common.Lib.Contracts;
using System.Collections;
using System.Collections.Immutable;
using System.Text;

namespace Chess.Lib.Hardware
{
	internal sealed class Board : IBoard
	{
		internal static readonly Rank[] AllRanks = { Rank.R1, Rank.R2, Rank.R3, Rank.R4, Rank.R5, Rank.R6, Rank.R7, Rank.R8 };
		internal static readonly File[] AllFiles = { File.A, File.B, File.C, File.D, File.E, File.F, File.G, File.H };

		internal static readonly Board Default = new Board(false);

		private readonly ISquare[] _squares = new ISquare[64];
		private ImmutableList<IPiece> _pieces;
		private ImmutableDictionary<int, IPiece> _startPieces;
		private ImmutableList<IPiece> _removedPieces = ImmutableList<IPiece>.Empty;
		private ImmutableList<IPiece> _promotions = ImmutableList<IPiece>.Empty;
		private IGame _game = NoGame.Default;
		public Board() : this(true) { }

		public Board(string fen) : this(new FEN(fen)) { }

		public Board(FEN fen) : this(false)
		{
			string[] parts = fen.PiecePlacement.Split('/');
			Rank r = Rank.R8;
			List<IPiece> pieces = new();
			foreach (string p in parts)
			{
				int nFile = 0;
				foreach (char c in p)
				{
					if (char.IsDigit(c)) nFile += int.Parse(c.ToString());
					else
					{
						Hue h = char.IsUpper(c) ? Hue.Light : Hue.Dark;
						PieceType type = PieceTypeExtensions.Promotion(c);
						File f = (File)nFile;
						ISquare s = (ISquare)this[f, r];
						IPiece piece = NoPiece.Default;
						switch (type)
						{
							case PieceType.Pawn: piece = new Pawn(new FileRank(f, r), h, this); break;
							case PieceType.Rook: piece = new Rook(new FileRank(f, r), h, this); break;
							case PieceType.Knight: piece = new Knight(new FileRank(f, r), h, this); break;
							case PieceType.Bishop: piece = new Bishop(new FileRank(f, r), h, this); break;
							case PieceType.Queen: piece = new Queen(new FileRank(f, r), h, this); break;
							case PieceType.King: piece = new King(new FileRank(f, r), h, this); break;
						}
						piece.SetSquare(s);
						s.SetPiece(piece);
						pieces.Add(piece);
						nFile++;
					}
				}
				r--;
			}
			_pieces = ImmutableList<IPiece>.Empty.AddRange(pieces);
			IsVariantBoard = true;
			// TODO: maintain fen-state
		}

		internal Board(bool populatePieces = true)
		{
			List<IPiece> pcs = new();
			for (int r = 0; r < 8; ++r)
			{
				for (int f = 0; f < 8; ++f)
				{
					Rank rnk = (Rank)r;
					File file = (File)f;
					Hue h = Hue.Default;
					bool reven = r % 2 == 0, feven = f % 2 == 0;
					if (reven) h = feven ? Hue.Dark : Hue.Light; else h = feven ? Hue.Light : Hue.Dark;
					ISquare s = new Square(this, file, rnk, h, IndexOf(file, rnk));
					_squares[s.Index] = s;
					if (populatePieces)
					{
						IPiece p = CreateDefaultPiece(file, rnk);
						p.SetSquare(s);
						s.SetPiece(p);
						if (p is not NoPiece) pcs.Add(p); ;
					}
				}
			}
			_pieces = ImmutableList.Create(pcs.ToArray());
			_startPieces = ImmutableDictionary<int, IPiece>.Empty.AddRange(_pieces.ToDictionary(p => p.StartPosition.ToSquareIndex));
		}

		public IChessSquare this[File file, Rank rank] => SquareAt(file, rank);
		public IChessSquare this[FileRank fileRank] => SquareAt(fileRank.File, fileRank.Rank);
		public IChessSquare this[int squareIndex] => this[PositionOf(squareIndex)];
		public event Handler<IChessMove>? MoveMade;
		ISquare IBoard.this[File file, Rank rank] => (ISquare)this[file, rank];
		IReadOnlyList<IChessPiece> IChessBoard.ActivePieces => _pieces;
		ImmutableList<IPiece> IBoard.ActivePieces => _pieces;
		IEnumerable<IChessPiece> IChessBoard.RemovedPieces => _removedPieces;
		public IReadOnlyList<IChessPiece> Promotions => _promotions;
		public IReadOnlyList<IChessPiece> Removed => _removedPieces;
		ImmutableList<IPiece> IBoard.Promotions => _promotions;
		ImmutableList<IPiece> IBoard.RemovedPieces => _removedPieces;
		public event Handler<IChessboardState>? StateApplied;
		public bool IsVariantBoard { get; private init; } = false;

		public IEnumerable<IChessSquare> AllowedMovesFrom(IChessSquare square)
		{
			if (square is null || square.Piece is NoPiece) yield break;
			if (Me.IsGameBoard && square.Piece.Side != Game.NextPlayer.Side) yield break;
			foreach (ISquare sq in _squares)
			{
				if (sq.Index == square.Index) continue;
				if (square.Piece.CanMoveTo(sq)) yield return sq;
			}
		}

		internal IEnumerable<ISquare> Squares => _squares;

		private ISquare SquareAt(File file, Rank rank)
		{
			int index = IndexOf(file, rank);
			return (index >= 0 && index < _squares.Length) ? _squares[index] : NoSquare.Default;
		}

		void IChessBoard.Display(TextWriter output)
		{
			const string DELIM = "+---+---+---+---+---+---+---+---+";
			foreach (Rank r in AllRanks.Reverse())
			{
				output.WriteLine(DELIM);
				foreach (File f in AllFiles)
				{
					ISquare s = SquareAt(f, r);
					output.Write("| ");
					if (s.HasPiece) output.Write(PieceTypeExtensions.PieceCharacter(s.Piece)); else output.Write(' ');
					output.Write(' ');
				}
				output.WriteLine($"| {1 + (int)r}");
			}
			output.WriteLine(DELIM);
			foreach (File f in AllFiles) output.Write($"  {f.ToString()[0]} ");
			output.WriteLine();

		}

		public string AsFEN()
		{
			StringBuilder s = new StringBuilder();
			for (Rank r = Rank.R8; r >= Rank.R1; --r)
			{
				if (s.Length > 0) s.Append('/');
				int nEmpty = 0;
				for (File f = File.A; f <= File.H; f++)
				{
					IChessSquare sq = SquareAt(f, r);
					if (sq.HasPiece)
					{
						if (nEmpty > 0) s.Append(nEmpty.ToString());
						char c = ' ';
						switch (sq.Piece.Type)
						{
							case PieceType.Knight: c = 'N'; break;
							default: c = sq.Piece.Type.ToString()[0]; break;
						}
						if (sq.Piece.Side == Hue.Dark) c = char.ToLower(c);
						s.Append(c);
						nEmpty = 0;
					}
					else nEmpty++;
				}
				if (nEmpty > 0) s.Append(nEmpty);
			}
			return s.ToString();
		}

		IChessMove IChessBoard.LastMove => LastMove;
		public IMove LastMove { get; private set; } = NoMove.Default;

		ISquare IBoard.ParseSquare(string fileRank)
		{
			var pos = FileRank.Parse(fileRank);
			return pos.IsOnBoard ? SquareAt(pos.File, pos.Rank) : NoSquare.Default;
		}

		async Task<bool> IBoard.ApplyInteractive(IMove move)
		{
			if (!move.IsPromotion || Game is not IPromotingGame g) return Me.Apply(move);
			PieceType toType = await g.RequestPromotion(move.Player.Side, (ISquare)move.ToSquare);
			move.SetPromotion(toType);
			return Me.Apply(move);
		}

		bool IBoard.Apply(IMove move)
		{
			/*
			 *		In an interactive game, attempt move on a copy of the board.
			 *		Verify player's King is not in check.
			 * 
			 *		During parsing skip this check.
			 * 
			 * */
			bool ret = false;
			IPiece capturedPiece = move.IsCapture ? (IPiece)move.CapturedPiece : NoPiece.Default;
			if (move.PromoteTo != PieceType.None && move.MovedPiece is IPawn pawn)
			{
				if (!pawn.CanPromote()) return false;
				ISquare from = pawn.Square;
				switch (move.ToSquare.Piece)
				{
					case NoPiece: break;
					case IPiece capture:
						capturedPiece = capture;
						_removedPieces = _removedPieces.Add(capturedPiece);
						_pieces = _pieces.Remove(capturedPiece);
						break;
				}
				IPiece promo = pawn.Promote(move.PromoteTo, (ISquare)move.ToSquare);
				_promotions = _promotions.Add(promo);
				promo.PromotionIndex = _promotions.Count;
				_pieces = _pieces.Add(promo).Remove(pawn);
				_removedPieces.Add(pawn);
				move.Promotion = new PromotedPawn(pawn, promo, from);
				ret = true;
			}
			else ret = ((IPiece)move.MovedPiece).Move(move);
			if (ret)
			{
				switch (capturedPiece)
				{
					case NoPiece: break;
					default:
						_pieces = _pieces.Remove(capturedPiece);
						_removedPieces = _removedPieces.Add(capturedPiece);
						break;
				}
				LastMove = move;
				MoveMade?.Invoke(move);
			}
			return ret;
		}

		public IChessGame Game => _game;

		IGame IBoard.Game
		{
			get => _game;
			set => _game = value;
		}

		private bool IsCheckingForCheck { get; set; }
		public bool WouldPutMyOwnKingInCheck(IPiece piece, ISquare toSquare)
		{
			if (IsCheckingForCheck) return false;
			// Check to see if, by removing a piece, the piece's King is put in check:
			// Cannot be certain that a king exists - this might be a composed board:
			foreach (IPiece p in _pieces)
			{
				if (p.Type == PieceType.King && p.Side == piece.Side)
				{
					IKing k = (IKing)p;
					using var rep = new PieceReplacer(this, piece, toSquare, PieceType.None);
					return k.IsInCheck();
				}
			}
			return false;
		}

		public KingState OtherKingsExpectedState(IPiece movedPiece, ISquare toSquare, PieceType promotion)
		{
			foreach (IPiece p in _pieces)
			{
				if (p.Type == PieceType.King && p.Side != movedPiece.Side)
				{
					IKing k = (IKing)p;
					using var rep = new PieceReplacer(this, movedPiece, toSquare, promotion);
					return new KingState(k.Side, k.IsInCheck(), k.IsMated);
				}
			}
			return KingState.DefaultFor(movedPiece.Side.Other);
		}

		public Board CreateCopy()
		{
			Board r = new Board(false);
			List<IPiece> pieces = new List<IPiece>();
			foreach (IPiece p in _pieces)
			{
				pieces.Add(p.MakeCopyFor(r));
			}
			r._pieces = ImmutableList.Create(pieces.ToArray());
			return r;
		}

		public IBoardState GetCurrentState() => new BoardState(this);

		public void ApplyState(IBoardState state)
		{
			foreach (ISquare square in this) square.SetPiece(NoPiece.Default);
			_promotions = state.Promotions;
			_removedPieces = state.Removed;
			List<IPiece> pieces = new(state.PieceStates.Count);
			foreach (PieceAndState ps in state.PieceStates)
			{
				ISquare square = (ISquare)this[ps.State.SquareIndex];
				IPiece p = ps.Piece;
				p.ApplyState(ps.State);
				square.SetPiece(p);
				pieces.Add(p);
			}
			_pieces = ImmutableList<IPiece>.Empty.AddRange(pieces);
			LastMove = state.LastMove;
			StateApplied?.Invoke(state);
		}

		public void Reset()
		{
			foreach (ISquare square in _squares) square.SetPiece(NoPiece.Default);
			foreach (IPiece piece in _startPieces.Values) piece.Reset();
			_pieces = ImmutableList<IPiece>.Empty.AddRange(_squares.Select(s => s.Piece).Where(p => p is not NoPiece));
			_removedPieces = ImmutableList<IPiece>.Empty;
		}

		private void ClearBoard()
		{
			foreach (ISquare s in _squares)
			{
				s.Piece.SetSquare(NoSquare.Default);
				s.SetPiece(NoPiece.Default);
			}
			_pieces = ImmutableList<IPiece>.Empty;
		}

		internal PlacedPiece DefaultsFor(File file, Rank rank)
		{
			int ndx = IndexOf(file, rank);
			switch (rank)
			{
				case Rank.R1:
					switch (file)
					{
						case File.A:
						case File.H: return new(ndx, PieceType.Rook, Hue.Light);
						case File.B:
						case File.G: return new(ndx, PieceType.Knight, Hue.Light);
						case File.C:
						case File.F: return new(ndx, PieceType.Bishop, Hue.Light);
						case File.D: return new(ndx, PieceType.Queen, Hue.Light);
						case File.E: return new(ndx, PieceType.King, Hue.Light);
					}
					break;
				case Rank.R2: return new(ndx, PieceType.Pawn, Hue.Light);
				case Rank.R7: return new(ndx, PieceType.Pawn, Hue.Dark);
				case Rank.R8:
					switch (file)
					{
						case File.A:
						case File.H: return new(ndx, PieceType.Rook, Hue.Dark);
						case File.B:
						case File.G: return new(ndx, PieceType.Knight, Hue.Dark);
						case File.C:
						case File.F: return new(ndx, PieceType.Bishop, Hue.Dark);
						case File.D: return new(ndx, PieceType.Queen, Hue.Dark);
						case File.E: return new(ndx, PieceType.King, Hue.Dark);
					}
					break;
			}
			return new(ndx, PieceType.None, Hue.Default);
		}

		private IPiece CreateDefaultPiece(File file, Rank rank)
		{
			var d = DefaultsFor(file, rank);
			FileRank pos = new FileRank(file, rank);
			switch (d.PieceType)
			{
				case PieceType.Pawn: return new Pawn(pos, d.Hue, this);
				case PieceType.Rook: return new Rook(pos, d.Hue, this);
				case PieceType.Knight: return new Knight(pos, d.Hue, this);
				case PieceType.Bishop: return new Bishop(pos, d.Hue, this);
				case PieceType.Queen: return new Queen(pos, d.Hue, this);
				case PieceType.King: return new King(pos, d.Hue, this);
				default: return NoPiece.Default;
			}
		}

		internal void Build(IEnumerable<PlacedPiece> pieces)
		{
			List<IPiece> pcs = new();
			foreach (var p in pieces)
			{
				IPiece piece;
				FileRank loc = PositionOf(p.Index);
				switch (p.PieceType)
				{
					case PieceType.Pawn: piece = new Pawn(loc, p.Hue, this); break;
					case PieceType.Rook: piece = new Rook(loc, p.Hue, this); break;
					case PieceType.Knight: piece = new Knight(loc, p.Hue, this); break;
					case PieceType.Bishop: piece = new Bishop(loc, p.Hue, this); break;
					case PieceType.Queen: piece = new Queen(loc, p.Hue, this); break;
					case PieceType.King: piece = new King(loc, p.Hue, this); break;
					default: continue;
				}
				ISquare s = _squares[p.Index];
				s.SetPiece(piece);
				piece.SetSquare(s);
				pcs.Add(piece);
			}
			_pieces = ImmutableList.Create(pcs.ToArray());
		}

		#region Square access by pieces

		IEnumerable<ISquare> IBoard.RankSquaresBetween(ISquare start, ISquare end)
		{
			if (start.File != end.File) yield break;
			int diff = end.Rank - start.Rank;
			if (diff > 1)
			{
				for (int i = 1; i < diff; i++) yield return (ISquare)this[start.File, start.Rank + i];
			}
			if (diff < -1)
			{
				for (int i = -1; i > diff; i--) yield return (ISquare)this[start.File, start.Rank + i];
			}
		}

		IEnumerable<ISquare> IBoard.FileSquaresBetween(ISquare start, ISquare end)
		{
			if (start.Rank != end.Rank) yield break;
			int diff = end.File - start.File;
			if (diff > 1)
			{
				for (int i = 1; i < diff; ++i) yield return (ISquare)this[start.File + i, start.Rank];
			}
			if (diff < -1)
			{
				for (int i = -1; i > diff; --i) yield return (ISquare)this[start.File + i, start.Rank];
			}
		}

		IEnumerable<ISquare> IBoard.DiagonalSquaresBetween(ISquare start, ISquare end)
		{
			int f0 = (int)start.File, f1 = (int)end.File, r0 = (int)start.Rank, r1 = (int)end.Rank;
			if (Math.Abs(f1 - f0) < 2 || Math.Abs(r1 - r0) < 2) yield break;
			int df = f1 > f0 ? 1 : -1, dr = r1 > r0 ? 1 : -1;
			int r = r0 + dr, f = f0 + df;
			while (r != r1 && f != f1)
			{
				yield return (ISquare)this[(File)f, (Rank)r];
				r += dr;
				f += df;
			}
		}

		IEnumerable<ISquare> IBoard.QueenMovesBetween(ISquare start, ISquare end)
		{
			IBoard b = this;
			return b.RankSquaresBetween(start, end).Concat(b.FileSquaresBetween(start, end)).Concat(b.DiagonalSquaresBetween(start, end)).Distinct();
		}

		IEnumerable<ISquare> IBoard.KingSquares(ISquare fromSquare)
		{
			int minR = Math.Max(0, (int)fromSquare.Rank - 1), maxR = Math.Min(7, (int)fromSquare.Rank + 1),
				minF = Math.Max(0, (int)fromSquare.File - 1), maxF = Math.Min(7, (int)fromSquare.File + 1);
			for (int r = minR; r <= maxR; r++)
			{
				for (int f = minF; f <= maxF; f++)
				{
					ISquare s = (ISquare)this[(File)f, (Rank)r];
					if (ReferenceEquals(s, fromSquare)) continue;
					yield return s;
				}
			}
		}

		IEnumerable<ISquare> IBoard.SquaresBetween(ISquare start, ISquare end)
		{
			IBoard b = this;
			if (start.Rank == end.Rank) return b.FileSquaresBetween(start, end);
			else
				if (start.File == end.File) return b.RankSquaresBetween(start, end);
			else
				return b.DiagonalSquaresBetween(start, end);
		}

		private static readonly (int dF, int dR)[] _knightCombos = { (-2, 1), (-1, 2), (1, 2), (2, 1), (2, -1), (1, -2), (-1, -2), (-2, -1) };
		IEnumerable<ISquare> IBoard.AllowedKnightMovesFrom(ISquare start)
		{
			int iR = (int)start.Rank, iF = (int)start.File;
			foreach (var c in _knightCombos)
			{
				int f = iF + c.dF, r = iR + c.dR;
				if (f >= 0 && f <= 8 && r >= 0 && r <= 8) yield return (ISquare)this[(File)f, (Rank)r];
			}
		}

		IEnumerable<IPiece> IBoard.PiecesTargeting(ISquare toSquare, Hue side)
		{
			bool hasPiece = toSquare.Piece is not NoPiece;
			foreach(Piece p in Me.ActivePieces.Where(p => p.Side == side) )
			{
				bool can = hasPiece ? p.CanCaptureTo(toSquare) : p.CanMoveTo(toSquare);
				if (can) yield return p;
			}
		}

		#endregion

		private IBoard Me => this;
		internal static FileRank PositionOf(int index) => new FileRank((File)(index % 8), (Rank)(index / 8));
		internal static int IndexOf(File file, Rank rank) => ((int)(rank)) * 8 + (int)file;

		IEnumerator<IChessSquare> IEnumerable<IChessSquare>.GetEnumerator()
		{
			return _squares.Cast<IChessSquare>().GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _squares.Cast<IChessSquare>().GetEnumerator();
		}

		private class PieceReplacer : IDisposable
		{
			internal PieceReplacer(Board board, IPiece toMove, ISquare toSquare, PieceType promotion)
			{
				Board = board;
				Pieces = Board._pieces;
				MovedPiece = toMove;
				ToSquare = toSquare;
				FromSquare = MovedPiece.Square;
				RemovedPiece = ToSquare.Piece;
				RemovedPiece.SetSquare(NoSquare.Default);
				if (promotion == PieceType.None || toMove is not IPawn p)
				{
					MovedPiece.SetSquare(ToSquare);
					ToSquare.SetPiece(MovedPiece);
					FromSquare.SetPiece(NoPiece.Default);
				}
				else
				{
					PromotedPawn = p;
					Promotion = p.Promote(promotion, toSquare);
					Board._pieces = Board._pieces.Remove(p).Add(Promotion);
				}
				if (RemovedPiece is not NoPiece) Board._pieces = Board._pieces.Remove(RemovedPiece);
				Board.IsCheckingForCheck = true;
			}

			private Board Board { get; init; }
			private ImmutableList<IPiece> Pieces { get; init; }
			private IPiece MovedPiece { get; init; }
			private IPiece RemovedPiece { get; init; }
			private IPawn PromotedPawn { get; init; } = NoPawn.Default;
			private IPiece Promotion { get; init; } = NoPiece.Default;
			private ISquare ToSquare { get; init; }
			private ISquare FromSquare { get; init; }
			private bool IsDisposed { get; set; }

			void IDisposable.Dispose()
			{
				if (IsDisposed) return;
				Board.IsCheckingForCheck = false;
				IsDisposed = true;
				Promotion.SetSquare(NoSquare.Default);
				MovedPiece.SetSquare(FromSquare);
				FromSquare.SetPiece(MovedPiece);
				ToSquare.SetPiece(RemovedPiece);
				RemovedPiece.SetSquare(ToSquare);
				Board._pieces = Pieces;
			}
		}
	}
}
