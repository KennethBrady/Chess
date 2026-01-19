namespace Chess.Lib.Hardware.Timing
{
	public record struct ChessClockSetup(TimeSpan WhiteMaxTime, TimeSpan WhiteIncrement, TimeSpan BlackMaxTime, TimeSpan BlackIncrement)
	{
		public static readonly ChessClockSetup Empty = new ChessClockSetup(TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero);

		public ChessClockSetup(TimeSpan maxTime): this(maxTime, TimeSpan.Zero, maxTime, TimeSpan.Zero) { }
		public ChessClockSetup(TimeSpan maxTime, TimeSpan increment): this(maxTime, increment, maxTime, increment) { }

		public bool IsEmpty => WhiteMaxTime == TimeSpan.Zero || BlackMaxTime == TimeSpan.Zero;

		internal IChessClockEx Create()
		{
			return IsEmpty ? NullClock.Instance : new ChessClock(this);
		}
	}
}
