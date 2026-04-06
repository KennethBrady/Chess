using Common.Lib.UI.MVVM;
using System.Collections.Immutable;
using System.Windows.Media;

namespace Common.Lib.UI.Controls.Models
{

	public sealed class StackedValuesModel : ViewModel
	{
		public static readonly StackedValuesModel Empty = new StackedValuesModel();
		private static readonly ImmutableList<StackedValue> _emptySettings = ImmutableList<StackedValue>.Empty;
		public record struct StackedSetting(int Order, Brush Background);
		public record struct StackedValue(double Width, double Height, Brush Background);
		public record Stack(Brush Background, double Width);

		private List<StackedSetting> _stack;
		private double _maximum;
		private List<string> _textLines = new();
		private Brush _textBackground = Brushes.Transparent, _textForeground = Brushes.Black;
		private Lazy<double[]> _clearValues;
		private List<double> _currentValues = new();
		private bool _logMode;
		public StackedValuesModel(params Brush[] backgrounds) : this(100, backgrounds) { }
		public StackedValuesModel(double maximum, params Brush[] backgrounds)
		{
			Maximum = maximum;
			int n = 0;
			_stack = backgrounds.Select(b => new StackedSetting(n++, b)).ToList();
			_clearValues = new Lazy<double[]>(() => Enumerable.Repeat<double>(0.0, _stack.Count).ToArray());
		}

		public double Maximum
		{
			get => _maximum;
			set
			{
				if (value != _maximum)
				{
					_maximum = value;
					Notify(nameof(Maximum));
				}
			}
		}

		public string Text
		{
			get => _textLines.Count == 0 ? string.Empty : _textLines[0];
			set => SetText(value);
		}

		public void SetText(params string[] text)
		{
			_textLines = text.ToList();
			Notify(nameof(TextLines), nameof(HasText));
		}

		public void ClearText()
		{
			_textLines = new();
			Notify(nameof(TextLines), nameof(HasText));
		}

		public IEnumerable<string> TextLines => _textLines;

		public Brush TextBackground
		{
			get => _textBackground;
			set
			{
				_textBackground = value;
				Notify(nameof(TextBackground));
			}
		}

		public Brush TextForeground
		{
			get => _textForeground;
			set
			{
				_textForeground = value;
				Notify(nameof(TextForeground));
			}
		}

		public bool HasText => !string.IsNullOrEmpty(Text);

		public bool LogMode
		{
			get => _logMode;
			set
			{
				_logMode = value;
				Notify(nameof(LogMode));
			}
		}

		internal bool IsEmpty => _stack.Count == 0 || Maximum <= 0;

		public IReadOnlyList<StackedValue> Values { get; private set; } = _emptySettings;

		internal Func<(double Width, double Height)> GetDimensions { get; set; } = () => (0, 0);

		public void ClearValues() => SetValues(_clearValues.Value);

		public void ClearAll()
		{
			ClearText();
			ClearValues();
		}

		public void SetValues(params double[] values)
		{
			if (IsEmpty) return;
			int nVal = 0;
			var size = GetDimensions();
			List<StackedValue> svs = new();
			foreach (double v in values)
			{
				int ndx = nVal++ % _stack.Count;
				Brush bg = _stack[ndx].Background;
				double w = size.Width * v / Maximum;
				svs.Add(new StackedValue(w, size.Height, bg));
			}
			Values = svs;
			Notify(nameof(Values));
			_currentValues.Clear();
			_currentValues.AddRange(values);
		}

		internal void Resize()
		{
			if (_currentValues.Count > 0) SetValues(_currentValues.ToArray());
		}

		internal Action<Action> Invoke { get; set; } = (a) => { };

	}
}
