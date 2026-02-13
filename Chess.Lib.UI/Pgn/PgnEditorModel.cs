using Chess.Lib.Games;
using Common.Lib.UI.MVVM;
using Chess.Lib.Pgn;
using System.Collections.Immutable;
using Chess.Lib.Moves.Parsing;
using Common.Lib.UI.Dialogs;
using System.Windows.Media;
using System.Windows;
using System.Windows.Input;
using System.Diagnostics;
using Common.Lib.UI.Settings;

namespace Chess.Lib.UI.Pgn
{
	public class PgnEditorModel : DialogModel<PGN>
	{
		private bool _canEditMoves, _includeEmptyTags, _allowIncompleteTags, _allowInvalidMoves;
		private string _moves, _acceptLabel = "Copy to Clipboard";

		public PgnEditorModel(IChessGame game) : this(game.ToPgn()) { }
		public PgnEditorModel(IPgnChessGame game) : this(game.ToPgn()) { }

		public PgnEditorModel(PGN pgn)
		{
			PGN = pgn;
			_moves = PGN.Moves;
			Tags = new TagEditorModel(PGN.Tags);
			Tags.Changed += () =>
			{
				RegeneratePGN();
			};
			RegeneratePGN();
		}

		public string Title => "Export Game as PGN";

		/// <summary>
		/// Get/set the label on the "OK" button
		/// </summary>
		public string AcceptLabel
		{
			get => _acceptLabel;
			set
			{
				_acceptLabel = value;
				Notify(nameof(AcceptLabel));
			}
		}

		public PGN PGN { get; init; }

		public TagEditorModel Tags { get; private init; }

		public bool CanEditMoves
		{
			get => _canEditMoves;
			set
			{
				_canEditMoves = value;
				Notify(nameof(CanEditMoves));
			}
		}

		[SavedSetting]
		public bool AllowIncompleteTags
		{
			get => _allowIncompleteTags;
			set
			{
				_allowIncompleteTags = value;
				Notify(nameof(AllowIncompleteTags));
				RaiseCanExecuteChanged();
				RegeneratePGN();
			}
		}

		[SavedSetting]
		public bool AllowInvalidMoves
		{
			get => _allowInvalidMoves;
			set
			{
				_allowInvalidMoves = value;
				Notify(nameof(AllowInvalidMoves));
				RaiseCanExecuteChanged();
			}
		}

		[SavedSetting]
		public bool IncludeEmptyTags
		{
			get => _includeEmptyTags;
			set
			{
				if (value != _includeEmptyTags)
				{
					_includeEmptyTags = value;
					Notify(nameof(IncludeEmptyTags));
					RegeneratePGN();
				}
			}
		}
		public string Moves
		{
			get => _moves;
			set
			{
				_moves = value;
				Notify(nameof(Moves));
			}
		}

		public string ResultingPGN { get; private set; } = string.Empty;

		private bool AreMovesValid()
		{
			if (!_canEditMoves) return true;
			switch (AlgebraicMoves.Parse(_moves))
			{
				case IParsedGameSuccess: return true;
				default: return false;
			}
		}

		protected override bool CanExecute(string? parameter)
		{
			switch (parameter)
			{
				case CancelParameter: return true;
				case OKParameter: return (AllowIncompleteTags || Tags.AreAllTagsValid && (AllowInvalidMoves || AreMovesValid()));
			}
			return false;
		}

		protected override void Execute(string? parameter)
		{
			switch (parameter)
			{
				case CancelParameter: Cancel(); break;
				case OKParameter:
					Accept(PGN.Empty with
					{ Moves = _moves, Tags = ImmutableDictionary<string, string>.Empty.AddRange(Tags.CurrentTags()) });
					break;
			}
		}

		protected override void HandleEscapeKey()
		{
			if (Tags.IsAddingTag) Tags.CancelAddNewTag(); else base.HandleEscapeKey();
		}

		private void RegeneratePGN()
		{
			var result = Tags.ResultTag;
			string m = Moves, gr = result.GameResult.ToTag();
			if (!m.EndsWith(gr)) m = $"{m} {gr}";
			PGN pgn = new PGN(Tags.CurrentTags(IncludeEmptyTags), m);
			ResultingPGN = pgn.ToString(0);
			Notify(nameof(ResultingPGN));
		}

		[DebuggerDisplay("{Tag}: {Value}")]
		public class TagModel : ViewModel
		{
			private static readonly GameResult[] _gameResults = (GameResult[])Enum.GetValues(typeof(GameResult));
			private string _tag = string.Empty, _value = string.Empty;
			private GameResult _result = GameResult.Unknown;
			private DateTime _date = DateTime.Now;
			internal TagModel(string tag, string value)
			{
				_tag = tag;
				_value = value;
				if (_tag == "Date" && string.IsNullOrEmpty(_value)) _value = _date.ToShortDateString();
			}

			internal TagModel(string tag, ICommand delCmd)
			{
				_tag = tag;
				DelCommand = delCmd;
			}

			public ICommand? DelCommand { get; private init; }

			public string Tag
			{
				get => _tag;
				set
				{
					_tag = value;
					Notify(nameof(Tag));
				}
			}

			public string Value
			{
				get => _value;
				set
				{
					_value = value;
					Notify(nameof(Value));
					if (IsRequired) Notify(nameof(RequiredBorder));
				}
			}

			public bool IsRequired => PgnTags.Required.Contains(_tag);

			public FontWeight TagWeight => IsRequired ? FontWeights.Bold : FontWeights.Regular;

			internal bool HasValue => !string.IsNullOrEmpty(_value);
			public IEnumerable<GameResult> GameResults => _gameResults;

			public GameResult GameResult
			{
				get => _result;
				set
				{
					if (_tag == PgnTags.Result)
					{
						_result = value;
						Value = _result.ToTag();
					}
				}
			}

			public DateTime EventDate
			{
				get => _date;
				set
				{
					if (_tag == PgnTags.Date)
					{
						_date = value;
						Value = _date.ToShortDateString();
					}
				}
			}

			internal bool IsValid => IsRequired ? !string.IsNullOrEmpty(_value) : true;

			public Brush RequiredBorder
			{
				get
				{
					if (!IsRequired) return Brushes.Transparent;
					return string.IsNullOrEmpty(_value) ? Brushes.Red : Brushes.Transparent;
				}
			}

		}
	}
}
