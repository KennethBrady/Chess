using Common.Lib.UI.Win32;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Common.Lib.UI.DragDrop.DDM
{
	internal class DragVisualWindow : Window
	{
		internal DragVisualWindow(FrameworkElement? content, Style? rectStyle)
		{
			RectStyle = rectStyle;
			WindowStyle = WindowStyle.None;
			AllowsTransparency = true;
			AllowDrop = false;
			Background = null;
			IsHitTestVisible = false;
			SizeToContent = SizeToContent.WidthAndHeight;
			Topmost = true;
			ShowInTaskbar = false;
			SourceInitialized += (o, e) =>
			{
				PresentationSource windowSource = PresentationSource.FromVisual(this);
				Handle = ((System.Windows.Interop.HwndSource)windowSource).Handle;
				Int32 styles = NativeMethods.GetWindowLong(Handle, NativeMethods.GWL_EXSTYLE);
				NativeMethods.SetWindowLong(Handle, NativeMethods.GWL_EXSTYLE, styles | NativeMethods.WS_EX_LAYERED | NativeMethods.WS_EX_TRANSPARENT);
			};
			SetContent(content);
			UpdateLocation();
		}

		private IntPtr Handle { get; set; }

		private Style? RectStyle { get; set; }

		internal void SetContent(FrameworkElement? content)
		{
			if (content == null) Content = null;
			else
			{
				Rectangle rect = new Rectangle();
				rect.Width = content.ActualWidth;
				rect.Height = content.ActualHeight;
				VisualBrush vb = new VisualBrush();
				vb.AutoLayoutContent = false;
				vb.Stretch = Stretch.Fill;
				vb.Visual = content;
				rect.Fill = vb;
				rect.HorizontalAlignment = HorizontalAlignment.Center;
				if (RectStyle != null) rect.Style = RectStyle;
				Content = rect;
			}
		}

		internal void UpdateLocation()
		{
			NativeMethods.POINT pt;
			if (NativeMethods.GetCursorPos(out pt))
			{
				NativeMethods.WindowPos.MoveWindow(Handle, pt.X - (int)ActualWidth, pt.Y - (int)ActualHeight);
			}
		}
	}
}
