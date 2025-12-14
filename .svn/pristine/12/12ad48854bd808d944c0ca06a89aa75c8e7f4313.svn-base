using Sql.Lib.Services;

namespace Chess.Lib.Pgn.DataModel
{
	[DBTable(TableName)]
	public record GameSource(int Id, string Name)
	{
		public static readonly GameSource Empty = new GameSource(0, string.Empty);

		public const string TableName = "gamesource";
	}
}
