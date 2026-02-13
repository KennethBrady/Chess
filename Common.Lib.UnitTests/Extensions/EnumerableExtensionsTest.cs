using Common.Lib.Extensions;

namespace Common.Lib.UnitTests.Extensions
{
	[TestClass]
	public class EnumerableExtensionsTest
	{
		[TestMethod]
		public void IndexAt()
		{
			var values = Enumerable.Range(0, 1000);
			int n = values.IndexAt(v => v == 500);
			Assert.AreEqual(500, n);
		}

		[TestMethod]
		public void IndexesAt()
		{
			var values = Enumerable.Range(0, 1000);
			List<int> found = values.IndexesAt(v => v % 2 == 0).ToList();
			Assert.HasCount(500, found);
		}

		[TestMethod]
		public void MinMaxOf()
		{
			var values = Enumerable.Range(0, 100);
			var mm = values.GetMinMax();
			Assert.AreEqual(0, mm.Min);
			Assert.AreEqual(99, mm.Max);
			mm = values.Shuffle().GetMinMax();
			Assert.AreEqual(0, mm.Min);
			Assert.AreEqual(99, mm.Max);
		}

		[TestMethod]
		public void Pivot()
		{
			var values = Enumerable.Range(0, 12);
			var pivot = values.Pivot(4);
			Assert.HasCount(values.Count(), pivot);
			Console.WriteLine(string.Join(',', pivot));
			// 0,3,6,9,1,4,7,10,2,5,8,11
			int ndx = 0, nBase = 0;
			foreach(var v in pivot)
			{
				Assert.AreEqual(values.ElementAt(ndx), v);
				ndx += 3;
				if (ndx >= 12) ndx = ++nBase;
			}
		}
	}
}
