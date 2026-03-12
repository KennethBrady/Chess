using Common.Lib.UI.MVVM;
using Common.Lib.UI.Windows;

namespace Common.Lib.UI.Dialogs
{
    public interface IDialogModel : IViewModel
	{
	}

	internal interface IDialogModelEx
	{
		void ProcessEscapeKey();
		void HandleCloseButtonClicked();
	}

	public interface IDialogModel<T> : IDialogModel;

	internal interface IDialogModelEx<T> : IDialogModel<T>, IDialogModelEx
	{
		event DialogResultHandler<T>? Closing;
		IAppWindow? Window { get; set; }
	}

	/// <summary>
	/// This interface provides a 'back-door' way to show dialogs without registering a DialogDef.
	/// </summary>
	public interface IDialogTypeSpecifier : IDialogModel
	{
		Type DialogType { get; }
	}
}
