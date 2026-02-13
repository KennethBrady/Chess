using Common.Lib.Extensions;

namespace Common.Lib.UnitTests.Extensions
{
	[TestClass]
	public class DoubleExtensionsTest
	{
		[TestMethod]
		public void AlmostEquals()
		{
			Assert.IsTrue(6.1.AlmostEquals(6.1000000001));
			Assert.IsTrue(6.1.AlmostEquals(6.1001, 1e-2));
			Assert.IsFalse(6.1.AlmostEquals(6.1001));
		}
	}
}
