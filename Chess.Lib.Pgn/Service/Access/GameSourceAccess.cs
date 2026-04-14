using Chess.Lib.Pgn.DataModel;

namespace Chess.Lib.Pgn.Service.Access
{
	public interface IGameSourceAccess : IDbAccess<GameSource>
	{
		bool Contains(string sourceName);
		GameSource Latest { get; }
	}

	internal sealed class GameSourceAccess : DbAccess<GameSource>, IGameSourceAccess
	{
		internal GameSourceAccess(): base(PgnGameService.Service.LoadAll<GameSource>()) { }

		protected override GameSource CreateNew(int id) => GameSource.Empty with { Id = id };
		

		bool IGameSourceAccess.Contains(string sourceName) //  => Values.Any(v => string.Equals(v.Name, sourceName, StringComparison.OrdinalIgnoreCase));
		{
			if (string.IsNullOrEmpty(sourceName)) return false;
			string ext = Path.GetExtension(sourceName), name = Path.GetFileNameWithoutExtension(sourceName);
			if (ext == ".pgn") sourceName = $"{name}g.zip";
			return Values.Any(v => string.Equals(v.Name, sourceName, StringComparison.OrdinalIgnoreCase));
		}

		GameSource IGameSourceAccess.Latest => Values.Count == 0 ? GameSource.Empty : Values.Last();
	}
}
