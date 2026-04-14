using Chess.Lib.Pgn.DataModel;

namespace Chess.Lib.Pgn.Service.Access
{
	public interface IPlayerAccess : IDbAccess<PgnPlayer>
	{

	}

	internal sealed class PlayerAccess : DbAccess<PgnPlayer>, IPlayerAccess
	{
		internal PlayerAccess(): base(PgnGameService.Service.LoadAll<PgnPlayer>()) { }

		protected override PgnPlayer CreateNew(int id) => PgnPlayer.NoPlayer with { Id = id };
	}
}
