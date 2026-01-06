using System.Windows;
using System.Windows.Input;

namespace Common.Lib.UI.DragDrop
{
	public static class ElementDragger
	{
		public static readonly DependencyProperty DragInfoProperty = DependencyProperty.RegisterAttached("DragInfo", typeof(ElementDragHelper),
			typeof(ElementDragger), new PropertyMetadata(null, HandleAllowDragChanged));

		private static void HandleAllowDragChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
		{
			if (o is UIElement uie)
			{
				if (e.NewValue == null) uie.MouseDown -= Uie_MouseDown;
				else
				{
					if (e.NewValue is ElementDragHelper) uie.MouseDown += Uie_MouseDown;
				}
			}
		}

		public static ElementDragHelper? GetAllowDrag(UIElement uie) => uie.GetValue(DragInfoProperty) as ElementDragHelper;

		public static void SetAllowDrag(UIElement uie, ElementDragHelper? value) => uie.SetValue(DragInfoProperty, value);

		private static void Uie_MouseDown(object sender, MouseButtonEventArgs e)
		{
			UIElement uie = (UIElement)sender;
			ElementDragHelper? di = GetAllowDrag(uie);
			if (di == null) return; // Why??
			di.Window = Window.GetWindow(uie);
			uie.CaptureMouse();
			di.SetDragDownPoint(e.GetPosition(di.Window));
			uie.MouseMove += Uie_MouseMove;
			uie.MouseUp += Uie_MouseUp;
			e.Handled = true;
		}

		private static void Uie_MouseMove(object sender, MouseEventArgs e)
		{
			UIElement uie = (UIElement)sender;
			ElementDragHelper? di = GetAllowDrag(uie);
			if (di == null) return;
			di.SetDragOffset(e.GetPosition(di.Window));
			e.Handled = true;
		}

		private static void Uie_MouseUp(object sender, MouseButtonEventArgs e)
		{
			UIElement uie = (UIElement)sender;
			uie.ReleaseMouseCapture();
			uie.MouseMove -= Uie_MouseMove;
			uie.MouseUp -= Uie_MouseUp;
			e.Handled = true;
			ElementDragHelper? di = GetAllowDrag(uie);
			if (di == null) return;
			di.EndDrag(e.GetPosition(di.Window));
		}

	}
}
