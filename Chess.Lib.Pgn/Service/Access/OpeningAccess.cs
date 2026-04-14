using Chess.Lib.Pgn.DataModel;

namespace Chess.Lib.Pgn.Service.Access
{
	public interface IOpeningAccess : IDbAccess<Opening>;

	internal sealed class OpeningAccess : DbAccess<Opening>, IOpeningAccess
	{
		internal OpeningAccess(): base(PgnGameService.Service.LoadAll<Opening>()) { }

		protected override Opening CreateNew(int id) => Opening.Empty with { Id = id };
		
	}
}
