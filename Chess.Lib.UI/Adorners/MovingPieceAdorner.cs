using Common.Lib.UI.Win32;
using DocumentFormat.OpenXml.Drawing.Charts;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Chess.Lib.UI.Adorners
{
	//internal static class PtExt
	//{
	//	extension(Point p)
	//	{
	//		public string ToPrint() => $"{p.X:F2},{p.Y:F2}";
	//	}
	//	extension(Rect r)
	//	{
	//		public string ToPrint() => $"{r.TopLeft.ToPrint()},{r.Width:F1},{r.Height:F1}";
	//	}
	//}

	internal class MovingPieceAdorner : Adorner
	{
		internal MovingPieceAdorner(ChessBoard board): base(board) 
		{
			IsHitTestVisible = false;
		}

		public ChessBoard Board => (ChessBoard)AdornedElement;

		private Image? Image { get; set; }

		private Point BoardDownPoint { get; set; }
	
		private ChessSquare? Square { get; set; }

		private Rect StartRect { get; set; }  // The current image's area on the board, in board coordinates

		private Rect NextRect { get; set; } = Rect.Empty;

		internal void StartDragImage(ChessSquare square, Image image, Point boardDownPoint)
		{
			Square = square;
			BoardDownPoint = boardDownPoint;
			Image = image;			
			StartRect = Image.TransformToVisual(Board).TransformBounds(LayoutInformation.GetLayoutSlot(image));
			NextRect = StartRect;
			//System.Diagnostics.Debug.WriteLine($"{StartRect.ToPrint()}\t{NextRect.ToPrint()}");
		}

		internal void UpdateDragImage(Point boardPoint)
		{
			double dx = boardPoint.X - BoardDownPoint.X, dy = boardPoint.Y - BoardDownPoint.Y;
			Point origin = new Point(StartRect.X + dx, StartRect.Y + dy);
			NextRect = new Rect(origin, StartRect.Size);
			UpdateLayout();
		}

		internal void Reset()
		{
			Image = null;
			Square = null;
			NextRect = Rect.Empty;
			UpdateLayout();
		}

		protected override void OnRender(DrawingContext drawingContext)
		{
			base.OnRender(drawingContext);
			if (Image != null && Square != null && !NextRect.IsEmpty)
			{
				//System.Diagnostics.Debug.Write(NextRect.ToPrint());
				drawingContext.DrawImage(Image.Source, NextRect);
			}
		}
	}
}
