namespace Common.Lib.Contracts
{
	public class EnumerableSequenceComparer<T> : IEqualityComparer<IEnumerable<T>> where T : notnull, IEquatable<T>
	{
		public static readonly EnumerableSequenceComparer<T> Instance = new EnumerableSequenceComparer<T>();
		public bool Equals(IEnumerable<T>? x, IEnumerable<T>? y) => x is null ? y is null : y is not null && x.SequenceEqual(y);

		public int GetHashCode(IEnumerable<T> seq) => seq.Aggregate(new HashCode(), AddToHash).ToHashCode();

		private static HashCode AddToHash(HashCode hc, T item)
		{
			hc.Add(item);
			return hc;
		}
	}

	public class EnumerableContentComparer<T> : IEqualityComparer<IEnumerable<T>> where T : notnull, IEquatable<T>, IComparable<T>
	{
		public static readonly EnumerableContentComparer<T> Instance = new();

		private static readonly EnumerableSequenceComparer<T> SeqCmp = EnumerableSequenceComparer<T>.Instance;
		public bool Equals(IEnumerable<T>? x, IEnumerable<T>? y) => SeqCmp.Equals(x?.Order(), y?.Order());

		public int GetHashCode(IEnumerable<T> obj) => SeqCmp.GetHashCode(obj.Order());
	}
}
