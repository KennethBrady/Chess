using Common.Lib.Contracts;

namespace Common.Lib.UI.Dialogs
{
	public abstract class DialogModel<T> : CommandModel, IDialogModelEx<T>, ICloseable
	{
		public const string CancelParameter = "cancel";
		public const string OKParameter = "ok";
		protected const string OperationCancelled = "Operation Cancelled";

		Action<IDialogResult<T>> IDialogModelEx<T>.Closing { get; set; } = Actions<IDialogResult<T>>.Empty;

		private IDialogModelEx<T> Me => (IDialogModelEx<T>)this;

		protected void Cancel(string reason = OperationCancelled) => Me.Closing?.Invoke(new DialogResultFailure<T>(reason));

		protected void Accept(T acceptedValue) => Me.Closing?.Invoke(new DialogResultSuccess<T>(acceptedValue));

		void ICloseable.Close() => Cancel(OperationCancelled);
		
	}
}
