using System.IO;

namespace CommonApps.Lib
{
	internal static class Avifil32
	{
		private const string DllFileName = "avifil32.dll";
		private static readonly string DllFilePath;
		static Avifil32()
		{
			DllFilePath = Path.Combine(AppCommon.Win32Folder, DllFileName);
			IsInstalled = File.Exists(DllFilePath);
		}

		public static bool IsInstalled { get; private set; }

	}
}
