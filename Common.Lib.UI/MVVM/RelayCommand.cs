using System.Windows;
using System.Windows.Input;
//using Application = System.Windows.Application;

namespace Common.Lib.UI.MVVM
{
	public class RelayCommand : ICommand
	{
		readonly Action<object?> _execute;
		readonly Predicate<object?>? _canExecute;

		public RelayCommand(Action<object?> execute) : this(execute, null) { }

		public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute)
		{
			_execute = execute;
			_canExecute = canExecute;
		}

		#region ICommand implementation

		public event EventHandler? CanExecuteChanged;

		public bool CanExecute(object? parameter) => (_canExecute == null) ? true : _canExecute(parameter);

		public void Execute(object? parameter) => _execute(parameter);

		#endregion

		public void RaiseCanExecuteChanged()
		{
			void raise()
			{
				CanExecuteChanged?.Invoke(this, EventArgs.Empty);
				CommandManager.InvalidateRequerySuggested();
			}
			if (Application.Current == null)
			{
				// Not relevant except for testing?
				CanExecuteChanged?.Invoke(this, EventArgs.Empty);
				return;
			}
			if (Application.Current.Dispatcher.Thread == Thread.CurrentThread) raise();
			else Application.Current.Dispatcher.Invoke(raise);
		}
	}

	public class RelayCommand<T> : RelayCommand
	{
		public static readonly RelayCommand<T> NoOp = new RelayCommand<T>((t) => { });

		#region ActionWrapper


		/// <summary>
		/// Helper class to convert Action<T?> to Action<object?>
		/// </summary>
		private class ActionWrapper
		{
			public static Action<object?> Convert(Action<T?> action)
			{
				return (obj) =>
				{
					action((T)obj!);	// CS8600 ignored because this code has been working a very long time
				};
			}

			public static Predicate<object?>? Convert(Predicate<T>? predicate) => (obj) => predicate == null ? true : obj is T t && predicate(t);
		}

		#endregion

		public RelayCommand(Action<T?> execute) : this(execute, null) { }

		public RelayCommand(Action<T?> execute, Predicate<T?>? canExecute) :
			base(ActionWrapper.Convert(execute), ActionWrapper.Convert(canExecute))
		{ }
	}
}
