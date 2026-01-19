using Chess.Lib.Games;
using Common.Lib.Contracts;

namespace Chess.Lib.Hardware.Timing
{
	internal class NullClock : IChessClockEx
	{
		internal static readonly NullClock Instance = new NullClock();
		private NullClock() { }

		public bool IsNull => true;
		public IClockPlayer Black => NullClockPlayer.Instance;
		public Hue FlaggedSide => Hue.Default;
		public bool IsFlagged => false;
		public bool IsPaused => false;
		public bool IsRunning => false;
		public bool IsStarted => false;
		public IClockPlayer White => NullClockPlayer.Instance;

#pragma warning disable 0067    // Disable warning about unused members                                           
		public event TypeHandler<Hue>? Flagged;
		public event TypeHandler<TimerTick>? Tick;
#pragma warning restore

		public void MakeMove() { }

		public void Pause() { }

		public void Attach(IInteractiveChessGame game) { }

		void IChessClockEx.OnFlagged(Hue flagged) { }

		void IChessClockEx.OnTick(TimerTick t) { }
	}
}
