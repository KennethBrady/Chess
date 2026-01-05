namespace Chess.Lib.Moves.Parsing
{
	public enum GameResult : sbyte { Unknown = 0, WhiteWin = 1, BlackWin = 2, Draw = 3 };

	public static class GameResultEx
	{
		extension(GameResult result)
		{
			public string ToTag()
			{
				switch (result)
				{
					case GameResult.Unknown: return "*";
					case GameResult.WhiteWin: return "1-0";
					case GameResult.BlackWin: return "0-1";
					default: return "1/2-1/2";
				}
			}
		}
	}

}
