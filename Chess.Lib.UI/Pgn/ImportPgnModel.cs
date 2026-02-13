using Chess.Lib.Moves.Parsing;
using Chess.Lib.Pgn;
using Common.Lib.UI.Dialogs;
using System.Windows;

namespace Chess.Lib.UI.Pgn
{
	public record struct ImportPgnResult(PGN Pgn, bool BranchGame);

	public class ImportPgnModel : DialogModel<ImportPgnResult>
	{
		private string _moves = string.Empty;
		private bool _branchGame;
		public ImportPgnModel()
		{
			TryExtractPGNFromClipboard();
			if (Tags == null) Tags = new TagEditorModel();
			
		}

		public TagEditorModel Tags { get; private set; }

		public bool BranchGame
		{
			get => _branchGame;
			set
			{
				_branchGame = value;
				Notify(nameof(BranchGame));
			}
		}

		public bool CanBranchGame { get; private set; }

		public string Moves
		{
			get => _moves;
			set
			{
				_moves = value;
				Notify(nameof(Moves));
			}
		}

		protected override bool CanExecute(string? parameter)
		{
			switch(parameter)
			{
				case CancelParameter: return true;
				case OKParameter: return !string.IsNullOrEmpty(_moves);
				case "parseClipboard": return true;
			}
			return false;
		}

		protected override void Execute(string? parameter)
		{
			switch (parameter)
			{
				case "parseClipboard": TryExtractPGNFromClipboard(); break;
				case OKParameter:
					Accept(new ImportPgnResult(new PGN(Tags.CurrentTags(), _moves), _branchGame));
					break;
				case CancelParameter: Cancel(); break;
			}
		}

		private bool TryExtractPGNFromClipboard()
		{
			if (Clipboard.ContainsText())
			{
				string spgn = Clipboard.GetText();
				try
				{
					PGN pgn = PGN.Parse(spgn);
					if (!pgn.IsEmpty)
					{
						Tags = new TagEditorModel(pgn.Tags);
						Moves = pgn.Moves;
						switch(AlgebraicMoves.Parse(pgn))
						{
							case IParsedGameSuccess succ:
								CanBranchGame = succ.GameEnd.Result == GameResult.Unknown;
								break;
						}
						Notify(nameof(Tags), nameof(Moves),nameof(CanBranchGame));
						return true;
					}
				}
				catch { }
			}
			return false;
		}
	}
}
