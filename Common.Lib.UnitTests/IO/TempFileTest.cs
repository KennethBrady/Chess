
using Common.Lib.IO;

namespace Common.Lib.UnitTests.IO
{
	[TestClass]
	public class TempFileTest
	{
		[TestMethod]
		public void FromContent()
		{
			using TempFile tf = TempFile.FromContent("Hello");
			Assert.IsTrue(File.Exists(tf.FilePath));
			string content = File.ReadAllText(tf.FilePath);
			Assert.AreEqual("Hello", content);
			byte[] bitez = [0, 1, 2, 3, 4];
			using TempFile tf2 = TempFile.FromContent(bitez);
			Assert.IsTrue(File.Exists(tf2.FilePath));
			byte[] bitez2 = File.ReadAllBytes(tf2);
			Assert.IsTrue(bitez.SequenceEqual(bitez2));
			string[] linez = { "Hello", "Goodbye" };
			using TempFile tf3 = TempFile.FromContent(linez);
			string[] linez2 = File.ReadAllLines(tf3.FilePath);
			Assert.IsTrue(linez.SequenceEqual(linez2));
		}

		[TestMethod]
		public void FromSourceContent()
		{
			using TempFile tf0 = TempFile.FromContent("Hello");
			using TempFile tf1 = TempFile.FromSourceContent(tf0);
			Assert.IsTrue(File.Exists(tf1));
			string content = File.ReadAllText(tf1);
			Assert.AreEqual("Hello", content);
		}

		[TestMethod]
		public void NotCreatedWithoutContent()
		{
			using TempFile tf = new TempFile();
			Assert.IsFalse(File.Exists(tf.FilePath));
		}

		[TestMethod]
		public void InFolder()
		{
			using TempFolder f = new TempFolder();
			Assert.HasCount(0, f.GetFiles());
			using TempFile tf = TempFile.InFolder(f);
			Assert.HasCount(0, f.GetFiles());
			File.WriteAllText(tf, "Hello");
			Assert.HasCount(1, f.GetFiles());
		}

		[TestMethod]
		public void InFolderWithExtension()
		{
			using TempFolder f = new TempFolder();
			using TempFile tf = TempFile.InFolderWithExtension(f.FolderPath, ".txt");
			Assert.EndsWith(".txt", tf.FilePath);
			Assert.StartsWith(f.FolderPath, tf.FilePath);
		}

		[TestMethod]
		public void InFolderWithExtensionFromPath()
		{
			string fpath = Path.Combine(Environment.CurrentDirectory, $"test.tmp");
			using TempFile tf = TempFile.InFolderWithExtension(fpath);
			Assert.IsFalse(File.Exists(tf.FilePath));
			File.WriteAllText(tf.FilePath, "Hello");
			Assert.IsTrue(File.Exists(tf.FilePath));
			Assert.AreEqual(".tmp", Path.GetExtension(tf.FilePath));
		}

		[TestMethod]
		public void EnsureExists()
		{
			using TempFile tf = new TempFile();
			Assert.IsFalse(tf.Exists);
			tf.EnsureExists();
			Assert.IsTrue(tf.Exists);
		}

		[TestMethod]
		public void ImplicitConversionToString()
		{
			using TempFile tf = new TempFile();
			string path = tf;
			Assert.AreEqual(tf.FilePath, path);
		}

	}
}
