using Chess.Lib.Hardware.Pieces;
using Chess.Lib.Moves;

namespace Chess.Lib.Games
{
	public record struct CompletedMove(IChessMove Move, int RepetitionCount)
	{
		public int SerialNumber => Move.SerialNumber;
		public bool IsCheck => Move.IsCheck;

		public bool IsCheckMate => Move.IsCheckMate;
		public bool IsPromotion => Move.Promotion.IsValid;
		public IChessKing CheckedKing
		{
			get
			{
				CompletedMove m = this;
				if (IsCheck) return (IChessKing)M.Board.ActivePieces.Where(p => p.Type == PieceType.King && p.Side != m.Move.Side);
				return NoKing.Default;
			}
		}
		private IMove M => (IMove)Move;

	}

}
