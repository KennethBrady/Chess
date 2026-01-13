using Common.Lib.Contracts;
using System.ComponentModel;

namespace Common.Lib.UI.MVVM
{
	public abstract class ViewModel : INotifyPropertyChanged, IViewModel
	{
		public event PropertyChangedEventHandler? PropertyChanged;

		protected bool AreNotificationsSuspended { get; private set; }
		protected virtual void Notify(params string[] propertyNames)
		{
			if (AreNotificationsSuspended) return;
			if (PropertyChanged != null)
			{
				foreach (var name in propertyNames) PropertyChanged.Invoke(this, new PropertyChangedEventArgs(name));
			}
		}

		IDisposable IViewModel.SuspendNotifications()
		{
			AreNotificationsSuspended = true;
			return new ActionDisposer(() => AreNotificationsSuspended = false);
		}
	}
}
