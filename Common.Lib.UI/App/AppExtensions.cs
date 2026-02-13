using Common.Lib.IO;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace Common.Lib.UI.App
{
	/// <summary>
	/// Specifies crash-handling for the Application
	/// </summary>
	/// <param name="UseFileVersioning">If set, a new file-version will be created for each exception.</param>
	/// <param name="AppendToFile">
	/// If true, exceptions will be appended to the current file if it already exists.
	/// This behavior overrides UseFileVersioning.
	/// </param>
	/// <param name="ExceptionFileName">The name of the exception file. The default is LastException.txt.</param>
	public record struct AppExceptionHandling(bool UseFileVersioning, bool AppendToFile, string ExceptionFileName)
	{
		public const string DefaultFileName = "LastException.txt";
		public static AppExceptionHandling Default = new AppExceptionHandling(true, false, DefaultFileName);
		internal string FilePath
		{
			get
			{
				string fname = string.IsNullOrEmpty(ExceptionFileName) ? DefaultFileName : ExceptionFileName;
				if (UseFileVersioning && !AppendToFile)
				{
					return VersionedFiles.NextVersionedPath(Environment.CurrentDirectory, fname);
				}
				return Path.Combine(Environment.CurrentDirectory, fname);
			}
		}
	}

	public static class AppExtensions
	{
		private const string UniqueKey = nameof(Unique);
		extension(Application app)
		{
			public void HandleUnhandledExceptions() => app.HandleUnhandledExceptions(AppExceptionHandling.Default);

			public void HandleUnhandledExceptions(AppExceptionHandling handling)
			{
				app.DispatcherUnhandledException += (o, e) =>
				{
					HandleException(e.Exception, handling);
				};
				AppDomain.CurrentDomain.UnhandledException += (o, e) =>
				{
					if (e.ExceptionObject is Exception ex) HandleException(ex, handling);
				};
			}

			public void SaveException(Exception ex, bool useVersioning = false, string name = "", bool append = false)
				=> HandleException(ex, new AppExceptionHandling(useVersioning, append, name));

			public bool EnsureUnique(bool autoFlash = false, bool bringToTop = false)
			{
				var un = new Unique(Process.GetCurrentProcess().ProcessName, autoFlash, bringToTop);
				if (un.IsAlreadyRunning)
				{
					app.Shutdown();
					return false;
				}
				else
				{
					app.Properties.Add(UniqueKey, un);
					return true;
				}
			}
		}

		private static void HandleException(Exception exception, AppExceptionHandling handling)
		{
			string fpath = handling.FilePath;
			if (handling.AppendToFile)
			{
				File.AppendAllText(fpath, exception.ToString());
			}
			else
			{
				File.WriteAllText(fpath, exception.ToString());
			}
		}
	}
}
