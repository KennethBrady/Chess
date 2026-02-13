using Common.Lib.Extensions;
using Common.Lib.IO;

namespace Common.Lib.UnitTests.Extensions
{
	[TestClass]
	public class PathExtensionsTest
	{
		[TestMethod]
		public void ClearFolder()
		{
			using TempFolder tf = new TempFolder();
			DirectoryInfo d = new DirectoryInfo(tf.FolderPath);
			Assert.HasCount(0, d.EnumerateFiles());
			for(int i=0;i<10;++i)
			{
				string fpath = Path.Combine(tf.FolderPath, $"tmp{i}.txt");
				File.WriteAllText(fpath, " ");
			}
			Assert.HasCount(10, d.EnumerateFiles());
			d.Clear();
			Assert.HasCount(0, d.EnumerateFiles());
		}
	}
}
