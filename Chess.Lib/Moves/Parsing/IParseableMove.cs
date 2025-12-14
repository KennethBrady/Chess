using Chess.Lib.Games;
using Chess.Lib.Hardware;
using Chess.Lib.Hardware.Pieces;

namespace Chess.Lib.Moves.Parsing
{
	/// <summary>
	/// Represents a move that might be parsed into a board transformation
	/// </summary>
	public interface IParseableMove
	{
		string Move { get; }
		/// <summary>
		/// The position of the move string within the moves string
		/// </summary>
		int SourceIndex { get; }
		int SerialNumber { get; }
		MoveFormat Format { get; }
		Hue Hue => SerialNumber % 2 == 0 ? Hue.Light : Hue.Dark;
		int GameMoveNumber => MoveCounter.SerialToGameNumber(SerialNumber);
		bool IsPromotion { get; }
		PieceType Promotion { get; }
		bool IsEmpty => string.IsNullOrEmpty(Move);
	}

	public interface IAlgebraicParseable : IParseableMove
	{
		bool IsCapture { get; }
		bool IsMate { get; }
		bool IsKingsideCastle { get; }
		bool IsQueensideCastle { get; }
		bool IsEndGame { get; }
	}

	internal interface IParseableMoveEx : IParseableMove
	{
		IMoveParseResult Parse(IBoard board);
	}

	public interface IMoveParser : IEnumerable<IParseableMove>
	{
		string Moves { get; }
		string FenSetup { get; }
		MoveFormat Format { get; }
		IParsedGame Parse();
	}

	public interface INonParser : IMoveParser;

	internal interface IMoveParserEx : IMoveParser
	{
		int ParseFor(IInteractiveChessGame game);
	}
}
