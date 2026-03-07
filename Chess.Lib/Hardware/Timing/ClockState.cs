namespace Chess.Lib.Hardware.Timing
{
	[Flags]
	public enum ClockState
	{
		NotStarted		= 0x0000,
		White					= 0x0001,
		Black					= 0x0002,
		Running				= 0x0010,
		Paused				= 0x0020,
		Flagged				= 0x0100,
		WhiteRunning	= White | Running,
		BlackRunning	= Black | Running,
		WhitePaused		= White | Paused,
		BlackPaused		= Black | Paused,
		WhiteFlagged	= White | Flagged,
		BlackFlagged	= Black | Flagged
	}

	public record struct ClockStateChange(ClockState Previous, ClockState Current)
	{
		public bool IsMoveMade => Current.HasFlag(ClockState.Running);
		public bool IsFlagged => !Previous.HasFlag(ClockState.Flagged) && Current.HasFlag(ClockState.Flagged);
		public Hue PlayerHue => Current.HasFlag(ClockState.White) ? Hue.White : Current.HasFlag(ClockState.Black) ? Hue.Black : Hue.Default;

		public bool IsPauseOrUnpause => Current.HasFlag(ClockState.Paused) && !Previous.HasFlag(ClockState.Paused) ||
			!Current.HasFlag(ClockState.Paused) && Previous.HasFlag(ClockState.Paused);
	}
}
