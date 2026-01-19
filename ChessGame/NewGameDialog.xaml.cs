using Common.Lib.UI.Dialogs;

namespace ChessGame
{
	/// <summary>
	/// Interaction logic for NewGameDialog.xaml
	/// </summary>
	public partial class NewGameDialog : DialogView
	{
		public NewGameDialog()
		{
			InitializeComponent();
		}
		protected async override void OnLoaded()
		{
			white.Focus();
		}
	}
}
