using Chess.Lib.Games;
using Common.Lib.UI.MVVM;
using System.Windows;

namespace ChessGame
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			DataContext = Model = new MainModel();
		}

		public MainModel Model { get; private init; }
	}

	public class MainModel : Bindable
	{
		private IChessGame _game = GameFactory.CreateInteractive();

		public IChessGame Game
		{
			get => _game;
			set
			{
				_game = value;
				OnChanged(nameof(Game));
			}
		}
	}
}