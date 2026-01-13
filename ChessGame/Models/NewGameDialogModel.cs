using Common.Lib.UI.Dialogs;
using Common.Lib.UI.Settings;

namespace ChessGame.Models
{

	public record struct PlayerNames(string White, string Black)
	{
		public static readonly PlayerNames Empty = new PlayerNames(string.Empty, string.Empty);
		public static readonly PlayerNames Default = new PlayerNames("White", "Black");
	}

	public class NewGameDialogModel : DialogModel<PlayerNames>
	{
		private string _white, _black;
		public NewGameDialogModel(PlayerNames playerNames)
		{
			_white = playerNames.White;
			_black = playerNames.Black;
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

		protected override bool CanExecute(string? parameter)
		{
			switch (parameter)
			{
				case CancelParameter: return true;
				case OKParameter: return !string.IsNullOrEmpty(_white) && !string.IsNullOrEmpty(_black);
			}
			return false;
		}

		protected override void Execute(string? parameter)
		{
			switch (parameter)
			{
				case CancelParameter: Cancel(); break;
				case OKParameter: Accept(PlayerNames.Empty with { White = _white, Black = _black }); break;
			}
		}
	}
}
