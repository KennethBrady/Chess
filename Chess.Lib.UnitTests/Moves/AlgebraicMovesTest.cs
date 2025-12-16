using Chess.Lib.Hardware;
using Chess.Lib.Hardware.Pieces;
using Chess.Lib.Moves;
using Chess.Lib.Moves.Parsing;
using Chess.Lib.Pgn.Parsing;
using System.Text.RegularExpressions;
using File = Chess.Lib.Hardware.File;

namespace Chess.Lib.UnitTests.Moves
{
	internal static class AMExtensions
	{
		public static IParsedGame Parse(this AlgebraicMoves ams, bool showDiagnostics)
		{
			AlgebraicMoves.ShowDiagnostics = showDiagnostics;
			return ams.Parse();
		}
	}

	[DeploymentItem("games.txt")]
	[DeploymentItem("Moves/UnmatchedMovePattern.pgn")]
	[TestClass]
	public class AlgebraicMovesTest
	{
		private const string TESTMOVES = "1. e4 e5 2. Nf3 Nc6 3. Bb5 Nf6 4. d3 Bc5 5. c3 O-O 6. O-O d6 7. Nbd2 a6 8. Bxc6 bxc6 9. Re1 Re8 10. h3 Bb6 11. Nf1 h6 12. Ng3 Be6 13. Be3 Bxe3 14. Rxe3 c5 15. Qc2 Nd7 16. d4 cxd4 17. cxd4 exd4 18. Nxd4 a5 19. b3 Nc5 20. Rd1 Bd7 21. e5 dxe5 22. Nf3 Nb7 23. Nxe5 Nd6 24. a4 Be6 25. Qc3 Qg5 26. Nf3 Qd8 27. Nd4 Kh8 28. Nh5 Qg5 29. Nxe6 Rxe6 30. Rxe6 fxe6 31. Qxc7 Qxh5 32. Qxd6 Qe2 33. Qd3 Qxd3 34. Rxd3 Rc8 35. Re3 Rc1+ 36. Kh2 Rc2 37. f3 Rc6 38. Re5 Ra6 39. Kg3 Kg8 40. Kf4 Kf7 41. Rb5 Kf6 42. h4 Ra8 43. g4 Ra7 44. Ke4 g5 45. hxg5+ hxg5 46. Kd4 Rd7+ 47. Kc4 Ra7 48. b4 axb4 49. Kxb4 Ke7 50. Rxg5 Kd6 51. a5 Rf7 52. a6 Kc6 53. Ra5 Rf4+ 54. Kc3 Rxf3+ 55. Kd2 1-0";
		private const string TESTMOVES2 = "1.e4 c6 2. Nc3 d5 3. Nf3 Bg4 4. h3 Bh5 5. d4 e6 6. Bd3 Bb4 7. O-O Ne7 8. a3 Ba5 9. Bg5 O-O 10. Re1 f6 11. Bh4 Ng6 12. Bg3 f5 13. exf5 exf5 14. Re6 f4 15. Bxg6 Bxf3 16. Qxf3 fxg3 17. Bxh7+ Kxh7 18. Qh5+ Kg8 19. Rh6 gxf2+ 20. Kf1 gxh6 21. Qg6+ Kh8 22. Qxh6+ Kg8 23. Qg6+ Kh8 24. Qh6+ Kg8 1/2-1/2";

		private static readonly Regex _rxMoveNumbers = new Regex(@"\d+\. *", RegexOptions.Compiled);
		[TestMethod]
		public void NormalizeMoves()
		{
			string m = AlgebraicMoves.Normalized("1.");
			Assert.AreEqual("1.", m);
			m = AlgebraicMoves.Normalized("1. e4  d5");
			Assert.AreEqual("1. e4 d5", m);
			m = AlgebraicMoves.Normalized("1. e4 d5 2. ");
			Assert.AreEqual("1. e4 d5 2.", m);
			m = AlgebraicMoves.Normalized("1.e4");
			Assert.AreEqual("1. e4", m);
			var g = GameDB.Get(638775);
			AlgebraicMoves ams = AlgebraicMoves.Create(g);
			Assert.IsNotEmpty(ams.Comments, "Game has comments");
			m = AlgebraicMoves.Normalized(g.Moves);
			ams = AlgebraicMoves.Create(m);
			Assert.IsEmpty(ams.Comments, "comments removed");
			Assert.IsTrue(_rxMoveNumbers.IsMatch(m));

		}

		[TestMethod]
		public void NormalizeMovesForTerminatedGame()
		{
			const string MOVES = "1. d4 Nf6 2. c4 e6 0-1";
			string moves = AlgebraicMoves.Normalized(MOVES);
			Assert.AreEqual(MOVES, moves);
		}

		[TestMethod]
		public void EnumerateMoves()
		{
			AlgebraicMoves moves = AlgebraicMoves.Create(TESTMOVES);
			Assert.AreEqual(110, moves.MoveCount);
			Assert.IsTrue(moves.All(m => !m.IsEmpty));
			int n = 0;
			foreach (var m in moves)
			{
				Assert.AreEqual(n++, m.SerialNumber);
			}
		}

		[TestMethod]
		public void ParseMoves()
		{
			AlgebraicMoves moves = AlgebraicMoves.Create(TESTMOVES);
			AlgebraicMoves.ShowDiagnostics = true;
			switch (moves.Parse())
			{
				case IParsedGameFail f: Assert.Fail($"{f.Error}"); break;
				case IParsedGameSuccess s:
					Assert.AreEqual(GameResult.WhiteWin, s.Result);
					Assert.HasCount(moves.MoveCount - 1, s.Moves);
					break;
			}
		}

		[TestMethod]
		public void ParseMoves2()
		{
			AlgebraicMoves moves = AlgebraicMoves.Create(TESTMOVES2);
			Assert.AreEqual(49, moves.MoveCount);
			AlgebraicMoves.ShowDiagnostics = true;
			switch (moves.Parse())
			{
				case IParsedGameFail f: Assert.Fail($"{f.Error}"); break;
				case IParsedGameSuccess s:
					Assert.HasCount(moves.MoveCount - 1, s.Moves);
					Assert.AreEqual(GameResult.Draw, s.Result);
					break;
			}
		}

		[TestMethod]
		public void ParseEnPassant()
		{
			AlgebraicMoves moves = AlgebraicMoves.Create("1. c4 g6 2. Nc3 Bg7 3. d4 c5 4. d5 Bxc3+ 5. bxc3 f5 6. Bd2 f4 7. e4");
			switch (moves.Parse(true))
			{
				case IParsedGameFail f: Assert.Fail($"Parsing Failed after {f.Moves.Count} moves: {f.Error.Error}"); break;
				case IParsedGameSuccess s:
					Assert.HasCount(13, s.Moves);
					IChessSquare f4 = s.FinalBoard[File.F, Rank.R4], e3 = s.FinalBoard[File.E, Rank.R3];
					Assert.IsTrue(f4.HasPiece);
					switch (f4.Piece)
					{
						case IChessPawn: break;
						default: Assert.Fail("Expected pawn on f4"); break;
					}
					IChessPawn pawn = (IChessPawn)f4.Piece;
					Assert.AreEqual(Hue.Dark, pawn.Side);
					Assert.IsFalse(e3.HasPiece);
					Assert.IsTrue(pawn.CanMoveTo(e3));
					break;

			}
		}

		[TestMethod]
		public void ParseTWIC920_6()
		{
			const string moves = "1. d4 d5 2. c4 c6 3. Nc3 Nf6 4. e3 e6 5. Nf3 Nbd7 6. Qc2 b6 7. Bd3 Bb7 8. O-O Bb4 9. Bd2 Rc8 10. cxd5 exd5 11. e4 Bxc3 12. bxc3 dxe4 13. Bxe4 O-O 14. Bg5 h6 15. Bh4 g5 16. Bg3 Nxe4 17. Qxe4 f5 18. Qe6+ Kh7 19. Ne5 Nxe5 20. Bxe5 Qd5 21. Qxd5 cxd5 22. a4 Ba6 23. Rfe1 Rc6 24. a5 b5 25. f4 Rxc3 26. Bd6 Rf7 27. Re8 b4 28. Bxb4 Rc2 29. fxg5 hxg5 30. Bd6 Bd3 31. Ra3 Rc1+ 32. Kf2 Rf1+ 33. Kg3 Be4 34. Be5 Rg1 35. Ra2 f4+ 36. Kg4 Bxg2 37. Rh8+ Kg6 38. Rg8+ Kh6 39. Rh8+ Kg6 40. Rg8+ Kh6 41. Rh8+ Rh7 42. Rxh7+ Kxh7 43. Kxg5 f3 44. Rb2 Bf1+ 45. Kf4 Be2 46. Ke3 a6 47. Rb7+ Kh6 48. Rd7 Bd1 49. Bg3 Rg2 50. Bf2 Rxh2 51. Rxd5 Be2 52. Rd8 Kg6 53. d5 Rh1 54. d6 Rd1 55. d7 Kf7 56. Rh8 Rxd7 57. Rh7+ Ke6 58. Rxd7 Kxd7 59. Ke4 1/2-1/2";
			AlgebraicMoves am = AlgebraicMoves.Create(moves);
			switch (am.Parse(true))
			{
				case IParsedGameSuccess s: Console.WriteLine(s.Moves.Count); break;
				case IParsedGameFail f: Assert.Fail(f.Error.Error.ToString()); break;
			}
		}

		[TestMethod]
		public void ParseGame28()
		{
			var g = GameDB.Get(28);
			AlgebraicMoves moves = AlgebraicMoves.Create(g.Moves);
			switch (moves.Parse(true))
			{
				case IParsedGameSuccess: break;
				case IParsedGameFail f: Assert.Fail(f.Error.ToString()!); break;
			}
		}

		[TestMethod]
		public void InitialCommentExtracted()
		{
			var g = GameDB.Get(2092644);
			Assert.IsNotNull(g);
			AlgebraicMoves moves = AlgebraicMoves.Create(g.Moves);
			Assert.HasCount(1, moves.Comments);
			Assert.AreEqual("Default", moves.Comments[0].Comment);
			g = GameDB.Get(1945610);
			Assert.IsNotNull(g);
			moves = AlgebraicMoves.Create(g.Moves);
			Assert.HasCount(1, moves.Comments);
			Assert.AreEqual("Missing Armageddon - Dubov wins as he was black", moves.Comments[0].Comment);
		}

		[TestMethod]
		public void MultipleCommentsExtracted()
		{
			var g = GameDB.Get(997576);
			AlgebraicMoves moves = AlgebraicMoves.Create(g.Moves);
			Assert.HasCount(16, moves.Comments);
			Assert.IsTrue(moves.Comments.All(c => string.Equals("book", c.Comment)));
		}

		[TestMethod]
		public void FinalComment()
		{
			var g = GameDB.Get(4407141);
			Assert.IsNotNull(g);
			AlgebraicMoves moves = AlgebraicMoves.Create(g.Moves);
			Assert.HasCount(1, moves.Comments);
			Assert.AreEqual("Result given as 0-0", moves.Comments[0].Comment);
			switch (moves.Parse())
			{
				case IParsedGameFail f: Assert.Fail(f.Error.Error.ToString()); break;
				case IParsedGameSuccess s: Assert.AreEqual(GameResult.Draw, s.GameEnd.Result); break;
			}
		}

		[TestMethod]
		public void MovedPieceUndefined()
		{
			var g = GameDB.Get(4402911);
			Assert.IsNotNull(g);
			AlgebraicMoves moves = AlgebraicMoves.Create(g.Moves);
			switch (moves.Parse(true))
			{
				case IParsedGameFail f:
					Assert.AreEqual(ParseErrorType.MovedPieceUndefined, f.Error.Error);
					Assert.HasCount(6, f.Moves);
					break;
				default: Assert.Fail("Expected parse error"); break;
			}
		}

		private const string TESTMOVES3 = "1. Nf3 Nf6 2. g3 c5 3. Bg2 Nc6 4. d4 cxd4 5. Nxd4 g6 6. O-O Bg7 7. c4 O-O 8. Nc3 Qb6 9. Nb3 d6 10. Be3 Qa6 11. Nb5 Rb8 12. h3 Be6 13. Nc7 Qxc4 14. Rc1 Qa4 15. Nxe6 fxe6 16. Nd4 Qxd1 17. Rfxd1 Nxd4 18. Bxd4 Nd5 19. Bxd5 exd5 20. Rc7 Bxd4 21. Rxd4 e6 22. Rh4 h5 23. Ra4 a6 24. Rb4 Rf7 25. Rc6 Rbf8 26. Rc1 Rxf2 27. Rxb7 e5 28. Rc8 Rf1+ 29. Kg2 Rf2+ 30. Kg1 Rxe2 31. Rcc7 Re1+ 1/2-1/2";
		[TestMethod]
		public void TestMoves3()
		{
			// These moves began failing in ImporterTest.Verifyparseable after performing king-check tests with every call to Piece.CanMoveTo.
			AlgebraicMoves moves = AlgebraicMoves.Create(TESTMOVES3);
			switch (moves.Parse(true))
			{
				case IParsedGameFail f: Assert.Fail(f.Error.Error.ToString()); break;
			}
		}

		[TestMethod]
		public void MoveNumber()
		{
			AlgebraicMoves moves = AlgebraicMoves.Create(TESTMOVES3);
			int n = 0, nMove = 1;
			foreach (AlgebraicMove move in moves.MoveList)
			{
				int moveNum = MoveCounter.SerialToGameNumber(move.SerialNumber);
				Assert.AreEqual(nMove, moveNum);
				if (++n % 2 == 0) nMove++;
			}
		}

		[TestMethod]
		public void ParseVariant()
		{
			// Game 4557406
			string FEN = "qrbknbrn/pppppppp/8/8/8/8/PPPPPPPP/QRBKNBRN w GBgb - 0 1";
			string MOVEZ = "1. d4 d5 2. f3 Ng6 3. Nd3 Nf6 4. g4 e6 5. Ng3 b6 6. h4 Nxh4 7. Ne5 Ke8 8. e4 Bd6 9. Bb5+ Kf8 10. g5 Bxe5 11. dxe5 Nxf3 12. exf6 dxe4 13. Bd2 Nxd2 14. O-O-O e3 15. a4 Qd5 16. Qa3+ Qc5 17. Qd3 Bb7 18. Qxh7 Rd8 19. g6 Nb3+ 20. Kb1 Rxd1+ 21. Rxd1 Nd2+ 22. Kc1 Nb3+ 23. Kb1 Nd2+ 24. Kc1 Qg5 25. Nh5 Qxg6 26. fxg7+ Rxg7 27. Qxg7+ Qxg7 28. Nxg7 Kxg7 29. Re1 f5 30. Rxe3 Ne4 31. a5 f4 32. a6 Bd5 33. Rh3 Ng5 34. Rh5 Kf6 35. Rh8 Nf7 36. Rb8 Nd6 37. Bf1 e5 38. c4 Bxc4 39. Bxc4 Nxc4 40. Ra8 f3 41. Rf8+ Ke6 42. Rxf3 Nd6 43. Rf8 Kd5 44. Kc2 Kc4 45. Ra8 Nb5 46. Re8 Nd4+ 47. Kd2 c5 48. Re7 Nc6 49. Rc7 Kb5 50. Ke3 Nb4 51. Rxa7 Nxa6 52. Re7 Kc4 53. Rxe5 Nb4 54. Kd2 Kb3 55. Re6 b5 56. Re5 Kc4 57. Re3 Nc6 58. Kc2 Nd4+ 59. Kd2 Nc6 60. Kc2 Nd4+ 61. Kd2 Nc6 1/2-1/2";
			AlgebraicMoves moves = AlgebraicMoves.Create(MOVEZ, FEN);
			switch (moves.Parse(true))
			{
				case IParsedGameFail f:
					System.Diagnostics.Debug.WriteLine(f.UnparsedMoves.Count);
					Assert.Fail(f.Error.Error.ToString()); break;
			}
		}

		[TestMethod]
		public void ParseVariant2()
		{
			// Game 4557413
			const string MOOVS = "1. f4 f5 2. g3 g6 3. Bf2 Nb6 4. e4 fxe4 5. Bxe4 e5 6. fxe5 Bxe5 7. Re1 d6 8. Nf3 Bf6 9. Qh3+ Bd7 10. Qxh7 Ne7 11. Be3 Nc4 12. Bh6 Qh8 13. Qxh8 Rxh8 14. Bf4 O-O-O 15. h4 d5 16. Bd3 Bg4 17. Nh2 Bf5 18. Bxf5+ Nxf5 19. c3 Rde8 20. Rxe8+ Rxe8 21. Kc2 Re2 22. Ng4 Be7 23. Nb3 Rg2 24. Re1 b6 25. Nh6 Bd6 26. Nxf5 gxf5 27. Rh1 Bxf4 28. gxf4 Ne3+ 29. Kc1 Ng4 30. Nd4 Nf2 31. Rf1 Kd7 32. Nxf5 Ne4 33. Rh1 Ke6 34. Nd4+ Kf7 35. h5 Kg8 36. Nf3 Ng3 37. Rh4 Nf5 38. Rh3 Rg4 39. h6 Rxf4 40. h7+ Kh8 41. Ne5 Ne7 42. Rh6 d4 43. Kc2 dxc3 44. Re6 1-0";
			const string FEN = "nrkrbqnb/pppppppp/8/8/8/8/PPPPPPPP/NRKRBQNB w DBdb - 0 1";
			AlgebraicMoves moves = AlgebraicMoves.Create(MOOVS, FEN);
			switch (moves.Parse(true))
			{
				case IParsedGameFail f: Assert.Fail(f.Error.Error.ToString()); break;
			}
		}

		[TestMethod]
		public void ParseVariant3()
		{
			// Game 4557411
			const string MOVES = "1. f4 f5 2. g3 g6 3. d3 e5 4. Bc3 d6 5. Nf3 Nb6 6. fxe5 Nd5 7. Bd2 dxe5 8. e4 fxe4 9. dxe4 Nb4 10. Qc4 a5 11. O-O b5 12. Qe2 Qc5+ 13. Be3 Qc4 14. Qe1 Nf6 15. a3 Nc6 16. Nb3 Ng4 17. Bg2 Nxe3 18. Qxe3 Rb6 19. Rfd1 Nd4 20. Nxa5 Qxc2 21. Nxd4 exd4 22. Qe1 d3 23. Rdc1 Bd4+ 24. Kh1 Qe2 25. Qxe2 dxe2 26. Re1 b4 27. Nc4 Rc6 28. b3 bxa3 29. Nxa3 Ra6 30. Nc4 Ra2 31. Bf3 Bc6 32. Rbc1 Bf2 33. Kg2 Bxe1 34. Rxe1 Rd3 35. Ne5 Rd1 36. Rxe2 Rxe2+ 37. Bxe2 Bxe4+ 38. Kf2 Rb1 39. Ba6+ Bb7 40. Bc4 Rb2+ 0-1";
			AlgebraicMoves moves = AlgebraicMoves.Create(MOVES, "nrkrbqnb/pppppppp/8/8/8/8/PPPPPPPP/NRKRBQNB w DBdb - 0 1");
			switch (moves.Parse(true))
			{
				case IParsedGameSuccess s:
					Console.WriteLine(s.Game.Moves.Count);
					break;
				case IParsedGameFail f:
					Assert.Fail(f.Error.Error.ToString());
					break;
			}
		}

		[TestMethod]
		public void IsKingsideCastle()
		{
			AlgebraicMove m = new AlgebraicMove(AlgebraicMove.QSCastle, 0, 10);
			Assert.IsTrue(m.IsQueensideCastle);
			Assert.IsFalse(m.IsKingsideCastle);
			m = new AlgebraicMove(AlgebraicMove.KSCastle, 10, 10);
			Assert.IsTrue(m.IsKingsideCastle);
			Assert.IsFalse(m.IsQueensideCastle);
		}

		[TestMethod]
		public void ParseComments()
		{
			List<QuickPgnGame> games = GameDB.All.Where(g => g.Moves.Contains("{")).ToList();
			foreach (var g in games)
			{
				try
				{
					AlgebraicMoves ams = AlgebraicMoves.Create(g.Moves);
				}
				catch (Exception e)
				{
					Assert.Fail(e.Message);
				}
			}
		}

		[TestMethod]
		public void ParseMultipleComments()
		{
			var g = GameDB.Get(997540);
			AlgebraicMoves moves = AlgebraicMoves.Create(g.Moves);
			Assert.HasCount(23, moves.Comments);
			switch (moves.Parse(true))
			{
				case IParsedGameSuccess s:
					Assert.HasCount(89, s.Game.Moves);
					break;
				case IParsedGameFail f:
					Assert.Fail(f.Error.Error.ToString());
					break;
			}
		}

		[TestMethod]
		public void ParseImmediateResignation()
		{
			switch (AlgebraicMoves.Parse("0-1"))
			{
				case IParsedGameSuccess s: Assert.HasCount(0, s.Game.Moves); break;
				case IParsedGameFail f: Assert.Fail(f.Error.Error.ToString()); break;
			}
		}

		[TestMethod]
		public void UnmatchedMovePattern()
		{
			string pgn = System.IO.File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "UnmatchedMovePattern.pgn"));
			var gb = PgnSourceParser.Parse(pgn);
			if (gb is IPgnParseSuccess ps)
			{
				AlgebraicMoves ams = AlgebraicMoves.Create(ps.Import.Moves);
				switch(ams.Parse())
				{
					case IParsedGameSuccess s: 
						Assert.HasCount(122, s.Game.Moves);
						Assert.AreEqual("d1=Q+", s.Game.Moves.Last().AlgebraicMove);
						Console.WriteLine(s.Game.Moves.Last().AlgebraicMove);
						break;
					case IParsedGameFail f: Assert.Fail(f.Error.Error.ToString()); break;
				}
			}
		}

		[TestMethod]
		public void MayBeAlgebraicFormat()
		{
			const string MOVES = "1. d4 d5 2. c4 e6 3. Nc3 Nf6 4. Bg5 c6 5. e3 Nbd7 6. cxd5 exd5 7. Nf3 Be7 8. Qc2 O-O 9. Bd3 Re8 10. O-O Nf8 11. Rab1 g6 12. b4 a6 13. a4 Ne6 14. Bh4 Ng7 15. b5 axb5 16. axb5 Bf5 17. bxc6 bxc6 18. Ne5 Bxd3 19. Qxd3 Rc8 20. Qa6 {incomplete} 1/2-1/2";
			Assert.IsTrue(AlgebraicMoves.MayBeAlgebraicFormat(MOVES));
		}
	}
}