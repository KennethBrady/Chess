using System.Windows;

namespace Common.Lib.UI.DragDrop
{
	public abstract class DragHelper
	{
		/// <summary>
		/// The point at which the current drag operation started
		/// </summary>
		public Point DownPoint { get; private set; }

		/// <summary>
		/// The current position of the mouse during a drag operation
		/// </summary>
		public Point CurrentPosition { get; private set; }

		/// <summary>
		/// The offset of the current position from the down point
		/// </summary>
		public Point Offset => new Point(CurrentPosition.X - DownPoint.X, CurrentPosition.Y - DownPoint.Y);

		/// <summary>
		/// Is a drag operation current?
		/// </summary>
		public bool IsDragging { get; private set; }

		/// <summary>
		/// Called when drag is being initiated at the given point.
		/// </summary>
		/// <param name="downPoint"></param>
		protected abstract void InitDrag(Point downPoint);

		/// <summary>
		/// Called repeatedly during the drag operation
		/// </summary>
		/// <param name="lastDelta">The incremental change since the prior call</param>
		protected abstract void SetDragOffset(Point lastDelta);

		/// <summary>
		/// Override this to do any clean-up
		/// </summary>
		protected virtual void EndDrag() { }

		internal void SetDragDownPoint(Point downPoint)
		{
			DownPoint = CurrentPosition = downPoint;
			IsDragging = true;
			InitDrag(DownPoint);
		}

		internal void ApplyDragOffset(Point current)
		{
			Point delta = new Point(current.X - CurrentPosition.X, current.Y - CurrentPosition.Y);
			CurrentPosition = current;
			SetDragOffset(delta);
		}

		internal void EndDrag(Point upPoint)
		{
			CurrentPosition = upPoint;
			EndDrag();
			IsDragging = false;
			DownPoint = CurrentPosition = new Point();
		}

		internal Window? Window { get; set; }
	}

	public static class ElementDragger
	{
		public static readonly DependencyProperty DragInfoProperty = DependencyProperty.RegisterAttached("DragInfo", typeof(DragHelper),
			typeof(ElementDragger), new PropertyMetadata(null, HandleAllowDragChanged));

		private static void HandleAllowDragChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
		{
			if (o is UIElement uie)
			{
				if (e.NewValue == null) uie.MouseDown -= Uie_MouseDown;
				else
				{
					if (e.NewValue is DragHelper) uie.MouseDown += Uie_MouseDown;
				}
			}
		}

		public static DragHelper? GetAllowDrag(UIElement uie) => uie.GetValue(DragInfoProperty) as DragHelper;

		public static void SetAllowDrag(UIElement uie, DragHelper? value) => uie.SetValue(DragInfoProperty, value);

		private static void Uie_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			UIElement uie = (UIElement)sender;
			DragHelper? di = GetAllowDrag(uie);
			if (di == null) return; // Why??
			di.Window = Window.GetWindow(uie);
			uie.CaptureMouse();
			di.SetDragDownPoint(e.GetPosition(di.Window));
			uie.MouseMove += Uie_MouseMove;
			uie.MouseUp += Uie_MouseUp;
			e.Handled = true;
		}

		private static void Uie_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
		{
			UIElement uie = (UIElement)sender;
			DragHelper? di = GetAllowDrag(uie);
			if (di == null) return;
			di.ApplyDragOffset(e.GetPosition(di.Window));
			e.Handled = true;
		}

		private static void Uie_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			UIElement uie = (UIElement)sender;
			uie.ReleaseMouseCapture();
			uie.MouseMove -= Uie_MouseMove;
			uie.MouseUp -= Uie_MouseUp;
			e.Handled = true;
			DragHelper? di = GetAllowDrag(uie);
			if (di == null) return;
			di.EndDrag(e.GetPosition(di.Window));
		}

	}
}
