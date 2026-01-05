namespace Common.Lib.Contracts
{
	public static class Actions
	{
		public static Action Empty => () => { };

	}

	public static class Actions<T>
	{
		public static Action<T> Empty => (T t) => { };
	}

	public static class Actions<T,K>
	{
		public static Action<T, K> Empty => (T t, K k) => { };
	}

}
