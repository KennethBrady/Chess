using System.Xml.Linq;

namespace Chess.Lib.Hardware.Timing
{
	public record struct ChessClockSetup(TimeSpan WhiteMaxTime, TimeSpan WhiteIncrement, TimeSpan BlackMaxTime, TimeSpan BlackIncrement)
	{
		public static readonly ChessClockSetup Empty = new ChessClockSetup(TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero);

		public ChessClockSetup(TimeSpan maxTime) : this(maxTime, TimeSpan.Zero, maxTime, TimeSpan.Zero) { }
		public ChessClockSetup(TimeSpan maxTime, TimeSpan increment) : this(maxTime, increment, maxTime, increment) { }

		public bool IsEmpty => WhiteMaxTime == TimeSpan.Zero || BlackMaxTime == TimeSpan.Zero;

		internal IChessClockEx Create()
		{
			return IsEmpty ? NullClock.Instance : new ChessClock(this);
		}

		internal static ChessClockSetup FromXml(XElement xml)
		{
			ChessClockSetup r = Empty;
			foreach (XAttribute a in xml.Attributes())
			{
				bool canParse = TimeSpan.TryParse(a.Value, out TimeSpan ts);
				if (!canParse) continue;
				switch (a.Name.LocalName)
				{

					case nameof(WhiteMaxTime): r = r with { WhiteMaxTime = ts }; break;
					case nameof(WhiteIncrement): r = r with { WhiteIncrement = ts }; break;
					case nameof(BlackMaxTime): r = r with { BlackMaxTime = ts }; break;
					case nameof(BlackIncrement): r = r with { BlackIncrement = ts }; break;
				}
			}
			return r;
		}

		internal XElement ToXml()
		{
			XElement x = new XElement(nameof(ChessClockSetup));
			x.Add(new XAttribute(nameof(WhiteMaxTime), WhiteMaxTime.ToString()));
			x.Add(new XAttribute(nameof(WhiteIncrement), WhiteIncrement.ToString()));
			x.Add(new XAttribute(nameof(BlackMaxTime), BlackMaxTime.ToString()));
			x.Add(new XAttribute(nameof(BlackIncrement), BlackIncrement.ToString()));
			return x;
		}
	}
}
