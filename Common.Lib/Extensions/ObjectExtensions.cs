namespace Common.Lib.Extensions
{
	public static class ObjectExtensions
	{
		public static IEnumerable<T> Yield<T>(this T value)
		{
			for(int i=0;i<1;i++) yield return value;
		}
	}
}
