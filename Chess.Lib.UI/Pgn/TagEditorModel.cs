using Chess.Lib.Moves.Parsing;
using Chess.Lib.Pgn;
using Common.Lib.Contracts;
using Common.Lib.UI.MVVM;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace Chess.Lib.UI.Pgn
{
	public class TagEditorModel : CommandModel
	{
		private List<TagModel> _tags;
		private ICollectionView _tagsView;
		private RelayCommand<TagModel> _delCmd;
		private string _newTag = string.Empty;
		public TagEditorModel(IReadOnlyDictionary<string, string> tags)
		{
			_tags = tags.Select(t => new TagModel(t.Key, t.Value)).ToList();
			_tags.Sort((mx, my) => PgnTags.TagComparer.Compare(mx.Tag, my.Tag));
			foreach (string tag in PgnTags.Required.Where(t => !_tags.Any(tt => tt.Tag == t))) _tags.Add(new TagModel(tag, string.Empty));
			_tagsView = CollectionViewSource.GetDefaultView(_tags);
			_delCmd = new RelayCommand<TagModel>(Delete);
			_tags.ForEach(m => Attach(m));
		}

		public TagEditorModel() : this(PgnTags.Required.ToDictionary(t => t, t => string.Empty)) { }

		public IEnumerable Tags => _tagsView;

		public event EmptyHandler? Changed;

		public bool IsAddingTag { get; private set; }

		public bool IsReadOnly { get; init; }

		public string AddTagLabel => IsAddingTag ? "Cancel" : "Add Tag";

		public Action FocusNewTagTextBox { get; internal set; } = Actions.Empty;

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

		public bool AreAllTagsValid => _tags.All(t => t.IsValid);

		public bool HasResultTag => _tags.Any(t => t.Tag == PgnTags.Result);
		internal TagModel ResultTag => _tags.First(t => t.Tag == PgnTags.Result);

		internal void CancelAddNewTag()
		{
			if (IsAddingTag)
			{
				IsAddingTag = false;
				Notify(nameof(IsAddingTag), nameof(AddTagLabel));
			}
		}

		public Dictionary<string,string> CurrentTags(bool includeEmpty = false)
		{
			IEnumerable<TagModel> tags = _tags;
			if (!includeEmpty) tags = tags.Where(t => t.IsValid);
			return tags.ToDictionary(t => t.Tag, t => t.Value);
		}

		private void Delete(TagModel? tm)
		{
			if (tm != null && _tags.Remove(tm))
			{
				_tagsView.Refresh();
			}
		}

		protected override bool CanExecute(string? parameter)
		{
			switch(parameter)
			{
				case "addTag": return !IsAddingTag && !IsReadOnly;
			}
			return false;
		}

		protected override void Execute(string? parameter)
		{
			switch(parameter)
			{
				case "addTag": AddNewTag(); break;
			}
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
				Attach(_tags.Last());
				_tagsView.Refresh();
				IsAddingTag = false;
				NewTag = string.Empty;
				Notify(nameof(IsAddingTag), nameof(AddTagLabel));
			}
		}

		private void Attach(TagModel m)
		{
			m.PropertyChanged += (o, e) =>
			{
				Changed?.Invoke();
			};
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
				switch(_tag)
				{
					case PgnTags.Date:
						if (PgnTags.TryParseDate(_value, out DateTime dt)) _date = dt; else _value = _date.ToString(PgnTags.DateFormat);
							break;
					case PgnTags.Result: 
						switch(_value)
						{
							case PgnTags.ResultTags.WhiteWin: _result = GameResult.WhiteWin; break;
							case PgnTags.ResultTags.BlackWin: _result = GameResult.BlackWin; break;
							case PgnTags.ResultTags.Draw: _result = GameResult.Draw; break;
						}
						break;
				}
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
						Value = _date.ToString(PgnTags.DateFormat);
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
