using Common.Lib.IO;

namespace Common.Lib.UnitTests.IO
{
	[TestClass]
	public class TempFolderTest
	{
		[TestMethod]
		public void IsCreated()
		{
			using TempFolder tf = new TempFolder();
			Assert.IsTrue(Directory.Exists(tf.FolderPath));
		}

		[TestMethod]
		public void IsRemoved()
		{
			var tf = new TempFolder();
			string path = tf.FolderPath;
			Assert.IsTrue(Directory.Exists(path));
			tf.Dispose();
			Assert.IsFalse(Directory.Exists(path));
		}

		[TestMethod]
		public void PrefixAndNumber()
		{
			using var tf = new TempFolder("Test");
			Assert.IsTrue(Directory.Exists(tf.FolderPath));
			Assert.StartsWith("Test", tf.FolderName);
			string snum = tf.FolderNumber.ToString();
			Assert.EndsWith(snum, tf.FolderName);
		}

		[TestMethod]
		public void ImplicitConversionToString()
		{
			using TempFolder tf = new TempFolder();
			string path = tf;
			Assert.AreEqual(tf.FolderPath, path);
		}
	}
}
