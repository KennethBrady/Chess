using Chess.Lib.Games;
using Chess.Lib.Hardware;
using Chess.Lib.UI.Clock;
using Chess.Lib.Variants;
using Common.Lib.UI.Dialogs;
using Common.Lib.UI.Settings;

namespace Chess.Lib.UI.Dialogs
{
	public class NewGameDialogModel : DialogModel<GameSetup>
	{

		private string _white, _black;
		private GameBoardType _boardType;
		private IChessBoard _board;
		private IChessBoard? _lastCustomBoard = null;
		private int _chess960Number = -1;
		private Hue _nextMove = Hue.Default;
		public NewGameDialogModel(GameSetup gameDefinition)
		{
			_white = gameDefinition.WhiteName;
			_black = gameDefinition.BlackName;
			ClockSettings = new ClockSettingsModel(gameDefinition.ClockSetup);
			_boardType = gameDefinition.Board.Type;
			switch(gameDefinition.Board.Type)
			{
				case GameBoardType.Classic: _board = GameFactory.CreateBoard(true); break;
				default: _board = gameDefinition.Board.Board; break;
			}
		}

		[SavedSetting]
		public string White
		{
			get => _white;
			set
			{
				_white = value;
				Notify(nameof(White));
			}
		}

		[SavedSetting]
		public string Black
		{
			get => _black;
			set
			{
				_black = value;
				Notify(nameof(Black));
			}
		}

		[SavedSetting]
		public GameBoardType BoardType
		{
			get => _boardType;
			set
			{
				var prev = _boardType;
				_boardType = value;
				Notify(nameof(BoardType));
				switch(_boardType)
				{
					case GameBoardType.Classic: SampleBoard = GameFactory.CreateBoard(true); break;
					case GameBoardType.FischerRandom: SampleBoard = Chess960.BoardFor(_chess960Number); break;
					case GameBoardType.Custom: BuildCustomBoard(prev); break;
				}
			}
		}

		[SavedSetting]
		public ClockSettingsModel ClockSettings { get; private init; }

		public IEnumerable<int> FischerNumbers => Enumerable.Range(1, 960);

		public int Chess960Number
		{
			get => _chess960Number;
			set
			{
				_chess960Number = value;
				Notify(nameof(Chess960Number));
				SampleBoard = Chess960.BoardFor(_chess960Number - 1);
			}
		}

		public IChessBoard SampleBoard
		{
			get => _board;
			private set
			{
				_board = value;
				Notify(nameof(SampleBoard));
			}
		}

		protected override bool CanExecute(string? parameter)
		{
			switch(parameter)
			{
				case OKParameter:
				case CancelParameter: return true;
			}
			return false;
		}

		protected override void Execute(string? parameter)
		{
			switch(parameter)
			{
				case CancelParameter: Cancel(); break;
				case OKParameter: Accept(); break;
			}
		}

		private bool IsBuildingCustomBoard { get; set; }  // Set true while showing BoardBuilder dialog
		private async void BuildCustomBoard(GameBoardType previous)
		{
			if (IsBuildingCustomBoard) return;
			IsBuildingCustomBoard = true;
			BoardBuilderDialogModel model = _lastCustomBoard == null ? new BoardBuilderDialogModel() : new BoardBuilderDialogModel(_lastCustomBoard);
			var result = await ShowDialog(model);
			if (result is IDialogResultAccepted<GameBoard> acc)
			{
				_lastCustomBoard = SampleBoard = acc.Value.Board;
				_nextMove = acc.Value.NextMove;
			}
			else
			{
				_boardType = previous;
				Notify(nameof(BoardType));
			}
			IsBuildingCustomBoard = false;
		}

		private void Accept()
		{
			Hue nxt = BoardType == GameBoardType.Custom ? _nextMove : Hue.White;
			GameSetup gs = new GameSetup(_white, _black, ClockSettings.ResultingClockSetup, new GameBoard(BoardType, _board, nxt));
			Accept(gs);
		}
	}
}
