using Common.Lib.UI.Dialogs;
using Common.Lib.UI.Windows;

namespace Common.Lib.UI.MVVM
{
	public abstract class AppWindowModel : CommandModel
	{
		protected AppWindowModel(IAppWindow window)
		{
			Window = window;
			Window.Activated += (_, _) => OnWindowActivated();
		}

		public IAppWindow Window { get; private init; }

		public Task<IDialogResult<T>> ShowDialog<T>(IDialogModel<T> dialogContext)
		{
			return Window.ShowDialog(dialogContext);
		}

		protected virtual void OnWindowActivated() { }


	}
}