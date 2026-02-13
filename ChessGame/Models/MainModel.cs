using Chess.Lib.Games;
using Chess.Lib.Pgn;
using Chess.Lib.UI.Pgn;
using Common.Lib.UI.Dialogs;
using Common.Lib.UI.MVVM;
using System.Windows;

namespace ChessGame.Models
{
	public class MainModel : MainWindowModel
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
				case "importPgn":
				case "newGame": return true;
				case "exportPgn": return Game.Moves.Count > 0;
				case "undo": return Game is IInteractiveChessGame ig && ig.Moves.Count > 0;
				case "branchGame": return Game.Moves.CurrentPosition > 0;
			}
			return false;
		}

		protected override void Execute(string? parameter)
		{
			switch (parameter)
			{
				case "newGame": StartNewGame(); break;
				case "exportPgn": ExportPgn(); break;
				case "importPgn": ImportPgn(); break;
				case "undo":
					if (Game is IInteractiveChessGame ig) ig.UndoLastMove();
					break;
				case "branchGame": BranchGame(); break;
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
			var result = await ShowDialog(new NewGameDialogModel(GameStartDefinition.Empty));
			if (result.Accepted)
			{
				var s = (IDialogResultAccepted<GameStartDefinition>)result;
				Game = GameFactory.CreateInteractive(s.Value);
			}
		}

		private void BranchGame()
		{
			Game = Game.Branch();
		}

		private async void ImportPgn()
		{
			var result = await ShowDialog(new ImportPgnModel());
			if (result is IDialogResultAccepted<ImportPgnResult> acc)
			{
				PGN pgn = acc.Value.Pgn;
				IPgnChessGame game = GameFactory.CreatePgn(pgn);
				Game = acc.Value.BranchGame ? game.Branch() : game;
			}
		}
		private async void ExportPgn()
		{
			var result = await ShowDialog<PGN>(new PgnEditorModel(_game) { AllowIncompleteTags = true });
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