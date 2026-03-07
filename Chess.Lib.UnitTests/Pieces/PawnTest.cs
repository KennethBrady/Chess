using Chess.Lib.Games;
using Chess.Lib.Hardware;
using Chess.Lib.Hardware.Pieces;
using Chess.Lib.Moves;
using Chess.Lib.Moves.Parsing;
using Org.BouncyCastle.Tls.Crypto.Impl.BC;
using File = Chess.Lib.Hardware.File;

namespace Chess.Lib.UnitTests.Pieces
{
	[TestClass]
	public class PawnTest
	{
		[TestMethod]
		public void DefaultBoard()
		{
			IBoard board = new Board();
			List<IPawn> pawns = board.ActivePieces.OfType<IPawn>().ToList();
			Assert.HasCount(16, pawns);
			Assert.AreEqual(8, pawns.Where(p => p.Side == Hue.White).Count());
			Assert.AreEqual(8, pawns.Where(p => p.Side == Hue.Black).Count());
			Assert.IsTrue(pawns.Where(p => p.Side == Hue.White).All(p => p.Square.Rank == Rank.R2));
			Assert.IsTrue(pawns.Where(p => p.Side == Hue.Black).All(p => p.Square.Rank == Rank.R7));
		}

		[TestMethod]
		public void CanMove()
		{
			IBoard board = new Board();
			List<IPawn> pawns = board.ActivePieces.OfType<IPawn>().ToList();
			foreach (IPawn p in pawns)
			{
				Assert.AreEqual(0, p.MoveCount);
				var n = p.Square.Neighbors;
				IChessSquare sA = p.Square.Neighbors.NextRank, sB = sA.Neighbors.NextRank;
				IChessSquare sC = p.Square.Neighbors.PrevRank, sD = sC.Neighbors.PrevRank;
				if (p.Square.Rank == Rank.R2)	// White pawns
				{
					Assert.IsTrue(p.CanMoveTo((ISquare)sA));
					Assert.IsTrue(p.CanMoveTo((ISquare)sB));  // such as e2e4
					foreach (ISquare sq in board)
					{
						if (ReferenceEquals(sq, sA) || ReferenceEquals(sq, sB)) continue;
						Assert.IsFalse(p.CanMoveTo(sq));
					}
				}
				else
				{
					Assert.IsTrue(p.CanMoveTo((ISquare)sC), p.ToString());
					Assert.IsTrue(p.CanMoveTo((ISquare)sD));
					foreach (Square sq in board)
					{
						if (ReferenceEquals(sq, sC) || ReferenceEquals(sq, sD)) continue;
						Assert.IsFalse(p.CanMoveTo(sq));
					}
				}

			}
		}

		[TestMethod]
		public void CannotMoveBackwards()
		{
			BoardBuilder bb = new BoardBuilder();
			bb.SetPiece(File.D, Rank.R4, PieceType.Pawn, Hue.White);
			IBoard b = (IBoard)bb.CreateBoard();
			IPawn p = b.ActivePieces.OfType<IPawn>().First();
			Assert.IsFalse(p.CanMoveTo(b[File.D, Rank.R3]));
		}

		private static IMoveAttemptSuccess AssertMove(IMoveAttempt move)
		{
			IMoveAttemptFail? f = move as IMoveAttemptFail;
			Assert.IsNull(f, f?.Reason.ToString());
			return (IMoveAttemptSuccess)move;
		}

		[TestMethod]
		public async Task BlackEnPassant()
		{
			var g = GameFactory.CreateInteractive();
			await g.White.AttemptMove("e2e4");
			await g.Black.AttemptMove("d7d5");
			await g.White.AttemptMove("g1f3");
			await g.Black.AttemptMove("d5d4");
			await g.White.AttemptMove("c2c4");
			var res = await g.Black.AttemptMove("d4e3");
			Assert.IsFalse(res.Succeeded, "Attempted illegal en-passant");
			res = await g.Black.AttemptMove("d4c3");
			Assert.IsTrue(res.Succeeded);
			var succ = res as IMoveAttemptSuccess;
			Assert.IsNotNull(succ);
			Assert.HasCount(3, succ.CompletedMove.AffectedSquares());
		}

		[TestMethod]
		public async Task WhiteEnPassant()
		{
			var g = GameFactory.CreateInteractive();
			await g.White.AttemptMove("e2e4");
			await g.Black.AttemptMove("d7d5");
			await g.White.AttemptMove("e4e5");
			await g.Black.AttemptMove("f7f5");
			IChessSquare sB = g.Board[File.F, Rank.R6];
			switch(await g.White.AttemptMove("e5d6"))
			{
				case IMoveAttemptFail: break;
				default: Assert.Fail("Attempted incorrect en-passant"); break;
			}
			switch(await g.White.AttemptMove("e5f6"))
			{
				case IMoveAttemptFail f: Assert.Fail(f.ParseError.ToString()); break;
				case IMoveAttemptSuccess s:
					Assert.AreEqual(3, s.CompletedMove.AffectedSquares().Count());
					break;
			}
		}

		[TestMethod]
		public async Task BlackFirstMoveNoEP()
		{
			var g = GameFactory.CreateInteractive();
			AssertMove(await g.White.AttemptMove("d2d4"));
			IChessPawn bp = (IChessPawn)g.Board[File.D, Rank.R7].Piece;
			Assert.IsNotNull(bp);
			Assert.IsFalse(bp.CanMoveTo(g.Board[File.D, Rank.R3]));

			var allowed = g.Board[File.D, Rank.R7].Piece.AllowedMoves();
			Assert.AreEqual(2, allowed.Count());
			Assert.IsTrue(allowed.All(s => s.File == File.D));
		}

		[TestMethod]
		public async Task White2ndMoveNoEP()
		{
			var g = GameFactory.CreateInteractive();
			AssertMove(await g.White.AttemptMove("d2d4"));
			AssertMove(await g.Black.AttemptMove("c7c5"));
			IChessSquare c6 = g.Board[File.C, Rank.R6];
			IPawn? p = g.Board[File.D, Rank.R4].Piece as IPawn;
			Assert.IsNotNull(p);
			Assert.IsFalse(p.CanMoveTo(c6));
		}

		[TestMethod]
		public async Task EnpassantMoveIsCapture()
		{
			var g = GameFactory.CreateInteractive();
			AssertMove(await g.White.AttemptMove("d2d4"));
			AssertMove(await g.Black.AttemptMove("c7c5"));
			AssertMove(await g.White.AttemptMove("d4d5"));
			AssertMove(await g.Black.AttemptMove("e7e5"));
			Assert.IsTrue(g.Board[File.E, Rank.R5].HasPiece);
			var m = AssertMove(await g.White.AttemptMove("d5e6"));
			Assert.IsTrue(m.CompletedMove.IsEnPassant);
			Assert.IsFalse(g.Board[File.E, Rank.R5].HasPiece);
			Assert.IsTrue(m.CompletedMove.CapturedPiece is IChessPawn);
		}

		[TestMethod]
		public async Task CaptureAfterEnPassant()
		{
			const string MOVES = "D2D4C7C5D4D5E7E5D5E6";
			var g = GameFactory.CreateInteractive();
			foreach(var mov in MOVES.Chunk(4))
			{
				AssertMove(await g.NextPlayer.AttemptMove(new string(mov)));
			}
			Assert.HasCount(5, g.Moves);
			IChessPawn? p = g.Board[File.F, Rank.R7].Piece as IChessPawn;
			Assert.IsNotNull(p);
			Assert.IsTrue(p.CanMoveTo(g.Board[File.E, Rank.R6]));
			IChessMove m = g.Moves.Last();
			Assert.IsTrue(m.IsCapture);
			Assert.IsTrue(m.IsEnPassant);
			Assert.IsFalse(g.Board[File.E, Rank.R5].HasPiece);
			var res = AssertMove(await g.NextPlayer.AttemptMove("f7e6"));
			Assert.IsTrue(res.CompletedMove.IsCapture);
			Assert.IsTrue(g.Board[File.E, Rank.R6].HasPiece);
		}

		[TestMethod]
		public async Task EnPassantCaptures()
		{
			IInteractiveChessGame ig = GameFactory.CreateInteractive();
			int n = ig.ApplyMoves("1. d4 d5 2. e3 c5 3. Nf3 c4 4. Nc3 Nc6 5. h3 e6 6. Be2 Nf6 7. O-O Bd6 8.a3 O-O 9.Bd2 Bd7 10. b4");
			Assert.AreEqual(19, n);
			//Console.WriteLine(ig.Board.Display());
			Assert.IsFalse(ig.Board[File.B, Rank.R3].HasPiece);
			IPawn? p = ig.Board[File.C, Rank.R4].Piece as IPawn, pCapture = ig.Board[File.B, Rank.R4].Piece as IPawn;
			Assert.IsNotNull(p);
			Assert.IsNotNull(pCapture);
			var result = await ig.Black.AttemptMove("cxb3", MoveFormat.Algebraic);
			switch(result)
			{
				case IMoveAttemptSuccess ms:
					//Console.WriteLine(ig.Board.Display());
					Assert.IsTrue(ig.Board[File.B, Rank.R3].HasPiece);
					Assert.AreSame(p, ig.Board[File.B, Rank.R3].Piece);
					Assert.IsFalse(ig.Board[File.B, Rank.R4].HasPiece);
					Assert.IsTrue(ms.CompletedMove.IsCapture);
					Assert.AreSame(pCapture, ms.CompletedMove.CapturedPiece);
					break;
				case IMoveAttemptFail mf: Assert.Fail(mf.Reason.ToString()); break;
			}
		}
	}
}
