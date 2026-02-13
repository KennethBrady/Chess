using System.Windows;

namespace Common.Lib.UI.DragDrop.DDM
{
	/// <summary>
	/// Delegate for DragQuery and DropQuery events
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	public delegate void DragDropQueryEventHandler(object sender, DragDropQueryEventArgs e);

	/// <summary>
	/// Event arguments for DragQuery and DropQuery events
	/// </summary>
	public class DragDropQueryEventArgs : DragDropEventArgs
	{
		internal DragDropQueryEventArgs(RoutedEvent evnt, object source, DragDropOptions options)
			: base(evnt, source, options)
		{
		}

		/// <summary>
		/// Get / Set whether a Drag operation can begin
		/// </summary>
		public bool? QueryResult { get; set; }
	}

}
