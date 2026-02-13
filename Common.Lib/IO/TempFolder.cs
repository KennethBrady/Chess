using Common.Lib.Contracts;
using Common.Lib.Extensions;

namespace Common.Lib.IO
{
	/// <summary>
	/// Represents a temporary folder in the OS's temporary workspace
	/// </summary>
	public class TempFolder : Disposable
	{
		private string _folder;
		private DirectoryInfo _dinfo;

		/// <summary>
		/// Create a TempFolder.
		/// </summary>
		/// <param name="prefix">Optionally, appends a prefix to the folder name, allowing for easier discovery</param>
		public TempFolder(string? prefix = null)
		{
			FolderNumber = Random.Shared.Next();
			string name = FolderNumber.ToString();
			if (!string.IsNullOrEmpty(prefix)) name = prefix + name;
			_folder = Path.Combine(Path.GetTempPath(), name);
			Directory.CreateDirectory(_folder);
			_dinfo = new DirectoryInfo(_folder);
		}

		/// <summary>
		/// Returns the folder's full path.
		/// </summary>
		public string FolderPath
		{
			get
			{
				CheckNotDisposed();
				return _folder;
			}
		}

		/// <summary>
		/// Returns the folder's name.
		/// </summary>
		public string FolderName => Path.GetFileName(FolderPath);

		/// <summary>
		/// Returns the numbered part of the FolderName.
		/// </summary>
		public int FolderNumber { get; private init; }

		/// <summary>
		/// Remove all files and subfolders from the folder
		/// </summary>
		public void Clear() => _dinfo.Clear();

		public IEnumerable<string> GetFiles(bool recurse = false) => GetFiles("*.*", recurse);

		public IEnumerable<string> GetFiles(string searchPattern, bool recurse)
		{
			CheckNotDisposed();
			SearchOption so = recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
			return Directory.GetFiles(FolderPath, searchPattern, so);
		}

		public IEnumerable<FileInfo> GetFileInfos() => Info.GetFiles();

		public IEnumerable<FileInfo> GetFileInfos(string searchPattern, bool recurse) => Info.GetFiles(searchPattern,
			recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

		public DirectoryInfo Info
		{
			get
			{
				CheckNotDisposed();
				if (_dinfo == null) _dinfo = new DirectoryInfo(FolderPath);
				return _dinfo;
			}
		}

		public void ShowInExplorer() => _dinfo.ShowInExplorer();

		protected override void Dispose(bool disposing)
		{
			if (Directory.Exists(FolderPath))
			{
				try
				{
					Directory.Delete(FolderPath, true);
				}
				catch { }
			}
			base.Dispose(disposing);
		}

		public static implicit operator string(TempFolder folder) => folder.FolderPath;

		public override string ToString() => FolderName;
	}
}
