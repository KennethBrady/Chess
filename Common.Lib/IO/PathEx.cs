using System.Text;

namespace Common.Lib.IO
{
	/// <summary>
	/// A receptacle for path-related functions not available on System.IO.Path
	/// </summary>
	public static class PathEx
	{
		/// <summary>
		/// Legal characters in a (windows) file name.  This may not be exhaustive.
		/// </summary>
		public const string ValidFileCharacters = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz_+&()-=%$#@~`^";  

		public static bool IsValidFilenameCharacter(char c) => ValidFileCharacters.Contains(c);

		public static readonly string InvalidNameCharacters = "<>:\"/\\|?*";

		/// <summary>
		/// Generate a random and valid path string
		/// </summary>
		/// <param name="length"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"></exception>
		public static string RandomFileName(int length = 12)
		{
			if (length < 1) throw new ArgumentException($"{nameof(length)} must be > 0");
			StringBuilder sb = new StringBuilder(length);
			while (sb.Length < length)
			{
				char c = (char)ValidFileCharacters[Random.Shared.Next(ValidFileCharacters.Length)];
				sb.Append(c);
			}
			return sb.ToString();
		}

		public static bool IsValidFileName(string fileName) => !string.IsNullOrEmpty(fileName) && !InvalidNameCharacters.Any(c => fileName.Contains(c));

		public static string MakeValidFileName(string proposedFileName, char replacement = '_')
		{
			if (string.IsNullOrEmpty(proposedFileName)) throw new ArgumentException("Cannot create a valid file name from an empty string.");
			StringBuilder s = new StringBuilder(proposedFileName);
			for (int i = 0; i < proposedFileName.Length; ++i)
			{
				if (!IsValidFilenameCharacter(s[i])) s[i] = replacement;
			}
			if (s.Length > 120) s.Length = 120;
			return s.ToString();
		}

		public static bool AreSameFolder(string pth1, string pth2)
		{
			if (string.IsNullOrEmpty(pth1) || string.IsNullOrEmpty(pth2)) return false;
			if (!Directory.Exists(pth1) || !Directory.Exists(pth2)) return false;
			// Path.GetDirectoryName is incorrect if path is not terminated:
			if (!pth1.EndsWith('\\')) pth1 += '\\';
			if (!pth2.EndsWith('\\')) pth2 += '\\';
			return string.Equals(Path.GetDirectoryName(pth1), Path.GetDirectoryName(pth2), StringComparison.OrdinalIgnoreCase);
		}

	}
}
