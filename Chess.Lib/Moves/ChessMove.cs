using Chess.Lib.Games;
using Chess.Lib.Hardware;
using Chess.Lib.Hardware.Pieces;
using Chess.Lib.Moves.Parsing;
using System.Collections.Immutable;

namespace Chess.Lib.Moves
{
	public enum CastleMoveType { None, Kingside, Queenside };

	public record struct MoveRepetition(IChessMove Move, int RepetitionCount, IEnumerable<IChessMove> PriorMoves);

	/// <summary>
	/// Implementation of IMove
	/// </summary>
	internal sealed record ChessMove(int SerialNumber, IChessPiece MovedPiece, IChessSquare FromSquare, IChessSquare ToSquare,
		IChessPiece CapturedPiece, IChessMove PreviousMove) : IMove
	{
		internal static ImmutableList<IChessMove> EmptyMoves = ImmutableList<IChessMove>.Empty;

		internal ChessMove(IMoveParseSuccess move) :
			this(move.Move.SerialNumber, move.MovedPiece, move.FromSquare, move.ToSquare, move.CapturedPiece, move.PreviousMove)
		{
			SerialNumber = move.Move.SerialNumber;
			IPawn ep = NoPawn.Default;
			IsEnPassant = MovedPiece is Pawn p && p.IsEnPassant(move, out ep);
			if (IsEnPassant && ep is not NoPawn) CapturedPiece = ep; else CapturedPiece = move.CapturedPiece;
			IsCapture = IsEnPassant || move.IsCapture;
			IsCheck = move.IsCheck;
			IsCheckMate = move.IsMate;
			if (IsCheck || IsCheckMate)
			{
				IBoard b = (IBoard)FromSquare.Board;
				CheckedKing = (IChessKing)b.ActivePieces.First(p => p.Type == PieceType.King && p.Side == move.Side.Other);
			}
			PreviousMove = move.PreviousMove;
			SourceMove = move.Move;
			Castle = Moves.Castle.Empty with { Type = move.Castle };
			AlgebraicMove = SourceMove is AlgebraicMove m ? m.Move : AlgebraicFromEngineMove(move.Castle);
			Player = MovedPiece.Board.Game.PlayerOf(MovedPiece.Side);
			PromoteTo = move.Promotion;
		}

		public IChessPlayer Player { get; private init; } = NoPlayer.Default;
		public IMoveCounter Number => new MoveCounter(SerialNumber, MovedPiece.Side);
		public bool IsCheck { get; private set; }
		// Promotion is set in Board.Apply, once the new piece is created.
		public IPromotion Promotion { get; set; } = PromotedPawn.None;

		public PieceType PromoteTo { get; private set; } = PieceType.None;

		public bool IsCapture { get; private init; }
		public bool IsCheckMate { get; private init; }
		public bool IsEnPassant { get; private init; }

		public ICastle Castle { get; set; } = Moves.Castle.Empty;
		public IChessKing CheckedKing { get; private set; } = NoKing.Default;

		//public bool IsCheckMate => CheckedKing is not NoKing && CheckedKing.IsMated;

		public IGameState GameState { get; set; } = Games.GameState.Empty;
		public IParseableMove SourceMove { get; private init; } = NotParseable.Default;
		public string AlgebraicMove { get; private set; } = string.Empty;
		public override string ToString() => M.AsEngineMove;

		public IEnumerable<IChessSquare> AffectedSquares()
		{
			if (FromSquare is not INoSquare) yield return FromSquare;
			if (ToSquare is not INoSquare) yield return ToSquare;
			if (IsEnPassant) yield return CapturedPiece.Square;
			if (Castle.Type > CastleMoveType.None)
			{
				yield return Castle.RookOrigin;
				yield return Castle.MovedRook.Square;
			}
		}

		public override int GetHashCode() => SerialNumber;

		internal IMove M => this;

		void IMove.SetPromotion(PieceType promoteTo)
		{
			PromoteTo = promoteTo;
			AlgebraicMove = SourceMove is AlgebraicMove m ? m.Move : AlgebraicFromEngineMove(Castle.Type);

		}

		bool IMove.IsPromotion
		{
			get
			{
				if (MovedPiece is not IPawn p) return false;
				Rank tgtRank = p.Side == Hue.Light ? Rank.R8 : Rank.R1;
				return ToSquare.Rank == tgtRank;
			}
		}

		private string AlgebraicFromEngineMove(CastleMoveType type)
		{
			// Move has not yet been applied to board.
			IBoard board = (IBoard)MovedPiece.Board;
			bool isPawn = MovedPiece is IPawn;
			string cMoved = isPawn ? IsCapture ? FromSquare.File.ToString().ToLower() : string.Empty : MovedPiece.Type.PGNChar().ToString();
			string sChecked = IsCheckMate ? "#" : IsCheck ? "+" : string.Empty, sPromo = PromoteTo == PieceType.None ? string.Empty : $"={PromoteTo.PGNChar()}",
				sCapture = IsCapture ? "x" : string.Empty;
			if (type == CastleMoveType.Kingside) return $"O-O{sChecked}";
			if (type == CastleMoveType.Queenside) return $"O-O-O{sChecked}";
			List<IPiece> moveCandidates = board.ActivePieces.Where(p => !ReferenceEquals(p, MovedPiece) && p.Side == MovedPiece.Side && p.CanMoveTo(ToSquare)).ToList();
			if (moveCandidates.Count == 0 || (isPawn && !moveCandidates.Any(p => p.Type == PieceType.Pawn))) return $"{cMoved}{sCapture}{ToSquare.Name}{sPromo}{sChecked}";
			string which = string.Empty;
			if (moveCandidates.Any(p => p.Type == MovedPiece.Type)) which = $"{MovedPiece.Square.File.FileChar}";
			return $"{cMoved}{which}{sCapture}{ToSquare.Name}{sPromo}{sChecked}";
		}
	}
}
