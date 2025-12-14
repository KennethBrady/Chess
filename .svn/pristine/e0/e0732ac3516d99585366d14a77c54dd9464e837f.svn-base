using Chess.Lib.Games;
using System.Collections.Immutable;
using System.Text;

namespace Chess.Lib.UnitTests
{
	internal record QuickPgnGame(int Id, ImmutableDictionary<string, string> Tags, string Moves) : IPgnGame
	{
		internal static readonly QuickPgnGame Empty = new QuickPgnGame(0, ImmutableDictionary<string, string>.Empty, string.Empty);
		public bool IsEmpty => Id == 0;

		string IPgnGame.WhiteName => Tags.First(t => t.Key == "White").Value;
		string IPgnGame.BlackName => Tags.First(t => t.Key == "Black").Value;
		string IPgnGame.FEN => Tags.ContainsKey("FEN") ? Tags["FEN"] : string.Empty;
		IReadOnlyDictionary<string,string> IPgnGame.Tags => Tags;		
	}

	internal static class GameDB
	{
		private static Dictionary<int, QuickPgnGame> _games = new();

		public static IReadOnlyDictionary<int, QuickPgnGame> Games => _games.AsReadOnly();

		static GameDB()
		{
			Dictionary<string, string> tags = new();
			StringBuilder moves = new();
			string[] lines = File.ReadAllLines("games.txt");
			for(int i=0;i<lines.Length;i++)
			{
				string l = lines[i];
				if (string.IsNullOrWhiteSpace(l)) continue;
				if (int.TryParse(l, out int id))
				{
					tags.Clear();
					moves.Clear();
					for(int j=i+1;j<lines.Length;j++)
					{
						l = lines[j];
						if (string.IsNullOrWhiteSpace(l))
						{
							if (moves.Length == 0) continue;
							i = j;
							_games.Add(id, new QuickPgnGame(id, ImmutableDictionary<string, string>.Empty.AddRange(tags), moves.ToString()));
							break;							
						}
						if (l.StartsWith('['))
						{
							string noQuo(string s)
							{
								return (s.StartsWith('"') && s.EndsWith('"')) ? s.Substring(1, s.Length - 2) : s;
							}
							string noBraces = l.Substring(1, l.Length - 2);
							int n = noBraces.IndexOf(' ');
							string key = noQuo(noBraces.Substring(0, n)), value = noQuo(noBraces.Substring(n + 1));
							if (value.StartsWith('"') && value.EndsWith('"')) value = value.Substring(1, value.Length - 2);
							tags.Add(key, value);
						}
						else
						{
							if (moves.Length > 0) moves.Append(' ');
							moves.Append(l);
						}
					}
				}
			}
		}

		public static QuickPgnGame Get(int id) => _games.ContainsKey(id) ? _games[id] : QuickPgnGame.Empty;

		public static IEnumerable<QuickPgnGame> All => _games.Values;
	}
}
