using Chess.Lib.Hardware;

namespace Chess.Lib.UnitTests.Hardware
{
	[TestClass]
	public class BoardInfoTest
	{
		[TestMethod]
		public void TotalPieceCount()
		{
			IBoard b = new Board(false);
			Assert.HasCount(0, b.ActivePieces);
			BoardInfo info = new BoardInfo(b);
			Assert.AreEqual(0, info.TotalPieceCount);
			b = new Board(true);
			Assert.HasCount(32, b.ActivePieces);
			info = new BoardInfo(b);
			Assert.AreEqual(32, info.TotalPieceCount);
		}

		[TestMethod]
		public void HasBothKings()
		{
			IBoard b = new Board(true);
			BoardInfo info = new BoardInfo(b);
			Assert.IsTrue(info.Exists(PieceDef.WhiteKing));
			Assert.IsTrue(info.Exists(PieceDef.BlackKing));
			Assert.IsTrue(info.HasBothKings);
		}
	}
}
