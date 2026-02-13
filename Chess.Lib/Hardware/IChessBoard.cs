using Chess.Lib.Games;
using Chess.Lib.Hardware.Pieces;
using Chess.Lib.Moves;
using Common.Lib.Contracts;
using System.Collections.Immutable;

namespace Chess.Lib.Hardware
{
	#region IChessBoard / IBoard Interfaces

	public interface IChessBoard : IEnumerable<IChessSquare>
	{
		#region Square Access
		IChessSquare this[File file, Rank rank] { get; }
		IChessSquare this[FileRank fileRank] { get; }
		IChessSquare this[int squareIndex] { get; }

		#endregion

		IReadOnlyList<IChessPiece> ActivePieces { get; }
		IReadOnlyList<IChessPiece> Promotions { get; }
		IReadOnlyList<IChessPiece> Removed { get; }
		IEnumerable<IChessPiece> RemovedPieces { get; }
		IEnumerable<IChessSquare> AllowedMovesFrom(IChessSquare square);
		void Display(TextWriter output);
		string Display()
		{
			using StringWriter w = new StringWriter();
			Display(w);
			return w.ToString();
		}
		string AsFEN();
		IChessGame Game { get; }
		bool IsGameBoard => Game is not INoGame;
		event Handler<IChessboardState>? StateApplied;
		bool IsVariantBoard { get; }
		IChessMove LastMove { get; }
	}

	internal interface IBoard : IChessBoard
	{
		new ISquare this[File file, Rank rank] { get; }
		new ImmutableList<IPiece> ActivePieces { get; }

		#region Methods for inspecting squares between start/end squares of a proposed move:

		IEnumerable<ISquare> RankSquaresBetween(ISquare start, ISquare end);
		IEnumerable<ISquare> FileSquaresBetween(ISquare start, ISquare end);
		IEnumerable<ISquare> DiagonalSquaresBetween(ISquare start, ISquare end);
		IEnumerable<ISquare> QueenMovesBetween(ISquare start, ISquare end);
		IEnumerable<ISquare> KingSquares(ISquare fromSquare);
		IEnumerable<ISquare> SquaresBetween(ISquare start, ISquare end);
		IEnumerable<ISquare> AllowedKnightMovesFrom(ISquare fromSquare);

		IEnumerable<IPiece> PiecesTargeting(ISquare toSquare, Hue side);

		#endregion

		/// <summary>
		/// Returns a list of all promotions that have occurred
		/// </summary>
		new ImmutableList<IPiece> Promotions { get; }
		new ImmutableList<IPiece> RemovedPieces { get; }

		/// <summary>
		/// Convert an engine-format move to an ISquare.
		/// </summary>
		/// <param name="fileRank">A file-rank (engine-move) format move</param>
		/// <returns>An ISquare, which will be NoSquare if the string is unparseable.</returns>
		ISquare ParseSquare(string fileRank);

		/// <summary>
		/// Apply the move to the board, changing the board state.
		/// </summary>
		/// <param name="move"></param>
		/// <returns>true if the move was made; false if the move is not possible</returns>
		bool Apply(IMove move);

		Task<bool> ApplyInteractive(IMove move);

		new IMove LastMove { get; }
		bool WouldPutMyOwnKingInCheck(IPiece movedPiece, ISquare toSquare);
		KingState OtherKingsExpectedState(IPiece movedPiece, ISquare toSquare, PieceType promotion);
		IBoardState GetCurrentState();

		/// <summary>
		/// Return the board to the given state
		/// </summary>
		/// <param name="state"></param>
		void ApplyState(IBoardState state);

		/// <summary>
		/// Reset the board to its start position
		/// </summary>
		void Reset();
		event Handler<IChessMove>? MoveMade;
		new IGame Game { get; set; }
	}

	#endregion


}
