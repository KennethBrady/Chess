namespace CommonTools.Lib.Contracts
{
	public abstract class Disposable : IDisposable
	{
		protected bool IsDisposed { get; private set; }

		protected void CheckNotDisposed()
		{
			if (IsDisposed) throw new ObjectDisposedException(GetType().FullName);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!IsDisposed)
			{
				IsDisposed = true;
			}
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}
