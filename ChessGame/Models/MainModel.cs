using Chess.Lib.Games;
using Chess.Lib.Pgn;
using Chess.Lib.UI.Pgn;
using Common.Lib.UI.Dialogs;
using Common.Lib.UI.MVVM;
using System.Windows;

namespace ChessGame.Models
{
	public class MainModel : AppWindowModel
	{
		private IChessGame _game = GameFactory.CreateInteractive();

		public MainModel(IChessGameWindow window) : base(window)
		{
			_game.MoveCompleted += Game_MoveCompleted;
		}

		public IChessGame Game
		{
			get => _game;
			private set
			{
				ApplyNewGame(value);
				Notify(nameof(Game));
			}
		}

		public new IChessGameWindow Window => (IChessGameWindow)base.Window;

		protected override bool CanExecute(string? parameter)
		{
			switch (parameter)
			{
				case "newGame": return true;
				case "exportPgn": return Game.Moves.Count > 0;
			}
			return false;
		}

		protected override void Execute(string? parameter)
		{
			switch (parameter)
			{
				case "newGame": StartNewGame(); break;
				case "exportPgn": ExportPgn(); break;
			}
		}

		private void ApplyNewGame(IChessGame game)
		{
			if (_game != null) _game.MoveCompleted -= Game_MoveCompleted;
			_game = game;
			_game.MoveCompleted += Game_MoveCompleted;
		}

		private void Game_MoveCompleted(CompletedMove value)
		{
			RaiseCanExecuteChanged();
		}

		private async void StartNewGame()
		{
			var result = await ShowDialog(new NewGameDialogModel(PlayerNames.Default));
			if (result.Accepted)
			{
				var s = (IDialogResultAccepted<PlayerNames>)result;
				Game = GameFactory.CreateInteractive(s.Value.White, s.Value.Black);
			}
		}

		private async void ExportPgn()
		{
			var result = await ShowDialog<PGN>(new PgnEditorModel(_game));
			switch(result)
			{
				case IDialogResultAccepted<PGN> s: 
					Clipboard.SetText(s.Value.ToString()); 
					//TODO: indicate status
					break;
			}
		}
	}
}