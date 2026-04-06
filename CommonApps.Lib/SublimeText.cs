using System.Diagnostics;
using System.IO;

namespace CommonApps.Lib
{
	public static class SublimeText
	{
		public const string ExePath = @"C:\Program Files\Sublime Text 3\sublime_text.exe";

		public static bool IsInstalled => File.Exists(ExePath);

		public static void Open(string textFilePath)
		{
			Process.Start(ExePath, textFilePath);
		}
	}
}
