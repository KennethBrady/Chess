using Chess.Lib.Hardware;
using File = Chess.Lib.Hardware.File;

namespace Chess.Lib.UnitTests.Hardware
{
	[TestClass]
	public class SquareTest
	{
		[TestMethod]
		public void BoardDefaults()
		{
			Board board = new Board();
			Assert.AreEqual(64, board.Squares.Count());
			Assert.AreEqual(32, board.Squares.Where(s => s.HasPiece).Count());
			Assert.AreEqual(8, board.Squares.Where(s => s.Rank == Rank.R1).Count());
			Assert.AreEqual(8, board.Squares.Where(s => s.Rank == Rank.R2).Count());
			Assert.AreEqual(8, board.Squares.Where(s => s.Rank == Rank.R3).Count());
			Assert.AreEqual(8, board.Squares.Where(s => s.Rank == Rank.R4).Count());
			Assert.AreEqual(8, board.Squares.Where(s => s.Rank == Rank.R5).Count());
			Assert.AreEqual(8, board.Squares.Where(s => s.Rank == Rank.R6).Count());
			Assert.AreEqual(8, board.Squares.Where(s => s.Rank == Rank.R7).Count());
			Assert.AreEqual(8, board.Squares.Where(s => s.Rank == Rank.R8).Count());
			Assert.AreEqual(8, board.Squares.Where(s => s.Rank == Rank.R1 && s.HasPiece).Count());
			Assert.AreEqual(8, board.Squares.Where(s => s.Rank == Rank.R2 && s.HasPiece).Count());
			Assert.AreEqual(0, board.Squares.Where(s => s.Rank == Rank.R3 && s.HasPiece).Count());
			Assert.AreEqual(0, board.Squares.Where(s => s.Rank == Rank.R4 && s.HasPiece).Count());
			Assert.AreEqual(0, board.Squares.Where(s => s.Rank == Rank.R5 && s.HasPiece).Count());
			Assert.AreEqual(0, board.Squares.Where(s => s.Rank == Rank.R6 && s.HasPiece).Count());
			Assert.AreEqual(8, board.Squares.Where(s => s.Rank == Rank.R7 && s.HasPiece).Count());
			Assert.AreEqual(8, board.Squares.Where(s => s.Rank == Rank.R8 && s.HasPiece).Count());
		}

		[TestMethod]
		public void NeighborSquares()
		{
			Board board = new Board();
			bool IsOnBoard(IChessSquare s) => s is not NoSquare;
			bool IsOffBoard(IChessSquare s) => s is NoSquare;
			foreach (ISquare s in board.Squares)
			{
				var n = s.Neighbors;
				Assert.IsTrue(IsOnBoard(n.Center));
				if (s.Rank == Rank.R1)
				{
					Assert.IsTrue(IsOffBoard(n.PrevRank));
					Assert.IsTrue(IsOffBoard(n.PrevRank));
					Assert.IsTrue(IsOffBoard(n.DiagBL));
					Assert.IsTrue(IsOffBoard(n.DiagBR));
					if (s.File == File.A || s.File == File.H)
					{
						Assert.AreEqual(4, n.All.Count());
					}
				}
				else if (s.Rank == Rank.R8)
				{
					Assert.IsTrue(IsOffBoard(n.NextRank));
					Assert.IsTrue(IsOffBoard(n.DiagUL));
					Assert.IsTrue(IsOffBoard(n.DiagUR));
				}
				else
				{
					Assert.IsTrue(IsOnBoard(n.PrevRank));
					Assert.IsTrue(IsOnBoard(n.NextRank));
				}
				if (s.File == File.A)
				{
					Assert.IsTrue(IsOffBoard(n.PrevFile));
					Assert.IsTrue(IsOffBoard(n.DiagBL));
					Assert.IsTrue(IsOffBoard(n.DiagUL));
				}
				else if (s.File == File.H)
				{
					Assert.IsTrue(IsOffBoard(n.NextFile));
					Assert.IsTrue(IsOffBoard(n.DiagUR), n.Center.ToString());
					Assert.IsTrue(IsOffBoard(n.DiagBR));
				}
				else
				{
					Assert.IsTrue(IsOnBoard(n.PrevFile));
					Assert.IsTrue(IsOnBoard(n.NextFile));
				}
			}
		}
	}
}
