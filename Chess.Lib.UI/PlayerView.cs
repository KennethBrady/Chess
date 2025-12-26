using Chess.Lib.Games;
using Chess.Lib.Hardware;
using Chess.Lib.Hardware.Pieces;
using Common.Lib.MVVM;
using Common.Lib.UI;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace Chess.Lib.UI
{
	public class PlayerView : GameViewBase
	{
		static PlayerView()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(PlayerView), new FrameworkPropertyMetadata(typeof(PlayerView)));
		}

		public static readonly DependencyProperty HueProperty = DependencyProperty.Register("Hue", typeof(Hue),
			typeof(PlayerView), new PropertyMetadata(Hue.Default));

		private static readonly DependencyPropertyKey PlayerPropertyKey = DependencyProperty.RegisterReadOnly("Player", typeof(IChessPlayer),
			typeof(PlayerView), new PropertyMetadata(GameFactory.NoPlayer));

		public Hue Hue
		{
			get => (Hue)GetValue(HueProperty);
			set => SetValue(HueProperty, value);
		}

		public IChessPlayer Player
		{
			get => (IChessPlayer)GetValue(PlayerPropertyKey.DependencyProperty);
			private set => SetValue(PlayerPropertyKey, value);
		}

		private PlayerModel Model { get; set; } = new PlayerModel(GameFactory.NoPlayer, DefaultControls.GroupBox);

		private Grid Grid { get; set; } = DefaultControls.Grid;
		private GroupBox RemovedPieces { get; set; } = DefaultControls.GroupBox;
		protected override void UseTemplate()
		{
			Grid = (Grid)GetTemplateChild("grid");
			RemovedPieces = (GroupBox)GetTemplateChild("removed");
			ApplyPlayer();
		}

		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnPropertyChanged(e);
			switch (e.Property.Name)
			{
				case "Player":
					if (IsTemplateApplied) ApplyPlayer();
					break;
			}
		}

		protected override void ApplyGame(IChessGame oldGame, IChessGame newGame)
		{
			base.ApplyGame(oldGame, newGame);
			switch (Hue)
			{
				case Hue.Light: Player = newGame.White; break;
				case Hue.Dark: Player = newGame.Black; break;
				default: Player = GameFactory.NoPlayer; break;
			}
			ApplyPlayer();
		}

		private void ApplyPlayer()
		{
			if (!IsTemplateApplied) return;
			if (ReferenceEquals(Player, Model.Player)) return;
			Model = new PlayerModel(Player, RemovedPieces);
			Grid.DataContext = Model;
		}

		protected override void HandleMoveCompleted(CompletedMove move)
		{
		}

		public class PlayerModel : ViewModel
		{
			private ObservableCollection<IChessPiece> _pieces = new();
			internal PlayerModel(IChessPlayer player, GroupBox removedPieces)
			{
				Player = player;
				RemovedPieces = removedPieces;
				Player.Game.MoveCompleted += Game_MoveCompleted;
				FontWeight = Player.Side == Hue.Light ? FontWeights.Bold : FontWeights.Normal;
				RemovedPieces.Visibility = Visibility.Collapsed;
			}

			public IChessPlayer Player { get; private init; }
			public string Name => Player.Name;
			public FontWeight FontWeight { get; set; } = FontWeights.Normal;

			public IEnumerable<IChessPiece> Pieces => _pieces;

			private GroupBox RemovedPieces { get; init; }

			private void Game_MoveCompleted(CompletedMove value)
			{
				if (!ReferenceEquals(Player, value.Move.Player))
				{
					if (value.Move.IsCapture)
					{
						_pieces.Add(value.Move.CapturedPiece);
						RemovedPieces.Visibility = Visibility.Visible;
					}
				}
				FontWeight = Player.HasNextMove ? FontWeights.Bold : FontWeights.Normal;
				Notify(nameof(FontWeight));
			}
		}
	}
}
