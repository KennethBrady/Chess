using Common.Lib.IO;
using Common.Lib.UI.Win32;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace Common.Lib.UI.App
{
	[Obsolete("Use AppExtensions")]
	public abstract class AppBase : Application
	{
		private Unique _unique;

		protected AppBase(bool ensureUnique, bool autoFlash = false, bool bringToTop = false)
		{
			_unique = new Unique(Process.GetCurrentProcess().ProcessName, autoFlash, bringToTop);
			if (ensureUnique && _unique.IsAlreadyRunning)
			{
				_unique.Dispose();
				Shutdown();
			}
		}

		protected AppBase(int message, int wParam = 0, int lParam = 0)
		{
			_unique = new Unique(Process.GetCurrentProcess().ProcessName, false, false);
			if (_unique.IsAlreadyRunning)
			{
				IntPtr? hwnd = null;
				NativeMethods.EnumWindows(handleTitle =>
				{
					if (IsWindowTitle(handleTitle.windowTitle))
					{
						hwnd = handleTitle.handle;
						return false;
					};
					return true;
				});
				if (hwnd.HasValue)
				{
					NativeMethods.SendMessage(hwnd.Value, (int)message, wParam, new IntPtr(lParam));
				}
				Shutdown(1);
			}
		}

		public bool UseExceptionFileVersioning { get; set; }

		public static void SaveException(Exception ex, bool useVersioning = false, string name = "")
		{
			if (string.IsNullOrEmpty(name)) name = "LastException";
			name += ".txt";
			string fpath = useVersioning ? VersionedFiles.NextVersionedPath(Environment.CurrentDirectory, name) : name;
			File.WriteAllText(fpath, ex.ToString());
		}

		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);
			DispatcherUnhandledException += HandleDispatcherUnhandledException;
			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
		}

		private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			if (e.ExceptionObject is Exception ex) SaveException(ex, UseExceptionFileVersioning);
		}

		protected virtual void HandleDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
		{
			SaveException(e.Exception, UseExceptionFileVersioning);
		}

		public static string ApplicationDataFolder
		{
			get
			{
				string exeName = Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().ProcessName);
				string r = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), exeName);
				if (!Directory.Exists(r)) Directory.CreateDirectory(r);
				return r;
			}
		}

		protected override void OnExit(ExitEventArgs e)
		{
			base.OnExit(e);
			_unique.Dispose();
		}

		/// <summary>
		/// Used to match a window handle with the known application.
		/// </summary>
		/// <param name="testWindowTitle"></param>
		/// <returns></returns>
		protected virtual bool IsWindowTitle(string testWindowTitle)
		{
			return Process.GetCurrentProcess().ProcessName == testWindowTitle;
		}
	}
}
