namespace Chess.Lib.Pgn.Parsing
{
	public enum PgnParseErrorType 
	{
		EmptyPgn,
		MissingRequiredHeaders,
		MissingPlayer,
		MissingSourceFile
	}

	public interface IPgnParseResult
	{
		string SourcePgn { get; }
		bool Succeeded { get; }
	}

	public interface IPgnParseSuccess : IPgnParseResult
	{
		GameImport Import { get; }
	}

	public interface IPgnParseError : IPgnParseResult
	{
		PgnParseErrorType ErrorType { get; }
	}

	internal record struct PgnParseError(string SourcePgn, PgnParseErrorType ErrorType, string ExtraInfo = ""): IPgnParseError
	{
		public bool Succeeded => false;
	}

	internal record struct PgnParseSuccess(GameImport Import): IPgnParseSuccess
	{
		public string SourcePgn => Import.Moves;
		public bool Succeeded => true;
	}

	public readonly record struct PgnParseProgress(int TotalParsed, int NSuccess, double PercentComplete)
	{
		public int NFail => TotalParsed - NSuccess;
	}
}
