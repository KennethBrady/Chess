using Chess.Lib.Hardware;
using Chess.Lib.Hardware.Pieces;
using Chess.Lib.Moves;
using Chess.Lib.Moves.Parsing;
using Common.Lib.Contracts;
using System.Collections.Immutable;
using System.Diagnostics;

namespace Chess.Lib.Games
{
	#region Game Interfaces

	public record struct CompletedMove(IChessMove Move, int RepetitionCount)
	{
		public int SerialNumber => Move.SerialNumber;
		public bool IsCheck => Move.IsCheck;
		public bool IsPromotion => Move.Promotion.IsValid;
		public IChessKing CheckedKing
		{
			get
			{
				CompletedMove m = this;
				if (IsCheck) return (IChessKing)M.Board.ActivePieces.Where(p => p.Type == PieceType.King && p.Side != m.Move.Side);
				return NoKing.Default;
			}
		}
		private IMove M => (IMove)Move;
	}

	public interface IReadOnlyChessGame
	{
		IChessBoard Board { get; }
		IReadOnlyChessPlayer White { get; }
		IReadOnlyChessPlayer Black { get; }
		IReadOnlyChessPlayer PlayerOf(Hue hue);
		IChessMove LastMoveMade { get; }
		event TypeHandler<CompletedMove>? MoveCompleted;
		IChessMoves Moves { get; }
		IInteractiveChessGame Branch();
		IChessgameState CurrentState { get; }
		event TypeHandler<IChessgameState>? GameStateApplied;
	}

	public interface IKnownChessGame : IChessGame
	{
		new IReadOnlyChessPlayer White { get; }
		new IReadOnlyChessPlayer Black { get; }
		GameResult Result { get; }
		bool IsEmpty { get; }
		ParseErrorType ParseError { get; }
		ImmutableList<IParseableMove> UnparsedMoves { get; }
	}

	public interface IPgnChessGame : IKnownChessGame
	{
		IPgnGame Source { get; }
	}

	public interface IChessGame : IReadOnlyChessGame
	{
		new IChessPlayer White { get; }
		new IChessPlayer Black { get; }
		IChessPlayer NextPlayer { get; }
		new IChessPlayer PlayerOf(Hue hue);
		bool UndoLastMove();
		event TypeHandler<IChessMove>? MoveUndone;
	}

	public interface IInteractiveChessGame : IChessGame 
	{
		int ApplyMoves(IMoveParser parser);
		int ApplyMoves(string moves, MoveFormat format = MoveFormat.Unknown);
	}

	internal interface IGame : IChessGame
	{
		new IMove LastMoveMade { get; }
		new IBoard Board { get; }
		new IPlayer White { get; }
		new IPlayer Black { get; }
		bool CanMakeMoves { get; }
		bool IsReadOnly { get; }
		new IMoves Moves { get; }
		IReadOnlyList<IMove> MoveList { get; }
	}

	public interface INoGame : IChessGame;

	#endregion

	internal abstract class ChessGame : IGame
	{
		private IPlayer _white, _black;
		private readonly IBoard _board;
		private IChessMove _lastMoveMade = NoMove.Default;
		private IMoves _moves;
		private Dictionary<int, IGameState> _gameStates = new();

		// KnownGame constructor
		protected ChessGame(ChessGame game, string whiteName = "", string blackName = "")
		{
			_board = game._board;
			_board.Game = this;
			_board.MoveMade += OnMoveMade;
			if (string.IsNullOrEmpty(whiteName)) whiteName = game.White.Name;
			if (string.IsNullOrEmpty(blackName)) blackName = game.Black.Name;
			_white = ChessPlayer.Create(whiteName, Hue.Light, this, true);
			_black = ChessPlayer.Create(blackName, Hue.Dark, this, true);
			_gameStates = game._gameStates;
			_lastMoveMade = game._lastMoveMade;
			_moves = new ChessMoves(this, game._moves);
			_moves.MoveApplied += OnMoveApplied;
		}

		protected ChessGame(bool isReadOnly, string whiteName = "", string blackName = ""): this(isReadOnly, whiteName, blackName, string.Empty) { }

		protected ChessGame(bool isReadOnly, string whiteName, string blackName, string fenSetup)
		{
			_board = string.IsNullOrEmpty(fenSetup) ? new Board() : new Board(fenSetup);
			_board.Game = this;
			_board.MoveMade += OnMoveMade;
			_white = ChessPlayer.Create(whiteName, Hue.Light, this, isReadOnly);
			_black = ChessPlayer.Create(blackName, Hue.Dark, this, isReadOnly);
			_moves = new ChessMoves(this, ImmutableList<IMove>.Empty);
			_moves.MoveApplied += OnMoveApplied;
			_gameStates.Add(-1, new GameState(this));
		}

		protected ChessGame(bool isReadonly, IBoard board)
		{
			_board = board;
			_board.Game = this;
			_board.MoveMade += OnMoveMade;
			_white = new ChessPlayer(Hue.Light, this, isReadonly);
			_black = new ChessPlayer(Hue.Dark, this, isReadonly);
			_moves = new ChessMoves(this, ImmutableList<IMove>.Empty);
			_moves.MoveApplied += OnMoveApplied;
			_gameStates.Add(-1, new GameState(this));
		}

		public abstract bool IsReadOnly { get; }

		public IChessMoves Moves => _moves;
		public IChessPlayer White => _white;
		public IChessPlayer Black => _black;
		IReadOnlyChessPlayer IReadOnlyChessGame.White => _white;
		IReadOnlyChessPlayer IReadOnlyChessGame.Black => _black;
		IReadOnlyChessPlayer IReadOnlyChessGame.PlayerOf(Hue hue) => hue == Hue.Light ? _white : hue == Hue.Default ? _black : NoPlayer.Default;
		IChessPlayer IChessGame.PlayerOf(Hue hue) => hue == Hue.Light ? _white : hue == Hue.Default ? _black : NoPlayer.Default;
		IPlayer IGame.White => _white;
		IPlayer IGame.Black => _black;
		IChessPlayer IChessGame.NextPlayer => White.HasNextMove ? White : Black.HasNextMove ? Black : NoPlayer.Default;

		public IChessgameState CurrentState => _gameStates.ContainsKey(Moves.CurrentPosition) ? _gameStates[Moves.CurrentPosition] : GameState.Empty;

		public IChessBoard Board => _board;
		IBoard IGame.Board => _board;

		bool IGame.CanMakeMoves => !IsReadOnly && _moves.IsAtEnd;

		IMoves IGame.Moves => _moves;
		IMove IGame.LastMoveMade
		{
			get
			{
				return Me.MoveList.Count == 0 ? NoMove.Default : Me.MoveList.Last();
			}
		}
		public IChessMove LastMoveMade => _lastMoveMade;
		public event TypeHandler<CompletedMove>? MoveCompleted;
		public event TypeHandler<IChessMove>? MoveUndone;
		public event TypeHandler<IChessgameState>? GameStateApplied;

		public bool UndoLastMove()
		{
			if (!IsReadOnly && _moves.AllMoves.Count > 0)
			{
				IChessMove last = LastMoveMade;
				int n = LastMoveMade.SerialNumber;
				_gameStates.Remove(n);
				_moves.UndoLastMove();
				MoveUndone?.Invoke(last);
				return true;
			}
			return false;
		}

		protected virtual void OnMoveMade(IChessMove move)
		{
			_moves.AddMove((IMove)move);
			int nReps = BoardState.CountRepetitions(Me.MoveList.Select(m => m.BoardState));
			_lastMoveMade = move;
			GameState state = new GameState(this);
			((IMove)move).GameState = state;
			_gameStates.Add(move.SerialNumber, state);
			MoveCompleted?.Invoke(new CompletedMove(move, nReps));
		}

		// Handler for Moves.MoveApplied
		private void OnMoveApplied(AppliedMove appliedMove)
		{
			if (appliedMove.IsNewMove) return;
			if (appliedMove.Move.SerialNumber != LastMoveMade.SerialNumber)
			{
				IGameState state = _gameStates[appliedMove.Move.SerialNumber];
				_board.ApplyState(state.BoardState);
				_lastMoveMade = state.Moves.Count == 0 ? NoMove.Default : state.Moves.Last();
				GameStateApplied?.Invoke(state);
			} 
		}

		protected IGame Me => this;
		IReadOnlyList<IMove> IGame.MoveList => _moves;

		public IInteractiveChessGame Branch()
		{
			string engineMoves = string.Concat(Moves.PriorMoves.Select(m => m.AsEngineMove));
			InteractiveGame r = new InteractiveGame(this);
			IInteractiveChessGame g = new InteractiveGame(White, Black);
			foreach (MoveRequest req in MoveRequest.ParseMoves(engineMoves))
			{
				switch(g.NextPlayer.AttemptMove(req))
				{
					case MoveAttemptFail f: throw new UnreachableException($"Unable to replay moves: {f.Reason}");
				}
			}
			return g;
		}
	}

}
