namespace Common.Lib.Extensions
{
	public static class ListExtensions
	{
		extension<T>(List<T> list) where T : struct, IComparable<T>
		{
			public IEnumerable<int> BinarySearchAll(T value)
			{
				int n = list.BinarySearch(value);
				if (n >= 0)
				{
					int n0 = n;
					while (--n0 >= 0 && value.CompareTo(list[n0]) == 0) n = n0;
					yield return n;
					while (++n < list.Count && value.CompareTo(list[n]) == 0) yield return n;
				}
			}

			public bool IsSorted
			{
				get
				{
					if (list.Count == 0) return false;
					if (list.Count == 1) return true;
					for(int i=1;i<list.Count;++i)
					{
						if (list[i - 1].CompareTo(list[i]) > 0) return false;
					}
					return true;
				}
			}
		}

		extension<T>(List<T> list)
		{
			/// <summary>
			/// Apply a Fisher-Yates shuffle in-place
			/// </summary>
			/// <typeparam name="T"></typeparam>
			/// <param name="list"></param>
			public void ShuffleInPlace()
			{
				if (list.Count < 2) return;
				for (int i = list.Count - 1; i >= 1; --i)
				{
					int ndx = Random.Shared.Next(i + 1);
					if (ndx == i) continue;
					// Swap
					T t = list[i];
					list[i] = list[ndx];
					list[ndx] = t;
				}
			}

			/// <summary>
			/// Create a shuffled copy of a list.
			/// </summary>
			/// <returns>A new list with same count and values, but shuffled.</returns>
			/// <remarks>
			/// C# has recently added Shuffle capability to the IEnumerable interface.
			/// Consider using those as a built-in option.
			/// </remarks>
			public List<T> Shuffled()
			{
				List<T> r = new List<T>(list);
				r.ShuffleInPlace();
				return r;
			}

			/// <summary>
			/// Insert a value in a position that retains the list's natural ordering.
			/// </summary>
			/// <param name="value"></param>
			/// <returns>The index at which the value was inserted</returns>
			public int BinaryInsert(T value)
			{
				int n = list.BinarySearch(value);
				if (n < 0) n = ~n;
				list.Insert(n, value);
				return n;
			}

			/// <summary>
			/// Insert a value in a position that retains the list's ordering based on a custom comparer.
			/// </summary>
			/// <param name="v"></param>
			/// <param name="comparer"></param>
			/// <returns>The index at which the value was inserted</returns>
			public int BinaryInsert(T v, IComparer<T> comparer)
			{
				int n = list.BinarySearch(v, comparer);
				if (n < 0) n = ~n;
				list.Insert(n, v);
				return n;
			}

			public IEnumerable<int> BinarySearchAll(T value, IComparer<T> comparer)
			{
				int n = list.BinarySearch(value, comparer);
				if (n >= 0)
				{
					int n0 = n;
					while (--n0 >= 0 && comparer.Compare(value, list[n0]) == 0) n = n0;
					yield return n;
					while (++n < list.Count && comparer.Compare(value, list[n]) == 0) yield return n;
				}
			}

			public bool IsOrdered(IComparer<T> comparer)
			{
				switch(list.Count)
				{
					case 0: return false;
					case 1: return true;
					default:
						for(int i=1;i<list.Count;++i)
						{
							if (comparer.Compare(list[i-1], list[i]) > 0) return false;
						}
						return true;
				}
			}

			/// <summary>
			/// Remove the first item that satifies a predicate.
			/// </summary>
			/// <param name="test"></param>
			/// <returns>The index from which the item was removed, or -1 if not item satisfied the predicate.</returns>
			public bool RemoveAt(Predicate<T> test)
			{
				for (int i = 0; i < list.Count; ++i)
				{
					if (test(list[i]))
					{
						list.RemoveAt(i);
						return true;
					}
				}
				return false;
			}
		}

		extension<T>(IReadOnlyList<T> list)
		{
			public T SelectRandom(T defaultValue) => list.Count == 0 ? defaultValue : list[Random.Shared.Next(list.Count)];
			public IEnumerable<T> SelectRandom(int count) => list.Shuffle().Take(count);
		}
	}
}
