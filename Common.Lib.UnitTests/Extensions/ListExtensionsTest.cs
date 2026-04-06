using Common.Lib.Contracts;
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

		[TestMethod]
		public void BinarySearchAll()
		{
			List<int> values = Enumerable.Range(0, 1000).ToList();
			for (int i = 0; i < 5; ++i) values.Add(500);  // 6 occurrences of 500
			values.Sort();
			List<int> indices = values.BinarySearchAll(500).ToList();
			Assert.HasCount(6, indices);
			Assert.AreEqual(500, indices[0]);
			Assert.AreEqual(505, indices[5]);
			Assert.IsTrue(indices.All(i => values[i] == 500));
			Assert.IsTrue(indices.IsSorted);
			values.Clear();

			// Test values at start
			for (int i = 0; i < 10; ++i) values.Add(5);
			values.Add(6);
			Assert.IsTrue(values.IsSorted);
			indices = values.BinarySearchAll(5).ToList();
			Assert.HasCount(10, indices);
			Assert.AreEqual(0, indices[0]);

			// Test values at end:
			values.Clear();
			values.AddRange(Enumerable.Range(0, 50));
			indices	= values.BinarySearchAll(49).ToList();
			Assert.HasCount(1, indices);
			Assert.AreEqual(49, indices[0]);

			// Test no values found
			indices = values.BinarySearchAll(-2).ToList();
			Assert.HasCount(0, indices);
		}

		[TestMethod]
		public void BinarySearchAllWithComparer()
		{
			List<IId> things = Enumerable.Range(0, 1000).Select(i => new IdThing(i)).Cast<IId>().ToList();
			for (int i = 0; i < 5; ++i) things.Add(new IdThing(500));
			things.Sort(IIdComparer.Instance);
			Assert.IsTrue(things.IsOrdered(IIdComparer.Instance));
			List<int> indices = things.BinarySearchAll<IId>(new IdThing(500), IIdComparer.Instance).ToList();
			Assert.HasCount(6, indices);
			Assert.AreEqual(500, indices[0]);
			Assert.AreEqual(505, indices[5]);
			Assert.IsTrue(indices.IsSorted);
		}

		private record IdThing(int Id) : IId;

		private class ReverseIntComparer : Comparer<int>
		{
			public static readonly ReverseIntComparer Instance = new();
			public override int Compare(int x, int y) => -Comparer<int>.Default.Compare(x, y);
		}
	}
}
