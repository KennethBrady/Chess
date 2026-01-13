using System.ComponentModel;

namespace Common.Lib.UI.Dialogs
{
    public interface IDialogModel : INotifyPropertyChanged
	{
		/// <summary>
		/// Close the associated dialog with a default (cancel) value
		/// </summary>
		void Close(string reason = "");
	}

	internal interface IDialogModelEx;

	public interface IDialogModel<T> : IDialogModel;

	internal interface IDialogModelEx<T> : IDialogModel<T>, IDialogModelEx
	{
		event DialogResultHandler<T>? Closing;
	}
}
