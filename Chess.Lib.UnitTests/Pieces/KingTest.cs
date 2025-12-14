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
			Assert.AreEqual(2, kings.Count);
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
			Assert.AreEqual(8, canMove.Count);
			bb.Clear();
			bb.SetPiece(File.A, Rank.R1, PieceType.King, Hue.Light);
			b = (IBoard)bb.CreateBoard();
			k = (IKing)b.ActivePieces.First();
			canMove.Clear();
			foreach(ISquare s in b)
			{
				if (k.CanMoveTo(s)) canMove.Add(s);
			}
			Assert.AreEqual(3, canMove.Count);

			bb.Clear();
			bb.SetPiece(File.D, Rank.R1, PieceType.King, Hue.Light);
			b = (IBoard)bb.CreateBoard();
			k = (IKing)b.ActivePieces.First();
			canMove.Clear();
			foreach (ISquare s in b)
			{
				if (k.CanMoveTo(s)) canMove.Add(s);
			}
			Assert.AreEqual(5, canMove.Count);
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
						case IParsedGameFail ee: Assert.Fail(ee.Error.ToString()); break;
						case IParsedGameSuccess ss:
							Console.WriteLine(ss.Game.Moves.Count);
							break;
					}
					break;
			}
		}

		//[TestMethod]
		//public async Task AfterE4()
		//{
		//	Game game = await Game.TwoPlayerGame();
		//	Board b = game.Board;
		//	King k = b[File.E, Rank.R1].Piece as King;
		//	Pawn p = b[File.E, Rank.R2].Piece as Pawn;
		//	var move = game.White.TryMove(k, b[File.E, Rank.R2]);
		//	Assert.IsFalse(move.Succeeded);
		//	move = game.White.TryMove(p, b[File.E, Rank.R4]);
		//	Assert.IsTrue(move.Succeeded);
		//	Pawn pb = b[File.E, Rank.R7].Piece as Pawn;
		//	move = game.Black.TryMove(p, b[File.E, Rank.R5]);
		//	Assert.IsTrue(move.Succeeded);
		//	move = game.White.TryMove(k, b[File.E, Rank.R2]);
		//	Assert.IsTrue(move.Succeeded);
		//}

		//[TestMethod]
		//public async Task RookMovesAfterKingsideCastle()
		//{
		//	using Game game = await Game.TwoPlayerGame();
		//	var moves = game.ParseMoves("E2E4D7D5G1F3B8C6F1B5C8D7").ToList();
		//	Assert.AreEqual(6, moves.Count);
		//	King wKing = game.Board[File.E, Rank.R1]?.Piece as King;
		//	Assert.IsNotNull(wKing);
		//	Assert.IsTrue(wKing.CanMoveTo(game.Board[File.G, Rank.R1]), "Can Castle Kingside");
		//	Rook wRook = game.Board[File.H, Rank.R1].Piece as Rook;
		//	Assert.IsNotNull(wRook);
		//	var move = game.White.TryMove(wKing, game.Board[File.G, Rank.R1]);
		//	Assert.IsTrue(move.Result.IsKingsideCastle);
		//	Assert.IsTrue(move.Succeeded);
		//	Assert.AreSame(wKing, game.Board[File.G, Rank.R1].Piece);
		//	Assert.IsFalse(game.Board[File.H, Rank.R1].HasPiece, "Rook is no longer on H1");
		//	Assert.IsTrue(game.Board[File.F, Rank.R1].HasPiece, "A piece is on F1");
		//	Assert.AreSame(wRook, game.Board[File.F, Rank.R1].Piece, "Room on F1");
		//	Assert.AreSame(wRook, move.Result.CastledRook);
		//}

		//[TestMethod]
		//public async Task RookMovesAfterWhiteQueensideCastle()
		//{
		//	using Game game = await Game.TwoPlayerGame();
		//	var moves = game.ParseMoves("D2D4D7D5C1F4C7C6B1C3G8F6D1D3C8D7").ToList();
		//	Assert.AreEqual(8, moves.Count);
		//	King wKing = game.Board[File.E, Rank.R1]?.Piece as King;
		//	Assert.IsNotNull(wKing);
		//	Assert.IsTrue(wKing.CanMoveTo(game.Board[File.C, Rank.R1]), "Can Castle Queenside");
		//	Rook wRook = game.Board[File.A, Rank.R1].Piece as Rook;
		//	Assert.IsNotNull(wRook);
		//	var move = game.White.TryMove(wKing, game.Board[File.C, Rank.R1]);
		//	Assert.IsTrue(move.Result.IsQueensideCastle);
		//	Assert.IsTrue(move.Succeeded);
		//	Assert.AreSame(wKing, game.Board[File.C, Rank.R1].Piece);
		//	Assert.IsFalse(game.Board[File.A, Rank.R1].HasPiece, "Rook is no longer on H1");
		//	Assert.IsTrue(game.Board[File.D, Rank.R1].HasPiece, "A piece is on F1");
		//	Assert.AreSame(wRook, game.Board[File.D, Rank.R1].Piece, "Rook on D1");
		//	Assert.AreSame(wRook, move.Result.CastledRook);
		//}

		//[TestMethod]
		//public async Task RookMovesAfterBlackQueesideCastle()
		//{
		//	using Game game = await Game.TwoPlayerGame();
		//	var moves = game.ParseMoves("D2D4D7D5C2C3C8F5G1F3B8C6E2E3D8D7B1D2").ToList();
		//	Assert.AreEqual(9, moves.Count);
		//	King bKing = (King)game.Board[File.E, Rank.R8].Piece;
		//	Rook bRook = (Rook)game.Board[File.A, Rank.R8].Piece;
		//	Square to = game.Board[File.C, Rank.R8];
		//	Assert.IsTrue(bKing.CanMoveTo(to));
		//	var move = game.Black.TryMove("e8c8");
		//	Assert.IsTrue(move.Succeeded);
		//	Assert.IsTrue(move.Result.IsQueensideCastle);
		//	Assert.AreSame(bRook, move.Result.CastledRook);
		//	Assert.IsFalse(move.Result.From.HasPiece);
		//	Assert.AreSame(bKing, game.Board[File.C, Rank.R8].Piece);
		//	Assert.IsFalse(game.Board[File.A, Rank.R8].HasPiece);
		//	Assert.AreSame(bRook, game.Board[File.D, Rank.R8].Piece);
		//}

		//[TestMethod]
		//public async Task CheckEventRaised()
		//{
		//	using Game game = await Game.TwoPlayerGame();
		//	var moves = game.ParseMoves("d2d4c7c5d4d5g8f6b1c3e7e6e2e4d7d6");
		//	Assert.AreEqual(8, moves.Count);
		//	King king = null;
		//	game.Check += (k) =>
		//	{
		//		king = k;
		//	};
		//	var move = game.NextPlayer.TryMove("f1b5");
		//	Assert.IsTrue(move.Succeeded);
		//	Assert.IsNotNull(king);
		//	Assert.AreEqual(Hue.Dark, king.Side);
		//}

		//[TestMethod]
		//public async Task MustResolveCheck()
		//{
		//	using Game game = await Game.TwoPlayerGame();
		//	var moves = game.ParseMoves("d2d4c7c5d4d5g8f6b1c3e7e6e2e4d7d6f1b5");
		//	Assert.AreEqual(9, moves.Count);
		//	King bk = game.Board.Pieces.First(p => p.Type == PieceType.King && p.Side == Hue.Dark) as King;
		//	Assert.IsNotNull(bk);
		//	Assert.IsTrue(bk.IsInCheck());
		//	var result = game.Black.TryMove("a7a6");
		//	Assert.IsFalse(result.Succeeded);
		//}

		//[TestMethod]
		//public async Task CanMoveG2()
		//{
		//	const string MOVES = "d2d4c7c5d4d5g8f6c2c4a7a6a2a4g7g6b1c3d7d6e2e4f8g7f1d3e7e6h2h3b8d7g1f3e8g8e1g1b7b6c1e3a8b8d1d2e6d5e4d5f6e8a1e1b8b7g1h1a6a5e3g5f7f6g5h4g6g5h4g3h7h6d2c2d7e5g3e5f6e5f3h2h6h5f2f3e5e4c3e4g7e5c2e2e8g7g2g4e5f4h1g2h5h4e2c2b7f7e4c3f4g3e1e2g3h2g2h2f7f3f1f3f8f3c3e4d8e7h2g2f3f4c2d2c8g4h3g4f4g4g2h1g4f4e4c5e7f6c5e4f6d4d2e3d4e3e2e3g7f5e3e1f4f3e4f6g8f7d3f5f7f6f5g4f3f4e1e6f6f7g4h5f7g7h5e2g5g4e6d6g4g3d6e6g7f7h1g1f4d4e2f1d4d2e6b6f7f8b2b3f8f7b6e6d2d1e6h6d1d4d5d6f7g7h6h5d4d6h5h4d6d8h4f4d8b8f4f3b8d8c4c5d8c8f3c3g7f6f1h3c8c6h3g2c6c8c5c6f6e7g2f3e7d6c3d3d6c5d3d7c8f8";
		//	using Game game = await Game.TwoPlayerGame();
		//	var moves = game.ParseMoves(MOVES);
		//	Assert.AreEqual(136, moves.Count);
		//	Assert.IsTrue(game.White.HasNextMove);
		//	Square g1 = game.Board[File.G, Rank.R1];
		//	Assert.IsTrue(g1.HasPiece);
		//	Assert.AreEqual(PieceType.King, g1.Piece.Type);
		//	King k = (King)g1.Piece;
		//	Assert.AreEqual(Hue.Light, k.Side);
		//	Square g2 = game.Board[File.G, Rank.R2];
		//	Assert.IsFalse(g2.HasPiece);
		//	Assert.IsTrue(k.CanMoveTo(g2));
		//}

		//[TestMethod]
		//public async Task ResolveCheck()
		//{
		//	const string MOVES = "d2d4 c7c5 d4d5 g8f6 c2c4 b7b5 c4b5 a7a6 b5a6 e7e6 b1c3 e6d5 c3d5 f8e7 d5e7 d8e7 c1f4 d7d5 e2e3 e8g8 g1f3 h7h6 a2a3 b8a6 f1b5 e7b7 a3a4 c8d7 b5d7 b7d7 e1g1 f8e8 d1d3 c5c4 d3a3 f6e4 b2b4 c4b3 a3b3 a6c5 b3b5 e8e7 f3e5 d7e6 f2f3 e7b7 b5c6 e6c6 e5c6 e4c3 f4e5 c3a4 f1b1";
		//	using Game game = await Game.TwoPlayerGame();
		//	var moves = game.ParseMoves(MOVES);
		//	Assert.AreEqual(53, moves.Count);
		//	Assert.AreSame(game.Black, game.NextPlayer);
		//	King king = null;
		//	game.Check += (k) =>
		//	{
		//		king = k;
		//	};
		//	var move = game.Black.TryMove("b7b1");
		//	Assert.IsTrue(move.Succeeded);
		//	Assert.IsNotNull(king);
		//}

		//[TestMethod]
		//public async Task GetStatusReturns()
		//{
		//	// Tests for re-entrant King.CanMoveTo()
		//	using Game game = await Game.FromFEN("8/1R5p/r4p2/1p1B1P2/k2KP3/p7/7P/8 b - - 0 49"); // from game id=302032
		//	Assert.AreEqual(Hue.Dark, game.NextPlayer.Side);
		//	var result = game.NextPlayer.TryMove("a4b4");
		//	Assert.IsTrue(result.Succeeded);
		//}
	}
}
