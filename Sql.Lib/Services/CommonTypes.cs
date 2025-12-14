namespace Sql.Lib.Services
{
	public record struct ValuePair<T>(T OriginalValue, T NewValue) where T : class;
}
