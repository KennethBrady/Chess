using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Chess.Lib.Hardware.Timing
{
	[DebuggerDisplay("{Side}")]
	internal class ClockPlayer : IClockPlayerEx
	{
		private Stopwatch _stopwatch;

		internal ClockPlayer(IChessClockEx owner, Hue side, TimeSpan maxTime, TimeSpan increment)
		{
			Owner = owner;
			Side = side;
			MaxTime = maxTime;
			Increment = increment;
			_stopwatch = new Stopwatch();
		}

		public Hue Side { get; private init; }

		public TimeSpan MaxTime { get; private init; }

		public TimeSpan Increment { get; private init; }

		private IChessClockEx Owner { get; init; }

		public bool IsRunning => _stopwatch.IsRunning;

		public TimeSpan Remaining => TimeSpan.FromTicks(Math.Max(0,RemainingTicks));


		public TimeSpan Elapsed => _stopwatch.Elapsed;

		public bool IsFlagged => RemainingTicks <= 0;

		public int MoveCount { get; private set; }

		public TimeSpan AccruedIncrements { get; private set; } = TimeSpan.Zero;

		public void Start()
		{
			if (IsRunning) return;
			_stopwatch.Start();
		}

		public void Stop()
		{
			if (IsRunning)
			{
				_stopwatch.Stop();
				AccruedIncrements += Increment;
			}
		}
		
		public void Pause()
		{
			if (IsRunning) _stopwatch.Stop();
		}

		public long RemainingTicks => MaxTime.Ticks - _stopwatch.ElapsedTicks + AccruedIncrements.Ticks;
	}
}
