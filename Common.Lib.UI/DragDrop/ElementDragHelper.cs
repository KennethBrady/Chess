using System.Windows;

namespace Common.Lib.UI.DragDrop
{
	/// <summary>
	/// Implement this class and use ElementDragger.SetAllowDrag simply drag/drop a single element
	/// </summary>
	/// <remarks>
	/// This implementation does not use Windows drag-drop. It does not support a payload, and cannot work inter-application.
	/// All Point values exposed in this model are relative to the owning Window.  Use Window.PointToScreen to convert to screen coordinates.
	/// </remarks>
	public abstract class ElementDragHelper
	{
		/// <summary>
		/// The point (in Window coordinates) at which the current drag operation started
		/// </summary>
		public Point DownPoint { get; private set; }

		/// <summary>
		/// The current position (in Window coordinates) of the mouse during a drag operation
		/// </summary>
		public Point CurrentPosition { get; private set; }

		/// <summary>
		/// The offset of the current position from the down point (in Window coordinates, duh)
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
		/// <param name="lastDelta">The incremental change (in Window coordinates) since the prior call</param>
		protected abstract void ApplyDragOffset(Point lastDelta);

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

		internal void SetDragOffset(Point current)
		{
			Point delta = new Point(current.X - CurrentPosition.X, current.Y - CurrentPosition.Y);
			CurrentPosition = current;
			ApplyDragOffset(delta);
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
}
