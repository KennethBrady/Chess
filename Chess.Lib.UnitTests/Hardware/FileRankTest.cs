using Chess.Lib.Hardware;
using File = Chess.Lib.Hardware.File;

namespace Chess.Lib.UnitTests.Hardware
{
	[TestClass]
	public class FileRankTest
	{
		[TestMethod]
		public void All()
		{
			Dictionary<int, FileRank> dfrs = new();
			foreach(var fr in FileRank.All)
			{
				int ndx = fr.ToSquareIndex;
				if (dfrs.ContainsKey(ndx)) Assert.Fail($"Duplicate FileRank: {fr}");
				dfrs.Add(ndx, fr);
			}
			Assert.HasCount(64, dfrs);
			Dictionary<FileRank, int> dfrs2 = new();
			foreach(var fr in FileRank.All)
			{
				if (dfrs2.ContainsKey(fr)) Assert.Fail($"Duplicate FileRank: {fr}");
				dfrs2.Add(fr, fr.ToSquareIndex);
			}
			Assert.HasCount(64, dfrs2);
		}

		[TestMethod]
		public void Parse()
		{
			const string MOVES = "D2D4C7C5D4D5E7E5D5E6";
			foreach(var m in MOVES.Chunk(2))
			{
				File f = FileEx.Parse(m[0]);
				Assert.AreNotEqual(File.Offboard, f);
				Rank r = RankEx.Parse(m[1]);
				Assert.AreNotEqual(Rank.Offboard, r);
				string s = new string(m);
				FileRank fr = FileRank.Parse(s);
				Assert.IsFalse(fr.IsOffBoard, s);
				Assert.AreEqual(f, fr.File);
				Assert.AreEqual(r, fr.Rank);
			}
		}
	}
}
