using Chess.Lib.Hardware;
using Chess.Lib.Hardware.Pieces;
using Chess.Lib.Moves;
using Chess.Lib.Moves.Parsing;
using Common.Lib.Contracts;
using System.Collections.Immutable;

namespace Chess.Lib.Games
{
	#region NoPlayer

	/// <summary>
	/// Represents a non-player.  Methods that query a non-existent player should return NoPlayer.Default.
	/// </summary>
	internal record struct NoPlayer : IPlayer, INoPlayer
	{
		internal static NoPlayer Default = new NoPlayer();

		private static readonly ImmutableList<IChessMove> _moves = ImmutableList<IChessMove>.Empty;
		public string Name => nameof(NoPlayer);
		public Hue Side => Hue.Default;
		public IEnumerable<IChessSquare> AvailableSquaresFor(IChessPiece piece) => Enumerable.Empty<IChessSquare>();
		IMoveAttempt IChessPlayer.AttemptMove(string move, MoveFormat format) => new MoveAttemptFail(false, MoveFailureReasons.WrongPlayer);
		public IReadOnlyList<IChessMove> CompletedMoves => _moves;
		public IChessGame Game => KnownGame.Empty;
		public IChessBoard Board => Game.Board;
		IGame IPlayer.Game => KnownGame.Empty;
		public IChessKing King => NoKing.Default;
		IKing IPlayer.King => NoKing.Default;
		bool IReadOnlyChessPlayer.IsReadOnly => true;
		bool IReadOnlyChessPlayer.HasNextMove => false;
		IMoveAttempt IChessPlayer.AttemptMove(IParseableMove move) => new MoveAttemptFail(false, MoveFailureReasons.WrongPlayer);
		IMoveAttempt IChessPlayer.AttemptMove(MoveRequest moveRequest) => new MoveAttemptFail(false, MoveFailureReasons.WrongPlayer);
		IEnumerable<IChessPiece> IReadOnlyChessPlayer.ActivePieces => Enumerable.Empty<IChessPiece>();
		IEnumerable<IChessPiece> IReadOnlyChessPlayer.CapturedPieces => Enumerable.Empty<IChessPiece>();
#pragma warning disable 00067
		public event TypeHandler<PlayerMove>? MoveMade;
		public event TypeHandler<bool>? CanMoveChanged;
#pragma warning restore 00067
		public void RaiseCanMoveChanged() { }
		public bool UndoLastMove() => false;
	}

	#endregion

	/// <summary>
	/// Represents a chess player
	/// </summary>
	/// <param name="Name"></param>
	/// <param name="Side"></param>
	/// <param name="Game"></param>
	/// <param name="IsReadOnly"></param>
	internal sealed record ChessPlayer(string Name, Hue Side, IGame Game, bool IsReadOnly) : IPlayer
	{
		private ImmutableList<IChessMove> _moves = ImmutableList<IChessMove>.Empty;

		internal static ChessPlayer Create(string name, Hue side, IGame game, bool isReadOnly)
		{
			if (string.IsNullOrEmpty(name)) name = $"{side} Player";
			return new ChessPlayer(name, side, game, isReadOnly);
		}

		internal ChessPlayer(Hue side, IGame game, bool isReadOnly) : this($"{side} Player", side, game, isReadOnly) { }

		IChessGame IReadOnlyChessPlayer.Game => Game;
		public IKing King => (IKing)Game.Board.ActivePieces.First(p => p.Side == Side && p is IKing);
		IChessKing IReadOnlyChessPlayer.King => King;
		IChessBoard IReadOnlyChessPlayer.Board => Game.Board;
		public IEnumerable<IChessPiece> ActivePieces => Me.Board.ActivePieces.Where(p => p.Side == Side);
		public IEnumerable<IChessPiece> CapturedPieces => Me.Board.RemovedPieces.Where(p => p.Side == Side);
		public event TypeHandler<PlayerMove>? MoveMade;
		public event TypeHandler<bool>? CanMoveChanged;
		IEnumerable<IChessSquare> IReadOnlyChessPlayer.AvailableSquaresFor(IChessPiece piece)
		{
			if (piece.Side != Side || !HasNextMoveIgnoreReadonly) return Enumerable.Empty<IChessSquare>();
			return Game.Board.Where(s => ((IPiece)piece).CanMoveTo((ISquare)s));
		}

		IReadOnlyList<IChessMove> IReadOnlyChessPlayer.CompletedMoves => _moves;

		IMoveAttempt IChessPlayer.AttemptMove(IParseableMove move) => ApplyMoveAttempt(Me.AttemptMove(move.Move, move.Format));

		IMoveAttempt IChessPlayer.AttemptMove(string move, MoveFormat format) => ApplyMoveAttempt(MoveAttempt.FromMove(this, move, format));

		IMoveAttempt IChessPlayer.AttemptMove(MoveRequest moveRequest) => ApplyMoveAttempt(MoveAttempt.FromMove(this, moveRequest));

		public bool HasNextMove
		{
			get
			{
				if (IsReadOnly) return false;
				return HasNextMoveIgnoreReadonly;
			}
		}

		private bool HasNextMoveIgnoreReadonly
		{
			get
			{
				switch (Game.MoveList.Count % 2)
				{
					case 0: return Side == Hue.Light;
					default: return Side == Hue.Dark;
				}
			}
		}

		public bool UndoLastMove()
		{
			if (!Me.CanUndo) return false;
			IChessMove last = Me.LastMoveMade;
			if (Game.UndoLastMove())
			{
				_moves = _moves.Remove(last);
				return true;
			}
			return false;
		}

		void IPlayer.RaiseCanMoveChanged() => CanMoveChanged?.Invoke(HasNextMove);
		private IPlayer Me => this;

		private IMoveAttempt ApplyMoveAttempt(IMoveAttempt moveAttempt)
		{
			if (moveAttempt is IMoveAttemptSuccess s)
			{
				_moves = _moves.Add(s.CompletedMove);
				MoveMade?.Invoke(new PlayerMove(this, s.CompletedMove));
				((IPlayer)Game.NextPlayer).RaiseCanMoveChanged();
			}
			return moveAttempt;
		}
	}
}
