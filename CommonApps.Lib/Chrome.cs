using Common.Lib.UI.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CommonApps.Lib
{
	public static class Chrome
	{
		public const string InstallFolder = @"C:\Program Files\Google\Chrome\";
		public const string ApplicationFolder = @"C:\Program Files\Google\Chrome\Application\";
		public const string ExeFileName = "chrome.exe";
		private const string CHROME = "chrome";
		public static readonly string ExecutableFilePath = Path.Combine(ApplicationFolder, ExeFileName);

		public static bool IsInstalled => File.Exists(ExecutableFilePath);

		public static IEnumerable<Process> GetChromeProcesses() => Process.GetProcessesByName(CHROME).Where(p =>
		{
			IntPtr prnt = NativeMethods.GetParent(p.MainWindowHandle);
			return prnt == IntPtr.Zero && !string.IsNullOrEmpty(p.MainWindowTitle);
		});

		public static void Launch(string url) => Process.Start(ExecutableFilePath, url);

		public static void LaunchIncognito(string url)
		{
			ProcessStartInfo psi = new ProcessStartInfo
			{
				FileName = ExecutableFilePath,
				Arguments = url + " --incognito"
			};
			Process.Start(psi);
		}

		public class TitleExtractor
		{
			private static readonly string[] _commonNames = { "view_video.php" };
			private static HttpClient _client;
			static readonly Regex _titleRegex = new Regex(@"<title>(.+)</title>");

			static TitleExtractor()
			{
				HttpClientHandler handler = new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate };
				_client = new HttpClient(handler);
				_client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.106 Safari/537.36");
				_client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
				_client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
				_client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");
			}

			public static bool RequiresTitleExtraction(string url)
			{
				foreach (string name in _commonNames) if (url.IndexOf(name) > 0) return true;
				return false;
			}

			public static Task<(bool required, bool success, string newPath)> ExtractUrlTitle(string url)
			{
				if (RequiresTitleExtraction(url))
				{
					var task = LoadTitle(url);
					task.Wait();
					var res = task.Result;
					if (res.success)
					{
						return Task.FromResult((true, true, res.title));
					}
				}
				return Task.FromResult((false, false, string.Empty));
			}

			public static bool GetTopmostBrowserTitle(out string title)
			{
				title = string.Empty;
				foreach (Process p in Process.GetProcessesByName(CHROME))
				{
					try
					{
						IntPtr prnt = NativeMethods.GetParent(p.MainWindowHandle);
						if (prnt != IntPtr.Zero) continue;
						if (string.IsNullOrEmpty(p.MainWindowTitle)) continue;
						title = p.MainWindowTitle;
						int ndx = title.IndexOf("- Google");
						if (ndx > 0) title = title.Substring(0, ndx).Trim();
						return true;
					}
					catch { }
				}
				return false;
			}

			private static Task<(bool success, string title)> LoadTitle(string url)
			{
				var task = _client.GetAsync(url);
				task.Wait();
				HttpResponseMessage msg = task.Result;
				var content = msg.Content;
				var task2 = content.ReadAsStringAsync();
				task2.Wait();
				string html = task2.Result;
				Match m = _titleRegex.Match(html);
				if (m.Groups.Count == 2)
				{
					string result = m.Groups[1].Value;
					return Task.FromResult((true, result));
				}
				return Task.FromResult((false, string.Empty));
			}

		}
	}
}
