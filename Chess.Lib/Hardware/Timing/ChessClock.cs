using Chess.Lib.Games;
using Common.Lib.Contracts;

namespace Chess.Lib.Hardware.Timing
{
	public record struct TimerTick(Hue CurrentPlayer, TimeSpan Elapsed, TimeSpan Remaining);

	internal class ChessClock : IChessClockEx
	{
		internal const int MonitorInterval = 100;
		public ChessClock(TimeSpan maxTime) : this(maxTime, TimeSpan.Zero) { }

		public ChessClock(TimeSpan maxTime, TimeSpan increment) : this(maxTime, increment, maxTime, increment) { }

		public ChessClock(TimeSpan whiteMaxTime, TimeSpan whiteIncrement, TimeSpan blackMaxTime, TimeSpan blackIncrement)
		{
			W = new ClockPlayer(this, Hue.Light, whiteMaxTime, whiteIncrement);
			B = new ClockPlayer(this, Hue.Dark, blackMaxTime, blackIncrement);
		}

		public ChessClock(ChessClockSetup setup): this(setup.WhiteMaxTime, setup.WhiteIncrement, setup.BlackMaxTime, setup.BlackIncrement) { }

		public bool IsNull => false;
		public IClockPlayer White => W;
		public IClockPlayer Black => B;

		public event TypeHandler<TimerTick>? Tick;
		public event TypeHandler<Hue>? Flagged;

		public bool IsStarted => StartTime.HasValue;
		public bool IsRunning => White.IsRunning || Black.IsRunning;

		public bool IsPaused => IsStarted && !IsFlagged && !IsRunning;
		public bool IsFlagged => White.IsFlagged || Black.IsFlagged;

		public Hue FlaggedSide => White.IsFlagged ? Hue.Light : Black.IsFlagged ? Hue.Dark : Hue.Default;

		public void Attach(IInteractiveChessGame game)
		{
			if (game.Moves.Count == 0)
			{
				game.MoveCompleted += m => MakeMove();
			}
		}

		private IClockPlayerEx W { get; init; }
		private IClockPlayerEx B { get; init; }
		private DateTime? StartTime { get; set; }
		public void MakeMove()
		{
			if (IsFlagged) return;
			if (!StartTime.HasValue)
			{
				StartTime = DateTime.Now;
				W.Start();
			}
			else
			{
				if (White.IsRunning)
				{
					W.Stop();
					B.Start();
				}
				else
						if (Black.IsRunning)
				{
					B.Stop();
					W.Start();
				}
				else
				{
					// Paused.  
					if (White.MoveCount > Black.MoveCount) B.Start(); else W.Start();
				}
			}
		}

		public void Pause()
		{
			if (!IsRunning) return;
			if (White.IsRunning) W.Stop(); else B.Stop();
		}

		void IChessClockEx.OnTick(TimerTick tick) => Tick?.Invoke(tick);
		void IChessClockEx.OnFlagged(Hue hue) => Flagged?.Invoke(hue);
	}
}
