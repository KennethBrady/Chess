using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Common.Lib.UI.Adorners
{
	public class WaterMarkAdorner : Adorner
	{
		internal WaterMarkAdorner(TextBox textBox, string watermark) : base(textBox)
		{
			WaterMark = watermark;
			IsHitTestVisible = false;
			FormattedText = new FormattedText(WaterMark, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface(TB.FontFamily, TB.FontStyle,
				TB.FontWeight, TB.FontStretch), TB.FontSize, Brushes.Gray, 120.0);
		}

		public string WaterMark { get; private init; } = string.Empty;
		private TextBox TB => (TextBox)AdornedElement;

		private FormattedText FormattedText { get; init; }

		protected override void OnRender(DrawingContext drawingContext)
		{
			base.OnRender(drawingContext);
			drawingContext.DrawText(FormattedText, new Point(4, TB.ActualHeight / 5.0));

		}
	}
}
