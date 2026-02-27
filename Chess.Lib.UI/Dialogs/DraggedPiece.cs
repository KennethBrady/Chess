using Chess.Lib.Hardware;
using Common.Lib.UI.DragDrop.DDM;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace Chess.Lib.UI.Dialogs
{
	/// <summary>
	/// DraggedPiece is used in the BoardBuilderDialog.
	/// </summary>
	internal class DraggedPiece : Control
	{
		static DraggedPiece()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(DraggedPiece), new FrameworkPropertyMetadata(typeof(DraggedPiece)));
		}

		public DraggedPiece()
		{
			DragDropManager.SetAllowDrag(this, true);
			DragDropManager.AddDragQueryHandler(this, HandleDragQuery);
		}

		protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			base.OnPreviewMouseLeftButtonDown(e);
			if (Keyboard.Modifiers == ModifierKeys.Shift && DataContext is BoardBuilderDialogModel.PieceModel pm)
			{
				pm.ToggleLocked();
				e.Handled = true;
			}
		}

		private void HandleDragQuery(object sender, DragDropQueryEventArgs e)
		{
			if (DataContext is BoardBuilderDialogModel.PieceModel pm)
			{
				e.Options.Payload = pm;
				e.Options.SourceCue = (Image)GetTemplateChild("image");
				e.QueryResult = true;
			}
		}
	}

	internal class HueToForegroundConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is Hue h)
			{
				switch (h)
				{
					case Hue.White: return ChessBoardProperties.DarkSquareBrush;
					case Hue.Black: return ChessBoardProperties.LightSquareBrush;
					default: return Brushes.Transparent;
				}
			}
			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

}
