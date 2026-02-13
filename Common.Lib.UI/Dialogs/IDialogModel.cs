using Common.Lib.UI.MVVM;

namespace Common.Lib.UI.Dialogs
{
    public interface IDialogModel : IViewModel
	{
		/// <summary>
		/// Close the associated dialog with a default (cancel) value
		/// </summary>
		void Close(string reason = "");
	}

	internal interface IDialogModelEx
	{
		void ProcessEscapeKey();
	}

	public interface IDialogModel<T> : IDialogModel;

	internal interface IDialogModelEx<T> : IDialogModel<T>, IDialogModelEx
	{
		event DialogResultHandler<T>? Closing;
	}

	/// <summary>
	/// This interface provides a 'back-door' way to show dialogs without registering a DialogDef.
	/// </summary>
	public interface IDialogTypeSpecifier : IDialogModel
	{
		Type DialogType { get; }
	}
}
