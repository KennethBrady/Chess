using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using System.Xml.Linq;

namespace Common.Lib.UI
{
	public static class DefaultControls
	{
		public static readonly Border Border = new Border();
		public static readonly ContentControl ContentControl = new ContentControl();
		public static readonly ContentPresenter ContentPresenter = new ContentPresenter();
		public static readonly DataTemplate DataTemplate = new DataTemplate();
		public static readonly DispatcherTimer DispatcherTimer = new DispatcherTimer();
		public static readonly DockPanel DockPanel = new DockPanel();
		public static readonly Grid Grid = new Grid();
		public static readonly GroupBox GroupBox = new GroupBox();
		public static readonly Image Image = new Image();
		public static readonly ItemsControl ItemsControl = new ItemsControl();
		public static readonly Label Label = new Label();
		public static readonly Popup Popup = new();
		public static readonly StackPanel StackPanel = new StackPanel();
		public static readonly TextBlock TextBlock = new TextBlock();
		public static readonly TextBox TextBox = new TextBox();
		public static readonly UIElement UIElement = new UIElement();
	}
}
