using Chess.Lib.Games;
using Chess.Lib.Hardware;
using Chess.Lib.Hardware.Pieces;
using Chess.Lib.Moves.Parsing;

namespace Chess.Lib.Moves
{
    /// <summary>
    /// Represents a non-move.  Queries for a non-existent move should return NoMove.Default.
    /// </summary>
    internal record struct NoMove(IChessPiece MovedPiece, IChessSquare FromSquare, IChessSquare ToSquare, IChessPiece CapturedPiece, IBoardState BoardState) : IMove, INoMove
	{
		internal static readonly NoMove Default = new NoMove();
		public NoMove() : this(NoPiece.Default, NoSquare.Default, NoSquare.Default, NoPiece.Default, Hardware.BoardState.Empty) { }

		IEnumerable<IChessSquare> IChessMove.AffectedSquares() => Enumerable.Empty<IChessSquare>();

		bool IChessMove.IsCheck => false;
		IPromotion IChessMove.Promotion => PromotedPawn.None;
		bool IChessMove.IsCheckMate => false;
		bool IChessMoveCore.IsCapture => false;
		int IChessMove.SerialNumber => -1;
		bool IChessMove.IsEnPassant => false;
		IChessMove IChessMoveCore.PreviousMove => Default;
		IPromotion IMove.Promotion { get; set; } = PromotedPawn.None;
		bool IMove.IsPromotion => false;
		PieceType IMove.PromoteTo => PieceType.None;
		IMoveCounter IChessMove.Number => MoveCounter.Default;
		IGameState IMove.GameState
		{
			get => GameState.Empty;
			set { }
		}
		IChessKing IChessMove.CheckedKing => NoKing.Default;
		IParseableMove IChessMove.SourceMove => NotParseable.Default;

		void IMove.SetPromotion(PieceType promoteTo) { }
		ICastle IChessMove.Castle => Castle.Empty;
		ICastle IMove.Castle
		{
			get => Castle.Empty;
			set { }
		}
		public string AlgebraicMove => string.Empty;

		IChessPlayer IChessMove.Player => NoPlayer.Default;

		public override int GetHashCode() => 1;
	}
}
