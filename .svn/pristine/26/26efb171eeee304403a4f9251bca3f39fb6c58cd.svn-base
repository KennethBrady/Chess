using CommonTools.Lib.MVVM;
using System;
using System.Windows.Media;

namespace Sql.Lib.Scoring.Scoring
{
	public class ScoreItem : ModelBase, IScoreItem
	{
		public static readonly ScoreItemGenerator<ScoreItem> DefaultGenerator = (n, ev, ov, w) => new ScoreItem(n, ev, ov, w);
		private double _weight = 1, _opacity = 1;
		private bool _includeScore = true, _ignoreCase = false;
		private Brush _color;
		public ScoreItem(string name, object expectedValue, object observedValue, double weight)
		{
			Name = name ?? throw new ArgumentNullException(nameof(name));
			ExpectedValue = expectedValue ?? throw new ArgumentNullException(nameof(expectedValue));
			ObservedValue = observedValue;
			Weight = weight;
			Opacity = 1;
		}

		public string Name { get; private set; }
		public object ExpectedValue { get; private set; }
		public object ObservedValue { get; private set; }
		public virtual bool IsCorrect => object.Equals(ExpectedValue, ObservedValue);
		public double Weight
		{
			get => _weight;
			set
			{
				_weight = value;
				RaisePropertyChanged(nameof(Weight), nameof(Score));
			}
		}

		public virtual double Score => IsCorrect ? Weight : 0;
		public bool IncludeScore
		{
			get => _includeScore;
			set
			{
				_includeScore = value;
				RaisePropertyChanged(nameof(IncludeScore));
			}
		}
		public bool IgnoreCase
		{
			get => _ignoreCase;
			set
			{
				_ignoreCase = value;
				RaisePropertyChanged(nameof(IgnoreCase));
			}
		}

		// Convenience properties supporting UI
		public Brush Color
		{
			get => _color;
			set
			{
				_color = value;
				RaisePropertyChanged(nameof(Color));
			}
		}
		public double Opacity
		{
			get => _opacity;
			set
			{
				_opacity = value;
				RaisePropertyChanged(nameof(Opacity));
			}
		}
	}
}
