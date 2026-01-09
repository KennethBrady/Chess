using System.Windows.Media;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Controls;

namespace Common.Lib.UI.Adorners
{
	// Thanks to https://github.com/NigelWGMajor/ResizeAdorner for some of the ideas used in this adorner.
	public class ResizeAdorner : Adorner
	{
		#region Helpers

		[Flags]
		private enum ThumbPosition
		{
			Left = 0x0001,
			Top = 0x0002,
			Right = 0x0004,
			Bottom = 0x0010,
			TopLeft = Top | Left,
			TopRight = Top | Right,
			BottomLeft = Bottom | Left,
			BottomRight = Bottom | Right
		}

		private record struct CanvasConstraint(double Left, double Top, double Right, double Bottom)
		{
			internal static readonly CanvasConstraint Empty = new CanvasConstraint(double.NaN, double.NaN, double.NaN, double.NaN);
			internal bool IsEmpty => double.IsNaN(Top) && double.IsNaN(Left) && double.IsNaN(Right) && double.IsNaN(Bottom);
			internal static CanvasConstraint FromElement(FrameworkElement e)
			{
				if (e.Parent is Canvas) return new CanvasConstraint(Canvas.GetLeft(e), Canvas.GetTop(e), Canvas.GetRight(e), Canvas.GetBottom(e));
				return Empty;
			}

			internal void Adjust(ThumbPosition position, FrameworkElement e, Point delta)
			{
				if (position.HasFlag(ThumbPosition.Top) && !double.IsNaN(Top)) Canvas.SetTop(e, Top + delta.Y);
				if (position.HasFlag(ThumbPosition.Bottom) && !double.IsNaN(Bottom)) Canvas.SetBottom(e, Bottom + delta.Y);
				if (position.HasFlag(ThumbPosition.Left) && !double.IsNaN(Left)) Canvas.SetLeft(e, Left + delta.X);
				if (position.HasFlag(ThumbPosition.Right) && !double.IsNaN(Right)) Canvas.SetRight(e, Right + delta.X);
			}
		}

		#endregion

		private const double ThumbSize = 10;
		private const double MinCtrlDimension = 10;
		private Brush ThumbBrush { get; set; } = Brushes.Black; // TODO: allow customization

		private VisualCollection Children { get; init; }

		public ResizeAdorner(FrameworkElement adornedElement) : this(adornedElement, Size.Empty)
		{
		}

		public ResizeAdorner(FrameworkElement adornedElement, Size minSize) : base(adornedElement)
		{
			Children = new VisualCollection(this);
			Children.Add(new ResizeThumb(this, ThumbPosition.Left));
			Children.Add(new ResizeThumb(this, ThumbPosition.TopLeft));
			Children.Add(new ResizeThumb(this, ThumbPosition.Top));
			Children.Add(new ResizeThumb(this, ThumbPosition.TopRight));
			Children.Add(new ResizeThumb(this, ThumbPosition.Right));
			Children.Add(new ResizeThumb(this, ThumbPosition.BottomRight));
			Children.Add(new ResizeThumb(this, ThumbPosition.Bottom));
			Children.Add(new ResizeThumb(this, ThumbPosition.BottomLeft));
			foreach (ResizeThumb rt in Children) Thumbs.Add(rt.Position, rt);
			Keyboard.AddKeyDownHandler(this, new KeyEventHandler(HandleKeyDown));
			if (minSize.IsEmpty)
			{
				double w = MinCtrlDimension, h = MinCtrlDimension;
				if (!double.IsNaN(AdornedElement.MinWidth)) w = AdornedElement.MinWidth;
				if (!double.IsNaN(AdornedElement.MinHeight)) w = AdornedElement.MinHeight;
				MinSize = new Size(w, h);
			}
			else MinSize = minSize;
			Visibility = Visibility.Hidden;
		}

		private Size MinSize { get; init; }

		private void HandleKeyDown(object sender, KeyEventArgs e)
		{
			//if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl) Visibility = Visibility.Visible; else Visibility = Visibility.Hidden;
		}

		private new FrameworkElement AdornedElement => (FrameworkElement)base.AdornedElement;

		protected override Visual GetVisualChild(int index) => Children[index];

		protected override int VisualChildrenCount => Children.Count;

		protected override Size ArrangeOverride(Size finalSize)
		{
			Size s = AdornedElement.DesiredSize;
			double t2 = ThumbSize / 2, x2 = s.Width / 2, y2 = s.Height / 2;
			Thumbs[ThumbPosition.Left].Arrange(new Rect(-t2, y2 - t2, ThumbSize, ThumbSize));
			Thumbs[ThumbPosition.Right].Arrange(new Rect(s.Width - t2, y2 - t2, ThumbSize, ThumbSize));
			Thumbs[ThumbPosition.Top].Arrange(new Rect(x2 - t2, -t2, ThumbSize, ThumbSize));
			Thumbs[ThumbPosition.Bottom].Arrange(new Rect(x2 - t2, s.Height - t2, ThumbSize, ThumbSize));
			Thumbs[ThumbPosition.TopLeft].Arrange(new Rect(-t2, -t2, ThumbSize, ThumbSize));
			Thumbs[ThumbPosition.TopRight].Arrange(new Rect(s.Width - t2, -t2, ThumbSize, ThumbSize));
			Thumbs[ThumbPosition.BottomLeft].Arrange(new Rect(-t2, s.Height - t2, ThumbSize, ThumbSize));
			Thumbs[ThumbPosition.BottomRight].Arrange(new Rect(s.Width - t2, s.Height - t2, ThumbSize, ThumbSize));
			return finalSize;
		}

		private Dictionary<ThumbPosition, ResizeThumb> Thumbs { get; init; } = new();

		private void DragDelta(ThumbPosition position, Point delta)
		{
			Size current = new Size(AdornedElement.ActualWidth, AdornedElement.ActualHeight);
			double nuW = current.Width, nuH = current.Height;
			CanvasConstraint cc = CanvasConstraint.FromElement(AdornedElement);
			switch (position)
			{
				case ThumbPosition.Top: nuH -= delta.Y; break;
				case ThumbPosition.Bottom: nuH += delta.Y; break;
				case ThumbPosition.Left: nuW -= delta.X; break;
				case ThumbPosition.Right: nuW += delta.X; break;
				default: nuW += delta.X; nuH += delta.Y; break;
			}
			nuW = Math.Max(nuW, MinSize.Width); nuH = Math.Max(nuH, MinSize.Height);
			AdornedElement.Width = nuW;
			AdornedElement.Height = nuH;
			cc.Adjust(position, AdornedElement, delta);
		}

		protected override void OnInitialized(EventArgs e)
		{
			base.OnInitialized(e);
			Window w = Window.GetWindow(AdornedElement);
			w.KeyDown += Window_KeyDown;
			w.KeyUp += Window_KeyUp;
			Unloaded += (_, _) =>
			{
				w.KeyDown -= Window_KeyDown;
				w.KeyUp -= Window_KeyUp;
			};
		}

		private void Window_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl) Visibility = Visibility.Visible;
		}

		private void Window_KeyUp(object sender, KeyEventArgs e)
		{
			if (Visibility == Visibility.Visible) Visibility = Visibility.Hidden;
		}

		private class ResizeThumb : Thumb
		{
			internal ResizeThumb(ResizeAdorner owner, ThumbPosition position)
			{
				Owner = owner;
				Position = position;
				Background = Owner.ThumbBrush;
				Width = Height = ThumbSize;
				Visibility = Visibility.Visible;
				DragDelta += ResizeThumb_DragDelta;
				switch (Position)
				{
					case ThumbPosition.Top:
					case ThumbPosition.Bottom: Cursor = Cursors.SizeNS; break;
					case ThumbPosition.Right:
					case ThumbPosition.Left: Cursor = Cursors.SizeWE; break;
					case ThumbPosition.TopLeft:
					case ThumbPosition.BottomRight: Cursor = Cursors.SizeNWSE; break;
					default: Cursor = Cursors.SizeNESW; break;
				}
			}

			public ThumbPosition Position { get; private init; }
			private ResizeAdorner Owner { get; init; }

			private void ResizeThumb_DragDelta(object sender, DragDeltaEventArgs e)
			{
				Owner.DragDelta(Position, new Point(e.HorizontalChange, e.VerticalChange));
			}

		}
	}
}
