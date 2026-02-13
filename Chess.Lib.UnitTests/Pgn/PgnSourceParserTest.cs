using Chess.Lib.Moves.Parsing;
using Chess.Lib.Pgn;
using Chess.Lib.Pgn.Parsing;

namespace Chess.Lib.UnitTests.Pgn
{
	[TestClass]
	[DeploymentItem("Pgn/twic920.pgn")]
	[DeploymentItem("Pgn/Wang-Jakovenko.txt")]
	public class PgnSourceParserTest
	{
		private const string WJFileName = "Wang-Jakovenko.txt";

		[TestMethod]
		public void ParseFromFile()
		{
			var result = PgnSourceParser.ParseFromFile(WJFileName).ToList();
			Assert.HasCount(1, result);
			switch (result[0])
			{
				case IPgnParseSuccess s:
					Assert.AreEqual("Wang Hao", s.Import.WhiteName);
					Assert.AreEqual("Jakovenko,D", s.Import.BlackName);
					Assert.AreEqual(GameResult.Draw, s.Import.Result);
					Assert.AreEqual("St Petersburg RUS", s.Import.Site);
					Assert.HasCount(17, s.Import.Tags);
					Assert.AreEqual(WJFileName, s.Import.SourceInfo.Name);
					break;
				case IPgnParseError e: Assert.Fail(e.ErrorType.ToString()); break;
			}
		}

		[TestMethod]
		public void ExportPgn()
		{
			string pgn = File.ReadAllText(WJFileName);
			var result = PgnSourceParser.Parse(pgn);
			Assert.IsTrue(result.Succeeded);
			var succ = result as IPgnParseSuccess;
			Assert.IsNotNull(succ);
			string pgn2 = PGN.ToPgn(succ.Import.Tags, succ.Import.Moves);

			// Cannot guarantee exactly same formatting of moves, but presence (not order) of tags is predictable:
			var result2 = PgnSourceParser.Parse(pgn2);
			Assert.IsTrue(result2.Succeeded);
			var succ2 = result2 as IPgnParseSuccess;
			Assert.IsNotNull(succ2);
			Assert.HasCount(succ.Import.Tags.Count, succ2.Import.Tags);
			Assert.IsTrue(succ.Import.Tags.Keys.All(k => succ2.Import.Tags.ContainsKey(k)));

			// Verify that moves are identical:
			AlgebraicMoves ams1 = AlgebraicMoves.Create(succ.Import.Moves), ams2 = AlgebraicMoves.Create(succ2.Import.Moves);
			Assert.AreEqual(ams1.MoveCount, ams2.MoveCount);
			Assert.IsTrue(ams1.MoveList.SequenceEqual(ams2.MoveList));
		}

		[TestMethod]
		public void ParseMultipleFromFile()
		{
			List<IPgnParseResult> results = PgnSourceParser.ParseFromFile("twic920.pgn").ToList();
			Assert.HasCount(2245, results);
			for(int i=0;i<results.Count;++i)
			{
				var res = results[i];
				IPgnParseSuccess? succ = res as IPgnParseSuccess;
				Assert.IsNotNull(succ, $"Parsed @ {i}");
				Assert.AreEqual("twic920.pgn", succ.Import.SourceInfo.Name);
			}
		}
	}
}
