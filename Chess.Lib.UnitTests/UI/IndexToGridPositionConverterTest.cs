using Chess.Lib.Games;
using Chess.Lib.Hardware;
using Chess.Lib.UI.Converters;

namespace Chess.Lib.UnitTests.UI
{
	[TestClass]
	public class IndexToGridPositionConverterTest
	{
		[TestMethod]
		public void RowAndColumnFor()
		{
			IChessBoard b = GameFactory.CreateBoard(false);
			foreach (var s in b) Console.WriteLine($"{IndexToGridPositionConverter.ColumnFor(s.Index)},{IndexToGridPositionConverter.RowFor(s.Index)}");
		}
	}
}
