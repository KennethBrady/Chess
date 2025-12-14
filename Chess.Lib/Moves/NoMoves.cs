using Common.Lib.Contracts;
using System.Collections;
using System.Collections.Immutable;

namespace Chess.Lib.Moves
{
	internal struct NoMoves : IMoves
	{
		internal static readonly NoMoves Default = new NoMoves();

		private static readonly ImmutableList<IMove> _moves = ImmutableList<IMove>.Empty;

		public IChessMove this[int index] => NoMove.Default;
		IMove IReadOnlyList<IMove>.this[int index] => NoMove.Default;

		public ImmutableList<IMove> AllMoves => _moves;
		public int CurrentPosition
		{
			get => -1;
			set { }
		}
		public IChessMove CurrentMove
		{
			get => NoMove.Default;
			set { }
		}
		public IEnumerable<IChessMove> PriorMoves => _moves;
		public IEnumerable<IChessMove> ForwardMoves => _moves;
		IReadOnlyList<IChessMove> IChessMoves.AllMoves => _moves;
		public bool CanAdvance => false;
		public bool IsAtEnd => true;
		public bool IsReadOnly => true;
		public int Count => 0;

#pragma warning disable 0067
		public event TypeHandler<AppliedMove>? MoveApplied;
#pragma warning restore
		public void AddMove(IMove move) { }

		public IChessMove Advance() => NoMove.Default;

		public IEnumerator<IChessMove> GetEnumerator() => _moves.Cast<IChessMove>().GetEnumerator();

		public IChessMove MoveTo(int moveNumber) => NoMove.Default;		

		public IChessMove MoveToEnd() => NoMove.Default;		

		public IChessMove MoveToStart() => NoMove.Default;

		public string ToEngineMoves(bool compact = false) => string.Empty;		

		IEnumerator<IMove> IEnumerable<IMove>.GetEnumerator() => _moves.GetEnumerator();		

		IEnumerator IEnumerable.GetEnumerator() => _moves.GetEnumerator();

		public void UndoLastMove() { }
	}
}
