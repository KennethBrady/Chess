namespace Chess.Lib.Hardware.Timing
{
	internal class NullClockPlayer : IClockPlayerEx
	{
		internal static readonly NullClockPlayer Instance = new();
		private NullClockPlayer() { }

		public TimeSpan Elapsed => TimeSpan.Zero;
		public TimeSpan Increment => TimeSpan.Zero;
		public bool IsFlagged => false;
		public bool IsRunning => false;
		public TimeSpan MaxTime => TimeSpan.Zero;
		public TimeSpan Remaining => TimeSpan.Zero;
		public Hue Side => Hue.Default;
		public int MoveCount => 0;

		public void Start() { }

		public void Stop() { }
	}
}
