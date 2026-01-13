using System.ComponentModel;

namespace Common.Lib.UI.MVVM
{
	internal interface IViewModel : INotifyPropertyChanged
	{
		/// <summary>
		/// Suspend PropertyChanged Notifications.
		/// </summary>
		/// <returns>A disposable that, after Disposal, re-enables notifications.</returns>
		/// <remarks>
		/// This is used internally to prevent calls to Notify() while Settings are being applied, which may have undesired side-effects.
		/// </remarks>
		IDisposable SuspendNotifications();
	}
}
