using System.ComponentModel;

namespace Common.Lib.MVVM
{
	public abstract class ViewModel : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler? PropertyChanged;

		protected void Notify(params string[] names)
		{
			if (PropertyChanged != null)
			{
				foreach(var name in names) PropertyChanged(this, new PropertyChangedEventArgs(name));
			}
		}
	}
}
