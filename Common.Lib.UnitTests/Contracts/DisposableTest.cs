
using Common.Lib.Contracts;

namespace Common.Lib.UnitTests.Contracts
{
	[TestClass]
	public class DisposableTest
	{
		[TestMethod]
		public void Dispose()
		{
			TestDisposer td = new TestDisposer();
			Assert.IsFalse(td.IsDisposed);
			td.DoSomething();
			td.Dispose();
			Assert.IsTrue(td.IsDisposed);
		}

		[TestMethod]
		public void CheckNotDisposed()
		{
			TestDisposer td = new TestDisposer();
			td.Dispose();
			Assert.ThrowsExactly<ObjectDisposedException>(td.DoSomething);
		}

		private class TestDisposer : Disposable
		{
			public void DoSomething()
			{
				CheckNotDisposed();
			}
		}
	}
}
