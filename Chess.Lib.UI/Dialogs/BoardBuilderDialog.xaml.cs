using Chess.Lib.Hardware;
using Chess.Lib.UI.Images;
using Common.Lib.UI.Dialogs;
using Common.Lib.UI.DragDrop.DDM;
using Common.Lib.UI.Windows;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Chess.Lib.UI.Dialogs
{
	/// <summary>
	/// Interaction logic for GameBuilderDialog.xaml
	/// </summary>
	public partial class BoardBuilderDialog : DialogView
	{
		public BoardBuilderDialog()
		{
			InitializeComponent();
		}

		protected override void OnDataContextChanged(object? oldValue, object? newValue)
		{
			base.OnDataContextChanged(oldValue, newValue);
			if (newValue is BoardBuilderDialogModel bbdm) 
			{
				bbdm.SetPieceCursor = SetSquaresCursor;
				bbdm.SetEraserCursor = () => SetSquaresCursor(ImageLoader.LoadImage("eraser"), 20);
			};
		}

		private void SetSquaresCursor(PieceDef pd)
		{
			if (pd.IsDefault) SetSquaresCursor(null); else SetSquaresCursor(ImageLoader.LoadImage(pd));
		}

		private void SetSquaresCursor(BitmapFrame? source, int dimension = 80)
		{
			if (source == null) squares.Cursor = Cursors.Arrow; else
			{
				Cursor c = CustomCursors.CreateCursor(source, 15, 15, new Size(dimension, dimension));
				squares.Cursor = c;
			}
		}
	}

	internal class DropGrid : Grid
	{
		public DropGrid()
		{
			DragDropManager.SetAllowDrop(this, true);
			DragDropManager.AddDropInfoHandler(this, HandleDropInfo);
			DragDropManager.AddDropQueryHandler(this, HandleDropQuery);
		}

		protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			base.OnPreviewMouseLeftButtonDown(e);
			if (DataContext is BoardBuilderDialogModel.SquareModel sm) sm.ApplyGridClick();
		}

		private void HandleDropQuery(object sender, DragDropQueryEventArgs e)
		{
			e.QueryResult = true;
		}

		private void HandleDropInfo(object sender, DragDropEventArgs e)
		{
			switch(e.Options.Status)
			{
				case DragStatus.DropComplete:
					if (DataContext is BoardBuilderDialogModel.SquareModel sm)
					{
						void attachImage()
						{
							Image image = (Image)Children[1];
							if (image.Tag == null)
							{
								DragDropManager.SetAllowDrag(image, true);
								DragDropManager.AddDragQueryHandler(image, HandleImageDragQuery);
								image.Tag = string.Empty;
							}
						}
						switch(e.Options.Payload)
						{
							case BoardBuilderDialogModel.PieceModel p:
								sm.SetPieceFromModel(p, true);
								attachImage();
								break;
							case BoardBuilderDialogModel.SquareModel smSource:
								sm.SetPieceFromSquare(smSource);
								attachImage();
								break;
						}
					}
					break;
			}
		}

		private void HandleImageDragQuery(object sender, DragDropQueryEventArgs e)
		{
			if (sender is Image image && image.DataContext is BoardBuilderDialogModel.SquareModel sm)
			{
				e.Options.Payload = sm;
				e.Options.SourceCue = image;
				e.QueryResult = true;
			}
		}

	}

	internal class NextMoveConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is Hue h && parameter is string s) return s[0] == char.ToLower(h.ToString()[0]);
			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is bool b && parameter is string s)
			{
				switch (s)
				{
					case "l": return b ? Hue.White : Hue.Black;
					case "d": return b ? Hue.Black : Hue.White;
				}
			}
			return value;
		}
	}
}
