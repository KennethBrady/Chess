using System;

namespace CommonApps.Lib
{
	internal static class AppCommon
	{
		public const string ProgramFiles = @"C:\Program Files\";
		public const string ProgramFilesX86 = @"C:\Program Files (x86)\";
		internal const string Win32Folder = @"C:\Windows\System32\";
		internal const string MyAppsFolder = @"C:\Sys\MyApps\";
		internal const string ExeExtension = ".exe";
	}

	public class NotInstalledException : ApplicationException
	{
		private static string MessageFor(string appName, string exePath)
		{
			return $"The application {appName} is not found at path '{exePath}'.";
		}

		internal NotInstalledException(string appName, string exePath) : base(MessageFor(appName, exePath))
		{
			AppName = appName;
			ExePath = exePath;
		}

		public string AppName { get; private init; }
		public string ExePath { get; private init; }
	}
}
