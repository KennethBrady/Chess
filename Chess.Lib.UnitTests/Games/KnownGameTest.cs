using Chess.Lib.Games;
using Chess.Lib.Hardware;
using Chess.Lib.Moves;
using Chess.Lib.Moves.Parsing;

namespace Chess.Lib.UnitTests.Games
{
	[DeploymentItem("games.txt")]
	[TestClass]
	public class KnownGameTest
	{
		private static void TestCommon(IKnownChessGame g, int expectedMoveCount = -1, int expectedPieceCount = 32)
		{
			if (expectedMoveCount >= 0) Assert.HasCount(expectedMoveCount, g.Moves);
			Assert.IsNotNull(g.White);
			Assert.IsTrue(g.White is not NoPlayer);
			Assert.IsNotNull(g.Black);
			Assert.IsTrue(g.Black is not NoPlayer);
			Assert.IsTrue(g.White.IsReadOnly);
			Assert.IsTrue(g.Black.IsReadOnly);
			Assert.IsTrue(g.Board is IBoard);
			if (expectedPieceCount < 32) Assert.HasCount(expectedPieceCount, g.Board.ActivePieces);
		}

		[TestMethod]
		public void Defaults()
		{
			IKnownChessGame g = new KnownGame(string.Empty);
			TestCommon(g, 0);
			Assert.HasCount(0, g.Moves);
			Assert.IsNotNull(g.White);
			Assert.IsTrue(g.White is not NoPlayer);
			Assert.IsNotNull(g.Black);
			Assert.IsTrue(g.Black is not NoPlayer);
			Assert.IsTrue(g.White.IsReadOnly);
			Assert.IsTrue(g.Black.IsReadOnly);
			Assert.IsTrue(g.Board is IBoard);
			Assert.HasCount(32, g.Board.ActivePieces);
			Assert.IsTrue(g.LastMoveMade is NoMove);
		}

		[TestMethod]
		public void WithMoves()
		{
			IKnownChessGame g = new KnownGame("1. e4 c6 2. Nc3 d5 3. Nf3 Bg4 4. h3 Bh5 5. d4 e6 6. Bd3 Bb4 7. O-O Ne7 8. a3 Ba5 9. Bg5 O-O 10. Re1 f6 11. Bh4 Ng6 12. Bg3 f5 13. exf5 exf5 14. Re6 f4 15. Bxg6 Bxf3 16. Qxf3 fxg3 17. Bxh7+ Kxh7 18. Qh5+ Kg8 19. Rh6 gxf2+ 20. Kf1 gxh6 21. Qg6+ Kh8 22. Qxh6+ Kg8 23. Qg6+ Kh8 24. Qh6+ Kg8 1/2-1/2");
			Assert.HasCount(48, g.Moves);
			Assert.AreEqual(GameResult.Draw, g.Result);
			IEnumerable<IChessMove> castles = g.Moves.Where(m => m.IsCastle);
			Assert.AreEqual(2, castles.Count());
			Assert.IsFalse(g.White.HasNextMove);
			Assert.IsFalse(g.Black.HasNextMove);
			Assert.AreEqual(47, g.LastMoveMade.SerialNumber);
		}

		[TestMethod]
		public void MoveToStart()
		{
			IKnownChessGame g = new KnownGame("e2e4d7d5g1f3b8c6f1b5c8d7");
			Assert.HasCount(6, g.Moves);
			Assert.AreEqual(5, g.LastMoveMade.SerialNumber);
			Assert.AreEqual(3, g.LastMoveMade.Number.GameMoveNumber);
			Assert.AreEqual(5, g.LastMoveMade.SerialNumber);
			g.Moves.MoveToStart();
			Assert.AreEqual(-1, g.LastMoveMade.SerialNumber);
		}

		[TestMethod]
		public void ChangeMovePointer2()
		{
			var pgn = GameDB.Get(23113);
			IReadOnlyChessGame g = new KnownGame(pgn.Moves);
			IReadOnlyList<IChessMove> moves = g.Moves;
			for (int i=0;i<moves.Count;++i)
			{
				IChessMove m = g.Moves.MoveTo(i);
				Assert.AreSame(m, moves[i]);
			}
		}

		internal static string LoadEngineMovesFor(int gameId)
		{
			var g = GameDB.Get(gameId);
			if (g.IsEmpty) Assert.Fail($"Missing game id: {gameId}");
			KnownPgnGame kg = new KnownPgnGame(g);
			return string.Concat(kg.Moves.Select(m => m.AsEngineMove));
		}

		[TestMethod]
		public async Task Branch()
		{
			KnownGame g = new KnownGame(LoadEngineMovesFor(23118));
			IGame g2 = (IGame)g.Branch();
			Assert.IsNotNull(g2);
			Assert.IsFalse(g2.IsReadOnly);
			Assert.AreEqual(g2.LastMoveMade.SerialNumber, g2.MoveList.Count - 1);
			g.Moves.MoveTo(50);
			g2 = (IGame)g.Branch();
			Assert.IsNotNull(g2);
			Assert.IsFalse(g2.IsReadOnly);
			Assert.AreEqual(g2.LastMoveMade.SerialNumber, g2.MoveList.Count - 1);
			Assert.IsTrue(g2.Black.HasNextMove);
			Assert.HasCount(51, g2.MoveList);
			Assert.AreEqual(50, g2.LastMoveMade.SerialNumber);
			switch(await g2.Black.AttemptMove(new MoveRequest("e7b4")))
			{
				case IMoveAttemptSuccess s: 
					Assert.IsTrue(s.Succeeded);
					Assert.AreSame(s.CompletedMove, g2.LastMoveMade);
					Assert.AreEqual(s.CompletedMove.SerialNumber, g2.MoveList.Count - 1);
					Assert.IsTrue(s.CompletedMove.IsCapture);
					break;
				case IMoveAttemptFail f: Assert.Fail($"{f.Reason}: {f.ParseError}"); break;
			}
			Assert.HasCount(52, g2.MoveList);
			Assert.AreEqual(51, g2.LastMoveMade.SerialNumber);
		}
	}
}
