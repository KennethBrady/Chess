namespace Common.Lib.Contracts
{
	/// <summary>
	/// IDisposable implementation using the Disposable pattern
	/// </summary>
	public abstract class Disposable : IDisposable
	{
		public bool IsDisposed { get; private set; }

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
