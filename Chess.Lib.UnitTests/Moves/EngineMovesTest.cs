using Chess.Lib.Hardware;
using Chess.Lib.Moves;
using Chess.Lib.Moves.Parsing;

namespace Chess.Lib.UnitTests.Moves
{
	internal static class EMExtensions
	{
		public static IParsedGame Parse(this EngineMoves ems, bool showDiagnostics)
		{
			AlgebraicMoves.ShowDiagnostics = showDiagnostics;
			return ems.Parse();
		}
	}

	[TestClass]
	public class EngineMovesTest
	{
		[TestMethod]
		public void MakeEngineMoves()
		{
			EngineMoves moves = EngineMoves.Create("e2e4d7d5e4d5d8d5b1c3b8c6c3d5");
			Assert.AreEqual(7, moves.MoveCount);
			Assert.IsTrue(moves.All(m => m.Move.Length == 4));
		}

		[TestMethod]
		public void ParseMoves()
		{
			EngineMoves moves = EngineMoves.Create("e2e4d7d5e4d5d8d5b1c3b8c6c3d5");
			switch (moves.Parse(true))
			{
				case IParsedGameFail f: Assert.Fail($"Failed after {f.Moves.Count}: {f.Error.Error}"); break;
				case IParsedGameSuccess s:
					Assert.AreEqual(7, s.Moves.Count);
					break;
			}
		}

		[TestMethod]
		public void MayMatch()
		{
			Assert.IsTrue(EngineMoves.MayBeEngineFormat("e2e4d7d5g1f3b8c6f1b5c8d7"));
		}

		[TestMethod]
		public void PromotionSpecified()
		{
			var g = GameDB.Get(4562027);
			Assert.IsFalse(g.IsEmpty, "Missing game id=4562027");
			AlgebraicMoves ams = AlgebraicMoves.Create(g);
			int nChecked = 0;
			bool parse(ParseIntermediate pi)
			{
				if (pi.Move is AlgebraicMove am && am.IsPromotion)
				{
					IBoard b = (IBoard)pi.Board;
					IChessMove prev = b.LastMove;
					Assert.IsTrue(prev.Promotion.IsValid);
					Assert.AreEqual(5, prev.AsEngineMove.Length);
					Assert.AreEqual('Q', prev.AsEngineMove[4]);
					nChecked++;
				}
				return true;
			}
			ams.Parse(parse);
			Assert.AreEqual(4, nChecked);
		}
	}
}
