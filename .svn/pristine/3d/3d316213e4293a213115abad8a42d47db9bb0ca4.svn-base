using Chess.Lib.Pgn.DataModel;
using Sql.Lib.Services;

namespace Chess.Lib.Pgn.Service
{
	internal record struct PlayerPair(PgnPlayer White, PgnPlayer Black)
	{
		internal static readonly PlayerPair Empty = new PlayerPair(PgnPlayer.NoPlayer, PgnPlayer.NoPlayer);
		internal static PlayerPair Create(int whiteId, List<PgnPlayer> players)
		{
			PgnPlayer? w = players[0].Id == whiteId ? players[0] : players[1],
				b = players[0].Id == whiteId ? players[1] : players[0];
			return new PlayerPair(w, b);
		}
	}

	public static partial class PgnGameService
	{
		public const string ConnectionString = "Server=localhost;Database=chessgames;Uid=chesser;Pwd=mate;UseCompression=false;CharSet=utf8mb4";
		private static MySqlService _service = new MySqlService(ConnectionString);
		public static ISqlService Service => _service;

		internal static Dictionary<string, string> LoadGameTags(int gameId)
		{
			string qry = $"SELECT tk.name, gt.value FROM tagkey tk JOIN gametag gt ON tk.id=gt.tagid WHERE gt.gameId={gameId} ORDER BY 1 ASC";
			Dictionary<string, string> r = new();
			_service.ExecuteCustomReader(qry, rdr =>
			{
				r.Add(rdr.GetString(0), rdr.GetString(1));
			});
			return r;
		}

		internal static PlayerPair LoadPlayersFor(int whiteId, int blackId)
		{
			var where = SqlClauses.WhereInList("id", whiteId, blackId);
			if (where.IsEmpty) return PlayerPair.Empty;
			return PlayerPair.Create(whiteId, _service.LoadWhere<PgnPlayer>(where.AsSql));
		}
	}
}
 