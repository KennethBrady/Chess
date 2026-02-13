using Common.Lib.UI.Adorners;
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
		public static TimeSpan DefaultAnimationDuration { get; set; } = TimeSpan.FromSeconds(0.70);
		public static double DefaultOpacityReduction { get; set; } = 0.5;

		#region Static Interface

		static DialogView()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(DialogView), new FrameworkPropertyMetadata(typeof(DialogView)));
		}

		public static readonly DependencyProperty ModelTypeProperty = DependencyProperty.Register("ModelType", typeof(Type),
			typeof(DialogView), new PropertyMetadata(null));

		public static readonly DependencyProperty PlacementProperty = DependencyProperty.Register("Placement", typeof(DialogPlacement),
			typeof(DialogView), new PropertyMetadata(DialogPlacement.Center));

		public static readonly DependencyProperty PlacementOffsetProperty = DependencyProperty.Register("PlacementOffset", typeof(Size),
			typeof(DialogView), new PropertyMetadata(Size.Empty));

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

		public static DependencyProperty AnimationDurationProperty = DependencyProperty.Register("AnimationDuration", typeof(double),
			typeof(DialogView), new PropertyMetadata(DefaultAnimationDuration.TotalSeconds));

		public static readonly DependencyProperty CloseIndicatorProperty = DependencyProperty.Register("CloseIndicator", typeof(CloseIndicatorType),
			typeof(DialogView), new PropertyMetadata(CloseIndicatorType.ReducedOpacity));

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

		public Size PlacementOffset
		{
			get => (Size)GetValue(PlacementOffsetProperty);
			set => SetValue(PlacementOffsetProperty, value);
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

		public double AnimationDuration
		{
			get => (double)GetValue(AnimationDurationProperty);
			set => SetValue(AnimationDurationProperty, value);
		}

		public CloseIndicatorType CloseIndicator
		{
			get => (CloseIndicatorType)GetValue(CloseIndicatorProperty);
			set => SetValue(CloseIndicatorProperty, value);
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
			Loaded += (o, e) => OnLoaded();
		}
		private DialogViewDragger Dragger { get; init; }
		private Button Closer { get; set; } = DefaultControls.Button;
		private Button Restorer { get; set; } = DefaultControls.Button;

		private ResizeAdorner? ResizeAdorner { get; set; }

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			Closer = (Button)GetTemplateChild("closer");
			Restorer = (Button)GetTemplateChild("restorer");
			Closer.Click += Closer_Click;
			Restorer.Click += Restorer_Click;
			SizeChanged += DialogView_SizeChanged;
			Visibility = Visibility.Hidden;
		}

		private void DialogView_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			SizeChanged -= DialogView_SizeChanged;
			InitialSize = e.NewSize;
			SetInitialPosition(false);
		}

		private void Closer_Click(object sender, RoutedEventArgs e)
		{
			if (DataContext is IDialogModel idm) idm.Close();
		}

		//TODO: What does "Restore" mean - occupy full window, return to a preset size?
		private void Restorer_Click(object sender, RoutedEventArgs e)
		{
			Width = InitialSize.Width;
			Height = InitialSize.Height;
			Dragger.StartPoint = StartPosition;
			Canvas.SetLeft(this, StartPosition.X);
			Canvas.SetTop(this, StartPosition.Y);
		}

		private Point StartPosition { get; set; }
		private Size InitialSize { get; set; } = Size.Empty;

		/// <summary>
		/// Calculate the TopLeft point for initial positioning.
		/// </summary>
		/// <returns>
		/// A point which determines the Dialogs's initial position (via Canvas attached properties)
		/// and the origin for Drag operations.
		/// </returns>
		/// <remarks>
		/// The base method uses the Placement, CustomPlacement, and PlacementOffset properties to calculate the position.
		/// </remarks>
		protected virtual Point CalculateInitialPosition()
		{
			if (Parent is not DialogLayer dl || InitialSize.IsEmpty) return new Point(0, 0);
			Point center = new Point(dl.ActualWidth / 2, dl.ActualHeight / 2);
			double ho2 = InitialSize.Height / 2, wo2 = InitialSize.Width / 2, x = 0, y = 0;
			switch (Placement)
			{
				case DialogPlacement.TopLeft: break;
				case DialogPlacement.CenterLeft: y = center.Y - ho2; break;
				case DialogPlacement.BottomLeft: y = dl.ActualHeight - InitialSize.Height; break;
				case DialogPlacement.TopCenter: x = center.X - wo2; break;
				case DialogPlacement.Center: x = center.X - wo2; y = center.Y - ho2; break;
				case DialogPlacement.BottomCenter: x = center.X - wo2; y = dl.ActualHeight - InitialSize.Height; break;
				case DialogPlacement.TopRight: x = dl.ActualWidth - InitialSize.Width; break;
				case DialogPlacement.CenterRight: x = dl.ActualWidth - InitialSize.Width; y = center.Y - ho2; break;
				case DialogPlacement.BottomRight: x = dl.ActualWidth - InitialSize.Width; y = dl.ActualHeight - InitialSize.Height; break;
				case DialogPlacement.Custom: x = CustomPlacement.X; y = CustomPlacement.Y; break;
			}
			if (!PlacementOffset.IsEmpty)
			{
				x += PlacementOffset.Width;
				y += PlacementOffset.Height;
			}
			return new Point(x, y);
		}

		private void SetInitialPosition(bool isReset)
		{
			Point canvasPosition = CalculateInitialPosition();
			Dragger.StartPoint = canvasPosition;
			Canvas.SetLeft(this, canvasPosition.X);
			Canvas.SetTop(this, canvasPosition.Y);
			if (!isReset)
			{
				StartPosition = canvasPosition;
				if (IsResizable && ResizeAdorner == null)
				{
					AdornerLayer.GetAdornerLayer(this)?.Add(ResizeAdorner = new ResizeAdorner(this));
				}
				if (Animation == AnimationType.None) Visibility = Visibility.Visible; else DialogAnimator.BeginOpenAnimation(this);
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

		protected virtual void OnLoaded() { }

		internal void ApplyCloseIndicator()
		{
			switch (CloseIndicator)
			{
				case CloseIndicatorType.ReducedOpacity: Opacity = Math.Min(1.0, Math.Max(0.0, DefaultOpacityReduction)); break;
			}
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
