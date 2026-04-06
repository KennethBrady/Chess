using Chess.Lib.Pgn.DataModel;

namespace Chess.Lib.Pgn.Service.Access
{
	public interface IGameSourceAccess : IDbAccess<GameSource>
	{
		bool Contains(string sourceName);
	}

	internal class GameSourceAccess : DbAccess<GameSource>, IGameSourceAccess
	{
		internal GameSourceAccess(): base(PgnGameService.Service.LoadAll<GameSource>()) { }

		protected override GameSource CreateNew(int id) => GameSource.Empty with { Id = id };
		

		bool IGameSourceAccess.Contains(string sourceName) => Values.Any(v => string.Equals(v.Name, sourceName, StringComparison.OrdinalIgnoreCase));


	}
}
