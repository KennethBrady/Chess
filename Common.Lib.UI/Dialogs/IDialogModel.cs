using Common.Lib.UI.MVVM;
using Common.Lib.UI.Settings;

namespace Common.Lib.UI.Dialogs
{
    public interface IDialogModel : IViewModel
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
