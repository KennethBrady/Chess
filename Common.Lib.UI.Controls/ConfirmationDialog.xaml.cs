using Common.Lib.UI.Dialogs;
using System.Windows;
using System.Windows.Controls;

namespace Common.Lib.UI.Controls
{
	/// <summary>
	/// Interaction logic for ConfirmationDialog.xaml
	/// </summary>
	public partial class ConfirmationDialog : DialogView
	{
		public static readonly DependencyProperty MessageStyleProperty = DependencyProperty.Register("MessageStyle", typeof(Style),
			typeof(ConfirmationDialog), new PropertyMetadata(null, null, CoerceMessageStyle));

		private static object? CoerceMessageStyle(DependencyObject owner, object value)
		{
			if (value is Style s)
			{
				return s.TargetType == typeof(TextBlock) ? s : null;
			}
			return null;
		}

		public Style? MessageStyle
		{
			get => (Style)GetValue(MessageStyleProperty);
			set => SetValue(MessageStyleProperty, value);
		}

		public ConfirmationDialog()
		{
			InitializeComponent();
		}
	}
}
