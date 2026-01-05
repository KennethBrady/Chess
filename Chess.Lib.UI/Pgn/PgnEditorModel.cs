using Chess.Lib.Games;
using Common.Lib.UI.MVVM;
using Chess.Lib.Pgn;
using Chess.Lib.Pgn.Parsing;
using System.ComponentModel;
using System.Windows.Data;
using System.Collections;
using System.Collections.Immutable;
using Chess.Lib.Moves.Parsing;
using Common.Lib.UI.Dialogs;
using System.Windows.Media;

namespace Chess.Lib.UI.Pgn
{
	public class PgnEditorModel : DialogModel<Lib.Pgn.Pgn>
	{
		private bool _canEditMoves;
		private string _moves;
		private List<TagModel> _tags;
		private ICollectionView _tagsView;

		public PgnEditorModel(IChessGame game): this(game.ToPgn()) { }
		public PgnEditorModel(IPgnChessGame game) : this(game.ToPgn()) { }

		public PgnEditorModel(Lib.Pgn.Pgn pgn)
		{
			Pgn = pgn;
			_tags = pgn.Tags.Select(t => new TagModel(t.Key, t.Value)).ToList();
			foreach (string tag in PgnSourceParser.RequiredTags.Where(t => !_tags.Any(tt => tt.Tag == t))) _tags.Add(new TagModel(tag, string.Empty));		
			_tagsView = CollectionViewSource.GetDefaultView(_tags);
			_moves = Pgn.Moves;
			ResultingPGN = Pgn.ToString();
			_tags.ForEach(t =>
			{
				t.PropertyChanged += (_, n) =>
				{
					if (n.PropertyName == "Value") RegeneratePGN();
					RaiseCanExecuteChanged();
				};
			});
		}
		public string Title => "Export Game as PGN";

		public Lib.Pgn.Pgn Pgn { get; init; }

		public IEnumerable Tags => _tagsView;

		public bool CanEditMoves
		{
			get => _canEditMoves;
			set
			{
				_canEditMoves = value;
				Notify(nameof(CanEditMoves));
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
				case OKParameter: return _tags.All(t => t.IsValid) && AreMovesValid();
			}
			return false;
		}

		protected override void Execute(string? parameter)
		{
			switch (parameter)
			{
				case CancelParameter: Cancel(); break;
				case OKParameter:
					Accept(Lib.Pgn.Pgn.Empty with 
						{ Moves = _moves, Tags = ImmutableDictionary<string, string>.Empty.AddRange(_tags.ToDictionary(t => t.Tag, t => t.Value)) });
					break;
			}
		}

		private void RegeneratePGN()
		{
			var result = _tags.First(t => t.Tag == "Result");

			string m = Moves, gr = result.GameResult.ToTag();
			if (!m.EndsWith(gr)) m = $"{m} {gr}";
			Lib.Pgn.Pgn pgn = new Lib.Pgn.Pgn(_tags.Where(t => t.IsValid).Select(t => (t.Tag, t.Value)), m);
			ResultingPGN = pgn.ToString();
			Notify(nameof(ResultingPGN));
		}

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
			}

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

			public bool IsRequired => PgnSourceParser.RequiredTags.Contains(_tag);

			internal bool HasValue => !string.IsNullOrEmpty(_value);
			public IEnumerable<GameResult> GameResults => _gameResults;

			public GameResult GameResult
			{
				get => _result;
				set
				{
					if (_tag == PgnSourceParser.ResultTag)
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
					if (_tag == PgnSourceParser.DateTag)
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
