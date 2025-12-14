using Chess.Lib.Hardware;
using Chess.Lib.Hardware.Pieces;
using System.Text.RegularExpressions;
using File = Chess.Lib.Hardware.File;
using System.Text;
using System.Diagnostics;

namespace Chess.Lib.Moves.Parsing
{
	[DebuggerDisplay("{SerialNumber}:{Move}")]
	public record struct AlgebraicMove(string Move, int SourceIndex, int SerialNumber, MoveFormat Format = MoveFormat.Algebraic) : IAlgebraicParseable, IParseableMoveEx
	{
		private static readonly string[] DrawMoves = { "* *", "(+)-(-) (+)-(-)", "1/2-1/2", "0-0 0-0", "+/- +/-", "1/2 1/2", "-/+ -/+", "*" };
		public static bool IsDrawMove(string move) => DrawMoves.Contains(move);
		private static readonly Regex _rxAlg = new Regex(@"[NRBQKP][abcdefgh]\d");
		private static readonly Regex _rxWhichPieceByFile = new Regex(@"[RNBQ][abcdefgh][abcdefgh]\d");
		private static readonly Regex _rxWhichPieceByRank = new Regex(@"[RNBQ]\d[abcdefgh]\d");
		public const string KSCastle = "O-O", QSCastle = "O-O-O";
		public const string WhiteWinResult = "1-0", BlackWinResult = "0-1", DrawResult = "1/2-1/2", IncompleteResult = "*", Resignation = "--";
		internal static bool IsWinResult(string move) => string.Equals(move, WhiteWinResult) || string.Equals(move, BlackWinResult);
		public static bool IsEndGameMove(string move) => IsDrawMove(move) || IsWinResult(move) || move == IncompleteResult || move == Resignation;

		public static readonly AlgebraicMove Empty = new(string.Empty, -1, -1);
		public bool IsEmpty => string.IsNullOrEmpty(Move);
		public string NumberedMove => $"{MoveCounter.SerialToGameNumber(SerialNumber)}. {Move}";

		public bool IsDrawResult => DrawMoves.Contains(Move);
		public bool IsKingsideCastle => Move.Contains(KSCastle) && !IsQueensideCastle;
		public bool IsQueensideCastle => Move.Contains(QSCastle);
		public bool IsCastle => IsKingsideCastle || IsQueensideCastle;
		public bool IsMate => Move.EndsWith("#");
		public bool IsCheck => Move.EndsWith("+");
		public bool IsPromotion => Move.Contains("=");
		public bool IsCapture => Move.Contains("x");
		public bool IsEndGame => Move == WhiteWinResult || Move == BlackWinResult || IsDrawResult || Move == Resignation || Move == IncompleteResult;
		public bool IsSimpleMove => !IsDrawResult && !IsKingsideCastle && !IsQueensideCastle && !IsPromotion && !IsCapture && !IsEndGame;
		public PieceType Promotion
		{
			get
			{
				if (!IsPromotion) return PieceType.None;
				int n = Move.IndexOf("=");
				return PieceTypeExtensions.Promotion(Move[n + 1]);
			}
		}

		internal IMoveParseResult Parse(IBoard B)
		{
			PieceType promotion = PieceType.None;
			IPiece movedPiece = NoPiece.Default;
			ISquare toSquare = NoSquare.Default;
			string move = Move;
			List<IPiece> movable = new();
			var hue = SerialNumber % 2 == 0 ? Hue.Light : Hue.Dark;
			void addMovable(Predicate<IPiece> canMoveTo)
			{
				movable.Clear();
				foreach (IPiece p in B.ActivePieces)
				{
					if (p.Side != hue) continue;
					if (canMoveTo(p)) movable.Add(p);
				}
			}
			if (IsEndGame)
			{
				GameResult result = GameResult.Unknown;
				switch (move)
				{
					case BlackWinResult: result = GameResult.BlackWin; break;
					case WhiteWinResult: result = GameResult.WhiteWin; break;
					case DrawResult: result = GameResult.Draw; break;
					case Resignation:
						switch (hue)
						{
							case Hue.Light: result = GameResult.BlackWin; break;
							default: result = GameResult.WhiteWin; break;
						}
						break;
					default:
						if (IsDrawResult) result = GameResult.Draw; else result = GameResult.Unknown;
						break;
				}
				return new GameEnd(this, result);
			}
			if (IsMate)
			{
				move = move.Substring(0, move.Length - 1);
			}
			if (IsCheck)
			{
				move = move.Substring(0, move.Length - 1);
			}
			if (IsPromotion)
			{
				int n = move.IndexOf('=');
				promotion = PieceTypeExtensions.Promotion(move[n + 1], false);
				move = move.Substring(0, n);
			}
			if (IsCapture)
			{
				int n = move.IndexOf('x');
				string taker = move.Substring(0, n), sTo = move.Substring(n + 1);
				toSquare = B.ParseSquare(sTo);
				if (toSquare is null || toSquare is NoSquare) return new ParseError(this, ParseErrorType.UnableToParseTargetSquare);
				ParseErrorType? parse1()
				{
					char t = taker[0];
					if (char.IsUpper(t))  // taker represents a piece
					{
						addMovable(p => p.Type.PGNChar() == t && p.CanCaptureTo(toSquare));
						switch (movable.Count)
						{
							case 1: movedPiece = movable[0]; break;
							case 0: return ParseErrorType.UnableToFindMovablePiece;
							default: return ParseErrorType.MoreThanOnePossibleMovedPiece;
						}
					}
					else // taker represents a file / pawn:
					{
						foreach (IPiece p in B.ActivePieces.Where(p => p.Type == PieceType.Pawn && p.Side == hue))
						{
							if (p.Square.File.FileChar() == t && p.CanCaptureTo(toSquare))
							{
								movedPiece = p;
								break;
							}
						}
					}
					return null;
				}
				bool parse2()
				{
					char t = taker[0], rf = taker[1];
					foreach (IPiece p in B.ActivePieces.Where(p => p.Side == hue))
					{
						if (p.Type.PGNChar() == t && (p.Square.File.FileChar() == rf || p.Square.Rank.RankChar() == rf) && p.CanCaptureTo(toSquare))
						{
							movedPiece = p;
							break;
						}
					}
					return movedPiece is not NoPiece;
				}
				ParseErrorType? parse3() // Qe2
				{
					PieceType? taken = PieceTypeExtensions.Promotion(taker[0]);

					if (!taken.HasValue) return ParseErrorType.UnrecognizedAlgebraicNotation;
					var onSquare = B.ParseSquare(taker.Substring(1));
					if (onSquare == null) return ParseErrorType.CannotParseCapturerSquare;
					if (!onSquare.HasPiece || onSquare.Piece.Type != taken.Value) return ParseErrorType.IncorrectPieceOnSquare;
					movedPiece = onSquare.Piece;
					return null;
				}
				switch (taker.Length)
				{
					case 1: parse1(); break;
					case 2: if (!parse2()) return new ParseError(this, ParseErrorType.CapturingPieceUndefined); break;
					case 3: var res = parse3(); if (res.HasValue) return new ParseError(this, res.Value); break;
					default: return new ParseError(this, ParseErrorType.UnableToParseCapture);
				}
				goto RET;
			}
			if (IsCastle)
			{
				bool kingSide = move == KSCastle;
				movedPiece = B.ActivePieces.Where(p => p.Type == PieceType.King && p.Side == hue).First();
				Rank toRank = movedPiece.Square.Rank;
				File toFile = kingSide ? File.G : File.C;
				toSquare = B[toFile, toRank];
				IKing k = (IKing)movedPiece;
				if (!k.IsCastle(toSquare, kingSide)) return new ParseError(this, ParseErrorType.InvalidCastle);
				goto RET;
			}
			toSquare = B.ParseSquare(move);
			if (toSquare is NoSquare)
			{
				PieceType type;
				Match m = _rxAlg.Match(move);
				if (m.Success)
				{
					PieceType? pt = PieceTypeExtensions.Promotion(m.Value[0]);
					if (!pt.HasValue) return new ParseError(this, ParseErrorType.UnrecognizedAlgebraicNotation);
					type = pt.Value;
					ISquare? from = null;
					switch (move.Length)
					{
						case 3:
						case 4: toSquare = B.ParseSquare(m.Value.Substring(1)); break;
						case 5:
							from = B.ParseSquare(m.Value.Substring(1, 2));
							if (from == null) return new ParseError(this, ParseErrorType.CannotParseFiveCharacterMove);
							movedPiece = from.Piece;
							toSquare = B.ParseSquare(move.Substring(3, 2));
							break;
					}
					if (toSquare is NoSquare) return new ParseError(this, ParseErrorType.TargetSquareUndefined);
					if (movedPiece is NoPiece)
					{
						addMovable(p =>
						{
							if (p.Type != type) return false;
							return p.CanMoveTo(toSquare);
						});
						switch (movable.Count)
						{
							case 0: movedPiece = NoPiece.Default; break;
							case 1: movedPiece = movable[0]; break;
							default: return new ParseError(this, ParseErrorType.MoreThanOnePossibleMovedPiece);
						}
					}
				}
				else
				{
					m = _rxWhichPieceByFile.Match(move);  // Nbd7
					if (m.Success)
					{
						PieceType? pt = PieceTypeExtensions.Promotion(m.Value[0]);
						if (!pt.HasValue) return new ParseError(this, ParseErrorType.UnrecognizedAlgebraicNotation);
						type = pt.Value;
						toSquare = B.ParseSquare(move.Substring(2));
						if (toSquare == null) return new ParseError(this, ParseErrorType.TargetSquareUndefined);
						addMovable(p => p.Type == type && p.Square.File.FileChar() == move[1] && p.CanMoveTo(toSquare));
						switch (movable.Count)
						{
							case 0: movedPiece = NoPiece.Default; break;
							case 1: movedPiece = movable[0]; break;
							default: return new ParseError(this, ParseErrorType.MoreThanOnePossibleMovedPiece);
						}
					}
					else
					{
						m = _rxWhichPieceByRank.Match(move);
						if (m.Success)
						{
							PieceType? pt = PieceTypeExtensions.Promotion(m.Value[0]);
							if (!pt.HasValue) return new ParseError(this, ParseErrorType.UnrecognizedAlgebraicNotation);
							type = pt.Value;
							toSquare = B.ParseSquare(move.Substring(2));
							if (toSquare == null) return new ParseError(this, ParseErrorType.TargetSquareUndefined);
							addMovable(p => p.Type == type && p.Square.Rank.RankChar() == move[1] && p.CanMoveTo(toSquare));
							switch(movable.Count)
							{
								case 1: movedPiece = movable[0]; break;
								case 0: return new ParseError(this, ParseErrorType.MovedPieceUndefined);
								default: return new ParseError(this, ParseErrorType.MoreThanOnePossibleMovedPiece);
							}
						}
						else return new ParseError(this, ParseErrorType.UnmatchedMovePattern);
					}
				}
			}
			else
			{
				addMovable(p =>
				{
					if (!p.CanMoveTo(toSquare)) return false;
					if (move.Length == 2 && p.Type != PieceType.Pawn) return false;
					return true;
				});
				switch (movable.Count)
				{
					case 0: return new ParseError(this, ParseErrorType.TargetSquareUnreachable);
					case 1: movedPiece = movable[0]; break;
					default: return new ParseError(this, ParseErrorType.MoreThanOnePossibleMovedPiece);
				}
			}
			RET:
			if (movedPiece is NoPiece) return new ParseError(this, ParseErrorType.MovedPieceUndefined);
			if (toSquare is NoSquare) return new ParseError(this, ParseErrorType.TargetSquareUndefined);
			IChessSquare fromSquare = movedPiece.Square;
			if (fromSquare is NoSquare) return new ParseError(this, ParseErrorType.MissingOriginSquare);
			IPiece captured = toSquare.Piece;
			if (IsCastle)
			{
				if (captured is IKing && ReferenceEquals(movedPiece, captured)) captured = NoPiece.Default;
				if (captured is IRook rook && rook.Side == movedPiece.Side) captured = NoPiece.Default;
			}
			if (IsCastle && captured is IKing && ReferenceEquals(movedPiece, captured)) captured = NoPiece.Default;
			if (IsCapture && captured is NoPiece)
			{
				if (movedPiece is not IPawn p) return new ParseError(this, ParseErrorType.UnableToParseCapture);
				if (p.IsEnPassant(toSquare, out IPawn ep)) captured = ep;
			}
			return new ParseSuccess(this, movedPiece, fromSquare, toSquare, captured, B.LastMove, IsKingsideCastle, IsQueensideCastle, promotion,
				IsCheck, IsMate);
		}

		IMoveParseResult IParseableMoveEx.Parse(IBoard board) => Parse(board);

		public static string ToAlgebraicMoves(IEnumerable<IParseableMove> moves)
		{
			StringBuilder s = new();
			int nMove = 1;
			foreach (var two in moves.Chunk(2))
			{
				s.Append($"{nMove++}. ");
				switch (two.Count())
				{
					case 1: s.Append(two[0].Move); break;
					case 2: s.Append($"{two[0].Move} {two[1].Move} "); break;
				}
			}
			return s.ToString().TrimEnd();
		}

		public static string ToAlgebraicMoves(IEnumerable<AlgebraicMove> moves) => ToAlgebraicMoves(moves.Cast<IParseableMove>());
	}
}