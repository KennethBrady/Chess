using Chess.Lib.Moves.Parsing;
using Chess.Lib.Pgn.DataModel;
using Chess.Lib.Pgn.Parsing;
using Chess.Lib.Pgn.Service;
using Sql.Lib.Services;

namespace ImportPGN;

public record struct PlayerImport(PgnGameBuilder Builder, PgnPlayer Player);

public class Program
{
	public static async Task Main()
	{
		await Insert("twic920.pgn");
		await Insert("twic921.pgn");
	}

	private static async Task Insert(string filePath)
	{
		// Generate a list of game builders.
		// Parse failures are not accounted for here.
		List<PgnGameBuilder> gameBuilders = PgnSourceParser.ParseFromFile(filePath).
			Where(pr => pr.Succeeded).
			Cast<IPgnParseSuccess>().
			Select(ps => new PgnGameBuilder(ps.Import)).
			ToList();

		#region Verify Move Parsing

		// Verify that moves can be parsed:
		Parallel.For(0, gameBuilders.Count, i =>
		{
			PgnGameBuilder b = gameBuilders[i];
			switch (AlgebraicMoves.Parse(b))
			{
				case IParsedGameSuccess s:
					gameBuilders[i] = b with { Result = s.Result, Status = b.Status | PgnImportStatus.PgnMovesParsed };
					break;
			}
		});

		#endregion

		#region Match Players
		// Match players with known players
		List<PgnPlayer> allPlayers = PgnGameService.Service.LoadAll<PgnPlayer>();
		Dictionary<int, PgnPlayer> dPlayers = allPlayers.Where(p => p.FideId > 0).ToDictionary(p => p.FideId);
		allPlayers.Sort(PgnPlayer.NameComparer);
		List<PgnPlayer> newPlayers = new();
		for (int i = 0; i < gameBuilders.Count; ++i)
		{
			PgnGameBuilder b = gameBuilders[i];
			PgnPlayer wh = b.White, bl = b.Black;
			if (dPlayers.ContainsKey(wh.FideId)) b = b with { Import = b.Import with { White = dPlayers[wh.FideId] } };
			else
			{
				int n = allPlayers.BinarySearch(wh, PgnPlayer.NameComparer);  // Attemp name match, though there are plenty of same names :(
				if (n >= 0) b = b with { Import = b.Import with { White = allPlayers[n] } };
				else
				{
					n = newPlayers.BinarySearch(wh, PgnPlayer.NameComparer);
					if (n < 0) newPlayers.Insert(~n, wh);
				}
			}
			if (dPlayers.ContainsKey(bl.FideId)) b = b with { Import = b.Import with { Black = dPlayers[bl.FideId] } };
			else
			{
				int n = allPlayers.BinarySearch(bl, PgnPlayer.NameComparer);
				if (n >= 0) b = b with { Import = b.Import with { Black = allPlayers[n] } };
				else
				{
					n = newPlayers.BinarySearch(bl, PgnPlayer.NameComparer);
					if (n < 0) newPlayers.Insert(~n, bl);
				}
			}
			if (b.WhiteId > 0 && b.BlackId > 0) b = b with { Status = b.Status | PgnImportStatus.PlayersLocated };
			gameBuilders[i] = b;
		}

		#endregion

		#region Match Openings

		List<Opening> openings = PgnGameService.Service.LoadAll<Opening>();
		// Sort by decreasing move sequence so that longest match is always found:
		openings.Sort((ox, oy) => -ox.Sequence.Length.CompareTo(oy.Sequence.Length));
		Parallel.For(0, gameBuilders.Count, i =>
		{
			PgnGameBuilder b = gameBuilders[(int)i];
			Opening? o = openings.FirstOrDefault(oo => b.Moves.StartsWith(oo.Sequence));
			if (o != null) b = b with { OpeningId = o.Id };
			gameBuilders[(int)i] = b with { Status = b.Status | PgnImportStatus.OpeningsMatched };
		});

		#endregion

		#region Verify Uniqueness

		// If you are pulling games from multiple sources, you may wish to check for duplications.
		// Do this efficiently by comparing games with the same players:
		List<int> foundPlayerIds = gameBuilders.Select(b => b.WhiteId).Concat(gameBuilders.Select(g => g.BlackId)).Distinct().Where(i => i > 0).Order().ToList();
		ISqlClause where = SqlClauses.And(SqlClauses.WhereInList("whiteId", foundPlayerIds), SqlClauses.WhereInList("blackId", foundPlayerIds));
		if (!where.IsEmpty)
		{
			List<PgnGame> withPlayers = PgnGameService.Service.LoadWhere<PgnGame>($"{where.AsSql}");
			for (int i = 0; i < gameBuilders.Count; i++)
			{
				PgnGameBuilder b = gameBuilders[i];
				if (b.WhiteId > 0 && b.BlackId > 0)
				{
					PgnGame? existing = withPlayers.FirstOrDefault(g => g.WhiteId == b.WhiteId && g.BlackId == b.BlackId);
					if (existing == null || !string.Equals(existing.Moves, b.Moves)) b = b with { Status = b.Status | PgnImportStatus.UniquenessVerified };
				}
				else b = b with { Status = b.Status | PgnImportStatus.UniquenessVerified };
				gameBuilders[i] = b;
			}
		}
		else
		{
			for (int i = 0; i < gameBuilders.Count; i++)
				gameBuilders[i] = gameBuilders[i] with { Status = gameBuilders[i].Status | PgnImportStatus.UniquenessVerified };
		}

		#endregion

		#region Insert PgnGames

		// Set Sourcing:
		GameSource source = new GameSource(0, filePath);

		// Now insert:
		using var tx = PgnGameService.Service.CreateTransactedService();
		try
		{
			source = tx.InsertOne(source);
			Dictionary<string, PgnPlayer> insertedPlayers = tx.Insert(newPlayers).ToDictionary(p => p.Name);
			Parallel.For(0, gameBuilders.Count, i =>
			{
				PgnGameBuilder b = gameBuilders[i];
				if (!b.Status.HasFlag(PgnImportStatus.PlayersLocated))
				{
					if (b.WhiteId == 0) b = b with { Import = b.Import with { White = insertedPlayers[b.White.Name] } };
					if (b.BlackId == 0) b = b with { Import = b.Import with { Black = insertedPlayers[b.Black.Name] } };
					gameBuilders[i] = b with { Status = b.Status | PgnImportStatus.PlayersLocated };
				}
			});
			List<PgnGame> toInsert = gameBuilders.Where(b => b.Status == PgnImportStatus.Completed).Select(b => b.ToGame(source.Id)).ToList();
			List<PgnGame> inserted = tx.Insert(toInsert);
			List<GameTag> gameTags = new();
			Dictionary<string, int> tagKeys = PgnGameService.Service.LoadAll<TagKey>().ToDictionary(tk => tk.Name, tk => tk.Id);
			// Assume for simplicity that no new TagKeys are added. This might now always be valid.
			for (int i = 0; i < inserted.Count; i++)
			{
				PgnGame orig = toInsert[i], nu = inserted[i];
				gameTags.AddRange(orig.Tags.Select(t => new GameTag(nu.Id, tagKeys[t.Key], t.Value)));
			}
			tx.Insert(gameTags);
			tx.Commit();
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine($"{inserted.Count:N0} games inserted.");
		}
		catch (Exception ex)
		{
			tx.Rollback();
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine(ex.ToString());
		}

		#endregion
	}
}

internal static class Extension
{
	extension(PgnGameBuilder builder)
	{
		public PgnPlayer White => builder.Import.White;
		public PgnPlayer Black => builder.Import.Black;
		public string Moves => builder.Import.Moves;
		public int WhiteId => builder.Import.White.Id;
		public int BlackId => builder.Import.Black.Id;
	}
}