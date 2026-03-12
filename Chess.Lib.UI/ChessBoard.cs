using Chess.Lib.Games;
using Chess.Lib.Hardware;
using Chess.Lib.Hardware.Timing;
using Chess.Lib.Moves;
using Chess.Lib.UI.Adorners;
using Chess.Lib.UI.Dialogs;
using Chess.Lib.UI.Moves;
using Common.Lib.UI.Dialogs;
using Common.Lib.UI.Extensions;
using Common.Lib.UI.Windows;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Chess.Lib.UI
{
	public partial class ChessBoard : GameViewBase
	{
		private static readonly IChessGame DefaultGame = GameFactory.NoGame;
		internal static readonly ChessBoard Default = new ChessBoard { IsDefault = true };
		static ChessBoard()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(ChessBoard), new FrameworkPropertyMetadata(typeof(ChessBoard)));
		}

		public static readonly DependencyProperty AutoplayProperty = DependencyProperty.Register("Autoplay", typeof(AutoplayOptions),
			typeof(ChessBoard), new FrameworkPropertyMetadata(AutoplayOptions.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

		public ChessBoard()
		{
			State = new BoardState(this); // temporary, as this will be replaced when Game is changed.
			ChessBoardProperties.BrushChanged += ChessBoardProperties_BrushChanged;
			MovingPiece = new MovingPieceAdorner(this);
		}

		public AutoplayOptions Autoplay
		{
			get => (AutoplayOptions)GetValue(AutoplayProperty);
			set => SetValue(AutoplayProperty, value);
		}

		private Dictionary<FileRank, ChessSquare> _squares = new();

		internal BoardState State { get; private set; }
		internal AdornerLayer AdornerLayer => ((AdornerDecorator)GetTemplateChild("adorner")).AdornerLayer;
		internal AdornerLayer MainAdorner => AdornerLayer.GetAdornerLayer(this);

		internal IReadOnlyDictionary<FileRank, ChessSquare> Squares => _squares.AsReadOnly();

		internal bool IsDefault { get; private init; }

		internal MovingPieceAdorner MovingPiece { get; private init; }

		protected override void UseTemplate()
		{
			MainAdorner.Add(MovingPiece);
			InitGrid();
			Grid g = (Grid)GetTemplateChild("board");
			foreach (var fr in FileRank.All)
			{
				ChessSquare sq = new ChessSquare(this, fr);
				g.Children.Add(sq);
				_squares.Add(fr, sq);
			}
			if (!ReferenceEquals(Game, DefaultGame)) State = new BoardState(this);
		}

		private void ChessBoardProperties_BrushChanged(BrushChange value)
		{
			if (!IsTemplateApplied) return;
			foreach (var cs in _squares.Values) cs.ApplyColors();
		}

		protected override void ApplyGame(IChessGame oldGame, IChessGame newGame)
		{
			base.ApplyGame(oldGame, newGame);
			foreach (var sq in _squares.Values) sq.Adornments = SquareAdornment.None;
			State = new BoardState(this);
			IsEnabled = newGame is IInteractiveChessGame;
		}

		/// <summary>
		/// Prepare the grid as 8x8 collection of squares
		/// </summary>
		private void InitGrid()
		{
			if (!IsTemplateApplied) return;
			Grid g = (Grid)GetTemplateChild("board");
			for (int i = 0; i < 8; ++i) g.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
			for (int i = 0; i < 8; ++i) g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
		}

		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnPropertyChanged(e);
			if (e.Property == AutoplayProperty) RunAutoplay();			
		}

		private AutoPlayDialogModel? AutoplayModel { get; set; }

		
		private async void RunAutoplay()
		{
			if (AutoplayModel != null) return;
			AppWindow? w = ((Visual)this).FindParent<AppWindow>();
			if (w != null)
			{
				w.PreviewKeyDown += Window_PreviewKeyDown;
				AutoplayModel = new AutoPlayDialogModel(Game, Autoplay);
				var rsult = await ((IAppWindow)w).ShowDialog(AutoplayModel);
				w.PreviewKeyDown -= Window_PreviewKeyDown;
				if (rsult is IDialogResultAccepted<AutoplayOptions> s)
				{
					Autoplay = s.Value;	// Set AutoPlay first to update binding
					await Task.Delay(500);
					Autoplay = AutoplayOptions.Empty;	// Reset AutoPlay to ready for next invocation
				}
				AutoplayModel = null;
			}
		}

		private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (Keyboard.Modifiers != ModifierKeys.None) return;
			if (AutoplayModel != null && AutoplayModel.Trigger == AutoplayTrigger.KeyPress && AutoplayModel.NextMove is not INoMove)
			{
				if (e.Key == Key.Escape) return;
				AutoplayModel.AdvanceGame();
				e.Handled = true;
			}
		}
	}
}
