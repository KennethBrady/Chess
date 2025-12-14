using Chess.Lib.Hardware;
using Chess.Lib.Hardware.Pieces;
using File = Chess.Lib.Hardware.File;

namespace Chess.Lib.UnitTests.Pieces
{
	[TestClass]
	public class RookTest
	{
		[TestMethod]
		public void Defaults()
		{
			IBoard board = new Board();
			List<IRook> rooks = board.ActivePieces.OfType<IRook>().ToList();
			Assert.AreEqual(4, rooks.Count);
			Assert.AreEqual(2, rooks.Where(r => r.Side == Hue.Light).Count());
			Assert.AreEqual(2, rooks.Where(r => r.Side == Hue.Dark).Count());
			foreach(IRook r in rooks)
			{
				switch(r.Side)
				{
					case Hue.Light:
						Assert.AreEqual(r.Square.Rank, Rank.R1);
						Assert.IsTrue(r.Square.File == File.A || r.Square.File == File.H);
						break;
					case Hue.Dark:
						Assert.AreEqual(r.Square.Rank, Rank.R8);
						Assert.IsTrue(r.Square.File == File.A || r.Square.File == File.H);
						break;
				}
			}
		}

		[TestMethod]
		public void CanMoveDefault()
		{
			IBoard board = new Board();
			List<IRook> rooks = board.ActivePieces.OfType<IRook>().ToList();
			foreach(Square s in board)
			{
				foreach (Rook rook in rooks) Assert.IsFalse(rook.CanMoveTo(s), $"{rook} => {s}");
			}
		}

		[TestMethod]
		public void CanMoveSolo()
		{
			BoardBuilder bb = new BoardBuilder();
			bb.SetPiece(File.A, Rank.R1, PieceType.Rook, Hue.Light);
			IBoard b = (IBoard)bb.CreateBoard();
			IRook rook = (IRook)b.ActivePieces.First();
			foreach(ISquare square in b)
			{
				bool canMove = !square.Equals(rook.Square) && (square.Rank == rook.Square.Rank || square.File == rook.Square.File);
				Assert.AreEqual(canMove, rook.CanMoveTo(square), square.Name);
			}
		}

		[TestMethod]
		public void Capture()
		{
			BoardBuilder bb = new BoardBuilder();
			bb.SetPiece(File.A, Rank.R1, PieceType.Rook, Hue.Light);
			bb.SetPiece(File.A, Rank.R8, PieceType.Queen, Hue.Dark);
			IBoard b = (IBoard)bb.CreateBoard();
			IRook rook = b.ActivePieces.OfType<IRook>().First();
			IQueen queen = b.ActivePieces.OfType<IQueen>().First();
			Assert.IsTrue(rook.CanMoveTo(queen.Square));
		}

		//[TestMethod]
		//public async Task MovesAfterCastle()
		//{
		//	using Game game = await Game.TwoPlayerGame();
		//	var moves = game.ParseMoves("e2e4e7e5g1f3b8c6f1b5g8f6");
		//	Assert.AreEqual(6, moves.Count);
		//	Move m = game.ParseMoves("e1g1").First();
		//	Assert.IsTrue(m.IsKingsideCastle);
		//	Square rSq = game.Board[File.F, Rank.R1];
		//	Assert.IsTrue(rSq.HasPiece);
		//	Rook rook = rSq.Piece as Rook;
		//	Assert.IsNotNull(rook);
		//	Assert.AreSame(rSq, rook.Square);
		//	Square to = game.Board[File.E, Rank.R1];
		//	Assert.IsFalse(to.HasPiece);
		//	Assert.IsTrue(rook.CanMoveTo(to));
		//}
	}
}
