using Chess.Lib.Games;

namespace Chess.Lib.UnitTests.Moves
{
	[TestClass]
	public class ChessMovesTest
	{
		[TestMethod]
		public void MakeEngineMovesUsesPriorMoves()
		{
			var game = GameDB.Get(4044);
			KnownGame g = new KnownGame(game.Moves);
			Assert.HasCount(83, g.Moves);
			string emoves = g.Moves.ToEngineMoves();
			string[] moves = emoves.Split(' ');
			Assert.HasCount(g.Moves.Count, moves);
			g.Moves.MoveTo(21);
			emoves = g.Moves.ToEngineMoves();
			moves = emoves.Split(' ');
			Assert.HasCount(22, moves);
		}
	}
}
