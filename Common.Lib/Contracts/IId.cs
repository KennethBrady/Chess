namespace Common.Lib.Contracts
{
	public interface IId
	{
		public int Id { get; }
	}

	public class IIdComparer : IComparer<IId>
	{
		public static readonly IIdComparer Instance = new();

		private IIdComparer() { }

		public int Compare(IId? x, IId? y)
		{
			if (x == null) return y == null ? 0 : 1;
			if (y == null) return 1;
			return Comparer<int>.Default.Compare(x.Id, y.Id);
		}
	}
}
