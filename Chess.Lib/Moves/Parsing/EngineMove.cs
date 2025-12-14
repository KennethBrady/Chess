using Chess.Lib.Hardware;
using Chess.Lib.Hardware.Pieces;

namespace Chess.Lib.Moves.Parsing
{
	public record EngineMove(string Move, int SourceIndex, int SerialNumber, MoveFormat Format = MoveFormat.Engine) : IParseableMove, IParseableMoveEx
	{
		internal static readonly EngineMove Empty = new EngineMove(string.Empty, -1, -1);

		internal EngineMove(ReadOnlySpan<char> move, int sourceIndex, int serialNumber): this(move.ToString(), sourceIndex, serialNumber) { }

		private bool IsLengthValid => Move.Length == 4 || Move.Length == 5;

		internal IMoveParseResult Parse(IBoard b)
		{
			if (!IsLengthValid) return new ParseError(this, ParseErrorType.InvalidInput);
			ISquare s1 = (ISquare)b[From], s2 = (ISquare)b[To];
			if (s1 is NoSquare) return new ParseError(this, ParseErrorType.UnableToParseSourceSquare);
			if (s2 is NoSquare) return new ParseError(this, ParseErrorType.UnableToParseTargetSquare);
			if (!s1.HasPiece) return new ParseError(this, ParseErrorType.MovedPieceUndefined);
			IPiece p = s1.Piece;
			if (!p.CanMoveTo(s2)) return new ParseError(this, ParseErrorType.IllegalMove);
			CastleMoveType castle = p is IKing king ? king.CastleTypeOf(s2) : CastleMoveType.None;
			var otherState = b.OtherKingsExpectedState(p, s2, Promotion);
			return new ParseSuccess(this, p, s1, s2, s2.Piece, b.LastMove, castle, Promotion, otherState.IsChecked, otherState.IsMated);
		}

		IMoveParseResult IParseableMoveEx.Parse(IBoard board) => Parse(board);

		public bool IsPromotion => Move.Length == 5;

		public PieceType Promotion
		{
			get
			{
				if (!IsPromotion) return PieceType.None;
				return PieceTypeExtensions.Promotion(Char.ToUpper(Move[4]), false);
			}
		}

		internal FileRank From => IsLengthValid ? FileRank.Parse(Move.Substring(0, 2)) : FileRank.OffBoard;
		internal FileRank To => IsLengthValid ? FileRank.Parse(Move.Substring(2, 2)) : FileRank.OffBoard;

		internal MoveRequest ToMoveRequest()
		{
			if (!IsLengthValid) return MoveRequest.Invalid;
			FileRank from = From, to = To;
			if (from.IsOffBoard || to.IsOffBoard) return MoveRequest.Invalid;
			return new MoveRequest(from, to, Promotion) { SerialNumber = SerialNumber };
		}
	}
}
