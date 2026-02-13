using Common.Lib.Extensions;

namespace Common.Lib.UnitTests.Extensions
{
	[TestClass]
	public class ListExtensionsTest
	{
		[TestMethod]
		public void ShuffleInPlace()
		{
			List<int> values = Enumerable.Range(0, 10000).ToList();
			Assert.AreEqual(0, OffIndexValues(values));
			values.ShuffleInPlace();
			// This will theoretically fail once in a few million years of repeated running.
			Assert.IsGreaterThan(0, OffIndexValues(values));
		}

		private static int OffIndexValues(List<int> values)
		{
			int r = 0;
			for (int i = 0; i < values.Count; ++i)
			{
				if (values[i] != i) r++;
			}
			return r;
		}

		[TestMethod]
		public void Shuffled()
		{
			List<int> values = Enumerable.Range(0, 10000).ToList();
			Assert.AreEqual(0, OffIndexValues(values));
			List<int> shuffled = values.Shuffled();
			Assert.AreNotSame(values, shuffled);
			Assert.AreEqual(0, OffIndexValues(values));
			Assert.IsGreaterThan(0, OffIndexValues(shuffled));
			Assert.HasCount(values.Count, shuffled);
		}

		[TestMethod]
		public void RemoveAt()
		{
			List<int> values = Enumerable.Range(0, 10).ToList();
			Assert.HasCount(10, values);
			bool removed = values.RemoveAt(n => n == 25);
			Assert.IsFalse(removed);
			Assert.HasCount(10, values);
			removed = values.RemoveAt(n => n == 5);
			Assert.IsTrue(removed);
			Assert.HasCount(9, values);
		}

		[TestMethod]
		public void BinaryInsert()
		{
			List<int> values = Enumerable.Range(0, 10).ToList();
			Assert.HasCount(10, values);
			int n = values.BinaryInsert(25);
			Assert.AreEqual(10, n);
			Assert.HasCount(11, values);
			Assert.AreEqual(25, values.Last());
		}

		[TestMethod]
		public void BinaryInsertWithComparer()
		{
			List<int> values = Enumerable.Range(0, 10).Reverse().ToList();
			Assert.HasCount(10, values);
			int n = values.BinaryInsert(25, ReverseIntComparer.Instance);
			Assert.AreEqual(0, n);
			Assert.HasCount(11, values);
		}

		private class ReverseIntComparer : Comparer<int>
		{
			public static readonly ReverseIntComparer Instance = new();
			public override int Compare(int x, int y) => -Comparer<int>.Default.Compare(x, y);
		}
	}
}
