namespace Common.Lib.Contracts
{
	/// <summary>
	/// Represents any type with a Count property.
	/// </summary>
	public interface ICountable : IComparable<ICountable>
	{
		int Count { get; }

		int IComparable<ICountable>.CompareTo(ICountable? other) => CountableComparer.Instance.Compare(this, other);
		
	}

	public class CountableComparer : Comparer<ICountable>
	{
		public static CountableComparer Instance = new CountableComparer();
		private CountableComparer() { }
		public override int Compare(ICountable? x, ICountable? y)
		{
			if (x is null) return y == null ? 0 : -1;
			if (y is null) return 1;
			return Comparer<int>.Default.Compare(x.Count, y.Count);
		}
	}
}
