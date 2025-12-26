using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Chess.Lib.UI.Adorners
{
	internal class SquareAdorner : Adorner
	{
		internal SquareAdorner(ChessSquare square) : base(square) 
		{
			IsHitTestVisible = false;
		}

		internal ChessSquare Square => (ChessSquare)AdornedElement;

		protected override void OnRender(DrawingContext drawingContext)
		{
			base.OnRender(drawingContext);
			if (Square.Adornments.HasFlag(SquareAdornment.MoveTarget))
			{
				double ctr = Square.ActualWidth / 2, rad = ctr * 0.5;
				Pen p = new Pen(ChessBoardProperties.MoveTargetColor, 2);
				drawingContext.DrawEllipse(null, p, new Point(ctr, ctr), rad, rad);
			}
			if (Square.Adornments.HasFlag(SquareAdornment.LastMove))
			{
				Rect r = new Rect(0, 0, Square.ActualWidth, Square.ActualHeight);
				drawingContext.DrawRectangle(ChessBoardProperties.LastMoveBrush, null, r);
			}
			if(Square.Adornments.HasFlag(SquareAdornment.Check))
			{
				Rect r = new Rect(0, 0, Square.ActualWidth, Square.ActualHeight);
				drawingContext.DrawRectangle(ChessBoardProperties.CheckedKingBrush, null, r);
			}
		}
	}
}
