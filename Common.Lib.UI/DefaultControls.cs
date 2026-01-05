using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;

namespace Common.Lib.UI
{
	public static class DefaultControlsEx
	{
		extension(Control c)
		{
			public bool IsDefault => c.Tag is string s && s == DefaultControls.DefaultTag;
		}
	}

	public static class DefaultControls
	{
		internal const string DefaultTag = "DEFAULT";

		public static readonly Border Border = new Border() { Tag = DefaultTag };
		public static readonly Button Button = new Button() { Tag = DefaultTag };
		public static readonly ContentControl ContentControl = new ContentControl() { Tag = DefaultTag };
		public static readonly ContentPresenter ContentPresenter = new ContentPresenter() { Tag = DefaultTag };
		public static readonly DataGrid DataGrid = new DataGrid() { Tag = DefaultTag };
		public static readonly DataTemplate DataTemplate = new DataTemplate();
		public static readonly DispatcherTimer DispatcherTimer = new DispatcherTimer();
		public static readonly DockPanel DockPanel = new DockPanel() { Tag = DefaultTag };
		public static readonly Grid Grid = new Grid() { Tag = DefaultTag };
		public static readonly GroupBox GroupBox = new GroupBox() { Tag = DefaultTag };
		public static readonly Image Image = new Image() { Tag = DefaultTag };
		public static readonly ItemsControl ItemsControl = new ItemsControl() { Tag = DefaultTag };
		public static readonly Label Label = new Label() { Tag = DefaultTag };
		public static readonly Popup Popup = new() { Tag = DefaultTag };
		public static readonly StackPanel StackPanel = new StackPanel() { Tag = DefaultTag };
		public static readonly TextBlock TextBlock = new TextBlock() { Tag = DefaultTag };
		public static readonly TextBox TextBox = new TextBox() { Tag = DefaultTag };
		public static readonly UIElement UIElement = new UIElement();
	}
}
