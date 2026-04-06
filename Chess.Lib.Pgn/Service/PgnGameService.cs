using Chess.Lib.Pgn.DataModel;
using Chess.Lib.Pgn.Parsing;
using Chess.Lib.Pgn.Service.Access;
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

		#region Game Methods

		public const int MaxGameBatchCount = 10000;

		internal static long TotalGameCount()
		{
			var r = _service.ExecuteScalar($"select count(*) from {PgnGame.TableName}");
			if (r is long l) return l;
			if (r is int i) return i;
			return Convert.ToInt64(r);
		}

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

		internal static List<PgnGame> LoadGames(int minId, int count = MaxGameBatchCount)
		{
			count = Math.Min(10000, count);   // prevent loading 3+ GB
			return _service.LoadWhere<PgnGame>($"id>={minId} limit {count}");
		}

		internal static List<PgnGame> InsertGames(string sourceName, IEnumerable<PgnGameBuilder> builders, Action<GameInsertion	>? feedback = null)
		{
			using var tx = _service.CreateTransactedService();
			try
			{
				GameSource source = tx.InsertOne(new GameSource(0, sourceName));
				Dictionary<string, TagKey> allTagKeys = tx.LoadAll<TagKey>().ToDictionary(t => t.Name);
				Dictionary<string, PgnPlayer> dAddedPlayers = new();
				PgnPlayer addPlayer(PgnPlayer player)
				{
					string key = $"{player.Name}-{player.FideId}";
					if (dAddedPlayers.ContainsKey(key)) return dAddedPlayers[key];
					PgnPlayer r = tx.InsertOne(player);
					dAddedPlayers.Add(key, r);
					return r;
				}
				List<PgnGame> r = new();
				int n = 0;
				foreach (PgnGameBuilder b in builders)
				{
					PgnGameBuilder bNew = b;
					GameImport imp = b.Import;
					if (imp.White.Id == 0) imp = imp with { White = addPlayer(imp.White) };
					if (imp.Black.Id == 0) imp = imp with { Black = addPlayer(imp.Black) };
					if (!imp.Equals(b.Import)) bNew = bNew with { Import = imp };
					PgnGame game = bNew.ToGame(source.Id);
					game = tx.InsertOne(game);
					List<GameTag> gameTags = new List<GameTag>(b.Import.Tags.Count);
					foreach (var kvp in bNew.Import.Tags)
					{
						if (!allTagKeys.ContainsKey(kvp.Key))
						{
							TagKey tk = tx.InsertOne(new TagKey(0, kvp.Value));
							allTagKeys.Add(tk.Name, tk);
						}
						gameTags.Add(new GameTag(game.Id, allTagKeys[kvp.Key].Id, kvp.Value));
					}
					if (gameTags.Count > 0) tx.Insert(gameTags);
					r.Add(game);
					feedback?.Invoke(new GameInsertion(b, game, n++));
				}
				tx.Commit();
				return r;
			}
			catch
			{
				tx.Rollback();
				throw;
			}
		}

		#endregion

		#region Player Methods
		internal static PlayerPair LoadPlayersFor(int whiteId, int blackId)
		{
			var where = SqlClauses.WhereInList("id", whiteId, blackId);
			if (where.IsEmpty) return PlayerPair.Empty;
			return PlayerPair.Create(whiteId, _service.LoadWhere<PgnPlayer>(where.AsSql));
		}

		internal static IEnumerable<PgnGame> GamesBetween(int playerId1, int playerId2)
		{
			string where = $"(whiteId={playerId1} and blackId={playerId2}) or (whiteId={playerId2} and blackId={playerId1}";
			return Service.LoadWhere<PgnGame>(where);
		}

		#endregion
	}
}
 