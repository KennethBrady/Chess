using Chess.Lib.Hardware;
using Chess.Lib.Hardware.Pieces;
using File = Chess.Lib.Hardware.File;

namespace Chess.Lib.UnitTests.Pieces
{
	[TestClass]
	public class KnightTest
	{
		[TestMethod]
		public void Defaults()
		{
			IBoard board = new Board();
			List<Knight> ks = board.ActivePieces.OfType<Knight>().ToList();
			Assert.HasCount(4, ks);
			Assert.AreEqual(2, ks.Where(k => k.Side == Hue.Light).Count());
			Assert.AreEqual(2, ks.Where(k => k.Square.Hue == Hue.Light).Count());
		}

		[TestMethod]
		public void CanMoveDefault()
		{
			IBoard board = new Board();
			List<IKnight> ks = board.ActivePieces.OfType<IKnight>().ToList();
			foreach(IKnight knight in ks)
			{
				int nMoves = 0;
				foreach(ISquare square in board)
				{
					if (knight.CanMoveTo(square)) nMoves++;
				}
				Assert.AreEqual(2, nMoves, knight.ToString());
			}
		}

		[TestMethod]
		public void MoveFromCenter()
		{
			BoardBuilder bb = new BoardBuilder();
			bb.SetPiece(File.D, Rank.R4, PieceType.Knight, Hue.Light);
			IBoard board = (IBoard)bb.CreateBoard();
			IKnight k = (IKnight)board.ActivePieces.First();
			int nMoves = 0;
			foreach(ISquare s in  board) if (k.CanMoveTo(s)) nMoves++;
			Assert.AreEqual(8, nMoves);
		}

		//[TestMethod]
		//public async Task CannotUnblockCheck()
		//{
		//	using Game game = await Game.FromFEN("r1bqkbnr/ppp1pppp/2n5/1B1p4/3P4/4P3/PPP2PPP/RNBQK1NR b KQkq - 2 3");
		//	Knight k = game.Board[File.C, Rank.R6].Piece as Knight;
		//	Assert.IsNotNull(k);
		//	Assert.AreEqual(Hue.Dark, k.Side);
		//	Assert.AreSame(game.Black, game.NextPlayer);
		//	Assert.IsFalse(k.CanMoveTo(game.Board[File.D, Rank.R5]));
		//}

		//[TestMethod]
		//public async Task CanMoveH4()
		//{
		//	// from game 426066 after move 31a:
		//	using Game game = await Game.FromFEN("3r1r2/5pk1/Q4nn1/6p1/3P2P1/1B2P3/Pq1B1P2/3RK1R1 b - - 0 31");
		//	Assert.AreSame(game.Black, game.NextPlayer);
		//	Knight k = game.Board[File.G, Rank.R6].Piece as Knight;
		//	Assert.IsNotNull(k);
		//	Assert.IsTrue(k.CanMoveTo(game.Board[File.H, Rank.R4]));
		//}
	}
}
