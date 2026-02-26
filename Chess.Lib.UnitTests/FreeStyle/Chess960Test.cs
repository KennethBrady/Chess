using Chess.Lib.Variants;
using Chess.Lib.Hardware;
using File = Chess.Lib.Hardware.File;
using Chess.Lib.Hardware.Pieces;

namespace Chess.Lib.UnitTests.FreeStyle
{
	[TestClass]
	public  class Chess960Test
	{
		[TestMethod]
		public void AllValid()
		{
			Dictionary<PieceType, int> counts = PieceTypeExtensions.All.ToDictionary(p => p, p => 0);
			void resetCounts()
			{
				foreach (PieceType pt in PieceTypeExtensions.All) counts[pt] = 0;
			}
			List<string> fens = new List<string>(960);
			for(int i=0;i<960;++i)
			{
				IChessBoard b = Chess960.BoardFor(i);
				string msg = $"Game #{i}";
				void testPieces(Rank r)
				{
					resetCounts();
					Hue? bishopHue = null;
					for (File f = File.A;f<=File.H;++f)
					{
						IChessSquare s = b[f, r];
						counts[s.Piece.Type]++;
						switch(s.Piece.Type)
						{
							case PieceType.King: Assert.AreEqual(1, counts[PieceType.Rook], msg + " King must be between the rooks"); break;
							case PieceType.Bishop:
								if (bishopHue == null) bishopHue = s.Hue; else Assert.AreNotEqual(bishopHue.Value, s.Hue, msg + " Bishops on different-color squares");
								break;
						}
					}
					Assert.AreEqual(2, counts[PieceType.Rook],msg);
					Assert.AreEqual(2, counts[PieceType.Knight],msg);
					Assert.AreEqual(2, counts[PieceType.Bishop], msg);
					Assert.AreEqual(1, counts[PieceType.Queen], msg);
					Assert.AreEqual(1, counts[PieceType.King], msg);
				}
				void testPawns(Rank r)
				{
					for(File f = File.A;f<=File.H;++f)
					{
						IChessSquare s = b[f, r];
						Assert.AreEqual(PieceType.Pawn, s.Piece.Type, msg);
					}
				}
				testPieces(Rank.R1);
				testPieces(Rank.R8);
				testPawns(Rank.R2);
				testPawns(Rank.R7);
				string fen = b.FENPiecePlacements;
				int n = fens.BinarySearch(fen);
				if (n < 0) fens.Insert(~n, fen); else Assert.Fail($"Duplicate Fen @ {i}: {fen}");
			}
			Assert.HasCount(960, fens);
		}

		[TestMethod]
		public void VariantSafeguards()
		{
			VariantNumber n = -1;
			Assert.AreEqual(0, n.Number, "disallow negative numbers");
			n = 2000;
			Assert.AreEqual(VariantNumber.MaxValue, n.Number);
			for(int i=0;i<=VariantNumber.MaxValue;i++)
			{
				n = i;
				Assert.AreEqual(i, n.Number);
			}
		}
	}
}
