using System.Diagnostics;

namespace Chess.Lib.Hardware.Timing
{
	internal class ClockPlayer : IClockPlayerEx
	{
		internal ClockPlayer(IChessClockEx owner, Hue side, TimeSpan maxTime, TimeSpan increment)
		{
			Owner = owner;
			Side = side;
			MaxTime = Remaining = maxTime;
			Increment = increment;
			Stopwatch = new Stopwatch();
		}

		public Hue Side { get; private init; }

		public TimeSpan MaxTime { get; init; }

		public TimeSpan Increment { get; init; }

		public bool IsRunning => Stopwatch.IsRunning;

		public bool IsFlagged => Remaining <= TimeSpan.Zero;

		public TimeSpan Elapsed { get; private set; } = TimeSpan.Zero;
		public TimeSpan Remaining { get; private set; }

		public int MoveCount { get; private set; }

		private Stopwatch Stopwatch { get; init; }
		private IChessClockEx Owner { get; init; }


		private record struct StartState(DateTime When, TimeSpan Elapsed, TimeSpan Remaining);
		private StartState Started { get; set; }

		public void Start()
		{
			Started = new StartState(DateTime.Now, Elapsed, Remaining);
			Stopwatch.Restart();
			Monitor();
		}

		public void Stop()
		{
			DateTime now = DateTime.Now;
			Stopwatch.Stop();
			MoveCount++;
			if (!IsFlagged)
			{
				// Apply most accurate valus:
				TimeSpan passed = now - Started.When;
				Elapsed = Started.Elapsed + (now - Started.When);
				Remaining = Started.Remaining - passed + Increment;
			}
		}

		private async void Monitor()
		{
			do
			{
				await Task.Delay(ChessClock.MonitorInterval);
				if (Stopwatch.IsRunning)
				{
					Elapsed += Stopwatch.Elapsed;
					Remaining -= Stopwatch.Elapsed;
					if (Remaining <= TimeSpan.Zero)
					{
						Stopwatch.Stop();
						Elapsed = MaxTime;
						Remaining = TimeSpan.Zero;
						Owner.OnFlagged(Side);
					}
					else Owner.OnTick(new TimerTick(Side, Elapsed, Remaining));
				}
			} while (Stopwatch.IsRunning);
		}
	}
}
