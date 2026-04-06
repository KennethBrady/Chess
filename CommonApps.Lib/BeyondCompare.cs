using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace CommonApps.Lib
{
	public static class BeyondCompare
	{
		public enum Result
		{
			Success = 0,
			BinarySame = 1,
			RulesBasedSame = 2,
			BinaryDifferences = 11,
			Similar = 12,
			RulesBasedDifferences = 13,
			ConflictsDetected = 14,
			Error = 100,
			ConflictsDetectedMergeOutputNotSaved = 101,
			MoreErrors
		}

		static BeyondCompare()
		{
			void setInstallFolder(string installFolder)
			{
				InstallFolder = installFolder;
				string gui = Path.Combine(InstallFolder, "BComp.exe");
				if (File.Exists(gui)) GuiExecutablePath = gui;
				string com = Path.Combine(InstallFolder, "BComp.com");
				if (File.Exists(com)) ComExecutablePath = com;
			}
			string appDir = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
			string[] folders = Directory.GetDirectories(appDir, "Beyond Compare*");
			IsInstalled = folders.Length > 0;
			if (folders.Length == 1) setInstallFolder(folders[0]);
			else
			{
				int highest = 0;
				string latest = string.Empty;
				foreach (string fpath in folders)
				{
					string f = fpath.Trim();
					int n = f.LastIndexOf(' ');
					if (n > 0)
					{
						string vstr = f.Substring(n + 1);
						if (int.TryParse(vstr, out int vno) && vno > highest)
						{
							highest = vno;
							latest = fpath;
						}
					}
				}
				if (highest > 0) setInstallFolder(latest);
			}
			if (!IsInstalled) return;
			FileVersion = FileVersionInfo.GetVersionInfo(GuiExecutablePath);
		}


		public static bool IsInstalled { get; private set; }
		public static string InstallFolder { get; private set; } = string.Empty;
		public static FileVersionInfo? FileVersion { get; private set; }
		public static string GuiExecutablePath { get; private set; } = string.Empty;
		public static string ComExecutablePath { get; private set; } = string.Empty;

		public static Result Compare(string fileLeft, string fileRight)
		{
			if (fileLeft.Contains(" ")) fileLeft = $"\"{fileLeft}\"";
			if (fileRight.Contains(" ")) fileRight = $"\"{fileRight}\"";
			ProcessStartInfo psi = new ProcessStartInfo
			{
				FileName = ComExecutablePath,
				Arguments = string.Join(" ", fileLeft, fileRight, "/qc=binary"),
				CreateNoWindow = true,
				WindowStyle = ProcessWindowStyle.Hidden,
				UseShellExecute = false
			};
			int exitCode;
			using (Process p = new Process { StartInfo = psi })
			{
				p.Start();
				p.WaitForExit();
				exitCode = p.ExitCode;
			}
			if (exitCode <= 101) return (Result)exitCode;
			return Result.MoreErrors;
		}

		public static Task<Result> CompareAsync(string fileLeft, string fileRight)
		{
			Result compare() => Compare(fileLeft, fileRight);
			return Task<Result>.Factory.StartNew(compare);
		}
	}
}
