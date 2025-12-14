namespace Sql.Lib.Scoring
{
	public delegate T ScoreItemGenerator<T>(string name, object expectedValue, object observedValue, double weight) 
		where T : IScoreItem;

	public interface IScoreItem
	{
		string Name { get; }
		object ExpectedValue { get; }
		object ObservedValue { get; }
		bool IsCorrect { get; }
		double Weight { get; }
		double Score { get; }
		bool IgnoreCase { get; set; }
		bool IncludeScore { get; }
	}
}