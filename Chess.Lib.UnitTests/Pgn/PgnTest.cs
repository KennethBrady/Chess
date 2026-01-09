using Chess.Lib.Moves.Parsing;
using Chess.Lib.Pgn;
using Common.Lib.Extensions;
using System.Text.RegularExpressions;

namespace Chess.Lib.UnitTests.Pgn
{
	[TestClass]
	public class PgnTest
	{
		[TestMethod]
		public void HasAllRequiredTags()
		{
			PGN p = PGN.Empty;
			Assert.IsFalse(p.HasAllRequiredTags);
			foreach(string t in PgnTags.Required)
			{
				p = p with { Tags = p.Tags.Add(t, "x") };
			}
			Assert.IsTrue(p.HasAllRequiredTags);
		}

		[TestMethod]
		public void MovesFormattedCorrectly()
		{
			const string moves = "1. d4 d5 2. c4 Nf6 3. Nf3 Bg4 4. e3 e6 5. Nc3 Bb4 6. Bd2 O-O 7. Bd3 Nc6 8. O-O dxc4 9. Bxc4 Re8 10. h3 Bh5 11. Be2 Ne4 12. Nxe4 Bxd2 13. Nexd2 Qf6 14. a3 a5 15. Bc4 Rad8";
			//AlgebraicMoves.ShowDiagnostics = true;
			switch(AlgebraicMoves.Parse(moves))
			{
				case IParsedGameSuccess s: break;
				case IParsedGameFail f: Assert.Fail(f.Error.Error.ToString()); break;
			}
			
			PGN pgn = PGN.Empty with { Moves = moves };
			string spgn = pgn.ToString();
			int moveStart = spgn.IndexOf("1. d4");
			Assert.IsGreaterThan(0, moveStart);
			string smoves = spgn.Substring(moveStart);
			switch(AlgebraicMoves.Parse(smoves))
			{
				case IParsedGameFail f: Assert.Fail(f.Error.Error.ToString()); break;
			}
			Console.WriteLine(smoves);
		}

		private static Regex _rxTags = new Regex(@"\[(\w+) .+]", RegexOptions.Compiled);

		[TestMethod]
		public void RequiredTagsAreOrdered()
		{
			Dictionary<string, string> tags = PgnTags.Required.OrderBy(t => Random.Shared.Next()).Reverse().ToDictionary(t => t, t => string.Empty);
			PGN pgn = new PGN(tags, "1. e4");
			string spgn = pgn.ToString();
			string[] reqTags = PgnTags.Required.ToArray();
			MatchCollection matches = _rxTags.Matches(spgn);
			Assert.HasCount(reqTags.Length, matches);
			for (int i = 0; i < matches.Count; i++)
			{
				Match match = matches[i];
				string tag = match.Groups[1].Value;
				int ndx = reqTags.IndexOf(tag);
				Assert.AreEqual(i, ndx, $"Tag {tag} is in correct order");
			}
		}

		[TestMethod]
		public void RequiredTagsComeFirst()
		{
			Dictionary<string, string> tags = PgnTags.Required.ToDictionary(t => t, t => string.Empty);
			tags.Add("Test", "Value");
			PGN p = new PGN(tags, "1. e4");
			string spgn = p.ToString();
			int ntest = spgn.IndexOf($"[Test");
			Assert.IsGreaterThan(0, ntest);
			foreach(string req in PgnTags.Required)
			{
				int n = spgn.IndexOf($"[{req}");
				Assert.IsGreaterThan(-1, n);
				Assert.IsLessThan(ntest, n, spgn);
			}
		}


		[TestMethod]
		public void SorterSortsRequiredThenAlpha()
		{
			List<string> stags = PgnTags.Required.ToList();
			stags.Add("Zeta");
			stags.Add("0");
			stags.Add("Kappa");
			stags.Add("Alpha");
			stags.ShuffleInPlace();
			Dictionary<string, string> tags = stags.ToDictionary(s => s, s => s);
			PGN p = new PGN(tags, "1. e4");
			string spgn = p.ToString();
			MatchCollection mtags = _rxTags.Matches(spgn);
			Assert.HasCount(tags.Count, mtags);
			stags.Sort(PgnTags.TagComparer);
			string[] reqTags = PgnTags.Required.ToArray();
			List<string> optionals = stags.Skip(PgnTags.RequiredTagCount).ToList();
			for (int i = 0; i < mtags.Count; i++)
			{
				Match m = mtags[i];
				string stag = m.Groups[1].Value;
				int tagNdx = stags.IndexOf(stag);
				Assert.IsGreaterThan(-1, tagNdx);
				if (i < PgnTags.RequiredTagCount)
				{
					Assert.AreEqual(i, tagNdx);
					Assert.AreEqual(i, reqTags.IndexOf(stag));
				} else
				{
					int ndx = optionals.IndexOf(stag);
					Assert.IsGreaterThan(-1, ndx);
					Assert.AreEqual(i, ndx + PgnTags.RequiredTagCount);
				}
			}
		}

	}
}
