using Chess.Lib.Games;
using Chess.Lib.Hardware;
using System.Collections;
using System.Collections.Immutable;

namespace Chess.Lib.Moves.Parsing
{
	internal record struct EngineMoves(string Moves, string FenSetup, ImmutableList<EngineMove> MoveList): IMoveParserEx
	{
		public static readonly EngineMoves Empty = new EngineMoves(string.Empty, string.Empty, ImmutableList<EngineMove>.Empty);
		public static EngineMoves Create(string moves, string fenSetup = "")
		{
			if (string.IsNullOrEmpty(moves)) return Empty with { FenSetup = fenSetup };
			return new EngineMoves(moves, fenSetup, ImmutableList<EngineMove>.Empty.AddRange(ParseMoves(moves)));
		}

		private const string FileChars = "abcdefgh";
		internal static bool MayBeEngineFormat(string possibleMoves)
		{
			if (string.IsNullOrEmpty(possibleMoves)) return false;
			possibleMoves = possibleMoves.Replace(" ", string.Empty).ToLower();
			if (possibleMoves.Any(c => !char.IsDigit(c) && !FileChars.Contains(c))) return false;
			return true;
		}

		private static readonly char[] _promos = { 'N', 'n', 'R', 'r', 'Q', 'q' };
		internal static IEnumerable<EngineMove> ParseMoves(string moves)
		{
			int nMove = 0;
			for (int i = 3; i < moves.Length; i += 4)
			{
				int start = i - 3;
				bool isPromo()
				{
					int next = i + 1;
					if (next >= moves.Length) return false;
					char c = moves[next];
					if (_promos.Contains(c)) return true;
					switch (moves[next])
					{
						case 'b':
						case 'B':
							if (next + 1 >= moves.Length) return true;
							return !char.IsDigit(moves[next + 1]);
					}
					return false;
				}
				if (isPromo()) i++;
				System.Diagnostics.Debug.WriteLine(moves.AsSpan(start, i - start + 1).ToString());
				yield return new EngineMove(moves.AsSpan(start, i - start + 1), start, nMove++);
			}
		}

		public int MoveCount => MoveList.Count;
		public bool HasMoves => MoveList.Count > 0;
		public bool IsEmpty => !HasMoves;

		MoveFormat IMoveParser.Format => MoveFormat.Engine;

		int IMoveParserEx.ParseFor(IInteractiveChessGame game)
		{
			if (MoveList.Count == 0) return 0;
			IBoard b = (IBoard)game.Board;
			int n = 0;
			foreach (EngineMove m in MoveList)
			{
				switch (m.Parse(b))
				{
					case IMoveParseSuccess s:
						ChessMove move = new ChessMove(s);
						if (b.Apply(move)) n++; else return n;
						break;
					case IMoveParseError:
					case IParseGameEnd: return n;
				}
			}
			return n;
		}

		public IParsedGame Parse()
		{
			IKnownChessGame game = new KnownGame();
			IBoard b = (IBoard)game.Board;
			if (MoveList.Count == 0) return new ParseGameFail(ImmutableList<IChessMove>.Empty, new ParseError(EngineMove.Empty, ParseErrorType.NoInput), game);
			List<IChessMove> results = new(MoveCount);
			ImmutableList<IChessMove> makeResults() => ImmutableList.Create(results.ToArray());
			EngineMove last = MoveList.Last();
			for (int i = 0; i < MoveList.Count; i++)
			{
				var moves = MoveList;
				ImmutableList<IParseableMove> remainingMoves()
				{
					return ImmutableList<IParseableMove>.Empty.AddRange(moves.Skip(Math.Max(0, i - 1)).Cast<IParseableMove>());
				}
				EngineMove m = MoveList[i];
				IParseableMove p = m;
				if (AlgebraicMoves.ShowDiagnostics)
				{
					System.Diagnostics.Debug.WriteLine(game.Board.Display());
					System.Diagnostics.Debug.Write($"Parsing {p.Hue} move {m.SerialNumber:N0}: {m.Move}:\t");
				}
				switch (m.Parse(b))
				{
					case IMoveParseSuccess s:
						ChessMove move = new ChessMove(s);
						if (b.Apply(move)) results.Add(move);
						else return new ParseGameFail(makeResults(), new ParseError(m, ParseErrorType.IllegalMove), game,
							remainingMoves());
						if (AlgebraicMoves.ShowDiagnostics)
						{
							System.Diagnostics.Debug.WriteLine(s.AsMove);
							System.Diagnostics.Debug.WriteLine(string.Empty);
						}
						break;
					case IMoveParseError e: return new ParseGameFail(makeResults(), e, game, remainingMoves());

				}
			}
			return new ParsedGameSuccess(makeResults(), new GameEnd(last, GameResult.Unknown), GameResult.Unknown, game);
		}

		IEnumerator<IParseableMove> IEnumerable<IParseableMove>.GetEnumerator() => MoveList.Cast<IParseableMove>().GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => MoveList.GetEnumerator();
	}
}
