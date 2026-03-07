using Chess.Lib.Games;
using Common.Lib.Contracts;

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
		TimeSpan AccruedIncrements { get; }

		long RemainingTicks { get; }
		Hue Side { get; }
		int MoveCount { get; }
	}

	public interface INoClockPlayer : IClockPlayer;

	internal interface IClockPlayerEx : IClockPlayer
	{
		void Start();
		void Stop();

		void Pause();
	}

	public interface IChessClock
	{
		IClockPlayer White { get; }
		IClockPlayer Black { get; }

		IClockPlayer CurrentPlayer { get; }

		public ClockState State { get; }
		Hue FlaggedSide => State == ClockState.WhiteFlagged ? Hue.White : State == ClockState.BlackFlagged ? Hue.Black : Hue.Default;
		bool IsFlagged  => State == ClockState.WhiteFlagged || State == ClockState.BlackFlagged;
		bool IsPaused => State == ClockState.Paused;
		bool IsRunning => State == ClockState.WhiteRunning || State == ClockState.BlackRunning;
		bool IsStarted => State > ClockState.NotStarted && !IsFlagged;

		event Handler<TimerTick>? Tick;
		event Handler<ClockStateChange>? StateChanged;

		void Start(Hue side);

		void Pause();

		void Resume();
	}

	public interface INoClock : IChessClock;

	internal interface IChessClockEx : IChessClock
	{
		void Attach(IInteractiveChessGame game);

	}
}