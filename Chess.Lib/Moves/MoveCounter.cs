using Chess.Lib.Hardware;

namespace Chess.Lib.Moves
{
	public interface IMoveCounter
	{
		int SerialMoveNumber { get; }
		Hue Side { get; }
		int GameMoveNumber { get; }
	}

	internal record struct MoveCounter(int SerialMoveNumber, Hue Side): IMoveCounter
	{
		internal static readonly MoveCounter Default = new MoveCounter(-1, Hue.Default);
		public int GameMoveNumber => SerialToGameNumber(SerialMoveNumber);
		internal static int SerialToGameNumber(int serialNumber) => 1 + serialNumber / 2;
	}
}
