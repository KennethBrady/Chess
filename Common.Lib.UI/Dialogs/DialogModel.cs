namespace Common.Lib.UI.Dialogs
{
	public delegate void DialogResultHandler<T>(IDialogResult<T> result);

	public abstract class DialogModel<T> : CommandModel, IDialogModelEx<T>
	{
		public const string CancelParameter = "cancel";
		public const string OKParameter = "ok";
		protected const string OperationCancelled = "Operation Cancelled";

		private List<DialogResultHandler<T>> _resultHandlers = new();

		public IDialogResult<T>? FinalResult { get; private set; } = null;

		protected void Cancel(string reason = OperationCancelled) => OnClosed(new DialogResultFailure<T>(reason));

		protected void Accept(T acceptedValue) => OnClosed(new DialogResultSuccess<T>(acceptedValue));

		protected virtual void HandleEscapeKey() => Cancel();

		protected bool HasClosed { get; private set; }

		protected virtual void OnClosed(IDialogResult<T> result)
		{
			if (HasClosed) return;	// prevent calling twice.
			HasClosed = true;
			FinalResult = result;
			foreach (var handler in _resultHandlers) handler(result);
		}

		void IDialogModel.Close(string reason) => Cancel(string.IsNullOrEmpty(reason) ? OperationCancelled : reason);

		event DialogResultHandler<T>? IDialogModelEx<T>.Closing
		{
			add
			{
				if (value != null) _resultHandlers.Add(value);
			}
			remove
			{
				if (value != null) _resultHandlers.Remove(value);
			}
		}

		protected override void Notify(params string[] propertyNames)
		{
			base.Notify(propertyNames);
			RaiseCanExecuteChanged();
		}

		void IDialogModelEx.ProcessEscapeKey() => HandleEscapeKey();
		
	}
}
