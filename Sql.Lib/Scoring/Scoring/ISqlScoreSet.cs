using System.Collections.Generic;

namespace Sql.Lib.Scoring
{
	public interface ISqlScoreSet
	{
		SqlType Type { get; }
		string Comments { get; }
		IEnumerable<ISqlScoreExportable> Scores { get; }
	}
}
