namespace Common.Lib.Extensions
{
	public static class ObjectExtensions
	{
		public static IEnumerable<T> Yield<T>(this T value)
		{
			yield return value;
		}
	}
}
