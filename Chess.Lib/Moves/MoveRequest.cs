using Chess.Lib.Games;
using Chess.Lib.Hardware;
using Chess.Lib.Hardware.Pieces;
using Chess.Lib.Moves.Parsing;
using System.Diagnostics;
using File = Chess.Lib.Hardware.File;

namespace Chess.Lib.Moves
{
	[DebuggerDisplay("{From} {To}")]
	public record struct MoveRequest(FileRank From, FileRank To, PieceType Promotion = PieceType.None): IParseableMove
	{
		public static MoveRequest Invalid = new MoveRequest(FileRank.OffBoard, FileRank.OffBoard);

		public string AsEngineMove => $"{From.ToEngineMove}{To.ToEngineMove}";
		string IParseableMove.Move => AsEngineMove;
		int IParseableMove.SourceIndex => 0;
		public int SerialNumber { get; internal set; } = 0;
		MoveFormat IParseableMove.Format => MoveFormat.EngineCompact;

		private MoveRequest((FileRank from, FileRank to, PieceType promotion) request): this(request.from, request.to, request.promotion) { }

		public MoveRequest(File fromFile, Rank fromRank, File toFile, Rank toRank): this(new FileRank(fromFile, fromRank), new FileRank(toFile, toRank)) { }

		public MoveRequest(string engineMove): this(new EngineMove(engineMove, 0, 0)) { }

		public MoveRequest(IChessPiece pieceToMove, IChessSquare toSquare):
			this(pieceToMove.Square.File, pieceToMove.Square.Rank, toSquare.File, toSquare.Rank) { }

		public MoveRequest(IChessSquare from, IChessSquare to) : this(from.File, from.Rank, to.File, to.Rank) { }

		private MoveRequest(EngineMove em): this(em.From, em.To, em.Promotion) 
		{
			SerialNumber = em.SerialNumber;
		}

		public bool IsPromotion => Promotion != PieceType.None;		

		public static IEnumerable<MoveRequest> ParseMoves(string engineMoves)
		{
			engineMoves = engineMoves.Replace(" ", string.Empty);
			return EngineMoves.ParseMoves(engineMoves).Select(em =>  new MoveRequest(em));			
		}

		internal IMoveParseResult Parse(IPlayer forPlayer)
		{
			ISquare sFrom = (ISquare)forPlayer.Board[From], sTo = (ISquare)forPlayer.Board[To];
			if (sFrom is NoSquare) return new ParseError(this, ParseErrorType.UnableToParseSourceSquare);
			if (sTo is NoSquare) return new ParseError(this, ParseErrorType.UnableToParseTargetSquare);
			if (!sFrom.HasPiece) return new ParseError(this, ParseErrorType.MovedPieceUndefined);
			if (sFrom.Piece.Side != forPlayer.Side) return new ParseError(this, ParseErrorType.IncorrectPieceOnSquare);
			IPiece p = sFrom.Piece;
			if (!p.CanMoveTo(sTo)) return new ParseError(this, ParseErrorType.IllegalMove);
			CastleMoveType castleType = p is IKing king ? king.CastleTypeOf(sTo) : CastleMoveType.None;
			KingState otherState = forPlayer.Board.OtherKingsExpectedState(p, sTo, Promotion);
			SerialNumber = forPlayer.Board.LastMove.SerialNumber + 1;
			return new ParseSuccess(this, p, sFrom, sTo, sTo.Piece, forPlayer.Board.LastMove, castleType, Promotion, otherState.IsChecked, otherState.IsMated);
		}
	}
}
