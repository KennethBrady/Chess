using Common.Lib.Contracts;
using Common.Lib.Extensions;
using Common.Lib.UI.Settings;
using System.ComponentModel;
using System.Drawing.Text;
using System.Reflection;
using System.Windows.Input;

namespace Common.Lib.UI.MVVM
{
	public abstract class ViewModel : IViewModel
	{
		protected virtual void Notify(params string[] propertyNames)
		{
			if (AreNotificationsSuspended) return;
			if (PropertyChanged != null)
			{
				foreach (var name in propertyNames) PropertyChanged.Invoke(this, new PropertyChangedEventArgs(name));
			}
			CommandManager.InvalidateRequerySuggested();
		}

		#region IViewModel Implementation

		public event PropertyChangedEventHandler? PropertyChanged;
		protected bool AreNotificationsSuspended { get; private set; }
		IDisposable IViewModel.SuspendNotifications()
		{
			AreNotificationsSuspended = true;
			return new ActionDisposer(() => AreNotificationsSuspended = false);
		}

		#endregion
	}
}
