using Common.Lib.Extensions;
using Common.Lib.IO;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CommonApps.Lib
{
	public static class DBBackup
	{
		static DBBackup()
		{
			ExecutablePath = Path.Combine(ExecutableFolder, AppName + AppCommon.ExeExtension);
			IsInstalled = File.Exists(ExecutablePath);
		}

		private const string AppName = "DBBackup";
		private static string ExecutableFolder => Path.Combine(AppCommon.MyAppsFolder, AppName);

		public static readonly string ExecutablePath;
		public static readonly bool IsInstalled;

		public static (bool valid, string reason) AreArgumentsValid(params string[] arguments)
		{
			if (!IsInstalled) return (false, $"{AppName} is not installed");
			if (arguments == null) return (false, "arguments is null");
			if (arguments.Length < 2) return (false, "at least two arguments are required");
			if (string.IsNullOrEmpty(arguments[0])) return (false, "dbname must be provided");
			if (string.IsNullOrEmpty(arguments[1])) return (false, "outputFolder must be provided");
			try
			{
				if (!Directory.Exists(arguments[1])) Directory.CreateDirectory(arguments[1]);
			}
			catch
			{
				return (false, "Output folder could not be created");
			}
			if (arguments.Length >= 3)
			{
				if (!int.TryParse(arguments[2], out int _)) return (false, "3d argument (maxAge) must be an integer");
			}
			if (arguments.Length >= 4)
			{
				if (!int.TryParse(arguments[3], out int _)) return (false, "4th argument (maxCount) must be an integer");
			}

			return (true, string.Empty);
		}

		public static (bool valid, string dbName, string outputFolder, int maxAge, int maxCount, bool notify, bool testRequired)
			ParseArguments(string[] arguments)
		{
			bool extractBool(string arg)
			{
				string sn = arg.ToLower();
				return sn == "1" || sn == "true";
			}
			var valid = AreArgumentsValid(arguments);
			if (!valid.valid) return (false, string.Empty, string.Empty, -1, -1, false, false);
			string dbName = arguments[0], outputFolder = arguments[1];
			int maxAge = -1, maxCount = -1;
			bool notify = false, testRequired = false;
			if (arguments.Length > 2) maxAge = int.Parse(arguments[2]);
			if (arguments.Length > 3) maxCount = int.Parse(arguments[3]);
			if (arguments.Length > 4) notify = extractBool(arguments[4]);
			if (arguments.Length > 5) testRequired = extractBool(arguments[5]);
			return (true, dbName, outputFolder, maxAge, maxCount, notify, testRequired);
		}

		public static void BeginBackup(string dbName, string outputFolder, int maxAgeDays = -1,
			int maxCount = -1, bool notify = true, bool testRequired = false)
		{
			var valid = AreArgumentsValid(dbName, outputFolder);
			if (!valid.valid) throw new ArgumentException(valid.reason);
			if (outputFolder.Contains(' ')) outputFolder = outputFolder.DoubleQuoted;
			ProcessStartInfo psi = new()
			{
				FileName = ExecutablePath,
				Arguments = string.Join(" ", dbName, outputFolder, maxAgeDays, maxCount, notify ? "1" : "0", testRequired ? "1" : "0"),
				CreateNoWindow = true,
				WindowStyle = ProcessWindowStyle.Hidden
			};
			Process.Start(psi);
		}

		public static async Task<bool> RequiresBackup(string dbName, string outputFolder)
		{
			if (string.IsNullOrEmpty(dbName)) throw new ArgumentNullException(nameof(dbName));
			if (!Directory.Exists(outputFolder)) throw new ArgumentException($"Folder '{outputFolder}' does not exist.");
			if (!BeyondCompare.IsInstalled) return true;
			VersionedFiles files = new VersionedFiles(outputFolder, dbName, ".sql");
			if (files.Count == 0) return true;
			var file = files.Last();
			using TempFolder tf = new TempFolder();
			var backupResult = await MariaDB.CreateVersionedBackup(tf.FolderPath, dbName);
			if (!backupResult.Succeeded) return true;
			string[] lines = File.ReadAllLines(file.FilePath);
			using TempFile tfA = TempFile.FromContent(lines.Take(lines.Length - 1));
			lines = File.ReadAllLines(backupResult.ResultFilePath);
			using TempFile tfB = TempFile.FromContent(lines.Take(lines.Length -1));
			BeyondCompare.Result bc = BeyondCompare.Compare(tfA.FilePath, tfB.FilePath);
			return bc != BeyondCompare.Result.BinarySame;
		}
	}
}
