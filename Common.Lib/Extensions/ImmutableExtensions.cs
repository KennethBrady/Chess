using System.Collections.Immutable;

namespace Common.Lib.Extensions
{
	public static class ImmutableExtensions
	{
		extension<T>(ImmutableList<T> list)
		{
			public ImmutableList<T> ReplaceAt(int i, T value)
			{
				var b = list.ToBuilder();
				b.RemoveAt(i);
				b.Insert(i, value);
				return b.ToImmutable();
			}
		}
	}
}
