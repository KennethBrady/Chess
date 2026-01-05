using Common.Lib.UI.MVVM;
using System.Windows.Input;

namespace Common.Lib.UI.MVVM
{
	public abstract class CommandModel<T> : ViewModel
	{
		private RelayCommand<T> _cmd;

		protected CommandModel()
		{
			_cmd = new RelayCommand<T>(Execute, CanExecute);
		}

		public ICommand Command => _cmd;

		protected virtual bool CanExecute(T? parameter) => false;

		protected abstract void Execute(T? parameter);

		protected void RaiseCanExecuteChanged() => _cmd.RaiseCanExecuteChanged();
	}
}

public abstract class CommandModel : CommandModel<string> { }
