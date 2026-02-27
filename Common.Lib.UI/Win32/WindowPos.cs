using System.Runtime.InteropServices;

namespace Common.Lib.UI.Win32
{
public static partial class NativeMethods
	{
        public static class WindowPos
		{
			// https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwindowpos

			[DllImport("user32.dll", CharSet = CharSet.Auto)]
			public static extern bool SetWindowPos(IntPtr hwnd, IntPtr insertAfter, int x, int y, int width, int height, uint flags);

			public static bool MoveWindow(IntPtr hwnd, int newX, int newY)
			{
				return SetWindowPos(hwnd, HWND_TOP, newX, newY, 0, 0, NOSIZE);
			}

			public static readonly IntPtr HWND_BOTTOM = new IntPtr(1);
			public static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
			public static readonly IntPtr HWND_TOP = new IntPtr(0);
			public static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);

			public static uint ASYNCWINDOWPOS = 0x4000;
			public static uint DEFERERASE = 0x2000;
			public static uint DRAWFRAME = 0x0020;
			public static uint HIDEWINDOW = 0x0080;
			public static uint NOACTIVATE = 0x0010;
			public static uint NOCOPYBITS = 0x0100;
			public static uint NOMOVE = 0x0002;
			public static uint NOOWNERZORDER = 0x0200;
			public static uint NOREDRAW = 0x0008;
			public static uint NOREPOSITION = 0x0200;
			public static uint NOSENDCHANGING = 0x0400;
			public static uint NOSIZE = 0x0001;
			public static uint NOZORDER = 0x0004;
			public static uint SHOWWINDOW = 0x0040;
		}


	}
}
