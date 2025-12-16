using Common.Lib.IO;

namespace CommonTools.Lib.IO
{
	/// <summary>
	/// A disposable file for holding temporary data
	/// </summary>
	public class TempFile : IDisposable
	{
		private bool _isDisposed;

		public static TempFile FromContent(string content)
		{
			TempFile r = new TempFile();
			File.WriteAllText(r.FilePath, content);
			return r;
		}

		public static TempFile FromContent(IEnumerable<string> lines)
		{
			TempFile r = new TempFile();
			File.WriteAllLines(r.FilePath, lines);
			return r;
		}

		public static TempFile FromSourceContent(string sourcePath, string? destName = null)
		{
			if (string.IsNullOrEmpty(destName)) destName = PathEx.RandomFileName();
			TempFile r = new TempFile(destName);
			File.Copy(sourcePath, r.FilePath, true);
			return r;
		}

		public static TempFile InFolderWithExtension(string? folder, string ext)
		{
			if (folder == null) folder = Path.GetTempPath();
			string nme = PathEx.RandomFileName() + ext;
			return new TempFile(nme, folder);
		}

		public static TempFile InFolderWithExtension(string filePath) => InFolderWithExtension(Path.GetDirectoryName(filePath), Path.GetExtension(filePath));

		public static TempFile InFolder(string folder)
		{
			return new TempFile(PathEx.RandomFileName(), folder);
		}

		public static TempFile WithExtension(string ext)
		{
			string nme = PathEx.RandomFileName() + ext;
			return new TempFile(nme);
		}

		public TempFile() : this(PathEx.RandomFileName()) { }

		public TempFile(string name) : this(name, Path.GetTempPath()) { }

		public TempFile(string name, string folder)
		{
			if (string.IsNullOrEmpty(name)) throw new ArgumentException($"{nameof(name)} cannot be null or empty.");
			if (!PathEx.IsValidFileName(name)) throw new ArgumentException($"'{name}' is not a valid file name.");
			if (!Directory.Exists(folder)) throw new ArgumentException($"Folder '{folder}' not found.");
			Name = name;
			FilePath = Path.Combine(folder, name);
		}

		public string Name { get; private init; }
		public string Folder
		{
			get
			{
				switch(Path.GetDirectoryName(FilePath))
				{
					case null: return Path.GetTempPath();
					case string path: return path;
				}
			}
		}

		public string FilePath { get; private set; }
		public string Extension => Path.GetExtension(FilePath);
		public byte[] ToBytes => Exists ? File.ReadAllBytes(FilePath) : Array.Empty<byte>();

		public Stream OpenWrite() => File.OpenWrite(FilePath);

		public Stream OpenRead() => Exists ? File.OpenRead(FilePath) : Stream.Null;

		public bool Exists => File.Exists(FilePath);

		public bool EnsureExists()
		{
			if (!Exists) File.WriteAllText(FilePath, string.Empty);
			return Exists;
		}

		public void SaveTo(string fpath, bool overWrite = false) => File.Copy(FilePath, fpath, overWrite);

		public void Dispose()
		{
			if (_isDisposed) return;
			if (File.Exists(FilePath))
			{
				try
				{
					File.Delete(FilePath);
				}
				catch(Exception ex)
				{ 
					System.Diagnostics.Debug.WriteLine(ex);
				}
			}
			_isDisposed = true;
		}

		public static implicit operator string(TempFile tempFile) => tempFile.FilePath;
	}
}
