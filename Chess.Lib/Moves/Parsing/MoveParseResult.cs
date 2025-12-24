using Chess.Lib.Hardware;
using Chess.Lib.Hardware.Pieces;

namespace Chess.Lib.Moves.Parsing
{
	public interface IMoveParseResult
	{
		IParseableMove Move { get; }
	}

	// TODO: get rid of this - ParseErrorType is enough!
	public interface IMoveParseError : IMoveParseResult
	{
		ParseErrorType Error { get; }
	}

	public interface IMoveParseSuccess : IMoveParseResult, IChessMoveCore
	{
		PieceType Promotion { get; }
		bool IsMate { get; }
		bool IsCheck { get; }

		CastleMoveType Castle { get; }
		string AsMove { get; }
	}

	public interface IParseGameEnd : IMoveParseResult
	{
		GameResult Result { get; }
	}

	internal record struct MoveParseResult(IParseableMove Move) : IMoveParseResult;

	internal record struct ParseError(IParseableMove Move, ParseErrorType Error) : IMoveParseError
	{
		internal static readonly ParseError NoError = new ParseError(new AlgebraicMove(string.Empty, -1, -1, MoveFormat.Unknown), ParseErrorType.NoError);

		internal ParseError(string move, ParseErrorType error) : this(new AlgebraicMove(move, -1, -1), error) { }
		public override string ToString() => $"{Move.SerialNumber}:{Move.Move}: {Error}";
	}

	internal record struct ParseSuccess(IParseableMove Move, IChessPiece MovedPiece, IChessSquare FromSquare, IChessSquare ToSquare,
			IChessPiece CapturedPiece, IChessMove PreviousMove, CastleMoveType Castle, PieceType Promotion, bool IsCheck, bool IsMate)
			: IMoveParseSuccess, IChessMoveCore
	{
		internal ParseSuccess(string sMove, IPiece movedPiece, ISquare toSquare) : this(new AlgebraicMove(sMove, -1, -1), movedPiece, movedPiece.Square,
				toSquare, toSquare.Piece, NoMove.Default, CastleMoveType.None, PieceType.None, false, false) { }
		public string AsMove => $"{FromSquare}->{ToSquare}";
		public override string ToString() => $"{MovedPiece}: {FromSquare.Name} {ToSquare.Name}";
	}

	internal record struct GameEnd(IParseableMove Move, GameResult Result) : IParseGameEnd;

	/// <summary>
	/// Enumerates the various parsing errors I've encountered
	/// </summary>
	public enum ParseErrorType
	{
		NoError,
		InvalidInput,										// Wrong formatting, incorrect length, etc.
		UnrecognizedAlgebraicNotation,
		UnmatchedMovePattern,
		TargetSquareUndefined,
		MovedPieceUndefined,
		MoreThanOnePossibleMovedPiece,
		CannotParseFiveCharacterMove,
		TargetSquareUnreachable,
		CapturingPieceUndefined,
		CannotParseCapturerSquare,
		IncorrectPieceOnSquare,
		UnableToParseCapture,
		UnableToFindMovablePiece,
		UnableToParseSourceSquare,
		UnableToParseTargetSquare,
		BoardMismatch,
		MissingOriginSquare,
		IllegalMove,
		InvalidCastle,
		NoInput,
		UnknownFormat
	}
}