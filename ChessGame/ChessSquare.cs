using Chess.Lib.Games;
using Chess.Lib.Hardware;
using ChessGame.Adorners;
using ChessGame.Images;
using Common.Lib.UI;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace ChessGame
{
	[Flags]
	public enum SquareAdornment
	{
		None = 0x0000,
		MoveTarget = 0x0001,
		LastMove = 0x0002,
		Check = 0x0004
	}

	[DebuggerDisplay("{FileRank}")]
	public class ChessSquare : Control
	{
		static ChessSquare()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(ChessSquare), new FrameworkPropertyMetadata(typeof(ChessSquare)));
		}

		private static readonly DependencyPropertyKey FileRankProperty = DependencyProperty.RegisterReadOnly("FileRank", typeof(FileRank),
			typeof(ChessSquare), new PropertyMetadata(FileRank.OffBoard));

		private static readonly DependencyPropertyKey AdornmentsProperty = DependencyProperty.RegisterReadOnly("Adornments", typeof(SquareAdornment),
			typeof(ChessSquare), new PropertyMetadata(SquareAdornment.None));


		internal IChessSquare Square => Game.Board[FileRank];

		public FileRank FileRank
		{
			get => (FileRank)GetValue(FileRankProperty.DependencyProperty);
			private set => SetValue(FileRankProperty, value);
		}

		public SquareAdornment Adornments
		{
			get => (SquareAdornment)GetValue(AdornmentsProperty.DependencyProperty);
			internal set => SetValue(AdornmentsProperty, value);
		}

		private ChessBoard Board { get; init; }
		private IChessGame Game => Board.Game;
		private SquareAdorner Adorner { get; init; }

		internal ChessSquare(ChessBoard board, FileRank filerank)
		{
			Board = board;
			FileRank = filerank;
			SetValue(Grid.RowProperty, 7 - (int)FileRank.Rank);
			SetValue(Grid.ColumnProperty, (int)FileRank.File);
			AllowDrop = true;
			Adorner = new SquareAdorner(this);
		}

		internal int Index => FileRank.ToSquareIndex;

		private bool IsTemplateApplied { get; set; }
		private Image Piece { get; set; } = DefaultControls.Image;
		public override void OnApplyTemplate()
		{
			IsTemplateApplied = true;
			Piece = (Image)GetTemplateChild("piece");
			ApplyColors();
			ApplySquare();
			Board.AdornerLayer.Add(Adorner);
		}

		internal void ApplyColors()
		{
			if (!IsTemplateApplied) return;
			Background = FileRank.SquareHue == Hue.Light ? ChessBoardProperties.LightSquareBrush : ChessBoardProperties.DarkSquareBrush;
			TextBlock txt = (TextBlock)GetTemplateChild("txt");
			txt.Text = FileRank.ToSquareIndex.ToString();
		}

		internal void ApplySquare()
		{
			if (!IsTemplateApplied) return;
			Piece.Source = ImageLoader.LoadImage(Square.Piece);
		}

		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnPropertyChanged(e);
			switch (e.Property.Name)
			{
				case "Adornments": Board.AdornerLayer.Update(); break;
			}
		}

		protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			base.OnMouseLeftButtonDown(e);
			Board.State.ApplyMouseDown(this);
		}

		private bool IsDragging { get; set; }
		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			if (Piece.Source == null) return; // no piece to drag
			if (IsDragging) return;
			if (e.LeftButton == MouseButtonState.Pressed && Board.State.DownSquare == this)
			{
				Size size = new Size(Piece.ActualWidth, Piece.ActualHeight);
				Point position = Mouse.GetPosition(Board);
				
				IsDragging = true;
				Board.MovingPiece.SetImage(ImageLoader.LoadImage(Square.Piece)!, size, position);
				DragDrop.DoDragDrop(this, new DataObject(typeof(IChessSquare), Square), DragDropEffects.Move);
				IsDragging = false;
				Board.MovingPiece.Reset();
				Board.MainAdorner.Update(Board);
			}
		}

		protected override void OnDragOver(DragEventArgs e)
		{
			base.OnDragOver(e);
			e.Effects = Adornments.HasFlag(SquareAdornment.MoveTarget) ? DragDropEffects.Move : DragDropEffects.None;
			Board.MainAdorner.Update(Board);
		}

		protected override void OnDragEnter(DragEventArgs e)
		{
			base.OnDragEnter(e);
			e.Effects = Adornments.HasFlag(SquareAdornment.MoveTarget) ? DragDropEffects.None : DragDropEffects.Move;
			Board.MainAdorner.Update(Board);
		}

		protected override void OnDrop(DragEventArgs e)
		{
			base.OnDrop(e);
			if (e.Data.GetData(typeof(IChessSquare)) is IChessSquare sq) Board.State.AttempMove(sq, this);
		}

	}
}
