using ChessGame.Models;
using Common.Lib.UI.Windows;

namespace ChessGame
{
	public interface IChessGameWindow : IAppWindow;

	public partial class MainWindow : AppWindow, IChessGameWindow
	{
		public MainWindow()
		{
			InitializeComponent();
			AppSettings = Settings.Default;
			DataContext = Model = new MainModel(this);
		}

		public MainModel Model { get; private init; }
	}
}