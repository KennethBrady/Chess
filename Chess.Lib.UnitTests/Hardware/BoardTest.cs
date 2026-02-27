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
			Assert.AreEqual(16, board.ActivePieces.Where(p => p.Side == Hue.White).Count());
			Assert.AreEqual(16, board.ActivePieces.Where(p => p.Side == Hue.Black).Count());
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

		[TestMethod]
		public void RankSquaresBetween()
		{
			IBoard b = new Board(false);
			for(int i=0;i<100;++i)
			{
				File f1 = (File)Random.Shared.Next(8), f2 = (File)Random.Shared.Next(8);
				var sqrs = b.RankSquaresBetween(b[f1, Rank.R1], b[f2, Rank.R8]);
				if (f1 == f2) Assert.HasCount(6, sqrs); else Assert.HasCount(0, sqrs);
			}
		}

		[TestMethod]
		public void FileSquaresBetween()
		{
			IBoard b = new Board(false);
			for(int i=0;i<100;++i)
			{
				Rank r1 = (Rank)Random.Shared.Next(8), r2 = (Rank)Random.Shared.Next(8);
				var sqrs = b.FileSquaresBetween(b[File.A, r1], b[File.H, r2]);
				if (r1 == r2) Assert.HasCount(6, sqrs); else Assert.HasCount(0, sqrs);
			}
		}

		[TestMethod]
		public void DiagonalSquaresBetween()
		{
			IBoard b = new Board(false);
			var sqrs = b.DiagonalSquaresBetween(b[File.A, Rank.R1], b[File.H, Rank.R8]);
			Assert.HasCount(6, sqrs);
			sqrs = b.DiagonalSquaresBetween(b[File.B, Rank.R1], b[File.H, Rank.R7]);
			Assert.HasCount(5, sqrs);
			sqrs = b.DiagonalSquaresBetween(b[File.C, Rank.R1], b[File.H, Rank.R6]);
			Assert.HasCount(4, sqrs);
			sqrs = b.DiagonalSquaresBetween(b[File.D, Rank.R1], b[File.H, Rank.R5]);
			Assert.HasCount(3, sqrs);
			sqrs = b.DiagonalSquaresBetween(b[File.E, Rank.R1], b[File.H, Rank.R4]);
			Assert.HasCount(2, sqrs);
			sqrs = b.DiagonalSquaresBetween(b[File.F, Rank.R1], b[File.H, Rank.R3]);
			Assert.HasCount(1, sqrs);
			sqrs = b.DiagonalSquaresBetween(b[File.G, Rank.R1], b[File.H, Rank.R3]);
			Assert.HasCount(0, sqrs);

			sqrs = b.DiagonalSquaresBetween(b[File.B, Rank.R2], b[File.E, Rank.R7]);
			Assert.HasCount(0, sqrs);
		}

		[TestMethod]
		public void QueenSquaresBetween()
		{
			IBoard b = new Board(false);
			for(int i=0;i<1000;++i)
			{
				File f1 = (File)Random.Shared.Next(8), f2 = (File)Random.Shared.Next(8);
				Rank r1 = (Rank)Random.Shared.Next(8), r2 = (Rank)Random.Shared.Next(8);
				var sqrs = b.QueenSquaresBetween(b[f1, r1], b[f2, r2]);
				int fdiff = Math.Abs((int)f1 - (int)f2), rdiff = Math.Abs((int)r1 - (int)r2);
				string msg = $"{f1},{r1}  {f2},{r2}";
				void assert(int n) => Assert.HasCount(n, sqrs, msg);
				switch (fdiff)
				{
					case 0: 
						switch(rdiff)
						{
							case 0:
							case 1: assert(0); break;  // Same or adjacent squares
							default: assert(rdiff - 1); break;
						}
						break;
					case 1: assert(0); break;  // Adjacent squares
					case 2: 
						switch(rdiff)
						{
							case 0:
							case 2: assert(1); break;
							default: assert(0); break;
						}
						break;
					case 3:
						switch(rdiff)
						{
							case 0:
							case 3: assert(2); break;
							default: assert(0); break;
						}
						break;
					case 4:
						switch(rdiff)
						{
							case 0:
							case 4: assert(3); break;
							default: assert(0); break;
						}
						break;
					case 5:
						switch(rdiff)
						{
							case 0:
							case 5: assert(4); break;
							default: assert(0); break;
						}
						break;
					case 6:
						switch(rdiff)
						{
							case 0:
							case 6: assert(5); break;
							default: assert(0); break;
						}
						break;
					case 7:
						switch(rdiff)
						{
							case 0:
							case 7: assert(6); break;
							default: assert(0); break;
						}
						break;
				}
			}
		}

		[TestMethod]
		public void KnightSquaresFrom()
		{
			IBoard b = new Board(false);
			for(int i=0;i<100;++i)
			{
				File f = (File)Random.Shared.Next(8);
				Rank r = (Rank)Random.Shared.Next(8);
				var sqrs = b.AllowedKnightMovesFrom(b[f, r]);
				void assert(int c) => Assert.HasCount(c, sqrs, $"{f},{r}");
				switch(f)
				{
					case File.A:
					case File.H:
						switch(r)
						{
							case Rank.R1:
							case Rank.R8: assert(2); break;  // corners
							case Rank.R2:
							case Rank.R7: assert(3); break;
							default: assert(4); break;
						}
						break;
				}
			}
		}
	}

}
