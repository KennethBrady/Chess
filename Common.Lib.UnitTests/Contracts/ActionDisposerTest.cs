using Common.Lib.Contracts;

namespace Common.Lib.UnitTests.Contracts
{
	[TestClass]
	public class ActionDisposerTest
	{

		[TestMethod]
		public void Dispose()
		{
			bool disposed = false;
			ActionDisposer ad = new ActionDisposer(() =>
			{
				disposed = true;
			});
			Assert.IsFalse(disposed);
			ad.Dispose();
			Assert.IsTrue(disposed);
			disposed = false;
			ad.Dispose();
			Assert.IsFalse(disposed, "Action invoked only once.");
		}
	}
}
