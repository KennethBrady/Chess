using Common.Lib.Contracts;
using Sql.Lib.Services;
using System.Diagnostics;

namespace Chess.Lib.Pgn.DataModel
{
	[DBTable(TableName)]
	[DebuggerDisplay("{Name} ({Id})")]
	public record PgnPlayer(int Id, string Name, int FideId = 0): IId
	{
		public const string TableName = "player";
		public static readonly PgnPlayer NoPlayer = new(0, string.Empty);
		public const string UnknownPlayerName = "Unknown";
		internal const int UnknownPlayerId = 1;
		public static readonly PgnPlayer UnknownPlayer = new PgnPlayer(1, UnknownPlayerName);

		public static readonly IComparer<PgnPlayer> NameComparer = new NComparer();
		public static readonly IComparer<PgnPlayer> IdComparer = IIdComparer.Instance;

		public bool IsNoPlayer => Id <= 0 && string.IsNullOrEmpty(Name);

		#region Comparers

		private class NComparer : Comparer<PgnPlayer>
		{
			public override int Compare(PgnPlayer? x, PgnPlayer? y)
			{
				return string.Compare(x?.Name, y?.Name, true);
			}
		}

		private class NSComparer : Comparer<string>
		{
			public override int Compare(string? x, string? y) => string.Compare(x, y, true);
		}

		#endregion

	}
}
