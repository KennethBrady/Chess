using Chess.Lib.Games;
using Chess.Lib.Moves.Parsing;
using System.Diagnostics;

namespace Chess.Lib.Moves
{
	public interface IMoveAttempt
	{
		bool Succeeded { get; }
	}

	public interface IMoveAttemptSuccess : IMoveAttempt
	{
		IChessMove CompletedMove { get; }
	}

	public enum MoveFailureReasons
	{
		NotParsed,
		WrongPlayer,
		GameIsReadOnly,
		GameIsNotAtEnd,
		ReadOnlyGame,
		ReadOnlyPlayer,
		IllegalMove,
		InvalidPromotion,
		UnableToApplyToBoard	// should not happen!
	}

	public interface IMoveAttemptFail : IMoveAttempt
	{
		MoveFailureReasons Reason { get; }
		ParseErrorType ParseError { get; }
	}

	internal record struct MoveAttemptFail(bool Succeeded, MoveFailureReasons Reason, ParseErrorType ParseError = ParseErrorType.NoError) : IMoveAttemptFail;
	internal record struct MoveAttemptSuccess(bool Succeeded, IChessMove CompletedMove) : IMoveAttemptSuccess { }

	internal static class MoveAttempt
	{
		internal static async Task<IMoveAttempt> FromMove(IPlayer player, string move, MoveFormat format)
		{
			if (!player.Game.Moves.IsAtEnd) return new MoveAttemptFail(false, MoveFailureReasons.GameIsNotAtEnd);
			if (player.Game.IsReadOnly) return new MoveAttemptFail(false, MoveFailureReasons.GameIsReadOnly);
			if (!player.HasNextMove) return new MoveAttemptFail(false, MoveFailureReasons.WrongPlayer);
			switch (Parsers.TryParseMove(move, format, player.Board))
			{
				case IMoveParseSuccess s:
					ChessMove m = new ChessMove(s);
					bool success = await player.Board.ApplyInteractive(m);
					if (success) return new MoveAttemptSuccess(true, m);
					return new MoveAttemptFail(false, MoveFailureReasons.UnableToApplyToBoard);
				case IMoveParseError e: return new MoveAttemptFail(false, MoveFailureReasons.NotParsed, e.Error);
			}
			throw new UnreachableException();
		}

		internal async static Task<IMoveAttempt> FromMove(IPlayer player, MoveRequest moveRequest)
		{
			if (!player.Game.Moves.IsAtEnd) return new MoveAttemptFail(false, MoveFailureReasons.GameIsNotAtEnd);
			if (player.Game.IsReadOnly) return new MoveAttemptFail(false, MoveFailureReasons.GameIsReadOnly);
			if (!player.HasNextMove) return new MoveAttemptFail(false, MoveFailureReasons.WrongPlayer);
			switch(moveRequest.Parse(player))
			{
				case IMoveParseSuccess s:
					ChessMove m = new ChessMove(s);
					if (await player.Board.ApplyInteractive(m)) return new MoveAttemptSuccess(true, m);
					return new MoveAttemptFail(false, MoveFailureReasons.UnableToApplyToBoard);
				case IMoveParseError e: return new MoveAttemptFail(false, MoveFailureReasons.NotParsed, e.Error);
			}
			throw new UnreachableException();
		}

	}
}
