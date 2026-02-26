using Microsoft.Win32.SafeHandles;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Common.Lib.UI.Windows
{
	public static class CustomCursors
	{
		private struct IconInfo
		{
			public bool fIcon;
			public int xHotspot;
			public int yHotspot;
			public IntPtr hbmMask;
			public IntPtr hbmColor;
		}

		[DllImport("user32.dll")]
		private static extern IntPtr CreateIconIndirect(ref IconInfo icon);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool GetIconInfo(IntPtr hIcon, ref IconInfo pIconInfo);


		private static Cursor InternalCreateCursor(System.Drawing.Bitmap bmp,
				int xHotSpot, int yHotSpot)
		{
			IconInfo tmp = new IconInfo();
			GetIconInfo(bmp.GetHicon(), ref tmp);
			tmp.xHotspot = xHotSpot;
			tmp.yHotspot = yHotSpot;
			tmp.fIcon = false;

			IntPtr ptr = CreateIconIndirect(ref tmp);
			SafeFileHandle handle = new SafeFileHandle(ptr, true);
			return CursorInteropHelper.Create(handle);
		}

		public static Cursor CreateCursor(UIElement element, int xHotSpot, int yHotSpot, Size cursorSize)
		{
			if (cursorSize.IsEmpty)
			{
				element.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
				cursorSize = element.DesiredSize;
			}
			RenderTargetBitmap rtb = new RenderTargetBitmap((int)cursorSize.Width,
					(int)cursorSize.Height, 96, 96, PixelFormats.Pbgra32);
			rtb.Render(element);

			PngBitmapEncoder encoder = new PngBitmapEncoder();
			encoder.Frames.Add(BitmapFrame.Create(rtb));

			MemoryStream ms = new MemoryStream();
			encoder.Save(ms);

			System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(ms);

			ms.Close();
			ms.Dispose();

			Cursor cur = InternalCreateCursor(bmp, xHotSpot, yHotSpot);

			bmp.Dispose();

			return cur;
		}

		public static Cursor CreateCursor(BitmapSource source, int xHotSpot, int yHotSpot) =>
			CreateCursor(source, xHotSpot, yHotSpot, Size.Empty);

		public static Cursor CreateCursor(BitmapSource source, int xHotSpot, int yHotSpot, Size size)
		{
			if (!size.IsEmpty)
			{
				double scaleX = size.Width / source.Width, scaleY = size.Height / source.Height;
				ScaleTransform tx = new ScaleTransform(scaleX, scaleY);
				source = new TransformedBitmap(source, tx);
			}
			PngBitmapEncoder encoder = new PngBitmapEncoder();
			encoder.Frames.Add(BitmapFrame.Create(source));
			using MemoryStream ms = new MemoryStream();
			encoder.Save(ms);
			System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(ms);
			return InternalCreateCursor(bmp, xHotSpot, yHotSpot);
		}

		public static Cursor CreateCursor(UIElement element, int xHotSpot, int yHotSpot)
		{
			element.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
			element.Arrange(new Rect(0, 0, element.DesiredSize.Width,
					element.DesiredSize.Height));

			RenderTargetBitmap rtb = new RenderTargetBitmap((int)element.DesiredSize.Width,
					(int)element.DesiredSize.Height, 96, 96, PixelFormats.Pbgra32);
			rtb.Render(element);

			PngBitmapEncoder encoder = new PngBitmapEncoder();
			encoder.Frames.Add(BitmapFrame.Create(rtb));

			MemoryStream ms = new MemoryStream();
			encoder.Save(ms);

			System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(ms);

			ms.Close();
			ms.Dispose();

			Cursor cur = InternalCreateCursor(bmp, xHotSpot, yHotSpot);

			bmp.Dispose();

			return cur;
		}

	}
}
