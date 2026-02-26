namespace Common.Lib.Contracts
{
	/// <summary>
	/// Represent any type with a Name property
	/// </summary>
	public interface INamed
	{
		string Name { get; }
	}

	public class NamedComparer : Comparer<INamed>
	{
		public static readonly NamedComparer Instance = new NamedComparer();

		private NamedComparer() { }

		public override int Compare(INamed? x, INamed? y)
		{
			if (x == null) return y == null ? 0 : -1;
			if (y == null) return 1;
			return Comparer<string>.Default.Compare(x.Name, y.Name);
		}
	}
}
