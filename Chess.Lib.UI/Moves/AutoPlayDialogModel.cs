using Chess.Lib.Games;
using Chess.Lib.Moves;
using Common.Lib.UI.Dialogs;

namespace Chess.Lib.UI.Moves
{
	public enum AutoplayTrigger { None, Auto, KeyPress };

	public record struct AutoplayOptions(AutoplayTrigger Trigger, TimeSpan Interval)
	{
		public static readonly TimeSpan MinInterval = TimeSpan.FromSeconds(1.0);

		public static readonly AutoplayOptions Default = new AutoplayOptions(AutoplayTrigger.Auto, TimeSpan.FromSeconds(2.0));
		public static readonly AutoplayOptions Empty = new AutoplayOptions(AutoplayTrigger.None, TimeSpan.Zero);
		public bool IsEmpty => Trigger == AutoplayTrigger.None;
		internal TimeSpan ValidInterval
		{
			get
			{
				switch (Trigger)
				{
					case AutoplayTrigger.Auto: return Interval < MinInterval ? MinInterval : Interval;
					default: return TimeSpan.Zero;
				}
			}
		}
	}

	internal class AutoPlayDialogModel : DialogModel<AutoplayOptions>, IDialogTypeSpecifier
	{
		internal const int ColumnCount = 5;
		private static readonly double[] _intervals = { 0.5, 1.0, 2.0, 4.0, 5.0 };
		private AutoplayTrigger _trigger = AutoplayTrigger.Auto;
		private double _interval = 1.0;
		private bool _isRunning;
		private IChessMove _prevMove = GameFactory.NoMove, _nextMove = GameFactory.NoMove;
		public AutoPlayDialogModel(IChessGame game, AutoplayOptions options)
		{
			Game = game;
			Game.Moves.CurrentPosition = -1;
			if (Game.Moves.Count > 0) _nextMove = Game.Moves[0];
			RowCount = Game.Moves.Count / ColumnCount;
			if (RowCount * ColumnCount < Game.Moves.Count) RowCount++;
			_interval = options.Interval.TotalSeconds;
			_trigger= options.Trigger;
		}

		public int RowCount { get; private init; }

		public AutoplayTrigger Trigger
		{
			get => _trigger;
			set
			{
				_trigger = value;
				Notify(nameof(Trigger));
			}
		}

		public IEnumerable<double> Intervals => _intervals;

		public double Interval
		{
			get => _interval;
			set
			{
				_interval = value;
				Notify(nameof(Interval));
			}
		}

		public bool IsRunning
		{
			get => _isRunning;
			private set
			{
				_isRunning = value;
				Notify(nameof(IsRunning),nameof(StartStopLabel));
			}
		}

		public string StartStopLabel => IsRunning ? "Stop" : "Start";

		public IChessGame Game { get; private init; }

		public IEnumerable<IChessMove> Moves => Game.Moves;

		public IChessMove PreviousMove
		{
			get => _prevMove;
			set	// Setter should only be invoked via the UI's list box.
			{
				_prevMove = value;
				Game.Moves.MoveTo(_prevMove.SerialNumber);
			}
		}

		public IChessMove NextMove => _nextMove;

		public AutoplayOptions CurrentOptions => new AutoplayOptions(_trigger, TimeSpan.FromSeconds(_interval));

		internal void AdvanceGame()
		{
			if (Game.Moves.CanAdvance)
			{
				_prevMove = Game.Moves.Advance();
				ApplyPreviousMove();
			}
		}

		private void ApplyPreviousMove()
		{
			int nNxt = _prevMove.SerialNumber + 1;
			if (nNxt < Game.Moves.Count) _nextMove = Game.Moves[nNxt]; else _nextMove = GameFactory.NoMove;
			Notify(nameof(PreviousMove), nameof(NextMove));
		}

		Type IDialogTypeSpecifier.DialogType => typeof(AutoPlayDialog);

		protected override bool CanExecute(string? parameter)
		{
			switch(parameter)
			{
				case "start": return Trigger == AutoplayTrigger.Auto && Game.Moves.CanAdvance;
				case "reset": return !IsRunning && _prevMove is not INoMove;
			}
			return false;
		}

		protected override void Execute(string? parameter)
		{
			switch(parameter)
			{
				case "start": StartStop(); break;
				case CancelParameter: Accept(CurrentOptions); break;
				case "reset":
					Game.Moves.CurrentPosition = -1;
					if (Game.Moves.Count > 0) _nextMove = Game.Moves[0];
					_prevMove = GameFactory.NoMove;
					Notify(nameof(PreviousMove), nameof(NextMove));
					break;
			}
		}

		protected async override void HandleEscapeKey()
		{
			if (IsRunning)
			{
				IsRunning = false;
				Notify(nameof(StartStopLabel));
			}
			else Accept(CurrentOptions);
		}

		protected override void OnCloseButtonClicked()
		{
			Accept(CurrentOptions);
		}

		private async void StartStop()
		{
			if (IsRunning)
			{
				IsRunning = false;
				return;
			}
			IsRunning = true;
			while (IsRunning && Game.Moves.CanAdvance)
			{
				AdvanceGame();
				await Task.Delay(TimeSpan.FromSeconds(_interval));
			}
			IsRunning = false;
		}

	}
}
