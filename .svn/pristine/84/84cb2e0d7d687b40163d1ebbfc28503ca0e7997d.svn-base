using Chess.Lib.Hardware;
using Chess.Lib.Hardware.Pieces;
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
			Assert.AreEqual(16, pawns.Count);
			Assert.AreEqual(8, pawns.Where(p => p.Side == Hue.Light).Count());
			Assert.AreEqual(8, pawns.Where(p => p.Side == Hue.Dark).Count());
			Assert.IsTrue(pawns.Where(p => p.Side == Hue.Light).All(p => p.Square.Rank == Rank.R2));
			Assert.IsTrue(pawns.Where(p => p.Side == Hue.Dark).All(p => p.Square.Rank == Rank.R7));
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
			bb.SetPiece(File.D, Rank.R4, PieceType.Pawn, Hue.Light);
			IBoard b = (IBoard)bb.CreateBoard();
			IPawn p = b.ActivePieces.OfType<IPawn>().First();
			Assert.IsFalse(p.CanMoveTo(b[File.D, Rank.R3]));
		}

		//[TestMethod]
		//public async Task BlackEnPassant()
		//{
		//	using Game game = await Game.TwoPlayerGame();
		//	var result = game.White.TryMove("e2e4");
		//	Assert.IsTrue(result.Succeeded);
		//	result = game.NextPlayer.TryMove("d7d5");
		//	Assert.IsTrue(result.Succeeded);
		//	result = game.NextPlayer.TryMove("g1f3");
		//	Assert.IsTrue(result.Succeeded);
		//	result = game.NextPlayer.TryMove("d5d4");
		//	Assert.IsTrue(result.Succeeded);
		//	result = game.NextPlayer.TryMove("c2c4");
		//	Assert.IsTrue(result.Succeeded);
		//	result = game.NextPlayer.TryMove("d4e3");
		//	Assert.IsFalse(result.Succeeded, "en passant wrong way:" + result.Error);
		//	result = game.NextPlayer.TryMove("d4c3");
		//	Assert.IsTrue(result.Succeeded, "en passant");
		//}

		//[TestMethod]
		//public async Task WhiteEnPassant()
		//{
		//	using Game game = await Game.TwoPlayerGame();
		//	var result = game.White.TryMove("e2e4");
		//	Assert.IsTrue(result.Succeeded);
		//	result = game.Black.TryMove("d7d5");
		//	Assert.IsTrue(result.Succeeded);
		//	result = game.White.TryMove("e4e5");
		//	Assert.IsTrue(result.Succeeded);
		//	result = game.Black.TryMove("f7f5");
		//	Assert.IsTrue(result.Succeeded);
		//	Square sB = game.Board[File.F, Rank.R6];
		//	result = game.White.TryMove("e5d6");
		//	Assert.IsFalse(result.Succeeded, "en passant wrong way");
		//	result = game.White.TryMove("e5f6");
		//	Assert.IsTrue(result.Succeeded, "en passant:" + result.Error);
		//}

		//[TestMethod]
		//public async Task BlackFirstMoveNoEP()
		//{
		//	using Game game = await Game.TwoPlayerGame();
		//	var res = game.White.TryMove("d2d4");
		//	Assert.IsTrue(res.Succeeded);
		//	Pawn bp = game.Board[File.D, Rank.R7].Piece as Pawn;
		//	Assert.IsNotNull(bp);
		//	Assert.IsFalse(bp.CanMoveTo(game.Board[File.D, Rank.R3]));

		//	var allowed = game.Black.AllowedMoves(game.Board[File.D, Rank.R7]).ToList();
		//	Assert.AreEqual(2, allowed.Count);
		//	Assert.IsTrue(allowed.All(m => m.ToSquare.File == File.D));
		//}

		//[TestMethod]
		//public async Task White2ndMoveNoEP()
		//{
		//	using Game game = await Game.TwoPlayerGame();
		//	var res = game.White.TryMove("d2d4");
		//	Assert.IsTrue(res.Succeeded);
		//	res = game.Black.TryMove("c7c5");
		//	Assert.IsTrue(res.Succeeded);
		//	Square c6 = game.Board[File.C, Rank.R6];
		//	Pawn p = game.Board[File.D, Rank.R4].Piece as Pawn;
		//	Assert.IsFalse(p.CanMoveTo(c6));
		//}

		//[TestMethod]
		//public async Task EnpassantMoveIsCapture()
		//{
		//	using Game game = await Game.TwoPlayerGame();
		//	var res = game.White.TryMove("d2d4");
		//	Assert.IsTrue(res.Succeeded);
		//	res = game.Black.TryMove("c7c5");
		//	Assert.IsTrue(res.Succeeded);
		//	res = game.White.TryMove("d4d5");
		//	Assert.IsTrue(res.Succeeded);
		//	res = game.Black.TryMove("e7e5");
		//	Assert.IsTrue(res.Succeeded);
		//	res = game.White.TryMove("d5e6");
		//	Assert.IsTrue(res.Succeeded);
		//	Assert.IsTrue(res.Result.IsEnPassant);
		//	Assert.IsNotNull(res.Result.CapturedPiece, "en-passant captures piece");
		//	Assert.IsFalse(game.Board[File.E, Rank.R5].HasPiece, "captured piece removed");
		//	Assert.AreEqual(PieceType.Pawn, res.Result.CapturedPiece.Type);
		//}

		//[TestMethod]
		//public async Task CaptureAfterEnPassant()
		//{
		//	const string MOVES = "D2D4C7C5D4D5E7E5D5E6";

		//	Game game = await Game.TwoPlayerGame();
		//	var moves = game.ParseMoves(MOVES).ToList();
		//	Assert.AreEqual(MOVES.Length / 4, moves.Count);
		//	Assert.IsTrue(game.Board[File.F, Rank.R7].HasPiece, "Pawn on E7");
		//	Pawn p = game.Board[File.F, Rank.R7].Piece as Pawn;
		//	Assert.IsTrue(p.CanMoveTo(game.Board[File.E, Rank.R6]));
		//	Move m = moves.Last();
		//	Assert.IsTrue(m.IsCapture);
		//	Assert.IsTrue(m.IsEnPassant);
		//	Assert.IsFalse(game.Board[File.E, Rank.R5].HasPiece);
		//	var res = game.Black.TryMove("F7E6");
		//	Assert.IsTrue(res.Succeeded);
		//	Assert.IsTrue(res.Result.IsCapture);
		//	Assert.IsTrue(game.Board[File.E, Rank.R6].HasPiece);
		//}

		//[TestMethod]
		//public async Task PawnsCannotJump()
		//{
		//	// From game 466529 after move 9:
		//	using Game game = await Game.FromFEN("r1bq1rk1/ppp1bppp/3p1n2/8/8/2P1BN2/PPPQ1PPP/2KR1B1R w - - 7 10");
		//	Pawn p = game.Board[File.C, Rank.R2].Piece as Pawn;
		//	Assert.IsNotNull(p);
		//	Square sOver = game.Board[File.C, Rank.R3], to = game.Board[File.C, Rank.R4];
		//	Assert.IsTrue(sOver.HasPiece);
		//	Assert.IsFalse(to.HasPiece);
		//	Assert.IsFalse(p.CanMoveTo(to));
		//}

		//[TestMethod]
		//public async Task CanMove19524()
		//{
		//	Game game = await Game.FromFEN("rnbqkb1r/pppppppp/5n2/8/2PP4/8/PP2PPPP/RNBQKBNR b KQkq - 0 2");
		//	Assert.AreSame(game.Black, game.NextPlayer);
		//	var move = game.Black.TryMove("e7e6");
		//	Assert.IsTrue(move.Succeeded);
		//}

		//[TestMethod]
		//public async Task CannotUnblockBishopCheck()
		//{
		//	using Game game = await Game.FromFEN("rnbqkbnr/pp2pppp/2p5/1B1p4/3P4/4P3/PPP2PPP/RNBQK1NR b KQkq - 1 3");
		//	Assert.AreSame(game.Black, game.NextPlayer);
		//	Pawn p = game.Board[File.C, Rank.R6].Piece as Pawn;
		//	Assert.IsNotNull(p);
		//	Assert.IsTrue(p.CanMoveTo(game.Board[File.B, Rank.R5]));  // capture bishop
		//	Assert.IsFalse(p.CanMoveTo(game.Board[File.C, Rank.R5]));	// exposes king to check by bishop
		//}
	}
}
