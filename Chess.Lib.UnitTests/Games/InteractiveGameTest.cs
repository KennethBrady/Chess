using Chess.Lib.Games;
using Chess.Lib.Hardware;
using Chess.Lib.Hardware.Pieces;
using Chess.Lib.Hardware.Timing;
using Chess.Lib.Moves;
using Chess.Lib.Moves.Parsing;
using File = Chess.Lib.Hardware.File;

namespace Chess.Lib.UnitTests.Games
{
	[TestClass]
	public class InteractiveGameTest
	{
		[TestMethod]
		public void Defaults()
		{
			IInteractiveChessGame g = new InteractiveGame();
			Assert.HasCount(0, g.Moves);
			Assert.IsNotNull(g.White);
			Assert.IsNotNull(g.Black);
			Assert.IsNotNull(g.Board);
			Assert.HasCount(32, g.Board.ActivePieces);
			Assert.IsNotNull(g.LastMoveMade);
			Assert.AreEqual(NoMove.Default, g.LastMoveMade);
			Assert.IsFalse(g.White.IsReadOnly);
			Assert.IsFalse(g.Black.IsReadOnly);
			Assert.IsTrue(g.White.HasNextMove);
			Assert.IsFalse(g.Black.HasNextMove);
			Assert.HasCount(0, g.Moves);
			Assert.AreEqual(-1, g.Moves.CurrentPosition);
			Assert.IsNotNull(g.Clock);
			Assert.IsTrue(g.Clock.IsNull);
		}

		[TestMethod]
		public void E4()
		{
			IChessGame g = new InteractiveGame();
			IChessMove? lastMove = null;
			g.MoveCompleted += (m) =>
			{
				lastMove = m.Move;
			};
			IChessSquare tosq = g.Board[File.E, Rank.R4], fromsq = g.Board[File.E, Rank.R2];
			Assert.IsFalse(tosq.HasPiece);
			Assert.IsTrue(fromsq.HasPiece);
			IChessPawn pawn = NoPawn.Default;
			switch(fromsq.Piece)
			{
				case IChessPawn p: pawn = p; break;
				default: Assert.Fail("Expected pawn on e2"); break;
			}
			Assert.IsTrue(pawn.CanMoveTo(tosq));
			switch (g.White.AttemptMove("e4", MoveFormat.Algebraic))
			{
				case IMoveAttemptSuccess s:
					{
						Assert.IsFalse(fromsq.HasPiece);
						Assert.IsTrue(tosq.HasPiece);
						Assert.HasCount(1, g.Moves);
						Assert.IsNotNull(lastMove);
						Assert.AreSame(lastMove, s.CompletedMove);
						switch(tosq.Piece)
						{
							case IChessPawn p2:
								Assert.AreSame(pawn, p2);
								break;
							default: Assert.Fail("Expected same pawn moved to e4"); break;
						}
						break;
					}
				case IMoveAttemptFail f: Assert.Fail(f.Reason.ToString()); break;
				default: Assert.Fail("Unknown return"); break;
			}
		}

		[TestMethod]
		public void MakeEngineMoves()
		{
			const string MOVES = "e2e4d7d5g1f3b8c6f1b5c8d7";
			InteractiveGame g = new();
			EngineMoves ems = EngineMoves.Create(MOVES);
			foreach(var m in ems)
			{
				IChessPlayer player = m.Hue == Hue.Light ? g.White : g.Black;
				switch(player.AttemptMove(m))
				{
					case IMoveAttemptSuccess: break;
					case IMoveAttemptFail e: Assert.Fail(e.Reason.ToString()); break;
				}
			}
			System.Diagnostics.Debug.WriteLine(g.Board.Display());
		}

		[TestMethod]
		public void UndoMove()
		{
			IInteractiveChessGame g = GameFactory.CreateInteractive();
			g.White.AttemptMove("e2e4", MoveFormat.EngineCompact);
			var wLast = g.White.LastMoveMade;
			Assert.AreSame(g.LastMoveMade, wLast);
			Assert.IsFalse(wLast is NoMove);
			var bLast = g.Black.LastMoveMade;
			Assert.IsTrue(bLast is NoMove);
			IMoveAttemptSuccess s = (IMoveAttemptSuccess)g.Black.AttemptMove("e7e5", MoveFormat.EngineCompact);
			Assert.IsTrue(g.Black.CanUndo);
			Assert.IsTrue(g.White.HasNextMove);
			Assert.AreSame(s.CompletedMove, g.Black.LastMoveMade);
			Assert.AreSame(g.Black.LastMoveMade, g.LastMoveMade);
			bool undone = false;
			g.MoveUndone += m =>
			{
				Assert.AreSame(s.CompletedMove, m);
				undone = true;
			};
			Assert.IsTrue(g.Black.UndoLastMove());
			Assert.IsTrue(undone);
			Assert.IsTrue(g.Black.HasNextMove);
			Assert.IsFalse(g.White.HasNextMove);
			Assert.AreSame(g.LastMoveMade, g.White.LastMoveMade);
			Assert.IsTrue(g.Black.LastMoveMade is NoMove);
		}

		[TestMethod]
		public async Task AttachClock()
		{
			IInteractiveChessGame g = new InteractiveGame();
			Assert.IsTrue(g.Clock.IsNull);
			TimeSpan ts5 = TimeSpan.FromMinutes(5);
			bool attached = g.AttachClock(new ChessClockSetup(ts5));
			Assert.IsTrue(attached);
			Assert.IsFalse(g.Clock.IsNull);
			Assert.AreEqual(ts5, g.Clock.White.MaxTime);
			Assert.AreEqual(ts5, g.Clock.Black.MaxTime);
			Assert.AreEqual(TimeSpan.Zero, g.Clock.White.Increment);
			Assert.AreEqual(TimeSpan.Zero, g.Clock.Black.Increment);	
			Assert.IsFalse(g.Clock.IsRunning);
			Assert.IsFalse(g.Clock.IsStarted);
			switch(g.White.AttemptMove("e2e4"))
			{
				case IMoveAttemptSuccess mas:
					Assert.IsTrue(g.Clock.IsRunning);
					Assert.IsTrue(g.Clock.IsStarted);
					Assert.IsTrue(g.Clock.White.IsRunning);
					Assert.IsFalse(g.Clock.Black.IsRunning);
					break;
				case IMoveAttemptFail f:
					Assert.Fail(f.Reason.ToString());
					break;
			}
		}
	}
}
