using Chess.Lib.Hardware;
using Chess.Lib.Hardware.Pieces;
using File = Chess.Lib.Hardware.File;

namespace Chess.Lib.UnitTests.Pieces
{
	[TestClass]
	public class QueenTest
	{
		[TestMethod]
		public void Defaults()
		{
			IBoard b = new Board();
			List<IQueen> queens = b.ActivePieces.OfType<IQueen>().ToList();
			Assert.AreEqual(2, queens.Count);
			Assert.AreEqual(1, queens.Where(q => q.Side == Hue.Light).Count());
			Assert.IsTrue(queens.All(q => q.Side == q.Square.Hue), "Queen on her color");
		}

		[TestMethod]
		public void CanMoveDefault()
		{
			IBoard b = new Board();
			IQueen q = (IQueen)b[File.D, Rank.R1].Piece;
			foreach(ISquare s in b)
			{
				Assert.IsFalse(q.CanMoveTo(s), s.ToString());
			}
		}

		[TestMethod]
		public void SoloQueenInCorner()
		{
			BoardBuilder bb = new BoardBuilder();
			bb.SetPiece(File.A, Rank.R1, PieceType.Queen, Hue.Light);
			IBoard b = (IBoard)bb.CreateBoard();
			IQueen q = (IQueen)b.ActivePieces.First();
			int nMove = 0;
			foreach (Square s in b) if (q.CanMoveTo(s)) nMove++;
			Assert.AreEqual(21, nMove);
		}

		[TestMethod]
		public void ObstructedQueenInCorner()
		{
			BoardBuilder bb = new BoardBuilder();
			bb.SetPiece(File.A, Rank.R1, PieceType.Queen, Hue.Light);
			bb.SetPiece(File.A, Rank.R2, PieceType.Pawn, Hue.Light);
			bb.SetPiece(File.B, Rank.R1, PieceType.Knight, Hue.Light);
			bb.SetPiece(File.B, Rank.R2, PieceType.Pawn, Hue.Light);
			IBoard b = (IBoard)bb.CreateBoard();
			IQueen q = (IQueen)b.ActivePieces.First();
			foreach (ISquare s in b)
			{
				Assert.IsFalse(q.CanMoveTo(s), s.ToString());
			}
		}
	}
}
