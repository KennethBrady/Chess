using Chess.Lib.Games;

namespace Chess.Lib.UnitTests.Moves
{
	[DeploymentItem("")]
	[TestClass]
	public class ChessMovesTest
	{
		[TestMethod]
		public void MakeEngineMovesUsesPriorMoves()
		{
			var game = GameDB.Get(4044);
			KnownGame g = new KnownGame(game.Moves);
			Assert.AreEqual(83, g.Moves.Count);
			string emoves = g.Moves.ToEngineMoves();
			string[] moves = emoves.Split(' ');
			Assert.AreEqual(g.Moves.Count, moves.Length);
			g.Moves.MoveTo(21);
			emoves = g.Moves.ToEngineMoves();
			moves = emoves.Split(' ');
			Assert.AreEqual(22, moves.Length);
		}
	}
}
