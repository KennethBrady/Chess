using Chess.Lib.Pgn.DataModel;
using Chess.Lib.Pgn.Service;
using System;
using System.Collections.Generic;
using System.Text;

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
