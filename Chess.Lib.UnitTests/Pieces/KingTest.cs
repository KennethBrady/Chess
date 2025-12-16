using Chess.Lib.Hardware;
using Chess.Lib.Hardware.Pieces;
using Chess.Lib.Moves.Parsing;
using Chess.Lib.Pgn.Parsing;
using Chess.Lib.UnitTests.Moves;
using File = Chess.Lib.Hardware.File;

namespace Chess.Lib.UnitTests.Pieces
{
	[TestClass]
	[DeploymentItem("Pieces/Variant.pgn")]
	public class KingTest
	{
		[TestMethod]
		public void Defaults()
		{
			IBoard board = new Board();
			List<IKing> kings = board.ActivePieces.OfType<IKing>().ToList();
			Assert.HasCount(2, kings);
			Assert.AreEqual(1, kings.Where(k => k.Side == Hue.Light).Count());
			Assert.IsTrue(kings.All(k => k.Side != k.Square.Hue));
		}

		[TestMethod]
		public void CanMoveDefault()
		{
			IBoard b = new Board();
			List<King> kings = b.ActivePieces.OfType<King>().ToList();
			foreach(ISquare s in b)
			{
				foreach(King k in kings)
				{
					Assert.IsFalse(k.CanMoveTo(s), $"{k.Name}\t{s}");
				}
			}
		}

		[TestMethod]
		public void SoloKing()
		{
			BoardBuilder bb = new BoardBuilder();
			bb.SetPiece(File.D, Rank.R4, PieceType.King, Hue.Light);
			IBoard b = (IBoard)bb.CreateBoard();
			IKing k = (IKing)b.ActivePieces.First();
			List<ISquare> canMove = new();
			foreach(ISquare s in b)
			{
				if (k.CanMoveTo(s)) canMove.Add(s);
			}
			Assert.HasCount(8, canMove);
			bb.Clear();
			bb.SetPiece(File.A, Rank.R1, PieceType.King, Hue.Light);
			b = (IBoard)bb.CreateBoard();
			k = (IKing)b.ActivePieces.First();
			canMove.Clear();
			foreach(ISquare s in b)
			{
				if (k.CanMoveTo(s)) canMove.Add(s);
			}
			Assert.HasCount(3, canMove);

			bb.Clear();
			bb.SetPiece(File.D, Rank.R1, PieceType.King, Hue.Light);
			b = (IBoard)bb.CreateBoard();
			k = (IKing)b.ActivePieces.First();
			canMove.Clear();
			foreach (ISquare s in b)
			{
				if (k.CanMoveTo(s)) canMove.Add(s);
			}
			Assert.HasCount(5, canMove);
		}

		[TestMethod]
		public void SimpleCastle()
		{
			BoardBuilder bb = new BoardBuilder();
			bb.SetPiece(File.E, Rank.R1, PieceType.King, Hue.Light);
			bb.SetPiece(File.H, Rank.R1, PieceType.Rook, Hue.Light);
			bb.SetPiece(File.A, Rank.R1, PieceType.Rook, Hue.Light);
			IBoard b = (IBoard)bb.CreateBoard();
			IKing k = (IKing)b.ActivePieces.First(p => p.Type == PieceType.King);
			Assert.IsTrue(k.CanMoveTo(b[File.G, Rank.R1]), "White Kingside");
			Assert.IsTrue(k.CanMoveTo(b[File.C, Rank.R1]), "White Queenside");
			bb.SetPiece(File.F, Rank.R8, PieceType.Rook, Hue.Dark);
			bb.SetPiece(File.D, Rank.R8, PieceType.Rook, Hue.Dark);
			b = (IBoard)bb.CreateBoard();
			k = (IKing)b.ActivePieces.First(p => p.Type == PieceType.King);
			Assert.IsFalse(k.CanMoveTo(b[File.G, Rank.R1]), "White Kingside with Black Rook");
			Assert.IsFalse(k.CanMoveTo(b[File.C, Rank.R1]), "White Queenside with Black Rook");
		}

		[TestMethod]
		public void CannotMoveIntoCheck()
		{
			BoardBuilder bb = new BoardBuilder(false);
			bb.SetPiece(File.E, Rank.R1, PieceType.King, Hue.Light);
			bb.SetPiece(File.H, Rank.R2, PieceType.Rook, Hue.Dark);
			IChessBoard board = bb.CreateBoard();
			IChessKing king = (IChessKing)board.ActivePieces.First(p => p.Type == PieceType.King);
			IChessSquare e2 = board[File.E, Rank.R2];
			Assert.IsFalse(king.CanMoveTo(e2));

			bb = new BoardBuilder(false);
			bb.SetPiece(File.E, Rank.R1, PieceType.King, Hue.Light);
			bb.SetPiece(File.E, Rank.R2, PieceType.Pawn, Hue.Dark);
			bb.SetPiece(File.H, Rank.R2, PieceType.Rook, Hue.Dark);
			board = bb.CreateBoard();
			IChessRook rook = (IChessRook)board.ActivePieces.First(p => p.Type == PieceType.Rook);
			e2 = board[File.E, Rank.R2];
			Assert.IsTrue(e2.HasPiece);
			Assert.IsFalse(rook.CanMoveTo(e2));
			king = (IChessKing)board.ActivePieces.First(p => p.Type == PieceType.King);
			string fen = board.AsFEN();
			Assert.IsFalse(king.CanMoveTo(e2));
			Assert.AreEqual(fen, board.AsFEN(), "Board unchanged");
		}

		[TestMethod]
		public void KingSideCastle960()
		{
			var result = PgnSourceParser.ParseFromFile(Path.Combine(Environment.CurrentDirectory, "Variant.pgn")).First();
			switch(result)
			{
				case IPgnParseError e: Assert.Fail(e.ErrorType.ToString()); break;
				case IPgnParseSuccess s:
					AlgebraicMoves moves = AlgebraicMoves.Create(s.Import.Moves, s.Import.Tags["FEN"]);
					switch(moves.Parse(true))
					{
						case IParsedGameFail ee: Assert.Fail(ee.Error.ToString()!); break;
						case IParsedGameSuccess ss:
							Console.WriteLine(ss.Game.Moves.Count);
							break;
					}
					break;
			}
		}
	}
}
