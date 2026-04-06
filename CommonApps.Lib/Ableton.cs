using System;
using System.Diagnostics;
using System.IO;

namespace CommonApps.Lib
{
	public static class Ableton
	{
		public const string ExePath = @"C:\ProgramData\Ableton\Live 12 Suite\Program\Ableton Live 12 Suite.exe";
		public static readonly DirectoryInfo DocumentsFolder = new DirectoryInfo(@"C:\Users\kenbr\Documents\Ableton");
		public static readonly DirectoryInfo ProjectFolder = new DirectoryInfo(@"C:\Users\kenbr\Documents\Ableton\Projects");
		public static readonly DirectoryInfo ArchiveFolder = new DirectoryInfo(@"C:\Users\kenbr\Documents\Ableton\Archives");
		public static readonly DirectoryInfo SamplesFolder = new DirectoryInfo(@"C:\Users\kenbr\Documents\Ableton\Samples");
		public const string ProjectFileExtension = ".als";
		public const string AnalysisFileExtension = ".asd";
		public static bool IsInstalled => File.Exists(ExePath);

		static Ableton()
		{
			if (!DocumentsFolder.Exists) DocumentsFolder.Create();
			if (!ProjectFolder.Exists) ProjectFolder.Create();
			if (!ArchiveFolder.Exists) ArchiveFolder.Create();
			if (!SamplesFolder.Exists) SamplesFolder.Create();
		}

		private static void VerifyInstalled()
		{
			if (!IsInstalled) throw new InvalidOperationException("Ableton is not installed.");
		}

		public static void OpenProject(string projectFilePath)
		{
			VerifyInstalled();
			if (!File.Exists(projectFilePath)) throw new FileNotFoundException(projectFilePath);
			if (Path.GetExtension(projectFilePath).ToLower() != ProjectFileExtension) throw new ArgumentException("Invalid Ableton project file.");
			ProcessStartInfo psi = new ProcessStartInfo(ExePath)
			{
				Arguments = projectFilePath,
				UseShellExecute = true
			};
			Process.Start(psi);
		}

	}
}
