using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;

namespace Common.Lib.UI.Extensions
{
	public static class TextBoxEx
	{
		#region UpdateBindingOnEnter Implementation

		public static readonly DependencyProperty UpdateBindingOnEnterProperty = DependencyProperty.RegisterAttached("UpdateBindingOnEnter",
			typeof(bool), typeof(TextBoxEx), new PropertyMetadata(false, HandleUpdateBindingOnEnterChanged));

		public static bool GetUpdateBindingOnEnter(TextBox textBox) => (bool)textBox.GetValue(UpdateBindingOnEnterProperty);
		public static void SetUpdateBindingOnEnter(TextBox textBox, bool value) => textBox.SetValue(UpdateBindingOnEnterProperty, value);


		private static void HandleUpdateBindingOnEnterChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
		{
			if (o is TextBox tb)
			{
				if (e.NewValue is bool b && b) tb.PreviewKeyDown += TextBox_PreviewKeyDown; else tb.PreviewKeyDown -= TextBox_PreviewKeyDown;
			}
		}

		private static void TextBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if (e.Key == System.Windows.Input.Key.Enter)
			{
				BindingExpression bexp = BindingOperations.GetBindingExpression(sender as TextBox, TextBox.TextProperty);
				if (bexp != null)
				{
					bexp.UpdateSource();
					e.Handled = true;
				}
			}
		}

		#endregion

		public static readonly DependencyProperty SelectAllOnFocusProperty = DependencyProperty.RegisterAttached("SelectAllOnFocus",
	typeof(bool), typeof(TextBoxEx), new FrameworkPropertyMetadata(false, HandleSelectAllOnFocusChanged));

		private static void HandleSelectAllOnFocusChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
		{
			if (o is not TextBox tb) return;
			if (e.NewValue is bool b) tb.GotFocus += TextBox_GotFocus; else tb.GotFocus -= TextBox_GotFocus;
		}

		public static bool GetSelectAllOnFocus(DependencyObject o) => (bool)o.GetValue(SelectAllOnFocusProperty);

		public static void SetSelectAllOnFocus(TextBox o, bool selectAllOnFocus) => o.SetValue(SelectAllOnFocusProperty, selectAllOnFocus);

		private static void TextBox_GotFocus(object sender, RoutedEventArgs e)
		{
			TextBox tb = (TextBox)sender;
			bool saof = GetSelectAllOnFocus(tb);
			if (saof) RunDelayed(() => tb.SelectAll());
		}

		private static void RunDelayed(Action a, int msDelay = 50)
		{
			DispatcherTimer t = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(msDelay) };
			t.Tick += (o, e) =>
			{
				t.Stop();
				a();
			};
			t.Start();
		}



	}
}
