using Chess.Lib.Games;
using Chess.Lib.Hardware;
using Chess.Lib.Hardware.Pieces;
using Chess.Lib.Moves.Parsing;
using System.Collections.Immutable;

namespace Chess.Lib.Moves
{
	public enum CastleMoveType { None, Kingside, Queenside };

	#region Move Interfaces

	public interface IChessMoveCore
	{
		IChessPiece MovedPiece { get; }
		IChessSquare FromSquare { get; }
		IChessSquare ToSquare { get; }
		IChessPiece CapturedPiece { get; }
		bool IsCapture => CapturedPiece is Piece;
		IChessMove PreviousMove { get; }
		Hue Side => MovedPiece.Side;
	}

	public interface IChessMove : IChessMoveCore
	{
		int SerialNumber { get; }
		IMoveCounter Number { get; }
		bool IsCheck { get; }
		IPromotion Promotion { get; }
		bool IsMate { get; }
		TimeSpan ClockTime { get; }
		bool IsEnPassant { get; }
		CastleMoveType Castling { get; }
		bool IsKingsideCastle => Castling == CastleMoveType.Kingside;
		bool IsQueensideCastle => Castling == CastleMoveType.Queenside;
		bool IsCastle => IsKingsideCastle || IsQueensideCastle;
		int RankChange => ToSquare.Rank - FromSquare.Rank;
		int FileChange => ToSquare.File - FromSquare.File;
		string AsEngineMove
		{
			get
			{
				string p = Promotion.IsValid ? Promotion.ToPiece.Type.PGNChar().ToString() : string.Empty;
				return $"{FromSquare.Name}{ToSquare.Name}{p}";
			}
		}
		IChessKing CheckedKing { get; }
		IParseableMove SourceMove { get; }
		string AlgebraicMove { get; }
	}

	public interface INoMove : IChessMove;

	internal interface IMove : IChessMove
	{
		new IPromotion Promotion { get; set; }
		PieceType PromoteTo { get; }
		IBoard Board => (IBoard)MovedPiece.Board;
		IGameState GameState { get; set; }
		IBoardState BoardState => GameState.BoardState;
	}

	#endregion

	#region NoMove

	internal record struct NoMove(IChessPiece MovedPiece, IChessSquare FromSquare, IChessSquare ToSquare, IChessPiece CapturedPiece, IBoardState BoardState): IMove, INoMove
	{
		internal static readonly NoMove Default = new NoMove();
		public NoMove() : this(NoPiece.Default, NoSquare.Default, NoSquare.Default, NoPiece.Default, Hardware.BoardState.Empty) { }

		bool IChessMove.IsCheck => false;
		IPromotion IChessMove.Promotion => Promotion.None;
		bool IChessMove.IsMate => false;
		bool IChessMoveCore.IsCapture => false;
		int IChessMove.SerialNumber => -1;
		TimeSpan IChessMove.ClockTime => TimeSpan.Zero;
		bool IChessMove.IsEnPassant => false;
		IChessMove IChessMoveCore.PreviousMove => Default;
		IPromotion IMove.Promotion { get; set; } = Promotion.None;
		PieceType IMove.PromoteTo => PieceType.None;
		IMoveCounter IChessMove.Number => MoveCounter.Default;
		IGameState IMove.GameState
		{
			get => GameState.Empty;
			set { }
		}
		IChessKing IChessMove.CheckedKing => NoKing.Default;
		IParseableMove IChessMove.SourceMove => NotParseable.Default;
		CastleMoveType IChessMove.Castling => CastleMoveType.None;
		string IChessMove.AlgebraicMove => string.Empty;
	}

	#endregion

	public record struct MoveRepetition(IChessMove Move, int RepetitionCount, IEnumerable<IChessMove> PriorMoves);

	internal sealed record ChessMove(int SerialNumber, IChessPiece MovedPiece, IChessSquare FromSquare, IChessSquare ToSquare, 
		IChessPiece CapturedPiece, IChessMove PreviousMove, PieceType PromoteTo = PieceType.None): IMove
	{
		internal static ImmutableList<IChessMove> EmptyMoves = ImmutableList<IChessMove>.Empty;

		internal ChessMove(IParseSuccess move): 
			this(move.Move.SerialNumber, move.MovedPiece, move.FromSquare, move.ToSquare, move.CapturedPiece, move.PreviousMove, move.Promotion)
		{
			SerialNumber = move.Move.SerialNumber;
			IPawn ep = NoPawn.Default;
			IsEnPassant = MovedPiece is Pawn p && p.IsEnPassant(move, out ep);
			if (IsEnPassant && ep is not NoPawn) CapturedPiece = ep; else CapturedPiece = move.CapturedPiece;
			IsCapture = IsEnPassant || move.IsCapture;
			IsCheck = move.IsCheck;
			if (IsCheck)
			{
				IBoard b = (IBoard)FromSquare.Board;
				CheckedKing = (IChessKing)b.ActivePieces.First(p => p.Type == PieceType.King && p.Side != MovedPiece.Side);
			}
			IsMate = move.IsMate;
			Castling = move.IsKingsideCastle ? CastleMoveType.Kingside : move.IsQueensideCastle ? CastleMoveType.Queenside : CastleMoveType.None;
			PreviousMove = move.PreviousMove;
			SourceMove = move.Move;
			AlgebraicMove = SourceMove is AlgebraicMove m ? m.Move : AlgebraicFromEngineMove();
		}

		public IMoveCounter Number => new MoveCounter(SerialNumber, MovedPiece.Side);
		public bool IsCheck { get; private init; }
		// Promotion is set in Board.Apply, once the new piece is created.
		public IPromotion Promotion { get; set; } = Chess.Lib.Moves.Promotion.None;
		public bool IsCapture { get; private init; }
		public bool IsMate { get; private init; }
		public bool IsEnPassant { get; private init; }
		public TimeSpan ClockTime { get; private init; } = TimeSpan.Zero;
		public CastleMoveType Castling { get; private init; }
		public IChessKing CheckedKing { get; private init; } = NoKing.Default;
		public IGameState GameState { get; set; } = Games.GameState.Empty;
		public IParseableMove SourceMove { get; private init; } = NotParseable.Default;
		public string AlgebraicMove { get; private init; } = string.Empty;
		public override string ToString() => M.AsEngineMove;

		internal IMove M => this;

		private string AlgebraicFromEngineMove()
		{
			// Move has not yet been applied to board.
			IBoard board = (IBoard)MovedPiece.Board;
			bool isPawn = MovedPiece is IPawn;
			string cMoved = isPawn ? IsCapture ? FromSquare.File.ToString().ToLower() : string.Empty : MovedPiece.Type.PGNChar().ToString();
			string sChecked = IsCheck ? "+" : string.Empty, sPromo = PromoteTo == PieceType.None ? string.Empty : $"={PromoteTo.PGNChar()}",
				sCapture = IsCapture ? "x" : string.Empty;
			if (Castling == CastleMoveType.Kingside) return $"O-O{sChecked}";
			if (Castling == CastleMoveType.Queenside) return $"O-O-O{sChecked}";
			List<IPiece> moveCandidates = board.ActivePieces.Where(p => !ReferenceEquals(p, MovedPiece) && p.Side == MovedPiece.Side && p.CanMoveTo(ToSquare)).ToList();
			if (moveCandidates.Count == 0 || (isPawn && !moveCandidates.Any(p => p.Type == PieceType.Pawn))) return $"{cMoved}{sCapture}{ToSquare.Name}{sPromo}{sChecked}";
			string which = string.Empty;
			if (moveCandidates.Any(p => p.Type == MovedPiece.Type)) which = $"{MovedPiece.Square.File.FileChar()}";
			return $"{cMoved}{which}{sCapture}{ToSquare.Name}{sPromo}{sChecked}";
		}
	}
}
