namespace Chess.Lib.Hardware.Timing
{
	public interface IClockPlayer
	{
		TimeSpan Elapsed { get; }
		TimeSpan Increment { get; }
		bool IsFlagged { get; }
		bool IsRunning { get; }
		TimeSpan MaxTime { get; }
		TimeSpan Remaining { get; }
		Hue Side { get; }
		int MoveCount { get; }
	}

	internal interface IClockPlayerEx : IClockPlayer
	{
		void Start();
		void Stop();
	}
}
