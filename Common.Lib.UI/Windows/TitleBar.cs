using Common.Lib.UI;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace Common.Lib.UI.Windows
{
	/// <summary>
	/// Represents a window's title bar, with sizing/dragging capabilities
	/// </summary>
	[TemplatePart(Name = PART_Title, Type = typeof(TextBlock))]
	[TemplatePart(Name = PART_CloseButton, Type = typeof(ButtonBase))]
	[TemplatePart(Name = PART_RestoreButton, Type = typeof(ButtonBase))]
	[TemplatePart(Name = PART_MaximizeButton, Type = typeof(ButtonBase))]
	[TemplatePart(Name = PART_MinimizeButton, Type = typeof(ButtonBase))]
	[TemplatePart(Name = PART_Icon, Type = typeof(Image))]
	[TemplatePart(Name = PART_ContentArea, Type = typeof(ContentControl))]
	public class TitleBar : ControlBase
	{
		[DllImport("user32.dll")]
		static extern uint GetDoubleClickTime();

		internal static readonly TitleBar Default = new();

		protected const string PART_Title = "PART_Title";
		protected const string PART_CloseButton = "PART_CloseButton";
		protected const string PART_MinimizeButton = "PART_MinimizeButton";
		protected const string PART_RestoreButton = "PART_RestoreButton";
		protected const string PART_MaximizeButton = "PART_MaximizeButton";
		protected const string PART_Icon = "PART_Icon";
		protected const string PART_ContentArea = "PART_ContentArea";

		[Flags]
		public enum WindowButtonType
		{
			None = 0x00,
			Close = 0x01,
			Restore = 0x02,
			Minimize = 0x04,
			CloseRestore = Close | Restore,
			CloseMinimize = Close | Minimize,
			RestoreMinimize = Restore | Minimize,
			All = Close | Minimize | Restore
		}

		static TitleBar()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(TitleBar), new FrameworkPropertyMetadata(typeof(TitleBar)));
			// TODO: load default style here. Is it possible?
		}

		public static readonly DependencyProperty ButtonTypesProperty = DependencyProperty.Register("ButtonTypes", typeof(WindowButtonType),
	typeof(TitleBar), new PropertyMetadata(WindowButtonType.All));

		public static readonly DependencyProperty ContentTemplateProperty = DependencyProperty.Register("ContentTemplate", typeof(DataTemplate),
			typeof(TitleBar));

		public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(TitleBar), new PropertyMetadata(String.Empty));

		public static readonly DependencyProperty TitleStyleProperty = DependencyProperty.Register("TitleStyle", typeof(Style),
			typeof(TitleBar), new FrameworkPropertyMetadata(null, null, CoerceTitleStyle));

		public static readonly DependencyProperty MenuProperty = DependencyProperty.Register("Menu", typeof(Menu),
			typeof(TitleBar), new FrameworkPropertyMetadata(null));

		private static object? CoerceTitleStyle(DependencyObject d, object? value) => (value is Style s && s.TargetType == typeof(TextBlock)) ? s : null;

		private Style? _defaultStyle;

		public TitleBar()
		{
			AddHandler(Button.ClickEvent, new RoutedEventHandler(HandleButtonClicked));
		}

		public WindowButtonType ButtonTypes
		{
			get { return (WindowButtonType)GetValue(ButtonTypesProperty); }
			set { SetValue(ButtonTypesProperty, value); }
		}

		public DataTemplate ContentTemplate
		{
			get { return (DataTemplate)GetValue(ContentTemplateProperty); }
			set { SetValue(ContentTemplateProperty, value); }
		}

		public string Title
		{
			get { return (string)GetValue(TitleProperty); }
			set { SetValue(TitleProperty, value); }
		}

		public Style TitleStyle
		{
			get => (Style)GetValue(TitleStyleProperty);
			set { SetValue(TitleStyleProperty, value); }
		}

		public Menu Menu
		{
			get => (Menu)GetValue(MenuProperty);
			set => SetValue(MenuProperty, value);
		}

		private const string StyleKey = "defaultStyle";
		private Style DefaultStyle
		{
			get
			{
				if (_defaultStyle == null)
				{
					if (TryFindResource(StyleKey) is Style style && style.TargetType == typeof(TitleBar))
					{
						_defaultStyle = style;
					}
					else throw new ApplicationException("Missing TitleBar resource: " + StyleKey);
				}
				return _defaultStyle;
			}
		}

		protected override void UseTemplate()
		{
			if (Style == null) Style = DefaultStyle;
			ContentControl cc = (ContentControl)GetTemplateChild(PART_ContentArea);
			cc.DataContext = DataContext;
			if (Menu != null) ApplyMenu();
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			if (IsMouseCaptured) Window.GetWindow(this).DragMove();
		}

		private bool SecondClickOccurred { get; set; }
		protected async override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			base.OnMouseLeftButtonDown(e);
			SecondClickOccurred = (e.ClickCount > 1);

			await Task.Delay((int)GetDoubleClickTime());
			if (SecondClickOccurred || e.ButtonState == MouseButtonState.Released) return;
			CaptureMouse();
			Window.GetWindow(this).DragMove();
		}

		protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
		{
			base.OnMouseLeftButtonUp(e);
			if (IsMouseCaptured) ReleaseMouseCapture();
		}

		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnPropertyChanged(e);
			switch (e.Property.Name)
			{
				case "Menu":	ApplyMenu(); break;
			}
		}

		private void ApplyMenu()
		{
			if (!IsTemplateApplied) return;
			StackPanel mc = (StackPanel)GetTemplateChild("menu");
			mc.Children.Clear();
			if (Menu != null) mc.Children.Add(Menu);
		}


		#region Handle Standard Button-Clicks
		private void HandleButtonClicked(object sender, RoutedEventArgs e)
		{
			ButtonBase? b = e.OriginalSource as ButtonBase;
			if (b != null)
			{
				switch (b.Name)
				{
					case PART_CloseButton:
						Close();
						break;
					case PART_MinimizeButton:
						Minimize();
						break;
					case PART_MaximizeButton:
						Maximize();
						break;
					case PART_RestoreButton:
						Restore();
						break;
				}
			}
		}

		protected virtual void Close()
		{
			Window w = Window.GetWindow(this);
			AppWindow? bw = w as AppWindow;
			if (w != null) w.Close();
		}

		protected virtual void Maximize()
		{
			Window w = Window.GetWindow(this);
			if (w != null) w.WindowState = WindowState.Maximized;
		}

		protected virtual void Minimize()
		{
			Window w = Window.GetWindow(this);
			if (w != null) w.WindowState = WindowState.Minimized;
		}

		protected virtual void Restore()
		{
			Window w = Window.GetWindow(this);
			if (w != null) w.WindowState = WindowState.Normal;
		}

		protected override void OnPreviewMouseDoubleClick(MouseButtonEventArgs e)
		{
			base.OnPreviewMouseDoubleClick(e);
			if (ButtonTypes.HasFlag(WindowButtonType.Restore))
			{
				Window w = Window.GetWindow(this);
				switch (w.WindowState)
				{
					case WindowState.Normal: Maximize(); break;
					case WindowState.Maximized: Restore(); break;
				}
				e.Handled = true;
			}
		}

		#endregion
	}

	#region Converters

	internal class WindowButtonTypeToVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (!(value is TitleBar.WindowButtonType)) return value;
			if (!(parameter is string)) return value;
			TitleBar.WindowButtonType wbt = (TitleBar.WindowButtonType)value;
			string p = (string)parameter;
			switch (p)
			{
				case "close": return wbt.HasFlag(TitleBar.WindowButtonType.Close) ? Visibility.Visible : Visibility.Collapsed;
				case "minimize": return wbt.HasFlag(TitleBar.WindowButtonType.Minimize) ? Visibility.Visible : Visibility.Collapsed;
				case "restore": return wbt.HasFlag(TitleBar.WindowButtonType.Restore) ? Visibility.Visible : Visibility.Collapsed;
			}

			throw new NotImplementedException();
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	internal class WindowStateButtonTypeToVisibilityConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (!(values[0] is TitleBar.WindowButtonType bt) || !(values[1] is WindowState ws)) return Visibility.Collapsed;
			if (parameter is not string pram) return Visibility.Collapsed;
			if (!bt.HasFlag(TitleBar.WindowButtonType.Restore)) return Visibility.Collapsed;
			switch (pram)
			{
				case "maximize":
					return (ws == WindowState.Normal) ? Visibility.Visible : Visibility.Collapsed;
				case "restore":
					return (ws == WindowState.Maximized) ? Visibility.Visible : Visibility.Collapsed;
			}
			return Visibility.Collapsed;
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	internal class TitleBarTitleConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (values.Length == 2 && values[0] is string wTitle && values[1] is string tbTitle)
			{
				if (!String.IsNullOrEmpty(wTitle)) return wTitle;
				return tbTitle;
			}
			return String.Empty;
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	#endregion
}
