using Chess.Lib.Moves.Parsing;
using Chess.Lib.Pgn.DataModel;
using Common.Lib.Extensions;
using System.Collections.Immutable;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Chess.Lib.Pgn.Parsing
{
	public static class PgnSourceParser
	{
		private static readonly string[] _requiredHeaders = { "Event", "Site", "Date", "Round", "White", "Black", "Result" };
		private static readonly string[] _playerTags = { "WhiteElo", "BlackElo", "WhiteTitle", "BlackTitle", "WhiteFideId", "BlackFideId", "WhiteTeam", "BlackTeam", "WhiteTeamCountry", "BlackTeamCountry" };
		private static readonly string[] _endCombinations = { "1/2-1/2", "1-0", "0-1", "+/- +/-", "-/+ -/+", "*", "* *", "(+)-(-) (+)-(-)", "0-0 0-0" };

		public static IEnumerable<string> RequiredHeaders => _requiredHeaders;
		public static IEnumerable<string> PlayerTags => _playerTags;

		public static bool TryParseDate(string sDate, out DateTime date) => TryParseDate(ref sDate, out date);

		private static readonly string[] _thirtyDayMonths = { "04", "06", "09", "11" };
		private static bool TryParseDate(ref string sDate, out DateTime date)
		{
			date = DateTime.MinValue;
			if (string.IsNullOrEmpty(sDate)) return false;
			while (sDate.EndsWith("'")) sDate = sDate.Substring(0, sDate.Length - 1);
			string[] parts = sDate.Split('.');
			if (parts.Length == 3)
			{
				for (int i = 0; i < parts.Length; i++)
				{
					if (parts[i].Length == 1) parts[i] = "0" + parts[i];
					if (parts[i] == "??")
					{
						if (i == 0) return false;
						if (i == 1) sDate = sDate.Replace("??", "01");
						parts[i] = "01";
					}
					if (_thirtyDayMonths.Contains(parts[1]))
						// Detect 30-day months with '31' for the day.
						if (_thirtyDayMonths.Contains(parts[1]) && parts[2] == "31") parts[2] = "30";
					// and for February:
					if (parts[1] == "02" && (parts[2] == "31" || parts[2] == "30" || parts[2] == "29")) parts[2] = "28";
				}
				sDate = string.Join('.', parts);
				try
				{
					date = DateTime.ParseExact(sDate, "yyyy.MM.dd", CultureInfo.InvariantCulture);
					return true;
				}
				catch
				{
					return DateTime.TryParseExact(sDate, "yyyy.dd.MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
				}
			}
			return false;
		}

		public static DateTime ParseDate(string sPgnDate) => DateTime.ParseExact(sPgnDate, "yyyy.MM.dd", CultureInfo.InvariantCulture);

		private static readonly Regex _rxHeaders = new Regex(@"\[(\w+)\s""(.*)""]");

		public static IPgnParseResult Parse(string pgn) => Parse(pgn, SourceInfo.Empty);

		private static string EnsureNoSpaceAfterCommas(string playerName)
		{
			if (string.IsNullOrEmpty(playerName)) return playerName;
			string[] parts = playerName.Split(',');
			return string.Join(",", parts.Select(p => p.Trim()).Where(p => !string.IsNullOrEmpty(p)));
		}

		private static IPgnParseResult Parse(string pgn, SourceInfo sourceInfo)
		{
			if (string.IsNullOrEmpty(pgn)) return new PgnParseError(pgn, PgnParseErrorType.EmptyPgn);
			Dictionary<string, string> r = new();
			MatchCollection matches = _rxHeaders.Matches(pgn);
			if (matches.Count == 0) return new PgnParseError(pgn, PgnParseErrorType.MissingRequiredHeaders);
			Match last = matches[matches.Count - 1];
			string moves = pgn.Substring(last.Index + last.Length).Trim().Replace("\r\n", " ");
			if (moves.Contains("\n")) moves = moves.Replace("\n", " ");
			DateTime? evtDate = null;
			string? whiteName = null, blackName = null;
			int wFide = 0, bFide = 0, notUsed = 0;
			foreach (Match match in matches)
			{
				if (match.Groups.Count > 2)
				{
					string key = match.Groups[1].Value.Trim(), val = match.Groups[2].Value.Trim();
					if (string.IsNullOrEmpty(val) && key != "Event") continue;
					switch (key)
					{
						case "Date": if (TryParseDate(ref val, out DateTime dt)) evtDate = dt; break;
						case "White": whiteName = val = EnsureNoSpaceAfterCommas(val); break;
						case "Black": blackName = val = EnsureNoSpaceAfterCommas(val); break;
						case "WhiteElo":
						case "BlackElo": if (!int.TryParse(val, out int _)) continue; break;
						case "WhiteFideId": if (int.TryParse(val, out notUsed)) wFide = notUsed; break;
						case "BlackFideId": if (int.TryParse(val, out notUsed)) bFide = notUsed; break;
					}
					r.Add(key, val);
				}
			}
			if (string.IsNullOrEmpty(whiteName) || string.IsNullOrEmpty(blackName)) return new PgnParseError(pgn, PgnParseErrorType.MissingPlayer);
			string missing = string.Join(',', _requiredHeaders.Where(h => !r.ContainsKey(h)));
			if (missing.Length > 0) return new PgnParseError(pgn, PgnParseErrorType.MissingRequiredHeaders, missing);
			if (!evtDate.HasValue || whiteName == null || blackName == null) return new PgnParseError(pgn, PgnParseErrorType.MissingRequiredHeaders);
			GameResult result = GameResult.Unknown;
			foreach(string ec in _endCombinations)
			{
				if (moves.EndsWith(ec))
				{
					switch(ec)
					{
						case "+/-":
						case "1-0": result = GameResult.WhiteWin; break;
						case "-/+":
						case "0-1": result = GameResult.BlackWin; break;
						case "*":
						case "* *": break; // unknown
						default: result = GameResult.Draw; break;
					}
				}
			}
			bool hasUnexpected = false;
			if (!moves.StartsWith("1."))
			{
				if (_endCombinations.Contains(moves)) goto finish;
				if (moves.StartsWith('{'))
				{
					if (moves.AreBalanced('{', '}', out _)) goto finish;
					hasUnexpected = true;
				}
			}
			finish:
			GameImport stub = new GameImport(sourceInfo, pgn, evtDate.Value, new PgnPlayer(0, whiteName, wFide), new PgnPlayer(0, blackName, bFide), 
				moves, result, hasUnexpected, ImmutableDictionary.Create<string,string>().AddRange(r));
			return new PgnParseSuccess(stub);
		}

		private const string EVENT = "[Event ";
		public static IEnumerable<IPgnParseResult> ParseMultiple(string content, string sourceName, long length, Action<PgnParseProgress>? feedback = null)
		{
			if (string.IsNullOrEmpty(content))
			{
				yield return new PgnParseError(content, PgnParseErrorType.EmptyPgn);
				yield break;
			} else
			{
				int ndx1 = content.IndexOf(EVENT), nParse = 0, nSuccess = 0;
				while (ndx1 >= 0)
				{
					int ndx2 = content.IndexOf(EVENT, ndx1 + EVENT.Length);
					string pgn = (ndx2 > ndx1) ? content.Substring(ndx1, ndx2 - ndx1) : content.Substring(ndx1);
					SourceInfo sinfo = new SourceInfo(sourceName, ndx1, nParse + 1);	// SourceIndex is 1-based
					var retval = Parse(pgn, sinfo);
					if (retval is IPgnParseSuccess) nSuccess++; else System.Diagnostics.Debugger.Break();
					yield return retval;
					ndx1 = ndx2;
					if (nParse++ % 50 == 0 && feedback != null)
					{
						double pcnt = 100.0 * ndx2 / length;
						feedback(new PgnParseProgress(nParse, nSuccess, pcnt));
					}
				}
				if (feedback != null) feedback(new PgnParseProgress(nParse, nSuccess, 100.0));
			}
		}

		public static IEnumerable<IPgnParseResult> ParseFromFile(string filePath, Action<PgnParseProgress>? feedback = null)
		{
			FileInfo f = new FileInfo(filePath);
			if (!f.Exists)
			{
				yield return new PgnParseError(string.Empty, PgnParseErrorType.MissingSourceFile, filePath);
				yield break;
			} else
			{
				foreach(var res in ParseMultiple(File.ReadAllText(filePath), f.Name, f.Length, feedback)) yield return res;
			}
		}

		public static Task<IEnumerable<IPgnParseResult>> ParseMultipleAsync(string content, string sourceName, Action<PgnParseProgress>? feedback = null)
		{
			IEnumerable<IPgnParseResult> parse() => ParseMultiple(content, sourceName, content.Length, feedback);
			return Task<IEnumerable<IPgnParseResult>>.Factory.StartNew(parse);
		}

		public static Task<IEnumerable<IPgnParseResult>> ParseFromFileAsync(string filePath, Action<PgnParseProgress>? feedback = null)
		{
			IEnumerable<IPgnParseResult> parse() => ParseFromFile(filePath, feedback);
			return Task<IEnumerable<IPgnParseResult>>.Factory.StartNew(parse);
		}

		internal static string ExportPgn(IReadOnlyDictionary<string,string> tags, string moves)
		{
			StringBuilder s = new StringBuilder();
			foreach(string tag in _requiredHeaders)
			{
				string val = tags[tag];
				s.Append($"[{tag} \"{val}\"]\n");
			}
			foreach(var nvp in tags)
			{
				if (_requiredHeaders.Contains(nvp.Key)) continue;
				s.Append($"[{nvp.Key} \"{nvp.Value}\"]\n");
			}
			s.Append('\n');
			s.Append(AlgebraicMoves.ToPgnFormat(moves));
			return s.ToString();
		}
	}
}
