using System.Windows;

namespace Common.Lib.UI.Controls.Imaging
{
	public interface IImageShifter
	{
		double XShift { get; }
		double YShift { get; }
		double Zoom { get; }

		Func<Size> ImageSize { get; set; }

		void StartShift(Point origin, List<(int id, Point p)> points);
		void DeltaShift(List<(int id, Point p)> points);
		void EndShift();
	}
}
