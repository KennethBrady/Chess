using Chess.Lib.Games;
using Chess.Lib.Hardware.Timing;
using Chess.Lib.Pgn;
using Chess.Lib.UI.Dialogs;
using Chess.Lib.UI.Pgn;
using Common.Lib.UI.Dialogs;
using Common.Lib.UI.MVVM;
using System.Diagnostics;
using System.Windows;

namespace ChessGame.Models
{
	public class MainModel : MainWindowModel
	{
		private IChessGame _game;

		public MainModel(IChessGameWindow window) : base(window)
		{
			GameSetup gs = GameSetup.FromXml(Settings.Default.NewGameXml);
			_game = GameFactory.CreateInteractive(gs);
			ApplyNewGame(_game);
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

		public string PauseLabel
		{
			get
			{
				const string PAUSE = "Pause Game";
				if (Game is IInteractiveChessGame ig && ig.Clock is not INoClock)
				{
					return ig.Clock.State.HasFlag(ClockState.Paused) ? "Resume Game" : PAUSE;
				}
				return PAUSE;
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
				case "pauseGame": return Game is IInteractiveChessGame ig2 && (ig2.Clock.IsRunning || ig2.Clock.State.HasFlag(ClockState.Paused));
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
				case "pauseGame": PauseGame(); break;
			}
		}

		private void ApplyNewGame(IChessGame game)
		{
			if (_game != null) _game.MoveCompleted -= Game_MoveCompleted;
			if (_game is IInteractiveChessGame ig && ig.Clock is not INoClock) ig.Clock.StateChanged += Clock_StateChanged;
			_game = game;
			_game.MoveCompleted += Game_MoveCompleted;
		}

		private async void StartNewGame()
		{
			GameSetup dflt = GameSetup.FromXml(Settings.Default.NewGameXml);
			NewGameDialogModel gdm = new NewGameDialogModel(dflt);
			var result = await ShowDialog(gdm);
			if (result.Accepted)
			{
				var s = (IDialogResultAccepted<GameSetup>)result;
				if (gdm.SaveAsDefault)
				{
					Settings.Default.NewGameXml = s.Value.ToXml();
				}
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

		private void PauseGame()
		{
			if (Game is IInteractiveChessGame ig)
			{
				if (ig.Clock.State.HasFlag(ClockState.Paused)) ig.Clock.Resume(); else ig.Clock.Pause();
				Notify(nameof(PauseLabel));
			}
		}

		private void Clock_StateChanged(ClockStateChange value)
		{
			if (value.IsPauseOrUnpause) Notify(nameof(PauseLabel));
		}
		private void Game_MoveCompleted(CompletedMove value) => RaiseCanExecuteChanged();

	}
}