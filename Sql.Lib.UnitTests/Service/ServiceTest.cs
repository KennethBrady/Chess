using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sql.Lib.Services;
using Sql.Lib.UnitTests.TestDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Sql.Lib.UnitTests.Service
{
	[TestClass]
	[DeploymentItem("TestDb\\TestDb.sql")]
	public class ServiceTest
	{
		private static readonly DirectoryInfo CurrentDir = new DirectoryInfo(Environment.CurrentDirectory);
		private MySqlService _service;

		[TestInitialize]
		public async Task InitService()
		{
			_service = await TestDBService.ReCreateAsync();
		}

		[TestMethod]
		public void NewDb()
		{
			var values = _service.LoadAll<FInfo>();
			Assert.HasCount(0, values);
			FInfo info = new FInfo(0, Environment.CurrentDirectory, DateTime.Now, null, false);
			FInfo nuInfo = _service.InsertOne<FInfo>(info);
			Assert.IsNotNull(nuInfo);
			Assert.AreEqual(1, nuInfo.Id);
			double diff = Math.Abs((info.FileDate - nuInfo.FileDate).TotalSeconds);
			Assert.IsLessThan(1e-6, diff);
			FInfo cpy = info with { Id = nuInfo.Id, FileDate = nuInfo.FileDate };
			Assert.AreEqual(nuInfo, cpy);
			Assert.IsFalse(nuInfo.RemoveDate.HasValue);
		}

		[TestMethod]
		public void MultiInsert()
		{
			List<FInfo> finfos = new List<FInfo>();
			foreach(FileInfo f in CurrentDir.Parent.Parent.Parent.EnumerateFiles("*.*", SearchOption.AllDirectories))
			{
				finfos.Add(new FInfo(0, f.FullName, f.CreationTime, null, true));
			}
			List<FInfo> infos2 = _service.Insert(finfos);
			Assert.HasCount(finfos.Count, finfos);
			Assert.IsTrue(infos2.All(f => f.Id > 0));
			Assert.IsTrue(infos2.All(f => File.Exists(f.FilePath)));
		}

		[TestMethod]
		public void UpdateOne()
		{
			FInfo info = new FInfo(0, Environment.CurrentDirectory, DateTime.Now, null, false);
			FInfo nuInfo = _service.InsertOne<FInfo>(info);
			FInfo nunu = nuInfo with { RemoveDate = DateTime.Now };
			Assert.IsTrue(_service.UpdateOne(nunu));
			var all = _service.LoadAll<FInfo>();
			Assert.HasCount(1, all);
			Assert.IsTrue(all[0].RemoveDate.HasValue);
			//Assert.AreEqual(nunu, all[0]);	// Date comparisons fail
		}

		[TestMethod]
		public void UpateMany()
		{
			MultiInsert();
			List<FInfo> all = _service.LoadAll<FInfo>().ToList();
			DateTime then = DateTime.Now.AddDays(-1);
			IEnumerable<FInfo> toUpdate = all.Select(f => f with { Maybe = false, RemoveDate = then });
			Assert.AreEqual(all.Count, _service.Update(toUpdate));
			List<FInfo> all2 = _service.LoadAll<FInfo>().ToList();
			Assert.HasCount(all.Count, all);
			Assert.IsTrue(all2.All(f => !f.Maybe));
			Assert.IsTrue(all2.All(f => f.RemoveDate.HasValue));
			Console.WriteLine(all2.Max(f => (f.RemoveDate.Value - then).TotalSeconds));
			Assert.IsTrue(all2.All(f => Math.Abs((f.RemoveDate.Value - then).TotalSeconds) < 1));
		}

		[TestMethod]
		public void UpdateCompare()
		{
			MultiInsert();
			List<FInfo> all = _service.LoadAll<FInfo>().ToList();
			List<ValuePair<FInfo>> pairs = new List<ValuePair<FInfo>>(all.Count);
			int nDiff = 0;
			foreach(var f in all)
			{
				if (Random.Shared.NextDouble() >= 0.5)
				{
					FInfo nu = f with { RemoveDate = DateTime.Now };
					pairs.Add(new(f, nu));
					nDiff++;
				} else
				{
					FInfo nu = f with { };
					pairs.Add(new(f, nu));
				}
			}
			Assert.AreEqual(nDiff, _service.Update(pairs));
		}

		[TestMethod]
		public void DeleteOne()
		{
			MultiInsert();
			var all = _service.LoadAll<FInfo>();
			all.Shuffle();
			FInfo f = all.First();
			Assert.AreEqual(1, _service.DeleteOne(f));
			all = _service.LoadAll<FInfo>();
			Assert.IsNull(all.FirstOrDefault(ff => ff.Id == f.Id));
		}

		[TestMethod]
		public void DeleteMany()
		{
			MultiInsert();
			var all = _service.LoadAll<FInfo>();
			all.Shuffle();
			Assert.AreEqual(10, _service.Delete(all.Skip(10).Take(10)));
			var all2 = _service.LoadAll<FInfo>();
			Assert.HasCount(all.Count - 10, all2);
		}

		[TestMethod]
		public void CompoundKey()
		{
			FInfo info = new FInfo(0, "test", DateTime.Now, null, true);
			FInfo nu = _service.InsertOne(info);
			GameTag gt = new GameTag(1, nu.Id, "test");
			GameTag gt2 = _service.InsertOne(gt);
			Assert.IsNotNull(gt2);
			Assert.AreEqual(gt, gt2);
			GameTag gt3 = gt2 with { GameId = 3 };
			Assert.IsFalse(_service.UpdateOne(gt3));
			gt3 = gt2 with { Value = "blah" };
			Assert.IsTrue(_service.UpdateOne(gt3));
		}
	}
}
