using System.Collections;
using System.Collections.Immutable;

namespace Common.Lib.Extensions
{
	public static class EnumerableExtensions
	{
		extension<T>(IEnumerable<T> values) where T : struct, IComparable
		{
			/// <summary>
			/// Find and return the minimum and maximum values in an enumeration.
			/// </summary>
			/// <returns>The </returns>
			public (T Min, T Max) GetMinMax()
			{
				T min = default(T), max = default(T);
				int n = 0;
				foreach (var t in values)
				{
					if (n++ == 0)
					{
						min = max = t;
						continue;
					}
					if (t.CompareTo(min) < 0) min = t;
					if (t.CompareTo(max) > 0) max = t;
				}
				return (min, max);
			}
		}

		extension<T>(IEnumerable<T> values)
		{
			/// <summary>
			/// Find the index of the first element that satisfies a predicate.
			/// </summary>
			/// <param name="test"></param>
			/// <returns>The index of the first element satisfying the predicate, or -1 if no element satisfies.</returns>
			public int IndexAt(Predicate<T> test)
			{
				int n = 0;
				foreach (T v in values)
				{
					if (test(v)) return n;
					n++;
				}
				return -1;
			}

			/// <summary>
			/// Find the indexes of all elements satisfying a predicate.
			/// </summary>
			/// <param name="test"></param>
			/// <returns>An enumeration of indexes of all elements satisfying the predicate.</returns>
			public IEnumerable<int> IndexesAt(Predicate<T> test)
			{
				int n = 0;
				foreach (var t in values)
				{
					if (test(t)) yield return n;
					n++;
				}
			}

			/// <summary>
			/// Find the first element matching a test.
			/// </summary>
			/// <param name="test"></param>
			/// <returns>The index of the first element matching the test, or -1 if no matches are found.</returns>
			/// <remarks>If a pure IEnumerable is used, the enumerable will be evaluated exactly once.</remarks>
			public IEnumerable<T?> Pivot(int columnCount, T? empty = default)
			{
				List<T> temp = (values is List<T> l) ? l : values.ToList();
				int rowCount = temp.Count / columnCount, nRet = 0;
				BitArray used = new BitArray(temp.Count, true);
				for (int r = 0; r < rowCount; r++)
				{
					for (int c = 0; c < columnCount; c++)
					{
						int n = c * rowCount + r;
						if (n < temp.Count)
						{
							yield return temp[n];
							used[n] = false;
							nRet++;
						}
					}
				}
				if (nRet < temp.Count)
				{
					int nEmpty = columnCount - (temp.Count - nRet);
					for (int i = 0; i < nEmpty; ++i) yield return empty;
					for (int i = 0; i < temp.Count; ++i) if (used[i]) yield return temp[i];
				}
			}

			public ImmutableList<T> ToImmutable() => ImmutableList<T>.Empty.AddRange(values);

		}
	}
}
