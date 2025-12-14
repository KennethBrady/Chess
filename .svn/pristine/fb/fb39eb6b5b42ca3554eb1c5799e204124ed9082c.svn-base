using Chess.Lib.Hardware;
using Chess.Lib.Hardware.Pieces;

namespace Chess.Lib.Moves
{
	public interface IPromotion
	{
		IChessPawn FromPawn { get; }
		IChessPiece ToPiece { get; }
		IChessSquare OnSquare { get; }
		bool IsValid => FromPawn is not NoPawn;
	}

	internal record struct Promotion(IChessPawn FromPawn, IChessPiece ToPiece, IChessSquare OnSquare) : IPromotion
	{
		internal static readonly Promotion None = new Promotion(NoPawn.Default, NoPiece.Default, NoSquare.Default);
	}
}
