using Chess.Lib.Pgn.DataModel;
using Chess.Lib.Pgn.Service;

namespace Chess.Lib.UnitTests.Pgn
{
	[TestClass]
	public class PgnGameTest
	{
		[TestMethod]
		public void Loader()
		{
			var loader = PgnGameService.Service.CreateLoaderFor<PgnGame>();
			Assert.IsNotNull(loader);
		}
	}
}
