using Chess.Lib.Games;
using Chess.Lib.Hardware;
using Chess.Lib.Hardware.Pieces;
using Chess.Lib.Moves.Parsing;

namespace Chess.Lib.Moves
{
	// Move Interfaces

	/// <summary>
	/// Core move properties are available through the IParseResult.  
	/// Full IChessMove properties are not available until the IParseResult is applied to the Board.
	/// </summary>
	public interface IChessMoveCore
	{
		IChessPiece MovedPiece { get; }
		IChessSquare FromSquare { get; }
		IChessSquare ToSquare { get; }
		IChessPiece CapturedPiece { get; }
		bool IsCapture => CapturedPiece is Piece;
		IChessMove PreviousMove { get; }
		Hue Side => MovedPiece.Side;

	}

	public interface IChessMove : IChessMoveCore
	{
		IChessPlayer Player { get; }
		int SerialNumber { get; }
		IMoveCounter Number { get; }
		bool IsCheck { get; }
		IPromotion Promotion { get; }
		bool IsCheckMate { get; }
		bool IsEnPassant { get; }
		IEnumerable<IChessSquare> AffectedSquares();
		ICastle Castle { get; }
		bool IsKingsideCastle => Castle.Type == CastleMoveType.Kingside;
		bool IsQueensideCastle => Castle.Type == CastleMoveType.Queenside;
		bool IsCastle => IsKingsideCastle || IsQueensideCastle;
		int RankChange => ToSquare.Rank - FromSquare.Rank;
		int FileChange => ToSquare.File - FromSquare.File;
		string AsEngineMove
		{
			get
			{
				string p = Promotion.IsValid ? Promotion.ToPiece.Type.PGNChar().ToString() : string.Empty;
				return $"{FromSquare.Name}{ToSquare.Name}{p}";
			}
		}
		IChessKing CheckedKing { get; }
		IParseableMove SourceMove { get; }
		string AlgebraicMove { get; }
	}

	public interface INoMove : IChessMove;

	internal interface IMove : IChessMove
	{
		bool IsPromotion { get; }
		new IPromotion Promotion { get; set; }
		PieceType PromoteTo { get; }
		IBoard Board => (IBoard)MovedPiece.Board;
		IGameState GameState { get; set; }
		IBoardState BoardState => GameState.BoardState;

		new ICastle Castle { get; set; }

		void SetPromotion(PieceType promoteTo);
	}
}
