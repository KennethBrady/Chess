using Common.Lib.Contracts;
using System.Diagnostics;

namespace Common.Lib.UnitTests.Contracts
{
	[TestClass]
	public class ContractTests
	{
		private static bool AreConsecutive(IEnumerable<ICountable> countables)
		{
			int lastVal = -1;
			foreach (var c in countables)
			{
				if (lastVal >= c.Count) return false;
				lastVal = c.Count;
			}
			return true;
		}

		[TestMethod]
		public void SortCountable()
		{
			var counts = Enumerable.Range(0, 10000).Select(i => new Countable(i)).Shuffle().ToList();
			Assert.IsFalse(AreConsecutive(counts));
			counts.Sort(CountableComparer.Instance);
			Assert.IsTrue(AreConsecutive(counts));
		}

		[TestMethod]
		public void SortNamed()
		{
			var counts = Enumerable.Range(0, 10000).Select(i => new Countable(i)).Shuffle().ToList();
			Assert.IsFalse(AreConsecutive(counts));
			counts.Sort(NamedComparer.Instance);
			Assert.IsTrue(AreConsecutive(counts));
		}

		[TestMethod]
		public void SortIds()
		{
			var counts = Enumerable.Range(0, 10000).Select(i => new Countable(i)).Shuffle().ToList();
			Assert.IsFalse(AreConsecutive(counts));
			counts.Sort(IIdComparer.Instance);
			Assert.IsTrue(AreConsecutive(counts));
		}

		[DebuggerDisplay("{Count}")]
		private class Countable : ICountable, INamed, IId
		{
			internal Countable(int value)
			{
				Count = value;
				string n = Count.ToString();
				while (n.Length < 5) n = "0" + n;
				Name = n;
			}
			public int Count { get; private init; }

			public string Name { get; private init; }

			int IId.Id => Count;
		}
	}
}
