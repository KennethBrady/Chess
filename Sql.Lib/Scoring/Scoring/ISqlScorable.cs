using System.Collections.Generic;
using System.Data;

namespace Sql.Lib.Scoring
{
	public interface ISqlScorable
	{
		SqlType Type { get; }
		DataTable Result { get; }
		DataTable RefResult { get; }
		string RefScript { get; }
	}

	public interface ISqlScoreExportable : ISqlScorable
	{
		string Name { get; }
		string Script { get; }
		string ReferenceDb { get; }
		string Instructions { get; }
		string Comments { get; }
		IEnumerable<IScoreItem> Scores { get; }
		bool OverrideScoring { get; }
		double CustomScore { get; }
		void SetResult(DataTable result, DataTable refResult, string errorMessage);
	}
}
