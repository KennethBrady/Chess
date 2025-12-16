using Common.Lib.IO;

namespace Common.Lib.UnitTests
{
	[TestClass]
	public sealed class PathExTest
	{
		[TestMethod]
		public void ValidFileCharsUnique()
		{
			Assert.AreEqual(PathEx.ValidFileCharacters.Length, PathEx.ValidFileCharacters.Distinct().Count());
		}

		[TestMethod]
		public void ValidFileChars()
		{
			List<char> fails = new();
			foreach (char c in PathEx.ValidFileCharacters)
			{
				string fname = c + ".txt";
				string fpath = Path.Combine(Path.GetTempPath(), fname);
				try
				{
					File.WriteAllText(fpath, "hello world");
					File.Delete(fpath);
				}
				catch
				{
					fails.Add(c);
				}
			}
			Assert.IsEmpty(fails, $"{new string(fails.ToArray())}");
		}
	}
}
