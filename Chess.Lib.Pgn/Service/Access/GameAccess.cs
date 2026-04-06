using Chess.Lib.Pgn.DataModel;
using Chess.Lib.Pgn.Parsing;
using Sql.Lib.Services;

namespace Chess.Lib.Pgn.Service.Access
{
	public readonly record struct GameInsertion(PgnGameBuilder SourceBuilder, PgnGame Game, int Index);

	public interface IGameAccess : IDbAccess<PgnGame>
	{
		IDynamicReadOnlyList<PgnGame> Games { get; }

		Task<List<PgnGame>> GamesWithPlayers(IEnumerable<int> whitePlayerIds, IEnumerable<int> blackPlayerIds);

		Task<List<PgnGame>> Insert(string sourceName, IEnumerable<PgnGameBuilder> builders, Action<GameInsertion>? feedback = null);
	}

	internal class GameAccess : DbAccess<PgnGame>, IGameAccess
	{
		private GameIterator Games { get; init; }
		internal GameAccess(): this(new GameIterator()) { }

		private GameAccess(GameIterator games): base(games.Games)
		{
			Games = games;
		}

		public override bool AreValuesDynamic => true;

		protected override PgnGame CreateNew(int id) => PgnGame.Empty with { Id = id };

		IDynamicReadOnlyList<PgnGame> IGameAccess.Games => Games;

		Task<List<PgnGame>> IGameAccess.GamesWithPlayers(IEnumerable<int> whitePlayerIds, IEnumerable<int> blackPlayerIds)
		{
			ISqlClause where = SqlClauses.And(SqlClauses.WhereInList("whiteId", whitePlayerIds), SqlClauses.WhereInList("blackId", blackPlayerIds));
			List<PgnGame> load() => PgnGameService.Service.LoadWhere<PgnGame>(where.AsSql);
			return Task<List<PgnGame>>.Factory.StartNew(load);
		}

		Task<List<PgnGame>> IGameAccess.Insert(string sourceName, IEnumerable<PgnGameBuilder> builders, Action<GameInsertion>? feedback)
		{
			List<PgnGame> insert() => PgnGameService.InsertGames(sourceName, builders, feedback);
			return Task<List<PgnGame>>.Factory.StartNew(insert);
		}
	}
}
