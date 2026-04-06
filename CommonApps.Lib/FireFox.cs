using Common.Lib.Extensions;
using Common.Lib.UI.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;

namespace CommonApps.Lib
{
	// https://wiki.mozilla.org/Firefox/CommandLineOptions

	public static class FireFox
	{
		public const string ExePath = @"C:\Program Files\Mozilla Firefox\firefox.exe";
		public static bool IsInstalled => File.Exists(ExePath);

		public static void Open(string url)
		{
			Process.Start(ExePath, url);
		}

		public static void OpenSearch(string searchTerm)
		{
			ProcessStartInfo psi = new ProcessStartInfo();
			psi.UseShellExecute = false;
			psi.FileName = ExePath;
			if (searchTerm.Any(c => c == ' ')) searchTerm = searchTerm.DoubleQuoted;
			psi.Arguments = $"-new-tab -search {searchTerm}";
			Process.Start(psi);
		}

		private const string StartPageQuery = @"https://www.startpage.com/do/dsearch?query=";
		public static void StartPageWebSearch(string search)
		{
			string query = string.Concat(StartPageQuery, HttpUtility.UrlEncode(search));
			Open(query);
		}

		public static void LaunchIncognito(string url)
		{
			ProcessStartInfo info = new ProcessStartInfo
			{
				FileName = ExePath,
				Arguments = "-private-window " + url
			};
			Process.Start(info);
		}

		public static bool GetTopmostBrowserTitle(out string title)
		{
			title = string.Empty;
			foreach (Process p in Process.GetProcesses())
			{
				if (!string.Equals("firefox", p.ProcessName)) continue;
				try
				{
					IntPtr prnt = NativeMethods.GetParent(p.MainWindowHandle);
					if (prnt != IntPtr.Zero) continue;
					if (string.IsNullOrEmpty(p.MainWindowTitle)) continue;
					title = p.MainWindowTitle;
					int ndx = title.IndexOf(" — Mozilla Firefox Private Browsing");
					if (ndx > 0) title = title.Substring(0, ndx).Trim();
					return true;
				}
				catch { }
			}
			return false;
		}
	}
}
