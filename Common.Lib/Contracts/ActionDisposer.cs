namespace Common.Lib.Contracts
{
	public class ActionDisposer : IDisposable
	{
		public static readonly ActionDisposer NoAction = new ActionDisposer(() => { });

		public ActionDisposer(Action action)
		{
			Action = action ?? throw new ArgumentNullException(nameof(action));
		}

		private Action Action { get; init; }

		public bool IsDisposed { get; private set; }
		public void Dispose()
		{
			if (IsDisposed) return;
			Action();
			IsDisposed = true;
		}
	}
}
