using Chess.Lib.Games;
using Chess.Lib.Hardware;
using Chess.Lib.Hardware.Pieces;
using Chess.Lib.Moves;
using Chess.Lib.Moves.Parsing;

namespace Chess.Lib.UnitTests.Games
{
	[TestClass]
	public class ChessPlayerTest
	{
		[TestMethod]
		public void MoveOnlyOnTurn()
		{
			IInteractiveChessGame g = new InteractiveGame();
			switch (g.Black.AttemptMove("e2e4", MoveFormat.EngineCompact))
			{
				case IMoveAttemptFail f: Assert.AreEqual(MoveFailureReasons.WrongPlayer, f.Reason); break;
				case IMoveAttemptSuccess s: Assert.Fail("Expected move failure"); break;
			}
			switch (g.White.AttemptMove("e2e4", MoveFormat.EngineCompact))
			{
				case IMoveAttemptFail f: Assert.Fail("Expected successful move: " + f.Reason.ToString()); break;
				case IMoveAttemptSuccess s:
					Assert.IsNotNull(s.CompletedMove);
					Assert.AreEqual(PieceType.Pawn, s.CompletedMove.MovedPiece.Type);
					break;
			}
			switch (g.White.AttemptMove("e4e5", MoveFormat.EngineCompact))
			{
				case IMoveAttemptSuccess: Assert.Fail("White cannot move"); break;
				case IMoveAttemptFail f: Assert.AreEqual(MoveFailureReasons.WrongPlayer, f.Reason); break;
			}
		}

		[TestMethod]
		public void IllegalMoves()
		{
			IInteractiveChessGame g = new InteractiveGame();
			switch (g.White.AttemptMove("e2e5", MoveFormat.EngineCompact))
			{
				case IMoveAttemptSuccess: Assert.Fail("Illegal pawn move"); break;
				case IMoveAttemptFail f:
					Assert.AreEqual(MoveFailureReasons.NotParsed, f.Reason);
					Assert.AreEqual(ParseErrorType.IllegalMove, f.ParseError);
					break;
			}
		}

		[TestMethod]
		public void CapturedAndLostPieces()
		{
			InteractiveGame g = new InteractiveGame();
			Assert.AreEqual(16, g.White.ActivePieces.Count());
			Assert.AreEqual(16, g.Black.ActivePieces.Count());
			Assert.AreEqual(0, g.White.CapturedPieces.Count());
			Assert.AreEqual(0, g.Black.CapturedPieces.Count());
			IMoveAttempt result = g.White.AttemptMove(new MoveRequest("d2d4"));
			Assert.IsTrue(result.Succeeded);
			result = g.Black.AttemptMove(new MoveRequest("e7e5"));
			Assert.IsTrue(result.Succeeded);
			result = g.White.AttemptMove(new MoveRequest("d4e5"));
			Assert.IsTrue(result.Succeeded);
			IMoveAttemptSuccess success = (IMoveAttemptSuccess)result;
			Assert.IsTrue(success.CompletedMove.IsCapture);
			Assert.AreEqual(1, g.Black.CapturedPieces.Count());
			Assert.AreEqual(15, g.Black.ActivePieces.Count());
		}

		[TestMethod]
		public void PlayersAlternate()
		{
			IInteractiveChessGame g = new InteractiveGame();
			List<MoveRequest> requests = MoveRequest.ParseMoves("d2d4 c7c5 d4d5 g8f6 c2c4 b7b5 c4b5 a7a6 b5a6 e7e6 b1c3 e6d5 c3d5 f8e7 d5e7 d8e7 c1f4 d7d5 e2e3 e8g8 g1f3 h7h6 a2a3 b8a6 f1b5 e7b7 a3a4 c8d7 b5d7 b7d7 e1g1 f8e8 d1d3 c5c4 d3a3 f6e4 b2b4 c4b3 a3b3 a6c5 b3b5 e8e7 f3e5 d7e6 f2f3 e7b7 b5c6 e6c6 e5c6 e4c3 f4e5 c3a4 f1b1 b7b1").ToList();
			Assert.HasCount(54, requests);
			Assert.AreSame(g.White, g.NextPlayer);
			int lastNumber = -1;
			IChessPlayer prevPlayer = g.NextPlayer;
			g.MoveCompleted += (move) =>
			{
				Assert.AreNotEqual(lastNumber, move.SerialNumber);
				lastNumber = move.SerialNumber;
				Assert.AreNotSame(g.NextPlayer, prevPlayer);
				prevPlayer = g.NextPlayer;
			};
			foreach (MoveRequest req in requests)
			{
				switch (g.NextPlayer.AttemptMove(req))
				{
					case MoveAttemptFail f: Assert.Fail($"Move {lastNumber}: {req.ToString()}: {f.Reason.ToString()}"); break;
					case MoveAttemptSuccess s: break;
				}
			}
		}

		private void G_MoveCompleted(IChessMove value)
		{
			throw new NotImplementedException();
		}

		[TestMethod]
		public void Undo()
		{
			IInteractiveChessGame g = GameFactory.CreateInteractive();
			string fen = g.Board.AsFEN();
			Assert.IsFalse(g.White.CanUndo);
			switch (g.White.AttemptMove(new MoveRequest("d2d4")))
			{
				case MoveAttemptFail f: Assert.Fail(f.Reason.ToString()); break;
				case MoveAttemptSuccess s:
					Assert.HasCount(1, g.Moves);
					Assert.HasCount(1, g.White.CompletedMoves);
					Assert.AreSame(s.CompletedMove, g.White.LastMoveMade);
					break;
			}
			Assert.IsTrue(g.White.CanUndo);
			Assert.AreNotEqual(fen, g.Board.AsFEN());
			Assert.IsTrue(g.White.UndoLastMove());
			Assert.HasCount(0, g.Moves);
			Assert.AreEqual(fen, g.Board.AsFEN());
			Assert.IsTrue(g.White.LastMoveMade is NoMove);
		}

		[TestMethod]
		public void AvailableSquaresFor()
		{
			IInteractiveChessGame game = GameFactory.CreateInteractive();
			IChessSquare e2 = game.Board[Lib.Hardware.File.E, Rank.R2], e7 = game.Board[Lib.Hardware.File.E, Rank.R7];
			Assert.IsTrue(e2.HasPiece);
			Assert.IsTrue(e7.HasPiece);
			IChessPawn p = (IChessPawn)e2.Piece, p2 = (IChessPawn)e7.Piece;
			Assert.AreEqual(2, game.White.AvailableSquaresFor(p).Count());
			Assert.AreEqual(0, game.White.AvailableSquaresFor(p2).Count());
			Assert.AreEqual(0, game.Black.AvailableSquaresFor(p).Count());
			Assert.AreEqual(0, game.Black.AvailableSquaresFor(p2).Count(), "Not black's move");
			Assert.AreEqual(0, game.Black.AvailableSquaresFor(e7.Piece).Count());
			Assert.AreEqual(-1, game.White.LastMoveMade.SerialNumber);
			game.White.AttemptMove(new MoveRequest("e2e4"));
			Assert.AreEqual(0, game.White.LastMoveMade.SerialNumber);
			Assert.AreEqual(Rank.R4, p.Square.Rank, "Move applied");
			Assert.AreEqual(0, game.White.AvailableSquaresFor(p).Count());
			Assert.AreEqual(2, game.Black.AvailableSquaresFor(p2).Count());
			Assert.AreEqual(0, game.Black.AvailableSquaresFor(p).Count());
		}

		[TestMethod]
		public void SourceMoveSet()
		{
			IInteractiveChessGame g = GameFactory.CreateInteractive();
			Assert.HasCount(0, g.Moves);
			g.White.AttemptMove("e2e4", MoveFormat.Engine);
			Assert.HasCount(1, g.Moves);
			Assert.IsNotNull(g.Moves[0].SourceMove);
			Assert.AreEqual("e2e4", g.Moves[0].SourceMove.Move);
		}
	}
}
