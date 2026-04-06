using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Common.Lib.IO
{
	/// <summary>
	/// Represents file versioned as "C:\Folder\NameV.ext, where V is an integer.
	/// </summary>
	/// <param name="Folder">The folder containing the file</param>
	/// <param name="Name">The root file name (with no final digits)</param>
	/// <param name="Extension">The file extension, with no leading '.'</param>
	/// <param name="Version">The file version number</param>
	[DebuggerDisplay("{Name}{Version}.{Extension}")]
	public record struct VersionedFile(string Folder, string Name, string Extension, int Version)
	{
		public static readonly VersionedFile Empty = new VersionedFile(string.Empty, string.Empty, string.Empty, 0);
		public string FilePath => Path.Combine(Folder, $"{Name}{Version}.{Extension}");
		public bool Exists => File.Exists(FilePath);

		public static VersionedFile FromPath(string filePath)
		{
			string? folder = Path.GetDirectoryName(filePath);
			if (folder == null) folder = string.Empty;
			string name = Path.GetFileNameWithoutExtension(filePath),
				extension = Path.GetExtension(filePath);
			if (extension.StartsWith('.')) extension = extension.Substring(1);
			name = StripDigits(name, out int version);
			return new VersionedFile(folder, name, extension, version < 0 ? 0 : version);
		}

		public static IEnumerable<VersionedFile> InFolder(string folder, string name, string extension)
		{
			if (!Directory.Exists(folder)) yield break;
			if (!extension.StartsWith('.')) extension = '.' + extension;
			string search = $"{name}*{extension}";
			Regex rxVersion = new Regex(@$"(.+)({name})(\d+){extension}");
			foreach (string fpath in Directory.EnumerateFiles(folder, search))
			{
				Match m = rxVersion.Match(fpath);
				if (m.Success && int.TryParse(m.Groups[3].Value, out int version))
				{
					yield return new VersionedFile(folder, name, extension.Substring(1), version);
				}
			}
		}

		public static IEnumerable<VersionedFile> FilesFromPath(string filePath)
		{
			string? dir = Path.GetDirectoryName(filePath), name = Path.GetFileNameWithoutExtension(filePath),
				ext = Path.GetExtension(filePath);
			if (string.IsNullOrEmpty(name)) return Enumerable.Empty<VersionedFile>();
			if (string.IsNullOrEmpty(dir)) dir = Environment.CurrentDirectory;
			if (string.IsNullOrEmpty(ext)) ext = string.Empty;
			name = VersionedFile.StripDigits(name);
			return InFolder(dir, name, ext);
		}

		internal static string StripDigits(string name) => StripDigits(name, out _);		

		internal static string StripDigits(string name, out int version)
		{
			var digits = name.Reverse().TakeWhile(c => char.IsDigit(c)).Reverse();
			string s = new string(digits.ToArray());
			if (int.TryParse(s, out version)) return name.Substring(0, name.Length - s.Length);
			version = -1;
			return name;
		}
	}

	[DebuggerDisplay("{Folder}:{Name}.{Extension}")]
	public record VersionedFiles(string Folder, string Name, string Extension, ImmutableList<VersionedFile> Files) : IEnumerable<VersionedFile>
	{
		public static readonly VersionedFiles Empty = new VersionedFiles(string.Empty, string.Empty, string.Empty, ImmutableList<VersionedFile>.Empty);
		public VersionedFiles(string folder, string name, string extension) :
			this(folder, name, extension, ImmutableList<VersionedFile>.Empty.AddRange(VersionedFile.InFolder(folder, name, extension)))
		{ }
		public static VersionedFiles FromFilePath(string exampleFilePath)
		{
			string? folder = Path.GetDirectoryName(exampleFilePath), name = Path.GetFileNameWithoutExtension(exampleFilePath), ext = Path.GetExtension(exampleFilePath);
			if (string.IsNullOrEmpty(name)) return Empty;
			if (string.IsNullOrEmpty(folder)) folder = Environment.CurrentDirectory;
			if (ext == null) ext = string.Empty;
			if (ext.StartsWith('.')) ext = ext.Substring(1);
			return new(folder, VersionedFile.StripDigits(name), ext);
		}

		public string Next
		{
			get
			{
				int nNxt = Files.Count == 0 ? 0 : Files.Max(f => f.Version) + 1;
				return Path.Combine(Folder, $"{Name}{nNxt}{Extension}");
			}
		}

		public VersionedFiles EnforceMaxAge(TimeSpan maxAge)
		{
			List<VersionedFile> remaining = new();
			foreach (var vf in Files)
			{
				if (vf.Exists)
				{
					TimeSpan ag = DateTime.Now - File.GetLastWriteTime(vf.FilePath);
					if (ag <= maxAge) remaining.Add(vf); else File.Delete(vf.FilePath);
				}
			}
			return this with { Files = remaining.ToImmutableList() };
		}

		public VersionedFiles EnforceCount(int maxFiles)
		{
			IEnumerable<VersionedFile> remaining()
			{
				var left = Files.Where(f => f.Exists).OrderBy(f => File.GetCreationTime(f.FilePath));
				foreach (var vf in left.Skip(maxFiles)) File.Delete(vf.FilePath);
				return left.Take(maxFiles);
			}
			return this with { Files = ImmutableList<VersionedFile>.Empty.AddRange(remaining()) };
		}

		public static string NextVersionedPath(string fpath) => FromFilePath(fpath).Next;

		public static string NextVersionedPath(string folder, string file) => NextVersionedPath(Path.Combine(folder, file));

		public bool IsEmpty => Files.IsEmpty;
		public int Count => Files.Count;

		#region IEnumerable<VersionedFile> implementation

		IEnumerator<VersionedFile> IEnumerable<VersionedFile>.GetEnumerator() => Files.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => Files.GetEnumerator();

		#endregion
	}
}
