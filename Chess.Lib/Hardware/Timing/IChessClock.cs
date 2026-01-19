using Chess.Lib.Games;
using Common.Lib.Contracts;

namespace Chess.Lib.Hardware.Timing
{
	public interface IChessClock
	{
		/// <summary>
		/// Get a value indication whether this is null (non-working)
		/// </summary>
		bool IsNull { get; }
		IClockPlayer Black { get; }
		Hue FlaggedSide { get; }
		bool IsFlagged { get; }
		bool IsPaused { get; }
		bool IsRunning { get; }
		bool IsStarted { get; }
		IClockPlayer White { get; }

		event TypeHandler<Hue>? Flagged;
		event TypeHandler<TimerTick>? Tick;

		void MakeMove();
		void Pause();

		void Attach(IInteractiveChessGame game);
	}

	internal interface IChessClockEx : IChessClock
	{
		void OnFlagged(Hue flagged);
		void OnTick(TimerTick t);
	}
}