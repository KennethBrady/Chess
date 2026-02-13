using Common.Lib.UI.MVVM;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Common.Lib.UI.Controls
{
	public class RatingChangedEventArgs : RoutedEventArgs
	{
		internal RatingChangedEventArgs(RatingsView source, int oldRating, int newRating) : base(RatingsView.RatingChangedEvent, source)
		{
			OldRating = oldRating;
			NewRating = newRating;
		}

		public RatingsView RatingsControl => (RatingsView)Source;

		public int OldRating { get; private init; }
		public int NewRating { get; private init; }
	}

	public delegate void RatingChangedHandler(object sender, RatingChangedEventArgs e);


	public class RatingsView : Control
	{
		#region Static Interface

		static RatingsView()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(RatingsView), new FrameworkPropertyMetadata(typeof(RatingsView)));
		}

		public static readonly DependencyProperty StarCountProperty = DependencyProperty.Register("StarCount", typeof(int), typeof(RatingsView),
			new PropertyMetadata(5, HandleDependencyPropertyChanged));

		public static readonly DependencyProperty RatingProperty = DependencyProperty.Register("Rating", typeof(int), typeof(RatingsView),
			new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, HandleDependencyPropertyChanged));

		public static readonly DependencyProperty RatedColorProperty = DependencyProperty.Register("RatedColor", typeof(Brush), typeof(RatingsView),
			new PropertyMetadata(Brushes.Gold, HandleDependencyPropertyChanged));

		public static readonly DependencyProperty UnratedColorProperty = DependencyProperty.Register("UnratedColor", typeof(Brush), typeof(RatingsView),
			new PropertyMetadata(Brushes.LightGray, HandleDependencyPropertyChanged));

		public static readonly DependencyProperty ColorsProperty = DependencyProperty.Register("Colors", typeof(IEnumerable<Brush>),
			typeof(RatingsView), new PropertyMetadata(null, HandleDependencyPropertyChanged));

		public static readonly DependencyProperty OrientationProperty = StackPanel.OrientationProperty.AddOwner(typeof(RatingsView),
			new PropertyMetadata(Orientation.Horizontal));

		public static readonly RoutedEvent RatingChangedEvent = EventManager.RegisterRoutedEvent("RatingChanged", RoutingStrategy.Bubble,
			typeof(RatingChangedHandler), typeof(RatingsView));

		private static void HandleDependencyPropertyChanged(DependencyObject dobj, DependencyPropertyChangedEventArgs e)
		{
			RatingsView rc = (RatingsView)dobj;
			if (e.Property.Name == "Colors")
			{
				if (e.NewValue is IEnumerable<Brush> colors) rc.ColorCount = colors.Count();
				else
				{
					rc.ColorCount = 0;
					if (rc.Rating > rc.StarCount) rc.Rating = rc.StarCount;
				}
			}
			if (rc.ActualWidth > 0 && rc.ActualHeight > 0) rc.ApplyRating(e.Property.Name != "Rating");
		}

		#endregion

		#region Dependency Properties

		public int StarCount
		{
			get => (int)GetValue(StarCountProperty);
			set => SetValue(StarCountProperty, value);
		}

		public int Rating
		{
			get => (int)GetValue(RatingProperty);
			set => SetValue(RatingProperty, value);
		}

		public Orientation Orientation
		{
			get => (Orientation)GetValue(OrientationProperty);
			set => SetValue(OrientationProperty, value);
		}

		public Brush RatedColor
		{
			get => (Brush)GetValue(RatedColorProperty);
			set => SetValue(RatedColorProperty, value);
		}

		public Brush UnratedColor
		{
			get => (Brush)GetValue(UnratedColorProperty);
			set => SetValue(UnratedColorProperty, value);
		}

		public IEnumerable<Brush> Colors
		{
			get => (IEnumerable<Brush>)GetValue(ColorsProperty);
			set => SetValue(ColorsProperty, value);
		}

		#endregion

		private StackPanel _starPanel = DefaultControls.StackPanel;
		private List<StarModel> _stars = new();
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			_starPanel = (StackPanel)GetTemplateChild("starPanel");
			CreateContextMenu();
		}

		public event RatingChangedHandler RatingChanged
		{
			add => AddHandler(RatingChangedEvent, value);
			remove => RemoveHandler(RatingChangedEvent, value);
		}

		private void CreateContextMenu()
		{
			ContextMenu ctx = new ContextMenu();
			MenuItem item = new MenuItem { Header = "Set Value..." };
			item.Click += (o, e) =>
			{
				SetRating();
			};
			ctx.Items.Add(item);
			ctx.Items.Add(new Separator());
			item = new MenuItem { Header = "Clear Rating" };
			item.Click += (o, e) =>
			{
				Rating = 0;
			};
			ctx.Items.Add(item);
			ContextMenu = ctx;
		}

		protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
		{
			base.OnRenderSizeChanged(sizeInfo);
			ApplyRating(true);
		}

		#region Scaling / Geometry

		private static readonly List<Point> _rawPoints = new List<Point>
		{
			new Point(0.951, 0),
			new Point(1.186, 0.676),
			new Point(1.902, 0.691),
			new Point(1.331,1.124),
			new Point(1.539,1.809),
			new Point(0.951,1.4),
			new Point(0.363,1.809),
			new Point(0.571, 1.124),
			new Point(0, 0.691),
			new Point(0.716, 0.676)
		};

		private static List<Point> ScaleRawPoints(double scale)
		{
			//System.Diagnostics.Debug.WriteLine($"Scaling to: {scale:F2}");
			double cx = _rawPoints.Average(p => p.X), cy = _rawPoints.Average(p => p.Y);
			List<Point> r = new List<Point>(_rawPoints.Count);
			foreach (Point p in _rawPoints)
			{
				double dx = cx - p.X, dy = cy - p.Y;
				double x = p.X + scale * dx, y = p.Y + scale * dy;
				r.Add(new Point(x, y));
			}
			double minX = r.Min(p => p.X), minY = r.Min(p => p.Y);
			for (int i = 0; i < r.Count; ++i)
			{
				Point p = r[i];
				r[i] = new Point(p.X - minX, p.Y - minY);
			}
			return r;
		}

		private Geometry Render(List<Point> points)
		{
			StreamGeometry g = new();
			using var ctx = g.Open();
			ctx.BeginFigure(points[0], true, true);
			foreach (Point p in points.Skip(1)) ctx.LineTo(p, true, true);
			ctx.Close();
			return g;
		}

		#endregion

		private void ApplyRating(bool createStars)
		{
			if (_starPanel == null || ActualWidth == 0 || ActualHeight == 0) return;
			if (createStars || _starPanel.Children.Count == 0) CreateStars();
			else
			{
				_stars.ForEach(s => s.ApplyRating());
			}
		}

		private void SetRating(int newValue)
		{
			int oldVal = Rating;
			if (oldVal != newValue)
			{
				Rating = newValue;
				RaiseEvent(new RatingChangedEventArgs(this, oldVal, newValue));
			}
		}

		private int ColorCount { get; set; }
		private bool HasColors => ColorCount > 0;
		private int MaxRating => ColorCount == 0 ? StarCount : StarCount * ColorCount;
		private Brush RatedBrush
		{
			get
			{
				switch (ColorCount)
				{
					case 0: return RatedColor;
					case 1: return Colors.ElementAt(0);
					default:
						int n = (Rating - 1) / StarCount;
						n = Math.Max(0, Math.Min(ColorCount - 1, n));
						return Colors.ElementAt(n);
				}
			}
		}
		private int ColorLevel => ColorCount == 0 ? -1 : (Rating - 1) / StarCount;

		private void CreateStars()
		{
			_starPanel.Children.Clear();
			_stars.Clear();
			if (StarCount <= 0) return;
			double scale = Scale;
			List<Point> scaledPoints = ScaleRawPoints(scale);
			Geometry geo = Render(scaledPoints);
			for (int i = 1; i <= StarCount + 1; ++i)
			{
				Path p = new Path();
				ToolTipService.SetShowOnDisabled(p, true);
				p.StrokeThickness = 1;
				p.Margin = new Thickness(4 * scale / 12);
				p.Data = geo;
				_stars.Add(new StarModel(this, p, i));
				p.Stroke = p.Fill;
				_starPanel.Children.Add(p);
			}
		}

		private double Scale
		{
			get
			{
				const double scaleFactor = 0.35;
				int nAdd = ColorCount > 1 ? 1 : 0;
				switch (Orientation)
				{
					case Orientation.Horizontal:
						return scaleFactor * ActualWidth / (StarCount + nAdd);
					default:
						return scaleFactor * ActualHeight / (StarCount + nAdd);
				}
			}
		}

		protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
		{
			base.OnMouseDoubleClick(e);
			if (!(e.OriginalSource is Path)) Rating = 0;
		}

		private void SetRating()
		{
			Popup p = (Popup)GetTemplateChild("popup");
			new RatingModel(this, p);
		}

		public class RatingModel : ViewModel
		{
			private int _rating;

			internal RatingModel(RatingsView ratingsControl, Popup popup)
			{
				Owner = ratingsControl;
				Popup = popup;
				_rating = Owner.Rating;
				Popup.DataContext = this;
				Popup.IsOpen = true;
				TextBox? tb = Popup.FindName("rating") as TextBox;
				if (tb != null)
				{
					tb.KeyDown += Tb_KeyDown;
					tb?.Focus();
					tb?.SelectAll();
				}
			}

			private void Tb_KeyDown(object sender, KeyEventArgs e)
			{
				if (e.Key == Key.Escape)
				{
					Popup.DataContext = null;
					Popup.IsOpen = false;
				}
			}

			public int Rating
			{
				get => _rating;
				set
				{
					if (value == _rating) return;
					_rating = value;
					Owner.Rating = Math.Min(_rating, Owner.MaxRating);
					Popup.DataContext = null;
					Popup.IsOpen = false;
				}
			}

			private RatingsView Owner { get; init; }
			private Popup Popup { get; init; }
		}

		public class StarModel : ViewModel
		{
			internal StarModel(RatingsView ratingsControl, Path star, int index)
			{
				Owner = ratingsControl;
				Index = index;
				Path = star;
				Path.MouseLeftButtonDown += Path_MouseLeftButtonDown;
				Path.MouseEnter += Path_MouseEnter;
				Path.MouseLeave += Path_MouseLeave;
				if (IsExtraStar) Path.Fill = Owner.UnratedColor;
				ApplyRating();
			}

			internal bool IsExtraStar => Index > Owner.StarCount;
			internal Path Path { get; private init; }
			internal RatingsView Owner { get; private init; }
			internal int Index { get; private init; }
			private int CurrentRatingValue => Owner.HasColors ? Index + Owner.ColorLevel * Owner.StarCount : Index;

			internal bool IsRated => CurrentRatingValue <= Owner.Rating;

			internal void ApplyRating()
			{
				if (IsExtraStar)
				{
					int n = Owner.Rating % Owner.StarCount;
					if (Owner.Rating > 0 && Owner.Rating < Owner.MaxRating && n == 0) Path.Visibility = Visibility.Visible; else Path.Visibility = Visibility.Hidden;
				}
				if (IsRated) Path.Fill = Owner.RatedBrush; else Path.Fill = Owner.UnratedColor;
				Path.Stroke = Path.Fill;
				Path.ToolTip = CurrentRatingValue.ToString();
			}

			private void Path_MouseLeave(object sender, MouseEventArgs e)
			{
				Path.Stroke = Path.Fill;
			}

			private void Path_MouseEnter(object sender, MouseEventArgs e)
			{
				Path.Stroke = Owner.Foreground;
			}

			private void Path_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
			{
				Owner.SetRating(CurrentRatingValue);
			}
		}
	}
}
