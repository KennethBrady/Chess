using Chess.Lib.Moves;
using Chess.Lib.Moves.Parsing;
using System.Diagnostics;

namespace Chess.Lib.Games
{
	internal sealed class InteractiveGame : ChessGame, IInteractiveChessGame
	{
		public InteractiveGame(): base(false) { }

		internal InteractiveGame(IChessPlayer white, IChessPlayer black): base(false, white.Name, black.Name) { }

		internal InteractiveGame(IGame basedOn): this(basedOn.White, basedOn.Black) 
		{
			IGame r = new InteractiveGame(basedOn.White, basedOn.Black);
			foreach(MoveRequest mr in basedOn.Moves.PriorMoves.Select(m => new MoveRequest(m.AsEngineMove)))
			{
				switch(r.NextPlayer.AttemptMove(mr))
				{
					case IMoveAttemptFail f: throw new UnreachableException(f.Reason.ToString());
				}
			}
		}

		internal InteractiveGame(string whiteName, string blackName): base(false, whiteName, blackName) { }

		public override bool IsReadOnly => false;

		public int ApplyMoves(IMoveParser parser)
		{
			if (parser is not IMoveParserEx ex) return 0;
			return ex.ParseFor(this);
		}

		public int ApplyMoves(string moves, MoveFormat format = MoveFormat.Unknown)
		{
			if (format == MoveFormat.Unknown) format = Parsers.DetectFormat(moves);
			IMoveParserEx? parser = null;
			switch(format)
			{
				case MoveFormat.Unknown: return 0;
				case MoveFormat.Algebraic: parser = AlgebraicMoves.Create(moves); break;
				case MoveFormat.Engine:
				case MoveFormat.EngineCompact:
				default: parser = EngineMoves.Create(moves); break;
			}
			return parser.ParseFor(this);
		}
	}
}
