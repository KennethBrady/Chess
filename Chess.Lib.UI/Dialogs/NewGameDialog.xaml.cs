using Chess.Lib.Games;
using Common.Lib.UI.Dialogs;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Chess.Lib.UI.Dialogs
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

	internal class GameBoardTypeConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is GameBoardType gbt && parameter is string s)
			{
				switch (s)
				{
					case "cl": return gbt == GameBoardType.Classic;
					case "fr": return gbt == GameBoardType.FischerRandom;
					case "cu": return gbt == GameBoardType.Custom;
					case "vis": return gbt == GameBoardType.FischerRandom ? Visibility.Visible : Visibility.Collapsed;
					case "siv": return gbt == GameBoardType.FischerRandom ? Visibility.Collapsed : Visibility.Visible;
				}
			}
			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is bool b && parameter is string s)
			{
				switch (s)
				{
					case "cl": if (b) return GameBoardType.Classic; break;
					case "fr": if (b) return GameBoardType.FischerRandom; break;
					case "cu": if (b) return GameBoardType.Custom; break;
				}
			}
			return value;
		}
	}
}
