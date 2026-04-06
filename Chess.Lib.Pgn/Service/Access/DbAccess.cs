using Common.Lib.Contracts;

namespace Chess.Lib.Pgn.Service.Access
{
	public interface IDbAccess<T> where T : class, IId, INamed
	{
		IReadOnlyList<T> Values { get; }
		int Count => Values.Count;

		bool AreValuesDynamic { get; }

		bool Contains(T value) => Contains(value.Id);
		bool Contains(int id);
	}

	internal abstract class DbAccess<T> : IDbAccess<T> where T : class, IId, INamed
	{
		private List<T> _values;

		protected DbAccess()
		{
			_values = new();
		}

		protected DbAccess(List<T> values, bool isDynamic = false)
		{
			values.Sort(IIdComparer.Instance);
			_values = values;
			_values = values;
		}

		public virtual bool AreValuesDynamic => false;

		public IReadOnlyList<T> Values => _values.AsReadOnly();
		
		public bool Contains(int id)
		{
			T tmp = CreateNew(id);
			int n = _values.BinarySearch(tmp, IIdComparer.Instance);
			return n >= 0;
		}

		protected abstract T CreateNew(int id);
	}
}
