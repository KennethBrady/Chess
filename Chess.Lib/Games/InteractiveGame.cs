using Chess.Lib.Hardware;
using Chess.Lib.Hardware.Pieces;
using Chess.Lib.Hardware.Timing;
using Chess.Lib.Moves;
using Chess.Lib.Moves.Parsing;
using Common.Lib.Contracts;
using System.Diagnostics;

namespace Chess.Lib.Games
{
	public record struct GameStartDefinition(string WhiteName, string BlackName, ChessClockSetup ClockSetup)
	{
		public static readonly GameStartDefinition Empty = new GameStartDefinition(string.Empty, string.Empty, ChessClockSetup.Empty);
	}

	internal sealed class InteractiveGame : ChessGame, IInteractiveChessGame, IPromotingGame
	{
		public InteractiveGame() : base(false) { }

		internal InteractiveGame(IChessPlayer white, IChessPlayer black) : base(false, white.Name, black.Name) { }

		internal InteractiveGame(IGame basedOn) : this(basedOn.White, basedOn.Black)
		{
			IGame r = new InteractiveGame(basedOn.White, basedOn.Black);
			foreach (MoveRequest mr in basedOn.Moves.PriorMoves.Select(m => new MoveRequest(m.AsEngineMove)))
			{
				switch (r.NextPlayer.AttemptMove(mr))
				{
					case IMoveAttemptFail f: throw new UnreachableException(f.Reason.ToString());
				}
			}
		}

		internal InteractiveGame(string whiteName, string blackName) : base(false, whiteName, blackName) { }

		internal InteractiveGame(GameStartDefinition gameDefinition) : base(false, gameDefinition.WhiteName, gameDefinition.BlackName)
		{
			Me.AttachClock(gameDefinition.ClockSetup);
		}

		public event Handler<IChessMove>? MoveUndone;
		public event AsyncHandler<Promotion,Promotion>? PromotionRequest;

		public override bool IsReadOnly => false;

		public int ApplyMoves(IMoveParser parser)
		{
			if (parser is not IMoveParserEx ex) return 0;
			return ex.ParseFor(this);
		}

		public IChessClock Clock { get; private set; } = NullClock.Instance;

		public int ApplyMoves(string moves, MoveFormat format = MoveFormat.Unknown)
		{
			if (format == MoveFormat.Unknown) format = Parsers.DetectFormat(moves);
			IMoveParserEx? parser = null;
			switch (format)
			{
				case MoveFormat.Unknown: return 0;
				case MoveFormat.Algebraic: parser = AlgebraicMoves.Create(moves); break;
				case MoveFormat.Engine:
				case MoveFormat.EngineCompact:
				default: parser = EngineMoves.Create(moves); break;
			}
			return parser.ParseFor(this);
		}

		bool IInteractiveChessGame.AttachClock(ChessClockSetup clockSetup)
		{
			if (clockSetup.IsEmpty) return false;
			if (Moves.Count > 0) return false;
			Clock = new ChessClock(clockSetup);
			Clock.Attach(this);
			return true;
		}

		private new IInteractiveChessGame Me => (IInteractiveChessGame)this;

		bool IInteractiveChessGame.UndoLastMove()
		{
			var move = base.UndoLastMove();
			if (move is not NoMove)
			{
				MoveUndone?.Invoke(move);
				return true;
			}
			return false;
		}

		async Task<PieceType> IPromotingGame.RequestPromotion(Hue forPlayer, ISquare onSquare)
		{
			if (PromotionRequest == null) return PieceType.Queen;   // or throw?
			Promotion p = new Promotion(PieceType.Queen, forPlayer, onSquare);
			Promotion resp =  await PromotionRequest(p);
			return resp.PieceType;
		}
	}
}
