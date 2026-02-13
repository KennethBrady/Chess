using System.Diagnostics;

namespace Common.Lib.Extensions
{
	public static class PathExtensions
	{
		public const string EXPLORER = "Explorer.exe";

		extension(string path)
		{
			public void ShowInExplorer()
			{
				if (File.Exists(path)) new FileInfo(path).ShowInExplorer(); else
					if (Directory.Exists(path)) new DirectoryInfo(path).ShowInExplorer();
			}
		}

		extension(DirectoryInfo dinfo)
		{
			public int Clear()
			{
				int r = 0;
				foreach (FileInfo fi in dinfo.GetFiles())
				{
					try
					{
						fi.Delete();
						r++;
					}
					catch { }
				}
				return r;
			}

			public void ShowInExplorer()
			{
				if (!dinfo.Exists) return;
				ProcessStartInfo psi = new ProcessStartInfo { FileName = EXPLORER, UseShellExecute = false, Arguments = dinfo.FullName };
				Process.Start(psi);
			}

		}

		extension(FileInfo file)
		{
			public void ShowInExplorer()
			{
				if (!file.Exists) return;
				string select = $"/select";
				ProcessStartInfo psi = new ProcessStartInfo { FileName = EXPLORER, UseShellExecute = false, Arguments = $"/select,{file.FullName}"};
				Process.Start(psi);
			}
		}
	}
}
