using Common.Lib.UI.Windows;
using PgnImporter.Models;

namespace PgnImporter
{
	public interface IPgnImporterWindow : IAppWindow
	{
		void ScrollToGame(object oModel);
	}

	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : AppWindow, IPgnImporterWindow
	{
		private MainModel Model { get; init; }
		public MainWindow()
		{
			InitializeComponent();
			AppSettings = Settings.Default;
			DataContext = Model = new MainModel(this);
		}

		public void ScrollToGame(object oGame) => games.ScrollIntoView(oGame);
	}
}