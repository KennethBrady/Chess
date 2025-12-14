using Chess.Lib.Moves.Parsing;

namespace Chess.Lib.Games
{
	/// <summary>
	/// A simplified game interface that defines core properties of a PGN game
	/// </summary>
	public interface IPgnGame
	{
		string Moves { get; }
		string FEN { get; }
		string WhiteName { get; }
		string BlackName { get; }
		IReadOnlyDictionary<string, string> Tags { get; }
	}

	internal class KnownPgnGame : KnownGame, IPgnChessGame
	{
		private KnownPgnGame(IPgnGame game, IParsedGame result): base(result, game.WhiteName, game.BlackName)
		{
			Source = game;
		}

		internal KnownPgnGame(IPgnGame pgnGame): this(pgnGame, Parsers.Parse(pgnGame)) { }

		public IPgnGame Source { get; private init; }
	}
}
