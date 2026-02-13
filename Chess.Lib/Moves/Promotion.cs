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

	public record struct Promotion(PieceType PieceType, Hue Hue, IChessSquare OnSquare)
	{
		public static readonly Promotion None = new Promotion(PieceType.None, Hue.Default, NoSquare.Default);
		internal bool IsValid => PieceType.IsPromotionTarget && Hue < Hue.Default;
	}

	internal record struct PromotedPawn(IChessPawn FromPawn, IChessPiece ToPiece, IChessSquare OnSquare) : IPromotion
	{
		internal static readonly PromotedPawn None = new PromotedPawn(NoPawn.Default, NoPiece.Default, NoSquare.Default);
	}
}
