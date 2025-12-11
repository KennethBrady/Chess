using Chess.Lib.Games;
using Chess.Lib.Hardware;
using Chess.Lib.Hardware.Pieces;
using System.Collections.Immutable;

namespace Chess.Lib.Moves.Parsing
{
	#region ParseResult

	public interface IParseResult
	{
		IParseableMove Move { get; }
	}

	// TODO: get rid of this - ParseErrorType is enough!
	public interface IParseError : IParseResult
	{
		ParseErrorType Error { get; }
	}

	public interface IParseSuccess : IParseResult, IChessMoveCore
	{
		new IChessPiece MovedPiece { get; }
		new IChessSquare FromSquare { get; }
		new IChessSquare ToSquare { get; }
		PieceType Promotion { get; }
		bool IsMate { get; }
		bool IsCheck { get; }
		bool IsKingsideCastle { get; }
		bool IsQueensideCastle { get; }
		string AsMove { get; }
	}

	public interface IParseGameEnd : IParseResult
	{
		GameResult Result { get; }
	}

	internal record struct ParseResult(IParseableMove Move) : IParseResult;

	internal record struct ParseError(IParseableMove Move, ParseErrorType Error) : IParseError
	{
		internal static readonly ParseError NoError = new ParseError(new AlgebraicMove(string.Empty, -1, -1, MoveFormat.Unknown), ParseErrorType.NoError);

		internal ParseError(string move, ParseErrorType error) : this(new AlgebraicMove(move, -1, -1), error) { }
		public override string ToString() => $"{Move.SerialNumber}:{Move.Move}: {Error}";
	}

	internal record struct ParseSuccess(IParseableMove Move, IChessPiece MovedPiece, IChessSquare FromSquare, IChessSquare ToSquare,
			IChessPiece CapturedPiece, IChessMove PreviousMove, bool IsKingsideCastle, bool IsQueensideCastle, PieceType Promotion, bool IsCheck, bool IsMate)
			: IParseSuccess, IChessMoveCore
	{
		internal ParseSuccess(IParseableMove move, IChessPiece movedPiece, IChessSquare fromSquare, IChessSquare toSquare,
			IChessPiece capturedPiece, IChessMove previousMove, CastleMoveType castleType, PieceType promotion, bool isCheck, bool isMate) :
			this(move, movedPiece, fromSquare, toSquare, capturedPiece, previousMove, castleType == CastleMoveType.Kingside, castleType == CastleMoveType.Queenside,
				promotion, isCheck, isMate)
		{ }

		internal ParseSuccess(string sMove, IPiece movedPiece, ISquare toSquare) : this(new AlgebraicMove(sMove, -1, -1), movedPiece, movedPiece.Square,
				toSquare, toSquare.Piece, NoMove.Default, false, false, PieceType.None, false, false)
		{
			if (movedPiece is IKing king)
			{
				switch (king.CastleTypeOf(toSquare))
				{ 
					case CastleMoveType.Kingside: IsKingsideCastle = true; break;
					case CastleMoveType.Queenside: IsQueensideCastle = true; break;
				}
			}
		}
		public string AsMove => $"{FromSquare}->{ToSquare}";
		public override string ToString() => $"{MovedPiece}: {FromSquare.Name} {ToSquare.Name}";
	}

	internal record struct GameEnd(IParseableMove Move, GameResult Result) : IParseGameEnd;

	public enum ParseErrorType
	{
		NoError,
		InvalidInput,
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

	#endregion

	#region ParsedGame

	public interface IParsedGame
	{
		IKnownChessGame Game { get; }
		IChessBoard FinalBoard => Game.Board;
		ImmutableList<IChessMove> Moves { get; }
	}

	public interface IParsedGameSuccess : IParsedGame
	{
		IParseGameEnd GameEnd { get; }
		GameResult Result { get; }
	}

	public interface IParsedGameFail : IParsedGame
	{
		IParseError Error { get; }
		ImmutableList<IParseableMove> UnparsedMoves { get; }
	}

	internal record struct ParsedGameSuccess(ImmutableList<IChessMove> Moves, IParseGameEnd GameEnd,
			GameResult Result, IKnownChessGame Game) : IParsedGameSuccess;

	internal record struct ParsedGameIncomplete(IKnownChessGame Game, ImmutableList<IChessMove> Moves) : IParsedGame;

	internal record struct ParseGameFail(ImmutableList<IChessMove> Moves, IParseError Error, IKnownChessGame Game,
		ImmutableList<IParseableMove> UnparsedMoves) : IParsedGameFail
	{
		internal ParseGameFail(ImmutableList<IChessMove> moves, IParseError error, IKnownChessGame game) :
			this(moves, error, game, ImmutableList<IParseableMove>.Empty)
		{ }
	}

	#endregion
}