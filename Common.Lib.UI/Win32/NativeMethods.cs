using System.Runtime.InteropServices;
using System.Text;
using System.Windows;

namespace Common.Lib.UI.Win32
{
	/// <summary>
	/// Placeholder for interop methods
	/// </summary>
	public static partial class NativeMethods
	{
		#region Windows
		[DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
		public static extern IntPtr GetParent(IntPtr hWnd);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool IsWindowVisible(IntPtr hWnd);

		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		static extern int GetWindowTextLength(IntPtr hWnd);

		[DllImport("user32.dll", EntryPoint = "GetWindowText", ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true)]
		private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpWindowText, int nMaxCount);

		[DllImport("User32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern int FlashWindow(IntPtr hwnd, int invert);
		public static int FlashWindow(IntPtr hwnd, bool invert)
		{
			return FlashWindow(hwnd, invert ? 1 : 0);
		}

		[DllImport("User32.dll", SetLastError = true)]
		public static extern bool BringWindowToTop(IntPtr hWnd);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool SetForegroundWindow(IntPtr hWnd);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool EnumWindows(EnumDelegate lpEnumFunc, IntPtr lParam);

		[DllImport("user32.dll", EntryPoint = "EnumDesktopWindows", ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true)]
		private static extern bool EnumDesktopWindows(IntPtr hDesktop, EnumDelegate lpEnumCallbackFunction, IntPtr lParam);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool EnumChildWindows(IntPtr hwndParent, EnumDelegate lpEnumFunc, IntPtr lParam);

		// Define the callback delegate's type.
		private delegate bool EnumDelegate(IntPtr hWnd, int lParam);

		public static void EnumWindows(Func<(IntPtr handle, string windowTitle), bool> action)
		{
			EnumWindows((hwnd, lp) =>
			{
				string title = GetWindowTitle(hwnd);
				bool result = action((hwnd, title));
				return result;
			}, IntPtr.Zero);
		}

		public static void EnumDesktopWindows(Func<IntPtr, bool> action)
		{
			EnumDesktopWindows(IntPtr.Zero, (hwnd, lp) =>
			{
				return action(hwnd);
			}, IntPtr.Zero);
		}

		public static void EnumChildWindows(IntPtr parentWindow, Func<IntPtr, bool> action)
		{
			EnumChildWindows(parentWindow, (hwnd, lParam) =>
			{
				return action(hwnd);
			}, IntPtr.Zero);
		}

		public static string GetWindowTitle(IntPtr hwnd)
		{
			int len = GetWindowTextLength(hwnd);
			if (len == 0) return string.Empty;
			StringBuilder s = new StringBuilder(len + 1);
			GetWindowText(hwnd, s, s.Capacity);
			return s.ToString();
		}

		[DllImport("user32.dll")]
		private static extern IntPtr SetActiveWindow(IntPtr hwnd);
		[DllImport("user32.dll")]
		private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

		public static void ActivateWindow(IntPtr hwnd)
		{
			ShowWindow(hwnd, 5);
			SetForegroundWindow(hwnd);
			SetActiveWindow(hwnd);
		}

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern Int32 GetWindowLong(IntPtr hWnd, Int32 nIndex);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern Int32 SetWindowLong(IntPtr hWnd, Int32 nIndex, Int32 newVal);

		public static Int32 GWL_EXSTYLE = -20;
		public static Int32 WS_EX_LAYERED = 0x00080000;
		public static Int32 WS_EX_TRANSPARENT = 0x00000020;

		#endregion

		#region Console

		[DllImport("kernel32.dll", ExactSpelling = true)]
		private static extern IntPtr GetConsoleWindow();

		public static void MaximizeWindow()
		{
			IntPtr ptr = GetConsoleWindow();
			ShowWindow(ptr, 3);   //SW_MAXIMIZE = 3
		}

		#endregion

		#region Cursor

		[StructLayout(LayoutKind.Sequential)]
		public struct POINT
		{
			public int X;
			public int Y;

			public POINT(int x, int y)
			{
				this.X = x;
				this.Y = y;
			}
		}

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern bool GetCursorPos(out POINT pt);

		public static Point GetCursorPos()
		{
			Point r = new Point(-1, -1);
			POINT pt;
			if (GetCursorPos(out pt))
			{
				r.X = pt.X;
				r.Y = pt.Y;
			}
			return r;
		}

		#endregion

		#region Messages

		/// <summary>
		/// Beginning of range available for use by applications
		/// </summary>
		public const int WM_APP = 0x8000;

		[return: MarshalAs(UnmanagedType.Bool)]
		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		public static extern bool PostMessage(HandleRef hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

		public static bool PostMessage(object owner, IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam)
		{
			HandleRef href = new HandleRef(owner, hwnd);
			return PostMessage(href, msg, wParam, lParam);
		}

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, IntPtr lParam);

		public const int WM_COPYDATA = 0x4A;

		public static void SendString(IntPtr hwnd, string messageData)
		{
			byte[] data = Encoding.Default.GetBytes(messageData);
			COPYDATASTRUCT cds = new COPYDATASTRUCT
			{
				dwData = new IntPtr(100),
				cbData = data.Length + 1,
				lpData = Marshal.StringToHGlobalAnsi(messageData)
			};
			IntPtr ptrCDS = IntPtr.Zero;
			try
			{
				ptrCDS = Marshal.AllocCoTaskMem(Marshal.SizeOf(cds));
				Marshal.StructureToPtr(cds, ptrCDS, false);
				NativeMethods.SendMessage(hwnd, WM_COPYDATA, 0, ptrCDS);
			}
			finally
			{
				if (ptrCDS != IntPtr.Zero) Marshal.FreeCoTaskMem(ptrCDS);
			}
		}

		public static string? ExtractString(IntPtr wmCopyLParam)
		{
			COPYDATASTRUCT cds = Marshal.PtrToStructure<COPYDATASTRUCT>(wmCopyLParam);
			return Marshal.PtrToStringAnsi(cds.lpData);
		}

		/// <summary>
		/// Contains data to be passed to another application by the WM_COPYDATA message.
		/// </summary>
		[StructLayout(LayoutKind.Sequential)]
		private struct COPYDATASTRUCT
		{
			/// <summary>
			/// User defined data to be passed to the receiving application.
			/// </summary>
			public IntPtr dwData;

			/// <summary>
			/// The size, in bytes, of the data pointed to by the lpData member.
			/// </summary>
			public int cbData;

			/// <summary>
			/// The data to be passed to the receiving application. This member can be IntPtr.Zero.
			/// </summary>
			public IntPtr lpData;
		}

		#endregion

	}
}
