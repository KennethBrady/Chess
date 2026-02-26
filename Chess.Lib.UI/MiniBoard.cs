using Chess.Lib.Games;
using Chess.Lib.Hardware;
using Chess.Lib.UI.Images;
using Common.Lib.UI;
using System.Collections.Immutable;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Chess.Lib.UI
{
	public class MiniBoard : Control
	{
		static MiniBoard()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(MiniBoard), new FrameworkPropertyMetadata(typeof(MiniBoard)));
		}

		public static readonly DependencyProperty BoardProperty = DependencyProperty.Register("Board", typeof(IChessBoard),
			typeof(MiniBoard), new PropertyMetadata(GameFactory.EmptyBoard));

		public IChessBoard Board
		{
			get => (IChessBoard)GetValue(BoardProperty);
			set => SetValue(BoardProperty, value);
		}

		private Border Border { get; set; } = DefaultControls.Border;
		private bool IsTempateApplied { get; set; }
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			Border = (Border)GetTemplateChild("border");
			IsTempateApplied = true;
			SizeChanged += MiniBoard_SizeChanged;
		}

		private void MiniBoard_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			ApplyBoard();
		}

		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnPropertyChanged(e);
			if (IsTempateApplied && e.Property == BoardProperty) ApplyBoard();

		}

		private void ApplyBoard()
		{
			if (Board is INoBoard && ActualWidth > 0 && ActualHeight > 0) Border.DataContext = null;
			else
			{
				Size s = new Size(ActualWidth / 9, ActualHeight / 9);
				Border.DataContext = new MiniBoardModel(Board, s);
			}
		}

		public class MiniBoardModel : CommandModel
		{
			internal MiniBoardModel(IChessBoard board, Size size) 
			{
				Board = board;
				var squares = board.Select(s => new SquareModel(s, size)).Chunk(8).Reverse().SelectMany(s => s);
				Squares = ImmutableList<SquareModel>.Empty.AddRange(squares);
			}

			public IChessBoard Board { get; private init; }

			public ImmutableList<SquareModel> Squares { get; private init; }

			protected override void Execute(string? parameter)
			{
				
			}

			public class SquareModel
			{
				internal SquareModel(IChessSquare square, Size size)
				{
					Square = square;
					if (Square.HasPiece) Piece = ImageLoader.LoadImage(Square.Piece);
					Width = size.Width;
					Height = size.Height;
				}

				public Hue Hue => Square.Hue;

				public ImageSource? Piece { get; private init; }

				private IChessSquare Square { get; init; }

				public double Width { get; private init; }
				public double Height { get; private init; }
			}
		}
	}
}
