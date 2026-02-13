using System.Windows;
using System.Windows.Controls;

namespace Common.Lib.UI
{
	public class UserControlBase : UserControl
	{
		protected UserControlBase()
		{
			Loaded += HandleLoaded;
			Unloaded += HandleUnloaded;
			SizeChanged += HandleSizeChanged;
		}


		protected virtual void OnLoaded() { }

		protected virtual void OnUnloaded() { }

		protected virtual void OnSizeChanged(SizeChangedEventArgs e) { }

		protected virtual void OnDataContextChanged(object oldValue, object newValue) { }

		private void HandleLoaded(object sender, EventArgs e) => OnLoaded();


		private void HandleUnloaded(object sender, EventArgs e) => OnUnloaded();

		private void HandleSizeChanged(object sender, SizeChangedEventArgs e) => OnSizeChanged(e);

		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnPropertyChanged(e);
			if (e.Property.Name == "DataContext") OnDataContextChanged(e.OldValue, e.NewValue);
		}
	}
}
