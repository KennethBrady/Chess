using Chess.Lib.Games;
using Chess.Lib.Hardware;
using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace Chess.Lib.Moves.Parsing
{
	public record struct ParseIntermediate(IChessMove Move, IChessBoard Board, IMoveParseResult Result);

	/// <summary>
	/// Represents an ordered collection of moves (and embedded comments) in algebraic format
	/// </summary>
	/// <param name="Moves"></param>
	/// <param name="FenSetup"></param>
	/// <param name="Comments"></param>
	/// <param name="MoveList"></param>
	public record struct AlgebraicMoves(string Moves, string FenSetup, ImmutableList<MoveComment> Comments, ImmutableList<AlgebraicMove> MoveList):
		IMoveParserEx
	{
		/// <summary>
		/// When set to true, calls to Parse with generate diagnostic output of the chess board for each move.
		/// </summary>
		public static bool ShowDiagnostics { get; set; }

		private enum PgnMovePosition { Number, MoveOne, MoveTwo };  // 3 parts of each Pgn move notation
		static readonly char[] _delims = { ' ', '\t', '\r', '\n', '.' };

		/// <summary>
		/// Attempts to reform an algebraic moves string into a format "N. M1 M2" where N is the move number, and M1, M2 are valid algebraic moves.  
		/// Real PGN imports can be inconsistent with the spacing, and this method attempts to normalize that.
		/// </summary>
		/// <param name="moves"></param>
		/// <returns></returns>
		public static string Normalized(string moves)
		{
			ExtractComments(ref moves);
			if (!moves.StartsWith("1.") && !AlgebraicMove.IsEndGameMove(moves)) return string.Empty;
			int moveNum(string sMov) => int.TryParse(sMov, out int n) ? n : -1;
			StringBuilder s = new StringBuilder();
			string[] tokens = moves.Split(_delims, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
			PgnMovePosition p = PgnMovePosition.Number;
			int lastMoveNumber = 0;
			foreach (string token in tokens)
			{
				if (AlgebraicMove.IsEndGameMove(token))
				{
					s.Append(token);
					break;
				}
				if (token.Length == 0) continue;  //??
				switch (p)
				{
					case PgnMovePosition.Number:
						int num = moveNum(token);
						if (num != (lastMoveNumber + 1)) return string.Empty;
						s.Append($"{num}. ");
						lastMoveNumber = num;
						p = PgnMovePosition.MoveOne;
						break;
					case PgnMovePosition.MoveOne:
						s.Append(token).Append(" ");
						p = PgnMovePosition.MoveTwo;
						break;
					case PgnMovePosition.MoveTwo:
						s.Append(token).Append(" ");
						p = PgnMovePosition.Number;
						break;
				}
			}
			return s.ToString().TrimEnd();
		}

		public static AlgebraicMoves Create(string moves, string fenSetup = "")
		{
			var gen = Generate(moves);
			return new AlgebraicMoves(moves, fenSetup, gen.comments, gen.moves);
		}

		public static AlgebraicMoves Create(IPgnGame game) => Create(game.Moves, game.FEN);

		private static ImmutableList<MoveComment> ExtractComments(ref string moves)
		{
			ImmutableList<MoveComment> r = ImmutableList<MoveComment>.Empty;
			int n = moves.IndexOf('{');
			while (n >= 0)
			{
				int nEnd = moves.IndexOf('}', n);
				string cmt = moves.Substring(n + 1, nEnd - n - 1).Trim();
				r = r.Add(new MoveComment(cmt, n));
				string mstart = moves.Substring(0, n).Trim(), send = (nEnd < moves.Length - 1) ? moves.Substring(nEnd + 1) : string.Empty;
				moves = $"{mstart.Trim()} {send.Trim()}";
				n = moves.IndexOf('{');
			}
			return r;
		}

		private static readonly Regex _rxMoveNumbers = new Regex(@"\d+\.\s*", RegexOptions.Compiled);
		private static (ImmutableList<AlgebraicMove> moves, ImmutableList<MoveComment> comments) Generate(string moves)
		{
			ImmutableList<MoveComment> comments = ExtractComments(ref moves);
			MatchCollection matches = _rxMoveNumbers.Matches(moves);
			if (matches.Count == 0)
			{
				AlgebraicMove m = new AlgebraicMove(moves, 0, 0);
				AlgebraicMove[] mm = { m };
				return (ImmutableList.Create(mm), comments);
			}
			string[] movesArray = _rxMoveNumbers.Split(moves);
			IEnumerable<AlgebraicMove> generate()
			{
				int n = 0;
				for (int i = 1; i <= matches.Count; ++i)
				{
					string move;
					Match m0 = matches[i - 1];
					int index = m0.Index + m0.Length;
					if (i < matches.Count)
					{
						Match m1 = matches[i];
						int len = m1.Index - m0.Index - m0.Length;
						move = moves.Substring(index, len).Trim();
					}
					else
					{
						move = moves.Substring(index);
					}
					string[] parts = move.Split(' ', StringSplitOptions.RemoveEmptyEntries);
					for (int j = 0; j < parts.Length; ++j)
					{
						string m = parts[j];
						if (j == 1 && parts.Length == 3)
						{
							if (!AlgebraicMove.IsEndGameMove(parts[2]))
							{
								m = string.Join(' ', parts.Skip(1));
								j++;
							}
						}
						yield return new AlgebraicMove(m, index, n++);
						if (j == 0) index += m.Length + 1;
					}
				}
				Match mLast = matches[matches.Count - 1];
				string lastMoves = moves.Substring(mLast.Index + mLast.Length);

			}
			return (ImmutableList.Create(generate().ToArray()), comments);
		}

		public AlgebraicMove this[int index] => index >= 0 && index < MoveCount ? MoveList[index] : AlgebraicMove.Empty;

		public int MoveCount => MoveList.Count;
		public bool HasMoves => MoveList.Count > 0;
		public bool IsEmpty => !HasMoves;

		MoveFormat IMoveParser.Format => MoveFormat.Algebraic;

		public static IParsedGame Parse(string moves) => Create(moves).Parse();

		public static IParsedGame Parse(IPgnGame game) => Create(game.Moves, game.FEN).Parse();

		public IParsedGame Parse() => Parse(im => true);

		int IMoveParserEx.ParseFor(IInteractiveChessGame game)
		{
			if (MoveList.Count == 0) return 0;
			IBoard b = (IBoard)game.Board;
			int nParsed = 0;
			foreach (AlgebraicMove m in MoveList)
			{
				switch (m.Parse(b))
				{
					case IMoveParseSuccess s:
						ChessMove move = new ChessMove(s);
						// Although this is an interactive game, this series of moves is non-interactive,
						// so use the synchronous Apply:
						if (b.Apply(move)) nParsed++; else return nParsed;
						break;
					case IParseGameEnd:
					case IMoveParseError: return nParsed;
				}
			}
			return nParsed;
		}

		/// <summary>
		/// Diagnostic method, useful for investigating failing PGN.
		/// </summary>
		/// <param name="parseIntermediate"></param>
		/// <returns>The parsed game (if allowed to complete)</returns>
		public IParsedGame Parse(Func<ParseIntermediate, bool> parseIntermediate)
		{
			IKnownChessGame game = KnownGame.EmptyGameWithSetup(FenSetup);
			IBoard b = (IBoard)game.Board;
			if (MoveList.Count == 0) return new ParseGameFail(ImmutableList<IChessMove>.Empty, new ParseError(AlgebraicMove.Empty, ParseErrorType.NoInput), game);
			if (MoveList.Count == 1 && MoveList[0].IsDrawResult)
				return new ParsedGameSuccess(ImmutableList<IChessMove>.Empty, new GameEnd(MoveList[0], GameResult.Draw), GameResult.Draw, game);
			List<IChessMove> results = new(MoveCount);
			ImmutableList<IChessMove> makeResults() => ImmutableList.Create(results.ToArray());
			for (int i = 0; i < MoveList.Count; i++)
			{
				var moves = MoveList;
				ImmutableList<IParseableMove> remainingMoves()
				{
					return ImmutableList<IParseableMove>.Empty.AddRange(moves.Skip(Math.Max(0, i - 1)).Cast<IParseableMove>());
				}
				AlgebraicMove m = MoveList[i];
				IParseableMove p = m;
				if (ShowDiagnostics)
				{
					Debug.WriteLine(b.Display());
					Debug.WriteLine($"Parsing {p.Hue} move {m.SerialNumber:N0}: {m.Move}:\t");
				}
				var result = m.Parse(b);
				bool quit = false;
				switch (result)
				{
					case IMoveParseSuccess s:
						ChessMove move = new ChessMove(s);
						if (b.Apply(move))  // Should(!) always be true, as parsing checks that move is valid.
						{
							results.Add(move);
							quit = !parseIntermediate(new ParseIntermediate(move, b, result));
						}
						else return new ParseGameFail(makeResults(), new ParseError(m, ParseErrorType.IllegalMove), game, remainingMoves());
						if (ShowDiagnostics)
						{
							Debug.WriteLine(s.AsMove);
							Debug.WriteLine(string.Empty);
						}
						break;
					case IMoveParseError e: return new ParseGameFail(makeResults(), e, game, remainingMoves());
					case IParseGameEnd end: return new ParsedGameSuccess(makeResults(), end, end.Result, game);
				}
				if (quit) return new ParsedGameIncomplete(game, makeResults());
			}
			if (ShowDiagnostics) Debug.WriteLine(b.Display());
			return new ParsedGameSuccess(makeResults(), new GameEnd(MoveList.Last(), GameResult.Unknown), GameResult.Unknown, game);
		}

		/// <summary>
		/// Convert a sequence of AlgebraicMove to a Pgn-formatted string
		/// </summary>
		/// <param name="moves"></param>
		/// <returns></returns>
		public static string ToPgnMoves(IEnumerable<AlgebraicMove> moves)
		{
			StringBuilder s = new();
			foreach (var move in moves)
			{
				AlgebraicMove am = (AlgebraicMove)move;
				IParseableMove pm = (IParseableMove)am;
				if (s.Length > 0) s.Append(" ");
				if (pm.Hue == Hue.Light && !am.IsEndGame) s.Append($"{pm.GameMoveNumber}. ");
				s.Append(move.Move);
			}
			return s.ToString();
		}

		public static IReadOnlyList<AlgebraicMove> ToMoveList(string moves) => Generate(moves).moves;

		/// <summary>
		/// Test whether the string meets basic criteria for PGN moves.
		/// </summary>
		/// <param name="possibleMoves"></param>
		/// <returns></returns>
		internal static bool MayBeAlgebraicFormat(string possibleMoves)
		{
			if (string.IsNullOrEmpty(possibleMoves)) return false;
			possibleMoves = possibleMoves.Trim();
			if (!possibleMoves.StartsWith("1.")) return false;
			return _rxMoveNumbers.IsMatch(possibleMoves);
		}

		IEnumerator IEnumerable.GetEnumerator() => MoveList.GetEnumerator();

		IEnumerator<IParseableMove> IEnumerable<IParseableMove>.GetEnumerator()
		{
			return MoveList.Cast<IParseableMove>().GetEnumerator();
		}
	}

}