namespace Common.Lib.Contracts
{
	public delegate void EmptyHandler();

	public delegate void Handler<T>(T value);

	public delegate void Handler<Key, Value>(Key key, Value value);

	public delegate void Handler<Key, Value1, Value2>(Key key, Value1 value1, Value2 value2);

	public delegate R ReturnHandler<T, R>(T inValue);

	public delegate void ProgressHandler(double percentComplete, bool completed);

	public delegate Task AsyncHandler<T>(T value);
	public delegate Task<R> AsyncHandler<R, T>(T value);

}
