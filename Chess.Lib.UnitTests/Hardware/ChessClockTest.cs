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
			ChessClock cc = new ChessClock(ts5);
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
			Assert.AreEqual(Hue.Light, cc.White.Side);
			Assert.IsNotNull(cc.Black);
			Assert.AreEqual(ts5, cc.Black.Remaining);
			Assert.AreEqual(TimeSpan.Zero, cc.Black.Elapsed);
			Assert.AreEqual(Hue.Dark, cc.Black.Side);
		}

		[TestMethod]
		public async Task MakeMoveAndPause()
		{
			ChessClock cc = new ChessClock(TimeSpan.FromMinutes(5));
			int nTicks = 0;
			cc.Tick += t =>
			{
				nTicks++;
			};
			cc.MakeMove();
			Assert.IsTrue(cc.IsStarted);
			Assert.IsTrue(cc.IsRunning);
			Assert.IsFalse(cc.IsPaused);
			Assert.IsTrue(cc.White.IsRunning);
			Assert.IsFalse(cc.Black.IsRunning);
			await Task.Delay(500);
			Assert.IsGreaterThan(3, nTicks);
			cc.Pause();
			Assert.IsTrue(cc.IsStarted);
			Assert.IsTrue(cc.IsPaused);
			Assert.IsFalse(cc.IsRunning);
			Assert.IsFalse(cc.White.IsRunning);
			Assert.IsFalse(cc.Black.IsRunning);
		}

		[TestMethod]
		public async Task MakeMoveTwice()
		{
			ChessClock cc = new ChessClock(TimeSpan.FromMinutes(5));
			cc.MakeMove();
			await Task.Delay(500);
			cc.MakeMove();
			Assert.IsTrue(cc.IsStarted);
			Assert.IsFalse(cc.IsPaused);
			Assert.IsTrue(cc.IsRunning);
			Assert.IsFalse(cc.White.IsRunning);
			Assert.IsTrue(cc.Black.IsRunning);
			cc.Pause();
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
			cc.MakeMove();
			await Task.Delay(500);
			cc.MakeMove();
			Assert.IsTrue(lastTick.HasValue);
			TimeSpan incr = cc.White.Remaining - lastTick.Value.Remaining;
			Assert.IsGreaterThan(tss5, incr);
		}

		[TestMethod]
		public async Task IncrementOnPause()
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
			cc.MakeMove();
			await Task.Delay(500);
			cc.Pause();
			Assert.IsTrue(lastTick.HasValue);
			TimeSpan incr = cc.White.Remaining - lastTick.Value.Remaining;
			Assert.IsGreaterThan(tss5, incr);
		}

		[TestMethod]
		public async Task Pause()
		{
			ChessClock cc = new ChessClock(TimeSpan.FromMinutes(5));
			int nTicks = 0;
			cc.Tick += tick =>
			{
				nTicks++;
			};
			cc.MakeMove();
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
			ChessClock cc = new ChessClock(TimeSpan.FromSeconds(8));
			Hue flaggedHue = Hue.Default;
			cc.Flagged += h =>
			{
				flaggedHue = h;
			};
			cc.MakeMove();
			await Task.Delay(2000);
			Assert.IsTrue(cc.IsFlagged);
			Assert.AreEqual(Hue.Light, flaggedHue);
			Assert.AreEqual(flaggedHue, cc.FlaggedSide);
			Assert.IsTrue(cc.IsFlagged);
			Assert.IsFalse(cc.IsRunning);
		}

	}
}
