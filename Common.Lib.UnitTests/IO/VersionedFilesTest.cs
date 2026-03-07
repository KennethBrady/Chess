using Common.Lib.IO;

namespace Common.Lib.UnitTests.IO
{
	[TestClass]
	public class VersionedFilesTest
	{
		[TestMethod]
		public void FromPath()
		{
			using TempVersionedFiles tvfs = new TempVersionedFiles("test", "txt", 10);
			List<VersionedFile> versionedFiles = VersionedFile.FilesFromPath(tvfs.VersionedFiles.Files[0].FilePath).ToList();
			Assert.AreEqual(versionedFiles.Count, tvfs.VersionedFiles.Count());
		}

		[TestMethod]
		public void FromFile()
		{
			using TempVersionedFiles tvfs = new TempVersionedFiles("test", "txt", 10);
			Assert.AreEqual(tvfs.FolderPath, tvfs.VersionedFiles.Folder);
			VersionedFiles vfs2 = VersionedFiles.FromFilePath(tvfs.VersionedFiles.Files[0].FilePath);
			Assert.HasCount(10, vfs2);
		}

		[TestMethod] 
		public void Exists()
		{
			using TempVersionedFiles tvfs = new TempVersionedFiles("temp", "txt", 10);
			Assert.HasCount(10, tvfs.VersionedFiles.Files);
			Assert.IsTrue(tvfs.VersionedFiles.All(f => f.Exists));
		}

		[TestMethod]
		public void StripDigits()
		{
			string result = VersionedFile.StripDigits("abcde52", out int version);
			Assert.AreEqual(52, version);
			Assert.AreEqual("abcde", result);
			result = VersionedFile.StripDigits("ab55cde27", out version);
			Assert.AreEqual("ab55cde", result);
			Assert.AreEqual(27, version);
		}

		[TestMethod]
		public void Next()
		{
			using TempVersionedFiles tvfs = new TempVersionedFiles("test", "txt", 10);
			string fpath = tvfs.VersionedFiles.Next;
			Assert.IsFalse(File.Exists(fpath));
			VersionedFile vnxt = VersionedFile.FromPath(fpath);
			Assert.IsFalse(vnxt.Exists);
		}

		[TestMethod]
		public void EnforceCount()
		{
			using TempVersionedFiles tvfs = new TempVersionedFiles("test", "txt", 10);
			Assert.HasCount(10, tvfs.VersionedFiles);
			VersionedFiles vfs = tvfs.VersionedFiles.EnforceCount(6);
			Assert.HasCount(6, vfs);
			int nExisting = tvfs.VersionedFiles.Where(vf => vf.Exists).Count();
			Assert.AreEqual(6, nExisting);
		}

		[TestMethod]
		public void EnforceMaxAge()
		{
			using TempVersionedFiles tvfs = new TempVersionedFiles("test", "txt", 10);
			VersionedFiles rem = tvfs.VersionedFiles.EnforceMaxAge(TimeSpan.FromDays(4));
			Assert.HasCount(4, rem);
			int nExisting = tvfs.VersionedFiles.Where(vf => vf.Exists).Count();
			Assert.AreEqual(4, nExisting);
		}


		internal class TempVersionedFiles : IDisposable
		{
			internal TempVersionedFiles(string name, string extension, int count)
			{
				Name = name;
				Extension = extension;
				Folder = new TempFolder();
				Files = new List<TempFile>(count);
				DateTime dt = DateTime.Now.AddDays(-10).AddMinutes(30);
				for(int i=0;i<count;++i)
				{
					string fpath = $"{Folder.FolderPath}\\{Name}{i}.{Extension}";
					TempFile tf = TempFile.FromPath(fpath);
					File.WriteAllText(tf.FilePath, " ");
					File.SetCreationTime(tf.FilePath, dt);
					dt = dt.AddDays(1);
					Files.Add(tf);
				}
				VersionedFiles = new VersionedFiles(Folder.FolderPath, Name, Extension);
			}
			public string Name { get; private init; }
			public string Extension { get; private init; }
			public string FolderPath => Folder.FolderPath;

			public VersionedFiles VersionedFiles { get; private init; }

			private TempFolder Folder { get; init; }
			private List<TempFile> Files { get; init; }

			public void Dispose()
			{
				if (Files.Count == 0) return;
				foreach (var tf in Files) tf.Dispose();
				Folder.Dispose();
				Files.Clear();
			}
		}
	}
}
