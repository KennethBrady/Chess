using Chess.Lib.Games;
using Chess.Lib.Hardware;
using Chess.Lib.Hardware.Pieces;
using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics;

// http://www.saremba.de/chessgml/standards/pgn/pgn-complete.htm#c8.2

namespace Chess.Lib.Moves.Parsing
{
	#region Parsing Interfaces

	public interface IParseableMove
	{
		string Move { get; }
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
		IParseResult Parse(IBoard board);
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

	#endregion

	internal struct NotParseable : IParseableMove, IParseableMoveEx
	{
		internal static NotParseable Default => new NotParseable();

		public string Move => string.Empty;
		public int SourceIndex => -1;
		public int SerialNumber => -1;
		public MoveFormat Format => MoveFormat.Unknown;
		public Hue Hue => Hue.Default;
		public bool IsPromotion => false;
		public PieceType Promotion => PieceType.None;

		public IParseResult Parse(IBoard b) => new ParseError(this, ParseErrorType.NoInput);
	}

	internal struct NonParser : INonParser
	{
		internal static readonly NonParser Default = new NonParser();

		private static readonly ImmutableList<IParseableMove> _moves = ImmutableList<IParseableMove>.Empty;
		public string Moves => string.Empty;
		public string FenSetup => string.Empty;
		public MoveFormat Format => MoveFormat.Unknown;
		public IParsedGame Parse() => new ParseGameFail(ImmutableList<IChessMove>.Empty,
			new ParseError(NotParseable.Default, ParseErrorType.NoInput), KnownGame.Empty, ImmutableList<IParseableMove>.Empty);


		IEnumerator<IParseableMove> IEnumerable<IParseableMove>.GetEnumerator() => _moves.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => _moves.GetEnumerator();
	}

	public static class Parsers
	{
		public static IMoveParser Create(string moves, MoveFormat format)
		{
			switch (format)
			{
				case MoveFormat.Algebraic: return AlgebraicMoves.Create(moves);
				case MoveFormat.Engine:
				case MoveFormat.EngineCompact: return EngineMoves.Create(moves);
				default: return NonParser;
			}
		}

		public static IParseableMove NonMove => NotParseable.Default;
		public static INonParser NonParser => Parsing.NonParser.Default;

		public static IMoveParser Create(string moves) => Create(moves, DetectFormat(moves));

		public static IParsedGame TryParseGame(string moves) =>
			TryParseGame(moves, DetectFormat(moves));

		public static IParsedGame TryParseGame(string moves, MoveFormat format)
		{
			switch (Create(moves, format))
			{
				case NonParser np: return new ParseGameFail(ChessMove.EmptyMoves, new ParseError(NotParseable.Default, ParseErrorType.UnknownFormat), KnownGame.Empty,
					ImmutableList<IParseableMove>.Empty);
				case IMoveParser parser: return parser.Parse();
				default: throw new UnreachableException();
			}
		}

		internal static IParseResult TryParse(string move, IBoard board) => TryParse(move, DetectFormat(move), board);
		internal static IParseResult TryParse(string move, MoveFormat format, IBoard board)
		{
			if (string.IsNullOrEmpty(move)) return new ParseError(string.Empty, ParseErrorType.InvalidInput);
			switch (format)
			{
				case MoveFormat.Algebraic:
					if (string.IsNullOrEmpty(move)) return new ParseError(AlgebraicMove.Empty, ParseErrorType.NoInput);
					AlgebraicMove am = new AlgebraicMove(move, 0, board.LastMove.SerialNumber + 1);
					return am.Parse(board);
				case MoveFormat.Engine:
				case MoveFormat.EngineCompact:
				default:
					if (string.IsNullOrEmpty(move)) return new ParseError(EngineMove.Empty, ParseErrorType.NoInput);
					EngineMove em = new EngineMove(move.Replace(" ", string.Empty), 0, board.LastMove.SerialNumber + 1);
					return em.Parse(board);
			}
		}

		internal static IGame ParseMoves(string moves)
		{
			IMoveParser parser;
			switch (DetectFormat(moves))
			{
				case MoveFormat.Algebraic: parser = AlgebraicMoves.Create(moves); break;
				default: parser = EngineMoves.Create(moves); break;
			}
			IParsedGame g = parser.Parse();
			return (IGame)g.Game;
		}

		internal static IParseResult TryParse(IParseableMove move, IBoard board)
		{
			IParseableMoveEx parser = NotParseable.Default;
			switch(move)
			{
				case AlgebraicMove am: parser = am; break;
					case EngineMove em: parser = em; break;
			}
			if (parser is NotParseable) return new ParseError(parser, ParseErrorType.InvalidInput);
			return parser.Parse(board);
		}

		public static MoveFormat DetectFormat(string moves)
		{
			if (string.IsNullOrEmpty(moves)) return MoveFormat.Unknown;
			if (EngineMoves.MayBeEngineFormat(moves)) return MoveFormat.Engine;
			if (AlgebraicMoves.MayBeAlgebraicFormat(moves)) return MoveFormat.Algebraic;
			return MoveFormat.Unknown;
		}

		internal static IParsedGame Parse(IPgnGame game)
		{
			AlgebraicMoves moves = AlgebraicMoves.Create(game.Moves, game.FEN);
			return moves.Parse();
		}
	}
}
