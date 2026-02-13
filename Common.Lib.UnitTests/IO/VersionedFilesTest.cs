using Common.Lib.IO;

namespace Common.Lib.UnitTests.IO
{
	[TestClass]
	public class VersionedFilesTest
	{
		private const string T1 = "TestFile1.txt", T2 = "TestFile2.txt";

		[TestInitialize]
		public async Task Init()
		{
			await Task.Delay(10);
			if (!File.Exists(T1)) File.WriteAllText(T1, " ");	// This occasionally threw due (I think) to async issues??
			await Task.Delay(10);
			if (!File.Exists(T2)) File.WriteAllText(T2, " ");
		}


		[TestMethod]
		public void FromPath()
		{
			List<VersionedFile> files = VersionedFile.FromPath(T1).ToList();
			Assert.HasCount(2, files);

		}

		[TestMethod]
		public void FromFile()
		{
			VersionedFiles vfs = VersionedFiles.FromFile(T1);
			Assert.AreEqual(Environment.CurrentDirectory, vfs.Folder);
			Assert.AreEqual("TestFile", vfs.Name);
			Assert.AreEqual("txt", vfs.Extension);
			Assert.HasCount(2, vfs.Files);
			System.Diagnostics.Debug.WriteLine(vfs.Next);
		}

		[TestMethod]
		public void NextVersionedPath()
		{
			string fpath = VersionedFiles.NextVersionedPath(T1);
			Assert.EndsWith("TestFile3.txt", fpath);
			
		}
	}
}
