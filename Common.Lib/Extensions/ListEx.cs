namespace Common.Lib.Extensions
{
	public static class ListEx
	{
		/// <summary>
		/// Apply a Fisher-Yates shuffle in-place
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="list"></param>
		public static void ShuffleInPlace<T>(this List<T> list)
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

		public static List<T> Shuffled<T>(this IReadOnlyList<T> list)
		{
			List<T> r = new(list);
			r.ShuffleInPlace();
			return r;
		}
		

	}
}
