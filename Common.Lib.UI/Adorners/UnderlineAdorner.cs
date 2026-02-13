using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Common.Lib.UI.Adorners
{
	public class UnderlineAdorner : Adorner
	{
		private bool _isActive = true;
		private Brush _lineBrush = Brushes.Black;
		public UnderlineAdorner(UIElement adornedElement) : base(adornedElement) 
		{
			ClipToBounds = true;
		}

		public UnderlineAdorner(UIElement adornedElement, Brush lineBrush, bool isActive = true): this(adornedElement)
		{
			_lineBrush = lineBrush;
			_isActive	= isActive;
		}

		public Brush LineBrush
		{
			get => _lineBrush;
			set
			{
				_lineBrush = value;
				AdornerLayer.GetAdornerLayer(AdornedElement)?.Update();
			}
		}
		public double Thickness { get; set; } = 1;
		public bool IsActive
		{
			get => _isActive;
			set
			{
				if (_isActive != value)
				{
					_isActive = value;
					AdornerLayer.GetAdornerLayer(AdornedElement)?.Update();
				}
			}
		}

		protected override void OnRender(DrawingContext drawingContext)
		{
			if (!IsActive) return;
			if (!IsClipEnabled) IsClipEnabled = true;
			Rect r = new Rect(AdornedElement.DesiredSize);
			if (AdornedElement is ContentPresenter cp && cp.Content is TextBlock tb)
			{
				double bottom = tb.ActualHeight + (cp.ActualHeight - tb.ActualHeight) / 2;
				r = new Rect(0, 0, tb.ActualWidth, bottom - 3);
			}
			else
			if (AdornedElement is FrameworkElement e) r = new Rect(0, 0, e.ActualWidth, e.ActualHeight);
			Pen p = new Pen(LineBrush, Thickness);
			drawingContext.DrawLine(p, r.BottomLeft, r.BottomRight);
		}
	}
}
