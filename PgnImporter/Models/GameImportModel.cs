using Chess.Lib.Moves.Parsing;
using Chess.Lib.Pgn.DataModel;
using Chess.Lib.Pgn.Parsing;
using Common.Lib.UI.MVVM;
using System.Collections.Immutable;
using System.Windows.Media;

namespace PgnImporter.Models
{
	public partial class MainModel
	{
		public class GameImportModel : ViewModel
		{
			private static readonly Brush _defaultBrush = Brushes.White, _foundBrush = Brushes.Lime, _notFoundBrush = Brushes.LightSkyBlue;
			internal GameImportModel(GameImport import)
			{
				Builder = new PgnGameBuilder(import);
				if (Builder.Import.White.FideId > 0) White = $"{Builder.Import.White.Name} ({Builder.Import.White.FideId})";
				else
					White = Builder.Import.White.Name;
				if (Builder.Import.Black.FideId > 0) Black = $"{Builder.Import.Black.Name} ({Builder.Import.Black.FideId})";
				else
					Black = Builder.Import.Black.Name;
			}

			public PgnGameBuilder Builder { get; private set; }

			public int Position => Builder.Import.SourceInfo.Position;
			public int Index => Builder.Import.SourceInfo.Index;

			public DateTime Date => Builder.Import.EventDate;

			public string White { get; private init; }
			public bool? WhiteExists { get; private set; } = null;

			public Brush WhiteBg => WhiteExists == null ? _defaultBrush : WhiteExists.Value ? _foundBrush : _notFoundBrush;

			public string Black { get; private init; }
			public bool? BlackExists { get; private set; } = null;

			public Brush BlackBg => BlackExists == null ? _defaultBrush : BlackExists.Value ? _foundBrush : _notFoundBrush;

			public string MovesParsed { get; private set; } = "--";
			public Brush MovesParsedBg { get; private set; } = _defaultBrush;

			public Brush UniqueBg => IsUnique == null ? _defaultBrush : IsUnique.Value ? _foundBrush : _notFoundBrush;

			public bool? IsUnique { get; set; } = null;

			public string OpeningName { get; private set; } = string.Empty;

			public Brush OpeningBg { get; private set; } = _defaultBrush;

			internal PgnPlayer WhitePlayer => Builder.Import.White;
			internal PgnPlayer BlackPlayer => Builder.Import.Black;

			internal void SetPlayers(PgnPlayer? foundWhite, PgnPlayer? foundBlack)
			{
				if (foundWhite != null)
				{
					WhiteExists = true;
					Builder = Builder with { Import = Builder.Import with { White = foundWhite } };
				}
				else WhiteExists = false;
				if (foundBlack != null)
				{
					Builder = Builder with { Import = Builder.Import with { Black = foundBlack } };
					BlackExists = true;
				}
				else BlackExists = false;
				Notify(nameof(WhiteExists), nameof(BlackExists), nameof(WhiteBg), nameof(BlackBg));
				var nuStatus = Builder.Status | PgnImportStatus.PlayersLocated;
				Builder = Builder with { Status = nuStatus };
			}

			internal void SetUnique(List<int> dupeIds)
			{
				IsUnique = dupeIds.Count == 0;
				if (!IsUnique.Value) Builder = Builder with { DupeGameIds = ImmutableList<int>.Empty.AddRange(dupeIds)};
				Builder = Builder with { Status = Builder.Status | PgnImportStatus.UniquenessVerified };
				Notify(nameof(IsUnique), nameof(UniqueBg));
			}

			internal bool ParseMoves()
			{
				bool r = false;
				var status = Builder.Status;
				switch (AlgebraicMoves.Parse(Builder.Import.Moves))
				{
					case IParsedGameSuccess s:
						MovesParsed = $"{s.Game.Moves.Count} Moves";
						MovesParsedBg = _foundBrush;
						r = true;
						break;
					case IParsedGameFail f:
						MovesParsed = f.Error.Error.ToString();
						MovesParsedBg = _notFoundBrush;
						break;
				}
				status |= PgnImportStatus.PgnMovesParsed;
				Builder = Builder with { Status = status };
				Notify(nameof(MovesParsed), nameof(MovesParsedBg));
				return r;
			}

			internal void SetOpening(Opening? opening)
			{
				if (opening == null) OpeningBg = _notFoundBrush; else
				{
					OpeningBg = _foundBrush;
					OpeningName = opening.Name;
					Builder = Builder with { OpeningId = opening.Id };
				}
				Builder = Builder with { Status = Builder.Status | PgnImportStatus.OpeningsMatched };
				Notify(nameof(OpeningName), nameof(OpeningBg));
			}
		}
	}

}
