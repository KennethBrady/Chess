using Common.Lib.IO;
using CommonApps.Lib.Console;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CommonApps.Lib
{
	public static class MariaDB
	{
		public record struct BackupResult(bool Succeeded, string Result, string ResultFilePath);

		static MariaDB()
		{
			int majorVersion = 0, minorVersion = 0;
			string mdbFolder = string.Empty;
			foreach (string folder in Directory.GetDirectories(PROGRAMFILES, "MariaDB*"))
			{
				//C:\Program Files\MariaDB 10.3
				//C:\Program Files\MariaDB 10.6
				Match m = _versionRx.Match(folder);
				if (m.Success && m.Groups.Count == 3)
				{
					int maj = int.Parse(m.Groups[1].Value), min = int.Parse(m.Groups[2].Value);
					if ((maj >= majorVersion) || (maj == majorVersion && min > minorVersion))
					{
						mdbFolder = folder;
						majorVersion = maj;
						minorVersion = min;
					}
				}
			}
			InstallFolder = mdbFolder;
			MajorVersion = majorVersion;
			MinorVersion = minorVersion;
		}

		private static void VerifyInstalled()
		{
			if (!IsInstalled) throw new InvalidOperationException($"MariaDB is not installed.");
		}

		public const string ServiceName = "MariaDB";
		public const string BackupFileExtension = ".sql";

		private const string PROGRAMFILES = AppCommon.ProgramFiles;
		private static readonly Regex _versionRx = new Regex(@"(\d+)\.(\d+)", RegexOptions.Compiled);
		public static bool IsInstalled => Directory.Exists(InstallFolder);
		public static readonly string InstallFolder;
		public static readonly int MajorVersion;
		public static readonly int MinorVersion;
		public static string MySqlDumpExePath => Path.Combine(InstallFolder, @"bin\mysqldump.exe");
		public static string DataDirectory => Path.Combine(InstallFolder, "data");
		public static string DbDataDirectory(string dbName) => Path.Combine(DataDirectory, dbName);
		public const string MySqlFilePath = @"C:\Program Files\MariaDB 10.9\bin\mysql.exe";

		private const char SQUOTE = '`';
		private const char DQUOTE = '"';

		public static bool IsValidIdentifier(string name)
		{
			// https://dev.mysql.com/doc/refman/8.0/en/identifiers.html
			if (string.IsNullOrEmpty(name)) return false;
			bool isQuoted = false;
			bool testQuoted(char quoteChar)
			{
				if (name[0] == quoteChar)
				{
					if (name.Length < 3) return false;
					if (name.Last() != quoteChar) return false;
					isQuoted = true;
					name = name.Substring(1, name.Length - 3);
				}
				return true;
			}
			if (!testQuoted(SQUOTE)) return false;
			if (!isQuoted && !testQuoted(DQUOTE)) return false;
			if (long.TryParse(name, out _)) return false;
			if (name.Last() == ' ') return false;
			if (isQuoted)
			{
				foreach (char c in name)
				{
					int ic = (int)c;
					if (ic == 0 || ic > 0xffff) return false;
				}
			}
			else
			{
				foreach (char c in name)
				{
					if (char.IsDigit(c)) continue;
					if (char.IsLetter(c)) continue;
					if (c == '$' || c == '_') continue;
					int ic = (int)c;
					if (ic == 0 || ic > 0xffff) return false;
				}
			}
			return true;
		}

		public static async Task<BackupResult>
			CreateVersionedBackup(string outFolder, string dbName)
		{
			VerifyInstalled();
			if (!Directory.Exists(outFolder)) throw new ArgumentException($"Directory '{outFolder}' not found.");
			VersionedFiles files = new VersionedFiles(outFolder, dbName, BackupFileExtension);
			string result = await DoBackup(outFolder, dbName, files.Name);
			string fpath = files.Next;
			bool success = File.Exists(fpath);
			return new BackupResult(success, result, fpath);
			//return (success, result, Path.GetFileName(fpath), fpath);
		}

		// https://dev.mysql.com/doc/refman/8.0/en/mysqldump.html
		private static async Task<string> DoBackup(string folder, string dbName, string fileName)
		{
			var result = await Runner.Execute(MySqlDumpExePath, folder, "--user=root", "--password=abcd", "--opt", "--routines", dbName, $"--result-file={fileName}");
			return result.LineOutput;
		}

		public static async Task<(bool Success, string Output)> CreateBackupFile(string dbName, string outputFilePath)
		{
			VerifyInstalled();
			string folder = Path.GetDirectoryName(outputFilePath)!, fileName = Path.GetFileName(outputFilePath);
			if (!Directory.Exists(folder)) throw new ArgumentException($"F	older '{folder}' not found.");
			string result = await DoBackup(folder, dbName, fileName);
			bool success = File.Exists(outputFilePath);
			return (success, result);
		}

		public static async Task<ConsoleResult> RestoreBackup(string backupFilePath, string dbName, string password)
		{
			if (!File.Exists(backupFilePath)) return ConsoleResult.Empty;
			if (string.IsNullOrEmpty(dbName)) return ConsoleResult.Empty;
			string? dir = Path.GetDirectoryName(backupFilePath);
			if (string.IsNullOrEmpty(dir)) return ConsoleResult.Empty;
			return await Runner.Execute(MySqlFilePath, dir, dbName, $"--database={dbName}", "--user=root", $"--password={password}");
		}
	}
}
