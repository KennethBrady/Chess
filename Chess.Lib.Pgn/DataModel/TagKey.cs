using Sql.Lib.Services;
using System.Diagnostics;

namespace Chess.Lib.Pgn.DataModel
{
	[DebuggerDisplay("{Id}: {Name}")]
	[DBTable(TableName)]
	public record TagKey(int Id, string Name)
	{
		public const string TableName = "tagkey";
	}
}
