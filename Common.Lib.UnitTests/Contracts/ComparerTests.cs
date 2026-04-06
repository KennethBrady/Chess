using Common.Lib.Contracts;
using Common.Lib.Extensions;

namespace Common.Lib.UnitTests.Contracts
{
	[TestClass]
	public class ComparerTests
	{
		private static readonly EnumerableSequenceComparer<int> SeqCmp = EnumerableSequenceComparer<int>.Instance;
		private static readonly EnumerableContentComparer<int> ConCmp = EnumerableContentComparer<int>.Instance;
		[TestMethod]
		public void SequenceComparerTest()
		{
			List<int> l1 = Enumerable.Range(0, 1000).ToList(), l2 = new List<int>(l1);
			Assert.AreNotEqual(l1, l2);
			Assert.IsTrue(SeqCmp.Equals(l1, l2));
			Assert.AreEqual(SeqCmp.GetHashCode(l1), SeqCmp.GetHashCode(l2));
			l2.ShuffleInPlace();
			Assert.IsFalse(SeqCmp.Equals(l1, l2));
			Assert.AreNotEqual(SeqCmp.GetHashCode(l1), SeqCmp.GetHashCode(l2));
		}

		[TestMethod]
		public void ContentComparerTest()
		{
			List<int> l1 = Enumerable.Range(0, 1000).ToList(), l2 = new List<int>(l1);
			Assert.AreNotEqual(l1, l2);
			Assert.IsTrue(ConCmp.Equals(l1, l2));
			Assert.AreEqual(ConCmp.GetHashCode(l1), ConCmp.GetHashCode(l2));
			l2.ShuffleInPlace();
			Assert.IsTrue(ConCmp.Equals(l1, l2));
			Assert.AreEqual(ConCmp.GetHashCode(l1), ConCmp.GetHashCode(l2));
		}
	}
}
