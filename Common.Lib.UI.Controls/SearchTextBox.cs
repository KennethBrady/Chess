using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Common.Lib.UI.Controls
{
	public class SearchTextBox : Control
	{
		static SearchTextBox()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(SearchTextBox), new FrameworkPropertyMetadata(typeof(SearchTextBox)));
		}

		public static readonly DependencyProperty WatermarkTextProperty = DependencyProperty.Register("WatermarkText", typeof(string), typeof(SearchTextBox), new PropertyMetadata(string.Empty));
		public static readonly DependencyProperty TextProperty = TextBox.TextProperty.AddOwner(typeof(SearchTextBox),
			new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

		public static readonly DependencyProperty UpdateBindingOnEnterProperty = DependencyProperty.Register("UpdateBindingOnEnter",
			typeof(bool), typeof(SearchTextBox), new PropertyMetadata(false));

		public static readonly RoutedEvent TextChangedEvent = TextBox.TextChangedEvent.AddOwner(typeof(SearchTextBox));

		public string Text
		{
			get => (string)GetValue(TextProperty);
			set => SetValue(TextProperty, value);
		}

		public string WatermarkText
		{
			get => (string)GetValue(WatermarkTextProperty);
			set => SetValue(WatermarkTextProperty, value);
		}

		public event TextChangedEventHandler TextChanged
		{
			add => AddHandler(TextChangedEvent, value);
			remove => RemoveHandler(TextChangedEvent, value);
		}

		public bool UpdateBindingOnEnter
		{
			get => (bool)GetValue(UpdateBindingOnEnterProperty);
			set => SetValue(UpdateBindingOnEnterProperty, value);
		}

		private TextBox _text = DefaultControls.TextBox;
		private Button _clear = DefaultControls.Button;
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			_text = (TextBox)GetTemplateChild("text");
			_text.TextChanged += _text_TextChanged;
			_clear = (Button)GetTemplateChild("clear");
			_clear.Click += Clear_Click;
		}

		public void SelectAll()
		{
			if (_text != null)
			{
				Focus();
				_text.Focus();
				_text.SelectAll();
			}
		}

		private async void Clear_Click(object sender, RoutedEventArgs e)
		{
			SetCurrentValue(TextProperty, string.Empty);
			await Task.Delay(50);
			_text.Focus();
		}

		private void _text_TextChanged(object sender, TextChangedEventArgs e)
		{
			_clear.IsEnabled = !string.IsNullOrEmpty(_text.Text);
			RaiseEvent(new TextChangedEventArgs(TextChangedEvent, e.UndoAction, e.Changes));
		}

		protected override void OnGotFocus(RoutedEventArgs e)
		{
			base.OnGotFocus(e);
			_text?.Focus();
		}
	}

	internal class TextToEnabledConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return (value is string s) ? string.IsNullOrEmpty(s) ? false : true : false;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

}
