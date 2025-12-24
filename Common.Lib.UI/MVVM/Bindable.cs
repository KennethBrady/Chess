using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Common.Lib.UI.MVVM
{
	public abstract class Bindable : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler? PropertyChanged;
		protected virtual void OnChanged(params string[] propertyNames)
		{
			if (PropertyChanged != null)
			{
				foreach (var name in propertyNames) PropertyChanged.Invoke(this, new PropertyChangedEventArgs(name));
			}
		}
	}
}
