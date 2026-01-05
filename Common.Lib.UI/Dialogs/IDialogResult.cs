namespace Common.Lib.UI.Dialogs
{
	public interface IDialogModel<T>;

	internal interface IDialogModelEx<T> : IDialogModel<T>
	{
		Action<IDialogResult<T>> Closing { get; set; }
	}

	internal interface ICloseable
	{
		void Close();
	}

	public interface IDialogResult<T>
	{
		bool Accepted { get; }
	}

	public interface IDialogResultAccepted<T> : IDialogResult<T>
	{
		T Value { get; }
	}

	public interface IDialogResultCancelled<T> : IDialogResult<T>
	{
		string Reason { get; }
	}

	internal record struct DialogResultSuccess<T>(T Value, bool Accepted = true) : IDialogResultAccepted<T>;
	internal record struct DialogResultFailure<T>(string Reason, bool Accepted = false) : IDialogResultCancelled<T>;

	public record struct DialogResult<T>(bool Accepted, T? Result)
	{
		internal static readonly DialogResult<T> Empty = new DialogResult<T>(false, default);
	}
}
