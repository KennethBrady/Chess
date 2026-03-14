using Chess.Lib.Games;
using Common.Lib.Contracts;

namespace Chess.Lib.Hardware.Timing
{
	public record struct TimerTick(Hue CurrentPlayer, TimeSpan Elapsed, TimeSpan Remaining);
	internal class ChessClock: IChessClockEx
	{
		internal const int MonitorInterval = 100;

		private ClockState _state = ClockState.NotStarted;
		internal ChessClock(TimeSpan maxTime): this(maxTime, TimeSpan.Zero, maxTime, TimeSpan.Zero) { }

		internal ChessClock(TimeSpan maxTime, TimeSpan increment): this(maxTime, increment, maxTime, increment) { }

		internal ChessClock(TimeSpan whiteMaxTime, TimeSpan whiteIncrement, TimeSpan blackMaxTime, TimeSpan blackIncrement)
		{
			W = new ClockPlayer(this, Hue.White, whiteMaxTime, whiteIncrement);
			B =new ClockPlayer(this, Hue.Black, blackMaxTime, blackIncrement);
		}

		internal ChessClock(ChessClockSetup setup): this(setup.WhiteMaxTime, setup.WhiteIncrement, setup.BlackMaxTime, setup.BlackIncrement)  { }

		public event Handler<TimerTick>? Tick;
		public event Handler<ClockStateChange>? StateChanged;

		public IClockPlayer White => W;
		public IClockPlayer Black => B;

		public IClockPlayer CurrentPlayer => RunningPlayer == null ? NullClockPlayer.Instance : RunningPlayer;

		public ClockState State
		{
			get => _state;
			private set
			{
				if (_state != value)
				{
					var old = _state;
					_state = value;
					StateChanged?.Invoke(new ClockStateChange(old, _state));
				}
			}
		}

		private IClockPlayerEx W { get; init; }
		private IClockPlayerEx B { get; init; }

		private IClockPlayerEx? RunningPlayer { get; set; }

		/// <summary>
		/// Start the clock corresponding to side.
		/// </summary>
		/// <param name="side"></param>
		public void Start(Hue side)
		{
			if (RunningPlayer != null) return;
			RunningPlayer = side == Hue.White ? W : B;
			RunningPlayer.Start();
			State = side == Hue.White ? ClockState.BlackRunning : ClockState.WhiteRunning;
			Monitor();
		}

		public void Pause()
		{
			if (Me.IsRunning)
			{
				RunningPlayer?.Pause();
				State = ClockState.Paused;
			}
		}

		public void Resume()
		{
			if (Me.IsPaused && RunningPlayer != null)
			{
				RunningPlayer.Start();
				State = RunningPlayer.Side == Hue.White ? ClockState.WhiteRunning : ClockState.BlackRunning;
				Monitor();
			}
		}

		public void MakeMove()
		{
			if (RunningPlayer == null) return;	// not started
			RunningPlayer.Stop();
			RunningPlayer = RunningPlayer.Side == Hue.White ? B : W;
			RunningPlayer.Start();
			State = RunningPlayer.Side == Hue.White ? ClockState.WhiteRunning : ClockState.BlackRunning;
		}

		private async void Monitor()
		{
			while (Me.IsRunning && RunningPlayer != null)
			{
				//System.Diagnostics.Debug.WriteLine(RunningPlayer.RemainingTicks.ToString());
				if (RunningPlayer.RemainingTicks <= 0)
				{
					RunningPlayer.Pause();
					State = RunningPlayer.Side == Hue.White ? ClockState.WhiteFlagged : ClockState.BlackFlagged;
					return;
				}
				TimerTick tt = new TimerTick(RunningPlayer.Side, RunningPlayer.Elapsed, RunningPlayer.Remaining);
				Tick?.Invoke(tt);
				await Task.Delay(MonitorInterval);
			}
		}

		void IChessClockEx.Attach(IInteractiveChessGame game)
		{
			if (game.Moves.Count == 0) game.MoveCompleted += (m) =>
			{
				if (RunningPlayer == null) Start(m.Move.Side.Other); else MakeMove();
			};
		}

		private IChessClockEx Me => (IChessClockEx)this;
	}

	public static class ChessClockExtensions
	{
		extension(TimeSpan ts)
		{
			public string RemainingOnClock
			{
				get
				{
					if (ts.Hours > 0) return $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}";
					if (ts.Minutes > 0) return $"{ts.Minutes:00}:{ts.Seconds:00}";
					return $"{ts.Seconds:00}";
				}
			}
		}

		extension(IChessClock cc)
		{
			public bool IsNull => cc is INoClock;
		}

		extension(IClockPlayer player)
		{
			public bool IsNull => player is INoClockPlayer;
		}
	}
}
