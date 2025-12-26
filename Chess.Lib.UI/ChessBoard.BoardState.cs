using Chess.Lib.Games;
using Chess.Lib.Hardware;
using Chess.Lib.Moves;

namespace Chess.Lib.UI
{
	public partial class ChessBoard
	{
		internal class BoardState
		{
			private List<ChessSquare> _lastAffected = new();
			private ChessSquare? _downSquare;

			internal BoardState(ChessBoard board)
			{
				ChessBoard = board;
				Board.Game.MoveCompleted += Game_MoveCompleted;
				foreach (var sq in ChessBoard._squares.Values) sq.ApplySquare();
			}

			private void Game_MoveCompleted(CompletedMove value)
			{
				ClearMoveTargets();
				foreach (var cs in _lastAffected) cs.Adornments = SquareAdornment.None;
				_lastAffected.Clear();
				_lastAffected.AddRange(value.Move.AffectedSquares().Select(s => ChessBoard._squares[s.Position]));
				_lastAffected.ForEach(s => s.ApplySquare());
				var sq = ChessBoard._squares[value.Move.ToSquare.Position];
				sq.Adornments |= SquareAdornment.LastMove;
				if (value.Move.IsCheck)
				{
					ChessSquare cs = ChessBoard._squares[value.Move.CheckedKing.Square.Position];
					_lastAffected.Add(cs);
					cs.Adornments |= SquareAdornment.Check;
				}

			}

			internal ChessBoard ChessBoard { get; private init; }
			private IChessBoard Board => ChessBoard.Game.Board;
			private IChessGame Game => ChessBoard.Game;

			internal ChessSquare? DownSquare => _downSquare;

			public IEnumerable<ChessSquare> MoveTargets => ChessBoard._squares.Values.Where(sq => sq.Adornments.HasFlag(SquareAdornment.MoveTarget));

			internal void ApplyMouseDown(ChessSquare downSquare)
			{
				if (_downSquare == null)
				{
					_downSquare = downSquare;
					foreach (var sq in Board.AllowedMovesFrom(_downSquare.Square))
					{
						ChessSquare tgt = Squares[sq.Position];
						tgt.Adornments |= SquareAdornment.MoveTarget;
					}
				}
				else
				{
					bool moveMade = false;
					if (downSquare.Adornments.HasFlag(SquareAdornment.MoveTarget))
					{
						MoveRequest req = new MoveRequest(_downSquare.Square, downSquare.Square);
						switch (Game.NextPlayer.AttemptMove(req))
						{
							case IMoveAttemptSuccess m:
								moveMade = true;
								break;
						}
					}
					_downSquare = null;
					if (!moveMade)
					{
						ClearMoveTargets();
						ApplyMouseDown(downSquare);
					}
				}
			}

			internal void AttemptMove(IChessSquare from, ChessSquare to)
			{
				if (ChessBoard._squares[from.Position] == _downSquare && to.Adornments.HasFlag(SquareAdornment.MoveTarget))
				{
					MoveRequest req = new MoveRequest(from, to.Square);
					switch(Game.NextPlayer.AttemptMove(req))
					{
						case IMoveAttemptSuccess _: break;

					}
					_downSquare = null;
				}
			}

			private Dictionary<FileRank, ChessSquare> Squares => ChessBoard._squares;
			private void ClearMoveTargets() => ClearAdornments(SquareAdornment.MoveTarget);

			private void ClearAdornments(SquareAdornment toClear)
			{
				foreach (var sq in ChessBoard._squares.Values.Where(s => s.Adornments.HasFlag(toClear)))
					sq.Adornments &= ~toClear;
			}
		}
	}
}
