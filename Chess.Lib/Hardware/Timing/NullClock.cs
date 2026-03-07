using Chess.Lib.Games;
using Common.Lib.Contracts;

namespace Chess.Lib.Hardware.Timing
{
	#region NullClockPlayer
	internal class NullClockPlayer : IClockPlayerEx, INoClockPlayer
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

		public void Pause() { }

		public long RemainingTicks => 0;

		public TimeSpan AccruedIncrements => TimeSpan.Zero;
	}

	#endregion

	#region NullClock
	internal class NullClock : IChessClockEx, INoClock
	{
		internal static readonly NullClock Instance = new NullClock();
		private NullClock() { }

		public bool IsNull => true;
		public IClockPlayer White => NullClockPlayer.Instance;
		public IClockPlayer Black => NullClockPlayer.Instance;
		public IClockPlayer CurrentPlayer => NullClockPlayer.Instance;
		public Hue FlaggedSide => Hue.Default;
		public bool IsFlagged => false;
		public bool IsPaused => false;
		public bool IsRunning => false;
		public bool IsStarted => false;

		public ClockState State => ClockState.NotStarted;

#pragma warning disable 0067    // Disable warning about unused members        
		public event Handler<ClockStateChange>? StateChanged;
		public event Handler<TimerTick>? Tick;
#pragma warning restore

		public void Start(Hue side) { }

		public void MakeMove() { }

		public void Pause() { }

		public void Attach(IInteractiveChessGame game) { }

		public void Resume() { }

	}
	#endregion
}
