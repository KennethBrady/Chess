using Chess.Lib.Games;
using Chess.Lib.Moves;
using Common.Lib.UI;
using Common.Lib.UI.MVVM;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace Chess.Lib.UI
{
	public class MovesView : GameViewBase
	{
		static MovesView()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(MovesView), new FrameworkPropertyMetadata(typeof(MovesView)));
		}

		private DataGrid MovesGrid { get; set; } = DefaultControls.DataGrid;
		private ObservableCollection<MovePair> _moves = new();
		protected override void UseTemplate()
		{
			MovesGrid = (DataGrid)GetTemplateChild("moves");
			MovesGrid.ItemsSource = _moves;
			MovesGrid.CurrentCellChanged += MovesGrid_CurrentCellChanged;
		}

		private void MovesGrid_CurrentCellChanged(object? sender, EventArgs e)
		{
			if (MovesGrid.CurrentCell.IsValid && MovesGrid.CurrentCell.Item is MovePair mp)
			{
				switch (MovesGrid.CurrentCell.Column.Header)
				{
					case "White": Game.Moves.CurrentMove = mp.WhiteMove; break;
					case "Black": if (mp.HasBlackMove) Game.Moves.CurrentMove = mp.BlackMove; break;
				}
			}
		}

		protected override void HandleMoveCompleted(CompletedMove move)
		{
			base.HandleMoveCompleted(move);
			if (move.Move.Side == Hardware.Hue.Light) _moves.Add(new MovePair(move.Move)); else _moves.Last().ApplyBlack(move.Move);
		}

		protected override void ApplyGame(IChessGame oldGame, IChessGame newGame)
		{
			base.ApplyGame(oldGame, newGame);
			_moves = new ObservableCollection<MovePair>(newGame.Moves.Chunk(2).Select(mm => new MovePair(mm)));
			if (IsTemplateApplied) MovesGrid.ItemsSource = _moves;
		}

		protected override void HandleGameStateApplied(IChessgameState value)
		{
			for (int i = 0; i < _moves.Count; i++)
			{
				int colIndex = 0;
				if (_moves[i].WhiteMove.SerialNumber == value.SerialNumber)
				{
					colIndex = 1;
				}
				else
					if (_moves[i].HasBlackMove && _moves[i].BlackMove.SerialNumber == value.SerialNumber)
				{
					colIndex = 2;
				}
				if (colIndex > 0)
				{
					MovesGrid.CurrentCell = new DataGridCellInfo(_moves[i], MovesGrid.Columns[colIndex]);
					break;
				}
			}
		}

		protected override void HandleMoveUndone(IChessMove undoneMove)
		{
			if (_moves.Count > 0)
			{
				MovePair last = _moves.Last();
				if(ReferenceEquals(undoneMove, last.BlackMove))
				{
					last.UndoBlackMove();
				}
				if (ReferenceEquals(undoneMove, last.WhiteMove)) _moves.RemoveAt(_moves.Count - 1);
			}
		}

		#region MovePair
		public class MovePair : ViewModel
		{
			private IChessMove _white = GameFactory.NoMove, _black = GameFactory.NoMove;

			internal MovePair(IChessMove whiteMove)
			{
				_white = whiteMove;
				MoveNumber = 1 + _white.SerialNumber / 2;
			}

			internal MovePair(IEnumerable<IChessMove> moves)
			{
				switch (moves.Count())
				{
					case 2:
						_white = moves.First();
						_black = moves.Last();
						break;
					case 1:
						_white = moves.First();
						break;
				}
				MoveNumber = 1 + _white.SerialNumber / 2;
			}

			internal bool HasBlackMove => _black is not INoMove;

			internal void ApplyBlack(IChessMove move)
			{
				_black = move;
				Notify(nameof(BlackMove));
			}

			public int MoveNumber { get; private init; }

			public IChessMove WhiteMove => _white;

			public IChessMove BlackMove => _black;

			internal void UndoBlackMove()
			{
				_black = GameFactory.NoMove;
				Notify(nameof(BlackMove));
			}
		}

		#endregion
	}
}
