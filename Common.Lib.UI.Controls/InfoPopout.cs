using Common.Lib.UI.Images;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace Common.Lib.UI.Controls
{
	public class InfoPopout : ContentControl
	{
		static InfoPopout()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(InfoPopout), new FrameworkPropertyMetadata(typeof(InfoPopout)));
		}

		public static readonly DependencyProperty IsPopoutOpenProperty = DependencyProperty.Register("IsPopoutOpen", typeof(bool),
			typeof(InfoPopout), new PropertyMetadata(false));

		public static DependencyProperty PopoutPlacementProperty = DependencyProperty.Register("PopoutPlacement", typeof(PlacementMode),
			typeof(InfoPopout), new PropertyMetadata(PlacementMode.Bottom));

		public static DependencyProperty HelpImageProperty = DependencyProperty.Register("HelpImage", typeof(ImageSource),
			typeof(InfoPopout), new PropertyMetadata(null));


		public bool IsPopoutOpen
		{
			get => (bool)GetValue(IsPopoutOpenProperty);
			set => SetValue(IsPopoutOpenProperty, value);
		}

		public PlacementMode PopoutPlacement
		{
			get => (PlacementMode)GetValue(PopoutPlacementProperty);
			set => SetValue(PopoutPlacementProperty, value);
		}

		public ImageSource HelpImage
		{
			get => (ImageSource)GetValue(HelpImageProperty);
			set => SetValue(HelpImageProperty, value);
		}

		public InfoPopout()
		{
			Loaded += InfoPopout_Loaded;
			Unloaded += InfoPopout_Unloaded;
		}

		private void InfoPopout_Unloaded(object sender, RoutedEventArgs e)
		{
			if (Window != null) Window.PreviewKeyDown -= Window_PreviewKeyDown;
		}

		private Window? Window { get; set; }
		private void InfoPopout_Loaded(object sender, RoutedEventArgs e)
		{
			Window = System.Windows.Window.GetWindow(this);
			if (Window != null) Window.PreviewKeyDown += Window_PreviewKeyDown;
		}

		private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (IsPopoutOpen)
			{
				IsPopoutOpen = false;
				e.Handled = true;
			}
		}
	}

	internal class HelpImageConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is ImageSource image) return value;
			return ImageProvider.Load(ImageType.Help_Open);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
