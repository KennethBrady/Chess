using Common.Lib.Contracts;

namespace Common.Lib.IO
{
	/// <summary>
	/// A disposable file for holding temporary data
	/// </summary>
	public class TempFile : Disposable
	{
		/// <summary>
		/// Creates a TempFile with the specified text content.
		/// </summary>
		/// <param name="content"></param>
		/// <returns></returns>
		public static TempFile FromContent(string content)
		{
			TempFile r = new TempFile();
			File.WriteAllText(r.FilePath, content);
			return r;
		}

		/// <summary>
		/// Creates a TempFile with the given text content.
		/// </summary>
		/// <param name="lines"></param>
		/// <returns></returns>
		public static TempFile FromContent(IEnumerable<string> lines)
		{
			TempFile r = new TempFile();
			File.WriteAllLines(r.FilePath, lines);
			return r;
		}

		/// <summary>
		/// Creates a TempFile with the given binary content.
		/// </summary>
		/// <param name="content"></param>
		/// <returns></returns>
		public static TempFile FromContent(byte[] content)
		{
			TempFile r = new TempFile();
			File.WriteAllBytes(r.FilePath, content);
			return r;
		}


		public static TempFile FromSourceContent(string sourcePath, string? destName = null)
		{
			if (string.IsNullOrEmpty(destName)) destName = PathEx.RandomFileName();
			TempFile r = new TempFile(destName);
			File.Copy(sourcePath, r.FilePath, true);
			return r;
		}

		public static TempFile InFolder(string folder)
		{
			return new TempFile(PathEx.RandomFileName(), folder);
		}

		public static TempFile InFolderWithExtension(string folder, string ext)
		{
			if (!string.IsNullOrEmpty(ext) && !ext.StartsWith('.')) ext = '.' + ext;
			string nme = PathEx.RandomFileName() + ext;
			return new TempFile(nme, folder);
		}

		public static TempFile InFolderWithExtension(string filePath)
		{
			string? folder = Path.GetDirectoryName(filePath), ext = Path.GetExtension(filePath);
			if (folder == null) folder = Path.GetTempPath();
			if (ext == null) ext = string.Empty;
			return InFolderWithExtension(folder, ext);
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
				switch (Path.GetDirectoryName(FilePath))
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

		public void CopyTo(string fpath, bool overWrite = false) => File.Copy(FilePath, fpath, overWrite);

		protected override void Dispose(bool disposing)
		{
			if (File.Exists(FilePath))
			{
				try
				{
					File.Delete(FilePath);
				}
				catch(Exception ex)
				{
					System.Diagnostics.Debug.WriteLine($"Unable to delete {nameof(TempFile)}: {ex.Message}");
				}
			}
			base.Dispose(disposing);
		}

		public static implicit operator string(TempFile tempFile) => tempFile.FilePath;
	}
}
