using System.Text;

namespace Common.Lib.IO
{
	public static class PathEx
	{
		private const string FILECHARS = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
		public static readonly string InvalidNameCharacters = "<>:\"/\\|?*";
		public static string RandomFileName(int length = 12)
		{
			if (length < 1) throw new ArgumentException($"{nameof(length)} must be > 0");
			StringBuilder sb = new StringBuilder(length);
			while (sb.Length < length)
			{
				char c = (char)FILECHARS[Random.Shared.Next(FILECHARS.Length)];
				sb.Append(c);
			}
			return sb.ToString();
		}

		public static bool IsValidFileName(string fileName)
		{
			if (string.IsNullOrEmpty(fileName)) return false;
			if (InvalidNameCharacters.Any(c => fileName.Contains(c))) return false;
			return true;
		}
	}
}
