using Chess.Lib.Pgn.DataModel;
using Chess.Lib.Pgn.Service;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySql.Data.MySqlClient;
using Sql.Lib.Services;
using Sql.Lib.UnitTests.TestDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sql.Lib.UnitTests.Service
{
	[DeploymentItem("TestDb\\TestDb.sql")]
	[TestClass]
	public class LoaderTest
	{
		private const string ConnectionString = PgnGameService.ConnectionString;
		private MySqlService _service;

		[TestInitialize]
		public void InitTest()
		{
			_service = new MySqlService(ConnectionString);
		}

		[TestMethod]
		public void LoadPGNGame()
		{
			var game = _service.LoadWhere<PgnGame>("id=1").FirstOrDefault();
			Assert.IsNotNull(game);
			Assert.AreEqual(76131, game.WhiteId);
			Assert.AreEqual(9674, game.BlackId);
			Assert.AreEqual("Prague CZE", game.Site);
			Assert.AreEqual(GameResult.Draw, game.Result);
			Assert.AreEqual(2108, game.OpeningId);
			var games = _service.LoadWhere<PgnGame>("id < 100").ToList();
			Assert.HasCount(99, games);
		}

		[TestMethod]
		public void LoadPlayers()
		{
			var morePlayers = _service.LoadWhere<PgnPlayer>("id <= 500");
			var players = _service.LoadWhere<PgnPlayer>("id between 1 and 10").ToList();
			Assert.HasCount(morePlayers.Where(p => p.Id >= 1 && p.Id <=10).Count(), players);
		}

		[TestMethod]
		public void LoadAllOpenings()
		{
			long n = (long)_service.ExecuteScalar($"select count(*) from {Opening.TableName}");
			List<Opening> openings = _service.LoadAll<Opening>().ToList();
			Assert.HasCount((int)n, openings);
		}


		[TestMethod]
		public void MissingAttribute()
		{
			try
			{
				var nos = _service.LoadAll<NoDBAttr>();
				Assert.Fail("Expected MissingAttributeException");
			}
			catch (MissingAttributeException) { }
		}

		[TestMethod]
		public void NoTable()
		{
			try
			{
				var nos = _service.LoadAll<NoTable>();
				Assert.Fail("Expected MySqlException - missing table");
			}
			catch(MySqlException) { }
		}

		[DBTable(PgnGame.TableName)]
		public record PartialGame(int Id, int WhiteId, int BlackId);

		[DBTable(PgnGame.TableName)]
		public record TooMuchGame(int Id, int WhiteId, int BlackId, bool WhiteWon);

		[TestMethod]
		public void LoaderAcceptConstructorWithMissingFields()
		{
			var loader = _service.CreateLoaderFor<PartialGame>();
			Assert.IsNotNull(loader);
			try
			{
				loader = _service.CreateLoaderFor<TooMuchGame>();
				Assert.Fail("Should not be able to create this loader");
			}
			catch(UnmatchedFieldException ex)
			{
				Assert.AreEqual("WhiteWon", ex.Property.Name);
				Assert.AreEqual("Unable to match property WhiteWon with a database field.", ex.Message);
			}
		}

		[DBTable(PgnGame.TableName)]
		public record PgnGame2(int WhiteId, int BlackId, string Moves, int Status, int SourceId, int SourceIndex, int SourcePos,
		DateTime EventDate, string Site, GameResult Result, int OpeningId);

		[TestMethod]
		public void SeamlesslyConvertIntToEnum()
		{
			var loader = _service.CreateLoaderFor<PgnGame2>();
			Assert.IsNotNull(loader);
			var game = _service.LoadWhere<PgnGame2>("id=1").FirstOrDefault();
			Assert.IsNotNull(game);
			Assert.AreEqual(GameResult.Draw, game.Result);
		}

		public record PgnGame3(int WhiteId, int BlackId, string Moves, int StatusFlag, int SourceId, int SourceIndex, int SourcePos,
		DateTime EventDate, string Site, GameResult Result, int OpeningId): 
			PgnGame2(WhiteId,BlackId,Moves, StatusFlag, SourceId, SourceIndex, SourcePos, EventDate, Site, Result, OpeningId) { }

		[TestMethod]
		public void DerivedClassesNotAllowed()
		{
			try
			{
				var loader = _service.CreateLoaderFor<PgnGame3>();
				Assert.Fail("Should not allow derived class without new DBTableAttribute");
			}
			catch(MissingAttributeException ex) 
			{
				Assert.AreEqual($"Type {nameof(PgnGame3)} is missing the required DBTableAttribute.", ex.Message);
			}
		}

		[TestMethod]
		public async Task MissingFilePathFields()
		{
			var svc = await TestDBService.ReCreateAsync();
			FInfoNoFPF f = new FInfoNoFPF(0, "c:\\temp\\info.txt", DateTime.Now, null, true);
			FInfoNoFPF f2 = svc.InsertOne(f);
			Assert.IsNotNull(f2);
			Assert.AreNotEqual(f.FilePath, f2.FilePath);

			FInfo fi = new FInfo(0, f.FilePath, f.FileDate, f.RemoveDate, f.Maybe);
			FInfo fi2 = svc.InsertOne(fi);
			Assert.IsNotNull(fi2);
			Assert.AreEqual(fi.FilePath, fi2.FilePath);
		}
	}


	public class NoDBAttr { }

	[DBTable("nonexistent_table")]
	public class NoTable { }

	[DBTable("fileinfo")]
	public record FInfoNoFPF(int Id, string FilePath, DateTime FileDate, DateTime? RemoveDate, bool Maybe);
}
