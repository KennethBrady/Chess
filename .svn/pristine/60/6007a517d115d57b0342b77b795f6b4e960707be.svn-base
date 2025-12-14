using Chess.Lib.Games;
using Chess.Lib.Hardware;
using Chess.Lib.Hardware.Pieces;
using Chess.Lib.Moves.Parsing;
using File = Chess.Lib.Hardware.File;

namespace Chess.Lib.UnitTests.Pieces
{
	[TestClass]
	public class BishopTest
	{
		[TestMethod]
		public void Defaults()
		{
			IBoard board = new Board();
			List<IBishop> bishops = board.ActivePieces.OfType<IBishop>().ToList();
			Assert.AreEqual(4, bishops.Count);
			Assert.AreEqual(2, bishops.Where(b => b.Side == Hue.Dark).Count());
			Assert.AreEqual(2, bishops.Where(b => b.Side == Hue.Dark).Count());
		}

		[TestMethod]
		public void CanMoveDefault()
		{
			IBoard b = new Board();
			IBishop b0 = (IBishop)b.ActivePieces.First(p => p.Type == PieceType.Bishop);
			foreach (ISquare s in b)
			{
				string name = s.Name;
				Assert.IsFalse(b0.CanMoveTo(s), $"{s.Index}: {s.Name}");
			}
		}

		[TestMethod]
		public void CanMoveSolo()
		{
			BoardBuilder bb = new BoardBuilder();
			bb.SetPiece(File.C, Rank.R1, PieceType.Bishop, Hue.Light);
			IChessBoard b = bb.CreateBoard();
			IBishop? bp = b.ActivePieces.First() as IBishop;
			Assert.IsNotNull(bp);
			List<IChessSquare> allowed = b.Where(s => bp.CanMoveTo((ISquare)s)).ToList();
			Assert.AreEqual(7, allowed.Count);
			Assert.IsTrue(allowed.All(s => s.Hue == bp.Square.Hue));
			Assert.AreEqual("b2, d2, a3, e3, f4, g5, h6", string.Join(", ", allowed));

			bb.Clear();
			bb.SetPiece(File.A, Rank.R1, PieceType.Bishop, Hue.Light);
			b = bb.CreateBoard();
			bp = (IBishop)b.ActivePieces.First();
			allowed = b.Where(s => bp.CanMoveTo((ISquare)s)).ToList();
			Assert.IsTrue(allowed.All(s => s.Hue == bp.Square.Hue));
			Assert.IsTrue(allowed.All(s => s.Hue == bp.Square.Hue));
			Assert.AreEqual("b2, c3, d4, e5, f6, g7, h8", string.Join(", ", allowed));
		}

		[TestMethod]
		public void CannotMoveIfPutsKingInCheck()
		{
			IInteractiveChessGame g = GameFactory.CreateInteractive();
			EngineMoves moves = EngineMoves.Create("d2d4d7d5b1c3b8c6c3d5c6d4d1d4d8d5d4d5g8f6d5b7e7e5b7a8");
			int n = g.ApplyMoves(moves);
			Assert.AreEqual(13, n);
			Assert.IsTrue(g.Black.HasNextMove);
			IChessSquare c8 = g.Board[File.C, Rank.R8];
			Assert.IsTrue(c8.HasPiece);
			Assert.AreEqual(PieceType.Bishop, c8.Piece.Type);
			IChessBishop bishop = (IChessBishop)c8.Piece;
			IChessSquare d7 = g.Board[File.D, Rank.R7];
			Assert.IsFalse(d7.HasPiece);
			Assert.IsFalse(bishop.CanMoveTo(d7));
		}

		//[TestMethod]
		//public async Task CanMove197()
		//{
		//	// from after move 8 of game 197:
		//	using Game game = await Game.FromFEN("r1b1k1nr/pp3ppp/1qn1p3/3pP3/1b1P4/5N2/PP1B1PPP/R2QKBNR w KQkq - 5 9");
		//	Assert.AreSame(game.White, game.NextPlayer);
		//	Bishop b = game.Board[File.D, Rank.R2].Piece as Bishop;
		//	Assert.IsNotNull(b);
		//	Square to = game.Board[File.C, Rank.R3];
		//	Assert.IsTrue(b.CanMoveTo(to));
		//}
	}
}
