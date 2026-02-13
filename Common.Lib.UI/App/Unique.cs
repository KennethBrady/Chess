using Common.Lib.UI.Win32;
using System.Diagnostics;

namespace Common.Lib.UI.App
{
	/// <summary>
	/// Represents a windowed application that allows only one running instance
	/// </summary>
	public class Unique : IDisposable
	{
		private Mutex? _appMutex;
		private Process? _running;
		public Unique(string uniqueAppName, bool autoFlash = true, bool bringToTop = false)
		{
			AppName = uniqueAppName;
			_appMutex = new Mutex(false, AppName, out bool isNew);
			IsAlreadyRunning = !isNew;
			if ((autoFlash || bringToTop) && IsAlreadyRunning) FlashWindow(bringToTop);
		}

		public bool IsAlreadyRunning { get; init; }

		public void FlashWindow(bool bringToTop = false)
		{
			if (!IsAlreadyRunning) return;
			if (GetRunningProcess() is Process p)
			{
				if (bringToTop) NativeMethods.ActivateWindow(p.MainWindowHandle);
				else
					NativeMethods.FlashWindow(p.MainWindowHandle, true);
			}
		}

		public Process? GetRunningProcess()
		{
			if (!IsAlreadyRunning) return null;
			if (_running != null) return _running;
			Process current = Process.GetCurrentProcess();
			foreach (Process p in Process.GetProcesses())
			{
				if (p.ProcessName.EndsWith("vshost")) continue;
				if (p.Id == current.Id) continue;
				if (p.ProcessName.StartsWith(AppName))
				{
					_running = p;
					return _running;
				}
			}
			return null;
		}

		public string AppName { get; init; }

		public void Dispose()
		{
			if (_appMutex != null)
			{
				_appMutex.Close();
				_appMutex = null;
			}
		}
	}

}
