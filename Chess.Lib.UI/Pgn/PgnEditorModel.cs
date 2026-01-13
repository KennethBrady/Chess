using Chess.Lib.Games;
using Common.Lib.UI.MVVM;
using Chess.Lib.Pgn;
using System.ComponentModel;
using System.Windows.Data;
using System.Collections;
using System.Collections.Immutable;
using Chess.Lib.Moves.Parsing;
using Common.Lib.UI.Dialogs;
using System.Windows.Media;
using Common.Lib.Contracts;
using System.Windows;
using System.Windows.Input;
using System.Diagnostics;
using Common.Lib.UI.Settings;

namespace Chess.Lib.UI.Pgn
{
	public class PgnEditorModel : DialogModel<PGN>
	{
		private bool _canEditMoves, _includeEmptyTags, _allowIncompleteTags, _allowInvalidMoves;
		private string _moves, _acceptLabel = "Copy to Clipboard", _newTag = string.Empty;
		private List<TagModel> _tags;
		private ICollectionView _tagsView;
		private RelayCommand<TagModel> _delCmd;

		public PgnEditorModel(IChessGame game): this(game.ToPgn()) { }
		public PgnEditorModel(IPgnChessGame game) : this(game.ToPgn()) { }

		public PgnEditorModel(PGN pgn)
		{
			PGN = pgn;
			_tags = pgn.Tags.Select(t => new TagModel(t.Key, t.Value)).ToList();
			_tags.Sort((mx, my) => PgnTags.TagComparer.Compare(mx.Tag, my.Tag));
			foreach (string tag in PgnTags.Required.Where(t => !_tags.Any(tt => tt.Tag == t))) _tags.Add(new TagModel(tag, string.Empty));		
			_tagsView = CollectionViewSource.GetDefaultView(_tags);
			_moves = PGN.Moves;
			RegeneratePGN();
			_tags.ForEach(t => AttachModel(t));
			_delCmd = new RelayCommand<TagModel>(Delete);
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

		public IEnumerable Tags => _tagsView;

		public string NewTag
		{
			get => _newTag;
			set
			{
				_newTag = value;
				Notify(nameof(NewTag));
				if (!string.IsNullOrEmpty(_newTag)) FinishNewTag(false);
			}
		}

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

		public bool IsAddingTag { get; private set; }

		public string AddTagLabel => IsAddingTag ? "Cancel" : "Add Tag";

		public Action FocusNewTagTextBox { get; internal set; } = Actions.Empty;

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
				case OKParameter: return (AllowIncompleteTags || _tags.All(t => t.IsValid)) && (AllowInvalidMoves || AreMovesValid());
				case "addTag": return true;			 //return IsAddingTag || _tags.All(t => t.IsValid);
			}
			return false;
		}

		protected override void Execute(string? parameter)
		{
			switch (parameter)
			{
				case CancelParameter: if (IsAddingTag) FinishNewTag(true); else Cancel(); break;
				case OKParameter:
					Accept(PGN.Empty with 
						{ Moves = _moves, Tags = ImmutableDictionary<string, string>.Empty.AddRange(_tags.ToDictionary(t => t.Tag, t => t.Value)) });
					break;
				case "addTag": AddNewTag(); break;
			}
		}

		private void AttachModel(TagModel tm)
		{
			tm.PropertyChanged += (_, n) =>
			{
				if (n.PropertyName == "Value") RegeneratePGN();
				RaiseCanExecuteChanged();
			};
		}

		private void AddNewTag()
		{
			NewTag = string.Empty;
			IsAddingTag = true;
			Notify(nameof(IsAddingTag), nameof(AddTagLabel));
			FocusNewTagTextBox();
		}

		private void FinishNewTag(bool cancel)
		{
			if (cancel)
			{
				IsAddingTag = false;
				Notify(nameof(IsAddingTag), nameof(AddTagLabel));
			}
			if (IsAddingTag && !string.IsNullOrEmpty(_newTag) && !_tags.Any(t => string.Equals(t.Tag, _newTag, StringComparison.OrdinalIgnoreCase)))
			{
				_tags.Add(new TagModel(_newTag, _delCmd));
				AttachModel(_tags.Last());
				_tagsView.Refresh();
				IsAddingTag = false;
				NewTag = string.Empty;
				Notify(nameof(IsAddingTag),nameof(AddTagLabel));
			}
		}

		private void RegeneratePGN()
		{
			var result = _tags.First(t => t.Tag == PgnTags.Result);
			string m = Moves, gr = result.GameResult.ToTag();
			if (!m.EndsWith(gr)) m = $"{m} {gr}";
			PGN pgn = new PGN(_tags.Where(t => IncludeEmptyTags || t.IsValid).Select(t => (t.Tag, t.Value)), m);
			ResultingPGN = pgn.ToString();
			Notify(nameof(ResultingPGN));
		}

		private void Delete(TagModel? tm)
		{
			if (tm != null && _tags.Remove(tm))
			{
				_tagsView.Refresh();
				RegeneratePGN();
			}
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
