using Chess.Lib.Games;
using Chess.Lib.Hardware.Pieces;
using Chess.Lib.Moves;
using Common.Lib.Contracts;
using System.Collections;
using System.Collections.Immutable;

namespace Chess.Lib.Hardware
{
	internal class NoBoard : IBoard, INoBoard
	{
		internal static readonly NoBoard Instance = new NoBoard();

		private NoBoard() { }
		IChessSquare IChessBoard.this[FileRank fileRank] => NoSquare.Default;
		IChessSquare IChessBoard.this[int squareIndex] => NoSquare.Default;
		ISquare IBoard.this[File file, Rank rank] => NoSquare.Default;
		IChessSquare IChessBoard.this[File file, Rank rank] => NoSquare.Default;

		ImmutableList<IPiece> IBoard.ActivePieces => ImmutableList<IPiece>.Empty;
		IReadOnlyList<IChessPiece> IChessBoard.ActivePieces => ImmutableList<IChessPiece>.Empty;
		ImmutableList<IPiece> IBoard.Promotions => ImmutableList<IPiece>.Empty;
		IReadOnlyList<IChessPiece> IChessBoard.Promotions => ImmutableList<IChessPiece>.Empty;
		ImmutableList<IPiece> IBoard.RemovedPieces => ImmutableList<IPiece>.Empty;
		IEnumerable<IChessPiece> IChessBoard.RemovedPieces => ImmutableList<IChessPiece>.Empty;
		IMove IBoard.LastMove => NoMove.Default;
		IChessMove IChessBoard.LastMove => NoMove.Default;
		IGame IBoard.Game
		{
			get => NoGame.Default;
			set { }
		}
		IChessGame IChessBoard.Game => NoGame.Default;
		IReadOnlyList<IChessPiece> IChessBoard.Removed => ImmutableList<IChessPiece>.Empty;
		bool IChessBoard.IsVariantBoard { get; }

#pragma warning disable 0067
		public event Handler<IChessMove>? MoveMade;
		public event Handler<IChessboardState>? StateApplied;
#pragma warning restore

		IEnumerable<ISquare> IBoard.AllowedKnightMovesFrom(ISquare fromSquare) => Enumerable.Empty<ISquare>();

		IEnumerable<IChessSquare> IChessBoard.AllowedMovesFrom(IChessSquare square) => Enumerable.Empty<IChessSquare>();

		bool IBoard.Apply(IMove move) => false;

		async Task<bool> IBoard.ApplyInteractive(IMove move) => false;

		void IBoard.ApplyState(IBoardState state) { }

		string IChessBoard.FENPiecePlacements => string.Empty;

		IEnumerable<ISquare> IBoard.DiagonalSquaresBetween(ISquare start, ISquare end)
			=> Enumerable.Empty<ISquare>();

		void IChessBoard.Display(TextWriter output) { }

		IEnumerable<ISquare> IBoard.FileSquaresBetween(ISquare start, ISquare end) => Enumerable.Empty<ISquare>();

		IBoardState IBoard.GetCurrentState() => BoardState.Empty;


		IEnumerator<IChessSquare> IEnumerable<IChessSquare>.GetEnumerator()
		{
			yield break;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			yield break;
		}

		IEnumerable<ISquare> IBoard.KingSquares(ISquare fromSquare) => Enumerable.Empty<ISquare>();

		KingState IBoard.OtherKingsExpectedState(IPiece movedPiece, ISquare toSquare, PieceType promotion)
		{
			return KingState.Default;
		}

		ISquare IBoard.ParseSquare(string fileRank) => NoSquare.Default;

		IEnumerable<IPiece> IBoard.PiecesTargeting(ISquare toSquare, Hue side) => Enumerable.Empty<IPiece>();

		IEnumerable<ISquare> IBoard.QueenSquaresBetween(ISquare start, ISquare end) => Enumerable.Empty<ISquare>();

		IEnumerable<ISquare> IBoard.RankSquaresBetween(ISquare start, ISquare end) => Enumerable.Empty<ISquare>();

		void IBoard.Reset() { }

		IEnumerable<ISquare> IBoard.SquaresBetween(ISquare start, ISquare end) => Enumerable.Empty<ISquare>();

		bool IBoard.WouldPutMyOwnKingInCheck(IPiece movedPiece, ISquare toSquare) => false;
	}
}
