using Sql.Lib.Services;
using Chess.Lib.Moves.Parsing;
using System.Diagnostics;
using Common.Lib.Contracts;

namespace Chess.Lib.Pgn.DataModel
{
	// https://chesspathways.com/chess-openings/
	// https://en.wikipedia.org/wiki/List_of_chess_openings

	public enum OpeningMatch { NoMatch, ExactMatch, Transposition }

	[DBTable(TableName)]
	[DebuggerDisplay("{Name} ({Id})")]
	public record Opening(int Id, string Code, string Name, string Sequence, int MoveCount): IId
	{
		public const string TableName = "opening";
		public static Opening Empty = new Opening(0, string.Empty, string.Empty, string.Empty, 0);
		public static Opening NoOpening = new Opening(1, "000", "None", string.Empty, 0);
		public static readonly IComparer<Opening> DescendingMoveCountComparer = new MoveCountComparer();
		public static readonly IComparer<Opening> AscendingMoveCountComparer = new OMoveCountComparer();
		private Lazy<AlgebraicMoves> _moves = new Lazy<AlgebraicMoves>(() => AlgebraicMoves.Create(Sequence));
		public bool IsEmpty => string.IsNullOrEmpty(Name);
		public bool IsNone => IsEmpty && Id == 1;
		public bool HasSequence => !string.IsNullOrEmpty(Sequence);
		public AlgebraicMoves Moves => _moves.Value;

		#region Comparers

		private class MoveCountComparer : IComparer<Opening>
		{
			public int Compare(Opening? x, Opening? y)
			{
				if (x == null) return y == null ? 0 : -1;
				if (y == null) return 1;
				int xLen = x.HasSequence ? x.MoveCount : 0, yLen = y.HasSequence ? y.MoveCount : 0;
				int r = -Comparer<int>.Default.Compare(xLen, yLen);
				if (r == 0) r = string.Compare(x.Name, y.Name);
				return r;
			}
		}

		private class OMoveCountComparer : IComparer<Opening>
		{
			public int Compare(Opening? x, Opening? y)
			{
				if (x == null) return y == null ? 0 : -1;
				if (y == null) return 1;
				int r = Comparer<int>.Default.Compare(x.MoveCount, y.MoveCount);
				if (r == 0) r = string.Compare(x.Name, y.Name);
				return r;
			}
		}

		#endregion
	}

	[DebuggerDisplay("{Name}: {Id} ({MoveCount})")]
	public record OpeningWithGameCount(int Id, string Code, string Name, string Sequence, int MoveCount, int GameCount):
		Opening(Id, Code, Name, Sequence, MoveCount)
	{
		public static new readonly OpeningWithGameCount Empty = new OpeningWithGameCount(Opening.Empty, 0);

		internal OpeningWithGameCount(Opening o, int gameCount): this(o.Id, o.Code, o.Name, o.Sequence, o.MoveCount, gameCount) { }
	}
}
