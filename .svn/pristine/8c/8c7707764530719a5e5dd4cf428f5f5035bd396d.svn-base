using Chess.Lib.Games;
using Common.Lib.Contracts;
using System.Collections;
using System.Collections.Immutable;

namespace Chess.Lib.Moves
{
	#region Interfaces

	public record struct AppliedMove(IChessMove Move, bool IsNewMove);

	public interface IChessMoves : IReadOnlyList<IChessMove>
	{
		int CurrentPosition { get; set; }
		IChessMove CurrentMove { get; set; }
		IReadOnlyList<IChessMove> AllMoves { get; }
		IEnumerable<IChessMove> PriorMoves { get; }
		IEnumerable<IChessMove> ForwardMoves { get; }
		event TypeHandler<AppliedMove>? MoveApplied;
		bool CanAdvance { get; }
		IChessMove Advance();
		IChessMove MoveTo(int moveNumber);
		IChessMove MoveToStart();
		IChessMove MoveToEnd();
		bool IsAtEnd { get; }
		bool IsReadOnly { get; }
		string ToEngineMoves(bool compact = false);
	}

	internal interface IMoves : IChessMoves, IReadOnlyList<IChessMove>, IReadOnlyList<IMove>
	{
		void AddMove(IMove move);
		new ImmutableList<IMove> AllMoves { get; }
		void UndoLastMove();
	}

	#endregion

	internal class ChessMoves : IMoves
	{
		private ImmutableList<IMove> _allMoves;
		private int _currentPosition;
		internal ChessMoves(IGame game, ImmutableList<IMove> allMoves)
		{
			Game = game;
			_allMoves = allMoves;
			_currentPosition = _allMoves.Count - 1;
		}

		internal ChessMoves(IGame game, IMoves moves): this(game, moves.AllMoves) { }

		public bool IsReadOnly => Game.IsReadOnly;
		public event TypeHandler<AppliedMove>? MoveApplied;

		public int CurrentPosition
		{
			get => _currentPosition;
			set
			{
				int v = Math.Min(_allMoves.Count - 1, Math.Max(-1, value));
				if (v != _currentPosition)
				{
					_currentPosition = value;
					MoveApplied?.Invoke(new AppliedMove(CurrentMove, false));
				}
			}
		}

		public bool IsAtEnd => CurrentPosition == _allMoves.Count - 1;

		public IChessMove CurrentMove
		{
			get
			{
				if (_currentPosition >= 0 && _currentPosition < _allMoves.Count) return _allMoves[_currentPosition];
				return NoMove.Default;
			}
			set
			{
				if (value is not NoMove && value is IMove m && ReferenceEquals(m.Board, Game.Board))
				{
					int n = _allMoves.IndexOf(m);
					if (n >= 0) CurrentPosition = n;
				}
			}
		}

		public IEnumerable<IChessMove> PriorMoves => _allMoves.Where(m => m.SerialNumber <= _currentPosition);
		public IEnumerable<IChessMove> ForwardMoves => _allMoves.Where(m => m.SerialNumber > _currentPosition);

		public bool CanAdvance => _currentPosition < _allMoves.Count - 1;
		public IChessMove Advance()
		{
			if (!CanAdvance) return NoMove.Default;
			CurrentPosition++;
			return CurrentMove;
		}
		public IChessMove MoveTo(int moveNumber) 
		{
			CurrentPosition = moveNumber;
			return CurrentMove;
		}

		public void UndoLastMove()
		{
			if (CurrentPosition >= 0)
			{
				_allMoves = _allMoves.RemoveAt(CurrentPosition);
				CurrentPosition--;
			}
		}

		public IChessMove MoveToStart()
		{
			CurrentPosition = -1;
			return CurrentMove;
		}

		public IChessMove MoveToEnd()
		{
			CurrentPosition = _allMoves.Count - 1;
			return CurrentMove;
		}

		private IGame Game { get; init; }

		void IMoves.AddMove(IMove move)
		{
			_allMoves = _allMoves.Add(move);
			_currentPosition = _allMoves.Count - 1;
		}

		ImmutableList<IMove> IMoves.AllMoves => _allMoves;
		IReadOnlyList<IChessMove> IChessMoves.AllMoves => _allMoves;
		public int Count => _allMoves.Count;

		#region IReadOnlyList<IChessMove> Implementation

		IEnumerator<IChessMove> IEnumerable<IChessMove>.GetEnumerator() => _allMoves.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => _allMoves.GetEnumerator();

		IEnumerator<IMove> IEnumerable<IMove>.GetEnumerator() => _allMoves.GetEnumerator();		

		IChessMove IReadOnlyList<IChessMove>.this[int index] => (index >= 0 && index < _allMoves.Count) ? _allMoves[index] : NoMove.Default;

		IMove IReadOnlyList<IMove>.this[int index] => _allMoves[index];

		#endregion

		public string ToEngineMoves(bool compact = false)
		{
			string spacer = compact ? "" : " ";
			return string.Join(spacer, PriorMoves.Select(m => m.AsEngineMove));
		}
	}
}
