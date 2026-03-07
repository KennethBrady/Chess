using Chess.Lib.Hardware;
using Chess.Lib.Hardware.Timing;

namespace Chess.Lib.UnitTests.Hardware
{
	[TestClass]
	public class ChessClockTest
	{
		[TestMethod]
		public void Defaults()
		{
			TimeSpan ts5 = TimeSpan.FromMinutes(5);
			IChessClock cc = new ChessClock(ts5);
			Assert.IsFalse(cc.IsStarted);
			Assert.IsFalse(cc.IsRunning);
			Assert.IsFalse(cc.IsPaused);
			Assert.AreEqual(ts5, cc.White.MaxTime);
			Assert.AreEqual(TimeSpan.Zero, cc.White.Increment);
			Assert.AreEqual(ts5, cc.Black.MaxTime);
			Assert.AreEqual(TimeSpan.Zero, cc.Black.Increment);
			Assert.IsNotNull(cc.White);
			Assert.AreEqual(ts5, cc.White.Remaining);
			Assert.AreEqual(TimeSpan.Zero, cc.White.Elapsed);
			Assert.AreEqual(Hue.White, cc.White.Side);
			Assert.IsNotNull(cc.Black);
			Assert.AreEqual(ts5, cc.Black.Remaining);
			Assert.AreEqual(TimeSpan.Zero, cc.Black.Elapsed);
			Assert.AreEqual(Hue.Black, cc.Black.Side);
		}

		[TestMethod]
		public async Task MakeMoveAndPause()
		{
			IChessClockEx cc = new ChessClock(TimeSpan.FromMinutes(5));
			int nTicks = 0, nMove = 0;
			cc.StateChanged += s =>
			{
				if (s.IsMoveMade) nMove++;
			};
			cc.Tick += t =>
			{
				nTicks++;
			};
			cc.Start(Hue.White);
			Assert.AreEqual(1, nMove);
			Assert.IsTrue(cc.IsStarted);
			Assert.IsTrue(cc.IsRunning);
			Assert.IsFalse(cc.IsPaused);
			Assert.IsTrue(cc.White.IsRunning);
			Assert.IsFalse(cc.Black.IsRunning);
			await Task.Delay(500);
			Assert.IsGreaterThan(3, nTicks);
			cc.Pause();
			Assert.AreEqual(1, nMove);
			Assert.IsTrue(cc.IsStarted);
			Assert.IsTrue(cc.IsPaused);
			Assert.IsFalse(cc.IsRunning);
			Assert.IsFalse(cc.White.IsRunning);
			Assert.IsFalse(cc.Black.IsRunning);
		}

		[TestMethod]
		public async Task Increment()
		{
			TimeSpan tss5 = TimeSpan.FromSeconds(5);
			ChessClock cc = new ChessClock(TimeSpan.FromMinutes(5), tss5);
			Assert.AreEqual(tss5, cc.White.Increment);
			Assert.AreEqual(tss5, cc.Black.Increment);
			TimerTick? lastTick = null;
			cc.Tick += tick =>
			{
				lastTick = tick;
			};
			cc.Start(Hue.White);
			await Task.Delay(500);
			cc.MakeMove();
			Assert.IsTrue(lastTick.HasValue);
			TimeSpan incr = cc.White.Remaining - lastTick.Value.Remaining, delta = incr - tss5;
			Assert.AreEqual(tss5, cc.White.AccruedIncrements);
			double deltas = Math.Abs(incr.TotalSeconds - tss5.TotalSeconds);
			Assert.IsLessThan(0.1, deltas);
		}

		[TestMethod]
		public async Task NoIncrementOnPause()
		{
			TimeSpan tss5 = TimeSpan.FromSeconds(5);
			ChessClock cc = new ChessClock(TimeSpan.FromMinutes(5), tss5);
			Assert.AreEqual(tss5, cc.White.Increment);
			Assert.AreEqual(tss5, cc.Black.Increment);
			TimerTick? lastTick = null;
			cc.Tick += tick =>
			{
				lastTick = tick;
			};
			cc.Start(Hue.White);
			await Task.Delay(500);
			cc.Pause();
			Assert.IsTrue(lastTick.HasValue);
			Assert.AreEqual(TimeSpan.Zero, cc.White.AccruedIncrements);
		}

		[TestMethod]
		public async Task Pause()
		{
			IChessClock cc = new ChessClock(TimeSpan.FromMinutes(5));
			int nTicks = 0;
			cc.Tick += tick =>
			{
				nTicks++;
			};
			cc.Start(Hue.White);
			Assert.IsFalse(cc.IsPaused);
			Assert.IsTrue(cc.IsRunning);
			await Task.Delay(500);
			cc.Pause();
			Assert.IsGreaterThan(0, nTicks);
			Assert.IsTrue(cc.IsPaused);
			Assert.IsFalse(cc.IsRunning);
			nTicks = 0;
			await Task.Delay(500);
			Assert.AreEqual(0, nTicks);
		}

		[TestMethod]
		public async Task Flagged()
		{
			IChessClock cc = new ChessClock(TimeSpan.FromSeconds(2));
			Hue flaggedHue = Hue.Default;
			cc.StateChanged += s =>
			{
				if (s.IsFlagged) flaggedHue = s.PlayerHue;
			};
			cc.Start(Hue.White);
			await Task.Delay(4000);
			Assert.IsTrue(cc.IsFlagged);
			Assert.AreEqual(Hue.White, flaggedHue);
			Assert.AreEqual(flaggedHue, cc.FlaggedSide);
			Assert.IsTrue(cc.IsFlagged);
			Assert.IsFalse(cc.IsRunning);
		}

		[TestMethod]
		public async Task RemainingOnClock()
		{
			ChessClock cc = new ChessClock(TimeSpan.FromMinutes(5));
			Assert.AreEqual("05:00", cc.White.Remaining.RemainingOnClock);
			cc = new ChessClock(TimeSpan.FromHours(1).Add(TimeSpan.FromMinutes(30)));
			Assert.AreEqual("01:30:00", cc.White.Remaining.RemainingOnClock);
			cc = new ChessClock(TimeSpan.FromSeconds(20));
			Assert.AreEqual("20", cc.White.Remaining.RemainingOnClock);
		}

		// Generate output of ticks
		//[TestMethod]
		public async Task RunClock()
		{
			ChessClock cc = new ChessClock(TimeSpan.FromMinutes(1));
			int nPass = 0;
			Hue h = Hue.White;
			bool completed = false;
			DateTime start = DateTime.Now;
			cc.Tick += tt =>
			{
				TimeSpan el = DateTime.Now - start;
				Console.WriteLine($"{h}: {tt.Remaining.RemainingOnClock}\t{el.TotalSeconds:F2}");
				if (++nPass % 20 == 0)
				{
					if (nPass > 100)
					{
						completed = true;
						cc.Pause();
					}
					h = h.Other;
					cc.MakeMove();
				}
			};
			cc.Start(h);
			while (!completed) await Task.Delay(500);
		}

	}
}
