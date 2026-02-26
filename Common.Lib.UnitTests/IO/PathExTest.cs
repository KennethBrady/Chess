using Common.Lib.IO;

namespace Common.Lib.UnitTests.IO
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

		[TestMethod]
		public void RandomFileName()
		{
			for(int i=1;i<50;++i)
			{
				string fn = PathEx.RandomFileName(i);
				Assert.AreEqual(i, fn.Length);
				Assert.IsTrue(fn.All(c => PathEx.IsValidFilenameCharacter(c)));
			}
		}

		[TestMethod]
		public void MakeValidFileName()
		{
			string s = PathEx.InvalidNameCharacters;
			Assert.IsFalse(PathEx.IsValidFileName(s));
			string sv = PathEx.MakeValidFileName(s);
			Assert.AreEqual(s.Length, sv.Length);
			Assert.IsTrue(PathEx.IsValidFileName(sv));
		}

		[TestMethod]
		public void AreSameFolder()
		{
			const string DIR = @"C:\Temp\";
			string dir = DIR.Substring(0, DIR.Length - 1);
			Assert.IsTrue(PathEx.AreSameFolder(DIR, dir));
		}
	}
}
