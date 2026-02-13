using System.Collections;
using System.Collections.Immutable;
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
	public record struct VersionedFile(string Folder, string Name, string Extension, int Version)
	{
		public static readonly VersionedFile Empty = new VersionedFile(string.Empty, string.Empty, string.Empty, 0);
		public string FilePath => Path.Combine(Folder, $"{Name}{Version}.{Extension}");
		public bool Exists => File.Exists(FilePath);

		public static IEnumerable<VersionedFile> InFolder(string folder, string name, string extension)
		{
			if (!Directory.Exists(folder)) yield break;
			if (extension.StartsWith('.')) extension = extension.Substring(1);
			string search = $"{name}*.{extension}";
			Regex rxVersion = new Regex(@$"(.+){name}(\d+).{extension}");
			foreach (string fpath in Directory.EnumerateFiles(folder, search))
			{
				Match m = rxVersion.Match(fpath);
				if (m.Success && int.TryParse(m.Groups[2].Value, out int version))
				{
					yield return new VersionedFile(folder, Path.GetFileNameWithoutExtension(fpath), Path.GetExtension(fpath).Substring(1), version);
				}
			}
		}

		public static IEnumerable<VersionedFile> FromPath(string filePath)
		{
			string? dir = Path.GetDirectoryName(filePath), name = Path.GetFileNameWithoutExtension(filePath),
				ext = Path.GetExtension(filePath);
			if (string.IsNullOrEmpty(name)) return Enumerable.Empty<VersionedFile>();
			if (string.IsNullOrEmpty(dir)) dir = Environment.CurrentDirectory;
			if (string.IsNullOrEmpty(ext)) ext = string.Empty;
			name = VersionedFile.StripDigits(name);
			return InFolder(dir, name, ext);
		}

		internal static string StripDigits(string name)
		{
			while (name.Length > 0 && char.IsDigit(name.Last())) name = name.Substring(0, name.Length - 1);
			return name;
		}
	}

	public record VersionedFiles(string Folder, string Name, string Extension, ImmutableList<VersionedFile> Files) : IEnumerable<VersionedFile>
	{
		public static readonly VersionedFiles Empty = new VersionedFiles(string.Empty, string.Empty, string.Empty, ImmutableList<VersionedFile>.Empty);
		public VersionedFiles(string folder, string name, string extension) :
			this(folder, name, extension, ImmutableList<VersionedFile>.Empty.AddRange(VersionedFile.InFolder(folder, name, extension)))
		{ }
		public static VersionedFiles FromFile(string exampleFilePath)
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
				return Path.Combine(Folder, $"{Name}{nNxt}.{Extension}");
			}
		}

		public static string NextVersionedPath(string fpath) => FromFile(fpath).Next;

		public static string NextVersionedPath(string folder, string file) => NextVersionedPath(Path.Combine(folder, file));

		public bool IsEmpty => Files.IsEmpty;
		public int Count => Files.Count;

		#region IEnumerable<VersionedFile> implementation

		IEnumerator<VersionedFile> IEnumerable<VersionedFile>.GetEnumerator() => Files.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => Files.GetEnumerator();

		#endregion
	}
}
