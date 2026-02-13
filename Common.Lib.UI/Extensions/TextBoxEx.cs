using Common.Lib.UI.Adorners;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
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

		#region SelectAllOnFocus Implementation

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

		#endregion

		#region Watermark

		public static DependencyProperty WaterMarkTextProperty = DependencyProperty.RegisterAttached("WaterMarkText", typeof(string),
	typeof(TextBoxEx), new FrameworkPropertyMetadata(string.Empty, HandleWatermarkChanged));

		private static void HandleWatermarkChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			switch (e.Property.Name)
			{
				case "WaterMarkText": SetWaterMarkText(d, (string)e.NewValue); break;
			}
		}

		public static string GetWaterMarkText(DependencyObject obj)
		{
			if (obj is TextBox tb)
			{
				object r = tb.GetValue(WaterMarkTextProperty);
				if (r is string s) return s;
			}
			return string.Empty;
		}

		public static void SetWaterMarkText(DependencyObject target, string watermarkText)
		{
			if (target is TextBox textBox)
			{
				if (string.IsNullOrEmpty(watermarkText))
				{
					textBox.ClearValue(WaterMarkTextProperty);
					Attach(textBox, false);
				}
				else
				{
					textBox.SetValue(WaterMarkTextProperty, watermarkText);
					if (textBox.IsLoaded)
					{
						Attach(textBox, true);
						ApplyText(textBox);
					}
					else textBox.Loaded += (_, _) =>
					{
						Attach(textBox, true);
						ApplyText(textBox);
					};
				}
			}
		}

		private static void Attach(TextBox textBox, bool attach)
		{
			if (attach) textBox.TextChanged += TextBox_TextChanged; else textBox.TextChanged -= TextBox_TextChanged;
		}

		private static void ApplyText(TextBox textBox)
		{
			var layer = AdornerLayer.GetAdornerLayer(textBox);
			if (layer == null) return;
			WaterMarkAdorner? getWMA()
			{
				Adorner[] adorners = layer.GetAdorners(textBox);
				if (adorners != null) return adorners.FirstOrDefault(a => a is WaterMarkAdorner) as WaterMarkAdorner;
				return null;
			}
			bool hasText = !string.IsNullOrEmpty(textBox.Text);
			WaterMarkAdorner? wma = getWMA();
			if (wma == null)
			{
				if (hasText) return;
				wma = new WaterMarkAdorner(textBox, GetWaterMarkText(textBox));
				layer.Add(wma);
			}
			else
			{
				if (hasText) layer.Remove(wma);
			}
		}

		private static void TextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (sender is TextBox textBox) ApplyText(textBox);
		}

		#endregion
	}
}
