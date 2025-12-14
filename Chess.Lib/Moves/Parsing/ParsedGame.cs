using Chess.Lib.Games;
using Chess.Lib.Hardware;
using System.Collections.Immutable;

namespace Chess.Lib.Moves.Parsing
{
	public interface IParsedGame
	{
		IKnownChessGame Game { get; }
		IChessBoard FinalBoard => Game.Board;
		ImmutableList<IChessMove> Moves { get; }
	}

	public interface IParsedGameSuccess : IParsedGame
	{
		IParseGameEnd GameEnd { get; }
		GameResult Result { get; }
	}

	public interface IParsedGameFail : IParsedGame
	{
		IMoveParseError Error { get; }
		ImmutableList<IParseableMove> UnparsedMoves { get; }
	}

	internal record struct ParsedGameSuccess(ImmutableList<IChessMove> Moves, IParseGameEnd GameEnd,
			GameResult Result, IKnownChessGame Game) : IParsedGameSuccess;

	internal record struct ParsedGameIncomplete(IKnownChessGame Game, ImmutableList<IChessMove> Moves) : IParsedGame;

	internal record struct ParseGameFail(ImmutableList<IChessMove> Moves, IMoveParseError Error, IKnownChessGame Game,
		ImmutableList<IParseableMove> UnparsedMoves) : IParsedGameFail
	{
		internal ParseGameFail(ImmutableList<IChessMove> moves, IMoveParseError error, IKnownChessGame game) :
			this(moves, error, game, ImmutableList<IParseableMove>.Empty)
		{ }
	}
}
