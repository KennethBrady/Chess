using System.Windows;

namespace Common.Lib.UI.DragDrop.DDM
{
	/// <summary>
	/// Options associated with a drag / drop operation
	/// </summary>
	public class DragDropOptions
	{
		/// <summary>
		/// Get the current mouse position in screen coordinates
		/// </summary>
		public Point CurrentDragPosition { get; internal set; }

		/// <summary>
		/// Get / Set the payload
		/// </summary>
		public object? Payload { get; set; }

		/// <summary>
		/// Get / Set visual feedback from the drag source
		/// </summary>
		public FrameworkElement? SourceCue { get; set; }

		/// <summary>
		/// Get / Set visual feedback from the drop target
		/// </summary>
		public FrameworkElement? DestinationCue { get; set; }

		/// <summary>
		/// Get the status associated with this drag / drop event.
		/// </summary>
		public DragStatus Status { get; internal set; }

		/// <summary>
		/// Test whether a drop is possible.  This can be used by the drag source to update the SourceCue.
		/// </summary>
		public bool CanDrop { get; internal set; }

		/// <summary>
		/// Get the current drop target (can be null).  This can be used by the drag source to update the SourceCue.
		/// </summary>
		public UIElement? DropTarget { get; internal set; }

		public Style? RectangleStyle { get; set; }
	}

}
