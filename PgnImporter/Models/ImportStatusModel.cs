using Chess.Lib.Pgn.Parsing;
using Common.Lib.Extensions;
using Common.Lib.UI.MVVM;

namespace PgnImporter.Models
{
	public class ImportStatusModel : ViewModel
	{
		private List<ImportStepModel> _steps = new();

		internal ImportStatusModel(string sourceName = "")
		{
			SourceName = sourceName;
			_steps.Add(new ImportStepModel(PgnImportStatus.PlayersLocated));
			_steps.Add(new ImportStepModel(PgnImportStatus.PgnMovesParsed));
			_steps.Add(new ImportStepModel(PgnImportStatus.UniquenessVerified));
			_steps.Add(new ImportStepModel(PgnImportStatus.OpeningsMatched));
			_steps.Add(new ImportStepModel(PgnImportStatus.Completed));
		}

		public string SourceName { get; private set; }

		public IEnumerable<ImportStepModel> Steps => _steps;

		public bool HasResults => _steps.Any(s => s.AreValuesSet);

		internal void SetResults(PgnImportStatus step, int nSucceeded, int nFailed)
		{
			_steps.FirstOrDefault(m => m.Step == step)?.SetResults(nSucceeded, nFailed);
			Notify(nameof(HasResults));
		}

		internal void Reset(string sourceName)
		{
			SourceName = sourceName;
			_steps.ForEach(s => s.Reset());
			Notify(nameof(HasResults),nameof(SourceName));
		}

		public class ImportStepModel : ViewModel
		{
			private int _nSucceeded, _nFailed;
			internal ImportStepModel(PgnImportStatus step)
			{
				Step = step;
			}
			internal PgnImportStatus Step { get; private init; }

			public string StepName => Step.ToPascal();

			public string NSucceeded => AreValuesSet ? _nSucceeded.ToString("N0") : string.Empty;
			public string NFailed => AreValuesSet ? _nFailed.ToString("N0") : string.Empty;
			
			internal void SetResults(int nSucceeded, int nFailed)
			{
				_nSucceeded = nSucceeded;
				_nFailed = nFailed;
				AreValuesSet = true;
				Notify(nameof(NSucceeded), nameof(NFailed));
			}

			internal void Reset()
			{
				_nSucceeded = _nFailed = 0;
				AreValuesSet = false;
				Notify(nameof(NSucceeded), nameof(NFailed));
			}

			internal bool AreValuesSet { get; private set; }
		}
	}
}
