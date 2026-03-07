using Chess.Lib.Games;
using Chess.Lib.Hardware;
using Chess.Lib.Moves;
using Chess.Lib.Moves.Parsing;
using File = Chess.Lib.Hardware.File;

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
		public async Task RepetitionCounts()
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
				switch (await g.NextPlayer.AttemptMove(moves[i]))
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

		[TestMethod]
		public void CastlingSet()
		{
			var g = GameDB.Get(28);
			var game = AlgebraicMoves.Parse(g.Moves);
			IChessMove m = game.Moves.First(m => m.SerialNumber == 11);
			Assert.IsTrue(m.IsKingsideCastle);
			Assert.IsNotNull(m.Castle);
			Assert.AreEqual(Rank.R8, m.Castle.RookOrigin.Rank);
			Assert.AreEqual(File.H, m.Castle.RookOrigin.File);
			Assert.AreEqual(Rank.R8, m.Castle.RookDestination.Rank);
			Assert.AreEqual(File.F, m.Castle.RookDestination.File);
			m = game.Moves[12];
			Assert.IsTrue(m.IsKingsideCastle);
			Assert.IsNotNull(m.Castle);
			Assert.AreEqual(Rank.R1, m.Castle.RookOrigin.Rank);
			Assert.AreEqual(File.H, m.Castle.RookOrigin.File);
			Assert.AreEqual(Rank.R1, m.Castle.RookDestination.Rank);
			Assert.AreEqual(File.F, m.Castle.RookDestination.File);
		}

		[TestMethod]
		public void AffectedSquares()
		{
			var g = GameDB.Get(28);
			AlgebraicMoves ams = AlgebraicMoves.Create(g);
			ams.Parse(pi =>
			{
				int n = pi.Move.AffectedSquares().Count();
				if (pi.Move.IsCastle) Assert.AreEqual(4, n, "4 affected squares for castle");
				if (pi.Move.IsEnPassant) Assert.AreEqual(3, n, "3 squares for en-passant");
				return true;
			});
		}

		[TestMethod]
		public void Player()
		{
			List<MoveRequest> requests = MoveRequest.ParseMoves("d2d4d7d5c2c4e7e6").ToList();
			IInteractiveChessGame g = new InteractiveGame();
			foreach (var mr in requests)
			{
				IChessPlayer p = g.NextPlayer;
				switch(p.AttemptMove(mr))
				{
					case IMoveAttemptFail f: Assert.Fail(f.Reason.ToString()); break;
					case IMoveAttemptSuccess s:
						Assert.AreSame(g, s.CompletedMove.MovedPiece.Board.Game);
						Assert.AreSame(p, s.CompletedMove.Player, mr.ToString()); break;
				}
			}
		}

		[TestMethod]
		public async Task CompletedMove()
		{
			var g = GameFactory.CreateInteractive();
			IChessMove? move = null;
			g.MoveCompleted += m =>
			{
				move = m.Move;
			};
			var m = await g.White.AttemptMove("d2d4");
			var msuccess = m as IMoveAttemptSuccess;
			Assert.IsNotNull(msuccess);
			Assert.AreSame(g.LastMoveMade, msuccess.CompletedMove);
			Assert.AreSame(g.Board.LastMove, msuccess.CompletedMove);
		}

		[TestMethod]
		public async Task AffectedSquaresWithEnPassant()
		{
			IInteractiveChessGame ig = GameFactory.CreateInteractive();
			int n = ig.ApplyMoves("1. d4 d5 2. e3 c5 3. Nf3 c4 4. Nc3 Nc6 5. h3 e6 6. Be2 Nf6 7. O-O Bd6 8.a3 O-O 9.Bd2 Bd7 10. b4", MoveFormat.Algebraic);
			Assert.AreEqual(19, n);
			var res = await ig.Black.AttemptMove("cxb3", MoveFormat.Algebraic);
			IMoveAttemptSuccess? ims = res as IMoveAttemptSuccess;
			Assert.IsNotNull(ims);
			Assert.IsTrue(ims.CompletedMove.AffectedSquares().All(sq => sq.Position.IsOnBoard));
		}
	}
}
