using Chess.Lib.Games;
using Chess.Lib.UI.Clock;
using Common.Lib.UI.Dialogs;
using Common.Lib.UI.Settings;

namespace ChessGame.Models
{

	public class NewGameDialogModel : DialogModel<GameStartDefinition>
	{
		private string _white, _black;
		public NewGameDialogModel(GameStartDefinition gameDefinition)
		{
			_white = gameDefinition.WhiteName;
			_black = gameDefinition.BlackName;
			ClockSettings = new ClockSettingsModel(gameDefinition.ClockSetup);
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
		public ClockSettingsModel ClockSettings { get; private init; }

		protected override bool CanExecute(string? parameter)
		{
			switch (parameter)
			{
				case CancelParameter: return true;
				case OKParameter: return !string.IsNullOrEmpty(_white) && !string.IsNullOrEmpty(_black) && ClockSettings.AreClockSettingsValid();
			}
			return false;
		}

		protected override void Execute(string? parameter)
		{
			switch (parameter)
			{
				case CancelParameter: Cancel(); break;
				case OKParameter: 
					Accept(GameStartDefinition.Empty with { WhiteName = _white, BlackName = _black, ClockSetup = ClockSettings.ResultingClockSetup }); 
					break;
			}
		}
	}
}
