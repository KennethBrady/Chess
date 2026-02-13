using Chess.Lib.Games;
using Chess.Lib.Hardware;
using Chess.Lib.UI.Adorners;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Chess.Lib.UI
{
	public partial class ChessBoard : GameViewBase
	{
		private static readonly IChessGame DefaultGame = GameFactory.NoGame;
		static ChessBoard()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(ChessBoard), new FrameworkPropertyMetadata(typeof(ChessBoard)));
		}

		public ChessBoard()
		{
			State = new BoardState(this); // temporary, as this will be replaced when Game is changed.
			ChessBoardProperties.BrushChanged += ChessBoardProperties_BrushChanged;
			MovingPiece = new MovingPieceAdorner(this);
		}

		private Dictionary<FileRank, ChessSquare> _squares = new();

		internal BoardState State { get; private set; }
		internal AdornerLayer AdornerLayer => ((AdornerDecorator)GetTemplateChild("adorner")).AdornerLayer;
		internal AdornerLayer MainAdorner => AdornerLayer.GetAdornerLayer(this);

		internal IReadOnlyDictionary<FileRank, ChessSquare> Squares => _squares.AsReadOnly();

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
	}
}
