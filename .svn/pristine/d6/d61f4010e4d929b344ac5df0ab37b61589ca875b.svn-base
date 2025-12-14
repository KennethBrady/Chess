using System.Diagnostics;

namespace Chess.Lib.Moves.Parsing
{
    [DebuggerDisplay("{Index}: {Comment}")]
	public record struct MoveComment(string Comment, int Index)
	{
		internal static readonly MoveComment Empty = new MoveComment(string.Empty, -1);
		bool IsEmpty => string.IsNullOrEmpty(Comment);
	}
}