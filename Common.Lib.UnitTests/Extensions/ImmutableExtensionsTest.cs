using Common.Lib.Extensions;
using System.Collections.Immutable;

namespace Common.Lib.UnitTests.Extensions
{
	[TestClass]
	public class ImmutableExtensionsTest
	{
		[TestMethod]
		public void ReplaceAt()
		{
			ImmutableList<int> list = ImmutableList<int>.Empty.AddRange(Enumerable.Range(0, 10000));
			List<int> indices = new List<int>(50);
			while(indices.Count < 50)
			{
				int n = Random.Shared.Next(list.Count);
				int i = indices.BinarySearch(n);
				if (i < 0) indices.Insert(~i, n);
			}
			foreach (int i in indices)
			{
				Assert.AreEqual(i, list[i]);
				list = list.ReplaceAt(i, -list[i]);
			}
			foreach(int i in indices) Assert.AreEqual(-i, list[i]);
		}
	}
}
