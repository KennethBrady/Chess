using Chess.Lib.Games;
using Chess.Lib.Hardware;
using Chess.Lib.Moves;
using Common.Lib.UI;
using Common.Lib.UI.MVVM;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace Chess.Lib.UI.Moves
{
	public class MovesView : GameViewBase
	{
		static MovesView()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(MovesView), new FrameworkPropertyMetadata(typeof(MovesView)));
		}

		private Border Border { get; set; } = DefaultControls.Border;
		protected override void UseTemplate()
		{
			Border = (Border)GetTemplateChild("border");
			MovesGrid = (DataGrid)GetTemplateChild("movesGrid");
			MovesGrid.SelectedCellsChanged += MovesGrid_SelectedCellsChanged;
			ApplyGame();
		}

		protected override void ApplyGame(IChessGame oldGame, IChessGame newGame)
		{
			base.ApplyGame(oldGame, newGame);
			ApplyGame();
		}

		protected override void HandleMoveCompleted(CompletedMove move) => Model.ApplyMove(move);

		private DataGrid MovesGrid { get; set; } = DefaultControls.DataGrid;

		private MovesViewModel Model { get; set; } = MovesViewModel.Empty;

		private void ApplyGame()
		{
			Border.DataContext = Model = new MovesViewModel(Game);
		}

		private void MovesGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
		{
			SelectedMove = GameFactory.NoMove;
			if (e.AddedCells.Count == 1)
			{
				var c = e.AddedCells[0];
				if (c.Item is MovesViewModel.MoveTrio trio)
				{
					switch(c.Column.DisplayIndex)
					{
						case 1: SelectedMove = trio.W; break;
						case 2: SelectedMove = trio.B; break;
					}
					if (SelectedMove is not INoMove)  Game.Moves.CurrentMove = SelectedMove;
				}
			}
		}

		private IChessMove SelectedMove { get; set; } = GameFactory.NoMove;

		internal class MovesViewModel : ViewModel
		{
			internal static MovesViewModel Empty = new MovesViewModel(GameFactory.NoGame);
			public record Header(string Name, double Width);

			private static readonly Header[] _headers =
			{
				new Header("#", 24),
				new Header("White", 60),
				new Header("Black", 60)
			};

			private ObservableCollection<MoveTrio> _moves = new();

			internal MovesViewModel(IChessGame game)
			{
				Game = game;
				_moves = new ObservableCollection<MoveTrio>(Game.Moves.Chunk(2).Select(c => new MoveTrio(c)));
			}
			public bool IsEmpty => Game is INoGame;

			public IChessGame Game { get; private init; }

			public IEnumerable<Header> Headers => _headers;
			public IEnumerable<MoveTrio> Moves => _moves;
			internal void ApplyMove(CompletedMove move)
			{
				switch(move.Move.Player.Side)
				{
					case Hue.White: _moves.Add(new MoveTrio(move.Move)); break;
					case Hue.Black: 
						if (_moves.Count == 0) _moves.Add(MoveTrio.BlackOnly(move.Move)); else
							_moves.Last().SetBlack(move.Move);
						break;
				}
			}

			internal class MoveTrio : ViewModel
			{
				private bool _wSel, _bSel;
				internal static MoveTrio BlackOnly(IChessMove blackMove)
				{
					MoveTrio r = new MoveTrio();
					r.B = blackMove;
					return r;
				}

				private MoveTrio()
				{
					W = GameFactory.NoMove;
				}

				internal MoveTrio(IChessMove w)
				{
					W = w;
				}

				internal MoveTrio(IEnumerable<IChessMove> movePair): this(movePair.First())
				{
					if (movePair.Count() == 2) B = movePair.Last();
				}

				internal void SetBlack(IChessMove move)
				{
					B = move;
					Notify(nameof(BlackMove));
				}

				public bool IsWhiteSelected
				{
					get => _wSel;
					set
					{
						_wSel = value;
						Notify(nameof(WhiteMove));
					}
				}

				public bool IsBlackSelected
				{
					get => _bSel;
					set
					{
						_bSel = value;
						Notify(nameof(BlackMove));
					}
				}

				public string WhiteMove => W.AlgebraicMove;
				public string BlackMove => B.AlgebraicMove;

				public string MoveNumber => $"{W.Number.GameMoveNumber}.";
				internal IChessMove W { get; private init; }
				internal IChessMove B { get; private set; } = GameFactory.NoMove;
			}
		}
	}
}
