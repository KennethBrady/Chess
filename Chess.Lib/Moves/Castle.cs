using Chess.Lib.Hardware;
using Chess.Lib.Hardware.Pieces;

namespace Chess.Lib.Moves
{
	public interface ICastle
	{
		CastleMoveType Type { get; }
		IChessPiece MovedRook { get; }
		IChessSquare RookOrigin { get; }

		IChessSquare RookDestination { get; }
	}


	internal record Castle(CastleMoveType Type, IChessPiece MovedRook, IChessSquare RookOrigin, IChessSquare RookDestination) : ICastle
	{
		internal static readonly Castle Empty = new Castle(CastleMoveType.None, NoPiece.Default, NoSquare.Default, NoSquare.Default);
	}
}
