using Chess.Lib.Pgn.Service.Access;

namespace Chess.Lib.Pgn.Service
{
	public static class ChessDB
	{
		private static Lazy<GameAccess> _gameAccess = new Lazy<GameAccess>(() => new GameAccess());
		private static Lazy<PlayerAccess> _playerAccess = new Lazy<PlayerAccess>(() =>  new PlayerAccess());
		private static readonly Lazy<GameSourceAccess> _gameSourceAccess = new Lazy<GameSourceAccess>(() => new GameSourceAccess());
		private static readonly Lazy<OpeningAccess> _openingAccess = new Lazy<OpeningAccess>(() => new OpeningAccess());

		/// <summary>
		/// Pre-load data to prevent slow first-use.
		/// </summary>
		public static async void Initialize()
		{
			void init()
			{
				_ = _gameAccess.Value;
				_ = _playerAccess.Value;
				_ = _gameSourceAccess.Value;
				_ = _openingAccess.Value;
			}
			await Task.Factory.StartNew(init);
		}

		public static IGameAccess Games => _gameAccess.Value;
		public static IPlayerAccess Players => _playerAccess.Value;
		public static IGameSourceAccess GameSources => _gameSourceAccess.Value;

		public static IOpeningAccess Openings => _openingAccess.Value;
	}
}
