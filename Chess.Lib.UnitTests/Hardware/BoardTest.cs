using Chess.Lib.Games;
using Chess.Lib.Hardware;
using Chess.Lib.Hardware.Pieces;
using File = Chess.Lib.Hardware.File;

namespace Chess.Lib.UnitTests.Hardware
{
	[TestClass]
	public class BoardTest
	{
		[TestMethod]
		public void DefaultBoard()
		{
			IBoard board = new Board();
			int nPiece = 0, nNoPiece = 0;
			foreach (IChessSquare sq in board)
			{
				if (sq.HasPiece)
				{
					nPiece++;
					Assert.AreEqual(board, sq.Piece.Board);
				}
				else nNoPiece++;
			}
			Assert.AreEqual(32, nPiece);
			Assert.AreEqual(32, nNoPiece);
			Assert.AreEqual(32, board.ActivePieces.Count());
			Assert.AreEqual(2, board.ActivePieces.Where(p => p.Type == PieceType.King).Count());
			Assert.AreEqual(2, board.ActivePieces.Where(p => p.Type == PieceType.Queen).Count());
			Assert.AreEqual(4, board.ActivePieces.Where(p => p.Type == PieceType.Rook).Count());
			Assert.AreEqual(4, board.ActivePieces.Where(p => p.Type == PieceType.Bishop).Count());
			Assert.AreEqual(4, board.ActivePieces.Where(p => p.Type == PieceType.Knight).Count());
			Assert.AreEqual(16, board.ActivePieces.Where(p => p.Type == PieceType.Pawn).Count());
			Assert.AreEqual(16, board.ActivePieces.Where(p => p.Side == Hue.Light).Count());
			Assert.AreEqual(16, board.ActivePieces.Where(p => p.Side == Hue.Dark).Count());
			Assert.IsNotNull(board.Game);
			Assert.IsTrue(board.Game is INoGame);
			Assert.IsFalse(board.IsGameBoard);
		}

		[TestMethod]
		public void EmptyBoard()
		{
			IBoard board = new Board(false);
			Assert.IsEmpty(board.ActivePieces);
		}

		[TestMethod]
		public void GameBoard()
		{
			IChessGame g = GameFactory.CreateInteractive();
			Assert.IsTrue(g.Board.IsGameBoard);
			Assert.AreEqual(0, g.Board.AllowedMovesFrom(g.Board[Lib.Hardware.File.A, Rank.R7]).Count());
		}

		[TestMethod]
		public void QueensOnTheirColor()
		{
			IBoard board = new Board();
			int n = 0;
			foreach (IPiece piece in board.ActivePieces.Where(p => p is IQueen q))
			{
				Assert.AreEqual(piece.Side, piece.Square.Hue);
				n++;
			}
			Assert.AreEqual(2, n);
		}

		[TestMethod]
		public void PositionOf()
		{
			IBoard b = new Board(false);
			foreach (ISquare s in b)
			{
				FileRank pos = Board.PositionOf(s.Index);
				Assert.AreEqual(pos.File, s.File);
				Assert.AreEqual(pos.Rank, s.Rank);
				Assert.AreEqual(Board.IndexOf(pos.File, pos.Rank), s.Index);
			}
		}

		[TestMethod]
		public void ParseSquare()
		{
			IBoard b = new Board(true);
			ISquare s = b.ParseSquare("e2");
			Assert.IsNotNull(s);
			Assert.IsTrue(s is not NoSquare);
			Assert.IsTrue(s.HasPiece);
			Assert.IsTrue(s.Piece is IPawn p);
			s = b.ParseSquare("xx");
			Assert.IsNotNull(s);
			Assert.IsTrue(s is NoSquare);
		}

		[TestMethod]
		public void GetEnumerator()
		{
			IBoard b = new Board();
			var it = ((System.Collections.IEnumerable)b).GetEnumerator();
			while (it.MoveNext())
			{
				Assert.IsTrue(it.Current is ISquare s);
			}
		}

		[TestMethod]
		public void FromFEN()
		{
			IBoard b = new Board("qrbknbrn/pppppppp/8/8/8/8/PPPPPPPP/QRBKNBRN w GBgb - 0 1");
			Assert.HasCount(32, b.ActivePieces);
			Assert.IsTrue(b[File.A, Rank.R8].Piece is IQueen);
			Assert.IsTrue(b[File.A, Rank.R1].Piece is IQueen);
		}
	}
}
