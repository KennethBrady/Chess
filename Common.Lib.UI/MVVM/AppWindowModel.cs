using Common.Lib.UI.Dialogs;

namespace Common.Lib.UI.MVVM
{
	public abstract class AppWindowModel : CommandModel
	{
		protected AppWindowModel(IAppWindow window)
		{
			Window = window;		
		}

		public IAppWindow Window { get; private init; }


		public Task<IDialogResult<T>> ShowDialog<T>(IDialogModel<T> dialogContext)
		{
			return Window.ShowDialog(dialogContext);
		}
	}
}
