using Common.Lib.UI.Dialogs;
using Common.Lib.UI.MVVM;

namespace Common.Lib.UI.Windows
{
    public interface IAppWindow : IWindow
	{
		Task<IDialogResult<T>> ShowDialog<T>(IDialogModel<T> dialogContext);
	}
}
