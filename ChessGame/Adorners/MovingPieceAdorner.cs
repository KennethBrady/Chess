using Common.Lib.UI.Win32;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace ChessGame.Adorners
{
	internal class MovingPieceAdorner : Adorner
	{
		internal MovingPieceAdorner(ChessBoard board): base(board) 
		{
			IsHitTestVisible = false;
		}

		internal Point StartLocation { get; private set; }
		internal Point GlobalStartLocation { get; private set; }
		internal Size Size { get; private set; }
		internal ImageSource? Image { get; private set; }

		internal void SetImage(ImageSource image, Size size, Point location)
		{
			Image = image;
			Size = size;
			StartLocation = location;
			GlobalStartLocation = NativeMethods.GetCursorPos();
		}

		internal void Reset()
		{
			Image = null;
			UpdateLayout();
		}

		protected override void OnRender(DrawingContext drawingContext)
		{
			base.OnRender(drawingContext);
			if (Image != null)
			{
				Point nuPos = NativeMethods.GetCursorPos();
				double dx = GlobalStartLocation.X - nuPos.X, dy = GlobalStartLocation.Y - nuPos.Y;
				Point dest = new Point(StartLocation.X - dx, StartLocation.Y - dy/2);
				Rect rect = new Rect(dest.X, dest.Y, Size.Width, Size.Height);
				drawingContext.DrawImage(Image, rect);
			}
		}
	}
}
