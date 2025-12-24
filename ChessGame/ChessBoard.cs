using Chess.Lib.Games;
using Chess.Lib.Hardware;
using ChessGame.Adorners;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace ChessGame
{
	public partial class ChessBoard : Control
	{
		private const string GamePropertyName = "Game";
		private static readonly IChessGame DefaultGame = GameFactory.NoGame;
		static ChessBoard()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(ChessBoard), new FrameworkPropertyMetadata(typeof(ChessBoard)));
		}

		public static readonly DependencyProperty GameProperty = DependencyProperty.Register(GamePropertyName, typeof(IChessGame),
			typeof(ChessBoard), new PropertyMetadata(DefaultGame, null, CoerceGame));

		private static object CoerceGame(DependencyObject o, object baseValue) =>
			baseValue == null ? DefaultGame : baseValue;

		public IChessGame Game
		{
			get => (IChessGame)GetValue(GameProperty);
			set => SetValue(GameProperty, value);
		}

		public ChessBoard()
		{
			State = new BoardState(this);	// temporary, as this will be replaces when Game is changed.
			ChessBoardProperties.BrushChanged += ChessBoardProperties_BrushChanged;
			MovingPiece = new MovingPieceAdorner(this);
		}

		private bool IsTemplateApplied { get; set; }
		private Dictionary<FileRank, ChessSquare> _squares = new();

		internal BoardState State { get; private set; }
		internal AdornerLayer AdornerLayer => ((AdornerDecorator)GetTemplateChild("adorner")).AdornerLayer;
		internal AdornerLayer MainAdorner => AdornerLayer.GetAdornerLayer(this);

		internal MovingPieceAdorner MovingPiece { get; private init; }

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			IsTemplateApplied = true;
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

		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnPropertyChanged(e);
			switch (e.Property.Name)
			{
				case GamePropertyName:
					if (!IsTemplateApplied) return;
					State = new BoardState(this);
					break;
			}
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
	}
}
