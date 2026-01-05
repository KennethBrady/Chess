using ChessGame.Models;
using Common.Lib.UI;

namespace ChessGame
{
	public interface IChessGameWindow : IAppWindow;

	public partial class MainWindow : AppWindow, IChessGameWindow
	{
		public MainWindow()
		{
			InitializeComponent();
			DataContext = Model = new MainModel(this);
		}

		public MainModel Model { get; private init; }
	}
}