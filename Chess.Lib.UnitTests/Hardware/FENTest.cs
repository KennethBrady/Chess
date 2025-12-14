using Chess.Lib.Games;
using Chess.Lib.Hardware;
using Chess.Lib.Moves;

namespace Chess.Lib.UnitTests.Hardware
{
	[TestClass]
	public class FENTest
	{
		[TestMethod]
		public void FullMoveCountIncrementsAfterBlackMoves()
		{
			const string MOVES = "c2c4g7g6b1c3f8g7d2d4c7c5d4d5g7c3b2c3f7f5c1d2f5f4e2e4";
			IChessGame g = new InteractiveGame();
			int gameMoveNumber = 0, n = 0;
			foreach (var req in MoveRequest.ParseMoves(MOVES))
			{
				if (n++ % 2 == 0) gameMoveNumber++;
				switch (g.NextPlayer.AttemptMove(req))
				{
					case MoveAttemptSuccess s:
						FEN f = new FEN(g);
						Assert.AreEqual(s.CompletedMove.Number.GameMoveNumber, gameMoveNumber, $"after {n} moves");
						//Assert.AreEqual(gameMoveNumber, f.FullMoveCount);
						break;
					case MoveAttemptFail fail: Assert.Fail(fail.Reason.ToString()); break;
				}
			}
		}

		[TestMethod]
		public void ExportFENNoEnPassant()
		{
			const string MOVES = "1. Nf3 g6 2. e4 c5 3. c4 Bg7 4. d4 cxd4 5. Nxd4 Nc6 6. Be3 Nf6 7. Nc3 O-O 8. Be2 d6";
			IReadOnlyChessGame g = new KnownGame(MOVES);
			g.Moves.MoveToEnd();
			FEN f = new FEN(g);
			Console.WriteLine(f.ToString());
			Assert.AreEqual("r1bq1rk1/pp2ppbp/2np1np1/8/2PNP3/2N1B3/PP2BPPP/R2QK2R w KQ - 0 9", f.ToString());
		}

		[TestMethod]
		public void ExportFENEnPassant()
		{
			const string MOVES = "1. c4 g6 2. Nc3 Bg7 3. d4 c5 4. d5 Bxc3+ 5. bxc3 f5 6. Bd2 f4 7. e4";
			IReadOnlyChessGame g = new KnownGame(MOVES);
			g.Moves.MoveToEnd();
			FEN f = new FEN(g);
			Assert.AreEqual("rnbqk1nr/pp1pp2p/6p1/2pP4/2P1Pp2/2P5/P2B1PPP/R2QKBNR b KQkq e3 0 7", f.ToString());
		}

		[TestMethod]
		public void FenAdjustsToCurrentState()
		{
			const string BENKOG = "1. d4 Nf6 2.c4 c5 3.d5 b5";
			var pgn = GameDB.Get(23113);
			if (pgn == null) return;
			IReadOnlyChessGame g = new KnownGame(pgn.Moves);
			IReadOnlyChessGame g2 = new KnownGame(BENKOG);
			IChessMove m2 = g2.Moves.Last();
			IChessMove m1 = g.Moves.First(mm => mm.SerialNumber == m2.SerialNumber);
			IChessMove m1a = g.Moves.MoveTo(m1.SerialNumber);
			IChessMove m2a = g2.Moves.MoveTo(m2.SerialNumber);
			Assert.AreSame(m1, m1a);
			Assert.AreSame(m2, m2a);
			Assert.AreEqual(16, g.Board.ActivePieces.Where(p => p.Side == Hue.Light).Count());
			Assert.AreEqual(16, g2.Board.ActivePieces.Where(p => p.Side == Hue.Light).Count());
			Assert.AreEqual(g.Board.AsFEN(), g2.Board.AsFEN());
			for(int i = 0; i < g2.Moves.Count; i++)
			{
				g.Moves.CurrentPosition = i;
				g2.Moves.CurrentPosition = i;
				Assert.AreEqual(g.Board.AsFEN(), g2.Board.AsFEN(), $"Move {i}");
				IChessMove m1Last = g.LastMoveMade, m2Last = g2.LastMoveMade;
				Assert.AreEqual(m1Last.SerialNumber, m2Last.SerialNumber);
				FEN f1 = new FEN(g), f2 = new FEN(g2);
				Assert.AreEqual(f1.ToString(), f2.ToString(), $"Move {i}");
			}
		}
	}
}
