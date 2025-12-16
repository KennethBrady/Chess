using Chess.Lib.Games;
using Chess.Lib.Moves;
using Chess.Lib.Moves.Parsing;

namespace Chess.Lib.UnitTests.Moves
{
	[TestClass]
	public class ChessMoveTest
	{
		[TestMethod]
		public void SerialNumberIncreasesForParsedMoves()
		{
			EngineMoves ems = EngineMoves.Create("d2d4d7d5c2c4e7e6");
			int prevNum = 0;
			switch (ems.Parse())
			{
				case IParsedGameSuccess s:
					Assert.HasCount(4, s.Moves);
					foreach (var move in s.Moves)
					{
						Assert.AreEqual(prevNum, move.SerialNumber);
						prevNum++;
					}
					break;
				case IParsedGameFail f: Assert.Fail(f.Error.Error.ToString()); break;
			}
		}

		[TestMethod]
		public void SerialNumberIncreasesInteractive()
		{
			List<MoveRequest> requests = MoveRequest.ParseMoves("d2d4d7d5c2c4e7e6").ToList();
			int nPrev = -1;
			foreach (var request in requests)
			{
				Assert.IsGreaterThan(nPrev, request.SerialNumber);
				nPrev++;
			}
			IInteractiveChessGame g = new InteractiveGame();
			nPrev = -1;
			foreach (var request in requests)
			{
				switch (g.NextPlayer.AttemptMove(request))
				{
					case IMoveAttemptFail f: Assert.Fail($"{request.SerialNumber}: {f.Reason}"); break;
					case IMoveAttemptSuccess s: Assert.AreEqual(++nPrev, s.CompletedMove.SerialNumber); break;
				}
			}
		}

		[TestMethod]
		public void RepetitionCounts()
		{
			IInteractiveChessGame g = new InteractiveGame();
			List<MoveRequest> moves = MoveRequest.ParseMoves("d2d4d7d5b1c3b8c6c3b1c6b8b1c3b8c6c3b1c6b8b1c3b8c6").ToList();
			List<string> boards = new List<string>();
			int lastReps = 0;
			g.MoveCompleted += cm =>
			{
				string pos = cm.Move.MovedPiece.Board.Display();
				int n = boards.BinarySearch(pos);
				if (n < 0)
				{
					boards.Insert(~n, pos);
					Assert.AreEqual(lastReps / 2, cm.RepetitionCount);
				}
				else
				{
					Assert.AreEqual((++lastReps/2), cm.RepetitionCount);
				}
			};
			for (int i = 0; i < moves.Count; i++)
			{
				switch (g.NextPlayer.AttemptMove(moves[i]))
				{
					case MoveAttemptFail f: Assert.Fail($"{i}: {f.Reason}"); break;
					case MoveAttemptSuccess s:
						Assert.AreEqual(i, s.CompletedMove.SerialNumber);
						break;
				}
			}
		}

		[TestMethod]
		public void AlgebraicFromEngineMoves()
		{
			var g = GameDB.Get(4562027);
			var game = GameFactory.CreateKnown(g.Moves);
			string engineMoves = string.Concat(game.Moves.Select(m => m.AlgebraicMove));
			EngineMoves ems = EngineMoves.Create(string.Concat(game.Moves.Select(m => m.AsEngineMove)));
			Assert.AreEqual(4, ems.Where(em => em.IsPromotion).Count());
			IChessGame real = GameFactory.CreateInteractive();
			//int nMove = 0;
			foreach(EngineMove em in ems)
			{
				IChessMove made = game.Moves[em.SerialNumber];
				switch(real.NextPlayer.AttemptMove(new MoveRequest(em.Move)))
				{
					case IMoveAttemptFail f: Assert.Fail($"{f.Reason.ToString()} @ {made.Number.GameMoveNumber}. {made.AlgebraicMove}"); break;
					case IMoveAttemptSuccess s: 
						IChessMove source = game.Moves[em.SerialNumber];
						Assert.AreEqual(source.AlgebraicMove, s.CompletedMove.AlgebraicMove, $"Move {made.SerialNumber}");
						break;
				}
			}
		}
	}
}
