using Common.Lib.UI.Adorners;
using Common.Lib.UI.Animations;
using Common.Lib.UI.DragDrop;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Common.Lib.UI.Dialogs
{
	#region Enums

	public enum DialogPlacement
	{
		Center, TopLeft, TopCenter, TopRight, BottomLeft, BottomRight, BottomCenter, CenterLeft, CenterRight, Custom
	}

	[Flags]
	public enum DialogButtons
	{
		None = 0x00,
		Close = 0x01,
		Restore = 0x02,
		All = Close | Restore
	}


	#endregion

	public class DialogView : HeaderedContentControl
	{
		public static Duration AnimationDuration { get; set; } = new Duration(TimeSpan.FromSeconds(0.5));

		#region Static Interface

		static DialogView()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(DialogView), new FrameworkPropertyMetadata(typeof(DialogView)));
		}

		public static readonly DependencyProperty ModelTypeProperty = DependencyProperty.Register("ModelType", typeof(Type),
			typeof(DialogView), new PropertyMetadata(null));

		public static readonly DependencyProperty PlacementProperty = DependencyProperty.Register("Placement", typeof(DialogPlacement),
			typeof(DialogView), new PropertyMetadata(DialogPlacement.Center));

		public static readonly DependencyProperty CustomPlacementProperty = DependencyProperty.Register("CustomPlacement", typeof(Point),
			typeof(DialogView), new PropertyMetadata(new Point()));

		public static readonly DependencyProperty TitleVisibilityProperty = DependencyProperty.Register("TitleVisibility", typeof(Visibility),
			typeof(DialogView), new PropertyMetadata(Visibility.Visible));

		public static readonly DependencyProperty TitleBackgroundProperty = DependencyProperty.Register("TitleBackground", typeof(Brush),
			typeof(DialogView), new PropertyMetadata(Brushes.Black));

		public static readonly DependencyProperty TitleForegroundProperty = DependencyProperty.Register("TitleForeground", typeof(Brush),
			typeof(DialogView), new PropertyMetadata(Brushes.White));

		public static readonly DependencyProperty DialogButtonsProperty = DependencyProperty.Register("DialogButtons", typeof(DialogButtons),
			typeof(DialogView), new PropertyMetadata(DialogButtons.None));

		public static readonly DependencyProperty AnimationProperty = DependencyProperty.Register("Animation", typeof(AnimationType),
			typeof(DialogView), new PropertyMetadata(AnimationType.None));

		public static readonly DependencyProperty IsModalProperty = DependencyProperty.Register("IsModal", typeof(bool),
			typeof(DialogView), new PropertyMetadata(true));

		public static readonly DependencyProperty IsResizableProperty = DependencyProperty.Register("IsResizable", typeof(bool),
			typeof(DialogView), new PropertyMetadata(true));

		#endregion

		#region Dependency Properties

		public Type ModelType
		{
			get => (Type)GetValue(ModelTypeProperty);
			set => SetValue(ModelTypeProperty, value);
		}

		public DialogButtons DialogButtons
		{
			get => (DialogButtons)GetValue(DialogButtonsProperty);
			set => SetValue(DialogButtonsProperty, value);
		}

		public DialogPlacement Placement
		{
			get => (DialogPlacement)GetValue(PlacementProperty);
			set => SetValue(PlacementProperty, value);
		}

		public Point CustomPlacement
		{
			get => (Point)GetValue(CustomPlacementProperty);
			set => SetValue(CustomPlacementProperty, value);
		}

		public Visibility TitleVisibility
		{
			get => (Visibility)GetValue(TitleVisibilityProperty);
			set => SetValue(TitleVisibilityProperty, value);
		}

		public Brush TitleBackground
		{
			get => (Brush)GetValue(TitleBackgroundProperty);
			set => SetValue(TitleBackgroundProperty, value);
		}

		public Brush TitleForeground
		{
			get => (Brush)GetValue(TitleForegroundProperty);
			set => SetValue(TitleForegroundProperty, value);
		}

		public AnimationType Animation
		{
			get => (AnimationType)GetValue(AnimationProperty);
			set => SetValue(AnimationProperty, value);
		}

		public bool IsModal
		{
			get => (bool)GetValue(IsModalProperty);
			set => SetValue(IsModalProperty, value);
		}

		public bool IsResizable
		{
			get => (bool)GetValue(IsResizableProperty);
			set => SetValue(IsResizableProperty, value);
		}

		#endregion

		public DialogView()
		{
			Dragger = new DialogViewDragger(this);
			ElementDragger.SetAllowDrag(this, Dragger);
		}
		private DialogViewDragger Dragger { get; init; }
		private Button Closer { get; set; } = DefaultControls.Button;
		private Button Restorer { get; set; } = DefaultControls.Button;
		private Point StartPosition { get; set; }

		private ResizeAdorner? ResizeAdorner { get; set; }

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			Closer = (Button)GetTemplateChild("closer");
			Restorer = (Button)GetTemplateChild("restorer");
			Closer.Click += Closer_Click;
			Restorer.Click += Restorer_Click;
		}

		private void Closer_Click(object sender, RoutedEventArgs e)
		{
			if (DataContext is ICloseable ic) ic.Close();
		}

		//TODO: What does "Restore" mean - occupy full window, return to a preset size?
		private void Restorer_Click(object sender, RoutedEventArgs e)
		{
			SetInitialPosition(StartPosition);	// for now, just return to start pos
		}

		protected internal virtual void Show(Point canvasPosition)
		{
			Dragger.StartPoint = canvasPosition;
		}

		internal void SetInitialPosition(Point canvasPosition)
		{
			StartPosition = canvasPosition;
			Dragger.StartPoint = canvasPosition;
			Canvas.SetLeft(this, canvasPosition.X);
			Canvas.SetTop(this, canvasPosition.Y);
			if (IsResizable && ResizeAdorner == null)
			{
				AdornerLayer.GetAdornerLayer(this)?.Add(ResizeAdorner = new ResizeAdorner(this));
			}
		}

		protected virtual void OnDataContextChanged(object? oldValue, object? newValue) { }

		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnPropertyChanged(e);
			if (e.Property.Name == nameof(DataContext)) OnDataContextChanged(e.OldValue, e.NewValue);
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);
		}

		#region DialogViewDragger

		private class DialogViewDragger : ElementDragHelper
		{
			internal DialogViewDragger(DialogView view)
			{
				View = view;
			}

			protected override void InitDrag(Point downPoint) { }

			private DialogView View { get; init; }
			internal Point StartPoint { get; set; }

			protected override void ApplyDragOffset(Point lastDelta)
			{
				double nux = Math.Max(0, StartPoint.X + Offset.X), nuy = Math.Max(0, StartPoint.Y + Offset.Y);
				Canvas.SetLeft(View, nux); Canvas.SetTop(View, nuy);
			}

			protected override void EndDrag()
			{
				StartPoint = new Point(Canvas.GetLeft(View), Canvas.GetTop(View));
			}
		}

		#endregion
	}

	#region Converters

	internal class DialogButtonTypeToVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is DialogButtons b && parameter is string s)
			{
				switch (s)
				{
					case "Close": return b.HasFlag(DialogButtons.Close) ? Visibility.Visible : Visibility.Collapsed;
					case "Restore": return b.HasFlag(DialogButtons.Restore) ? Visibility.Visible : Visibility.Collapsed;
				}
			}
			return Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	#endregion
}
