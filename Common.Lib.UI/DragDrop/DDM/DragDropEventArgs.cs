using Common.Lib.UI.Win32;
using System.Windows;

namespace Common.Lib.UI.DragDrop.DDM
{
	public delegate void DragDropEventHandler(object sender, DragDropEventArgs e);

	public class DragDropEventArgs : RoutedEventArgs
	{
		internal DragDropEventArgs(RoutedEvent evnt, object source, DragDropOptions options)
	: base(evnt, source)
		{
			Options = options;
			Options.CurrentDragPosition = NativeMethods.GetCursorPos();
		}

		public DragDropOptions Options { get; private set; }
	}
}
