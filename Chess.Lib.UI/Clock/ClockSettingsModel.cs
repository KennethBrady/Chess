using Chess.Lib.Hardware;
using Chess.Lib.Hardware.Timing;
using Common.Lib.UI.Dialogs;
using Common.Lib.UI.MVVM;
using Common.Lib.UI.Settings;

namespace Chess.Lib.UI.Clock
{
	public class ClockSettingsModel : DialogModel<ChessClockSetup>
	{
		public record struct ClockPreset(string Name, int TimeLimit, int Increment)
		{
			public static readonly ClockPreset None = new ClockPreset("None", 0, 0);
			public override string ToString() => $"{Name}: {TimeLimit}/{Increment}";			
		}

		private static readonly ClockPreset[] _presets =
		{
			ClockPreset.None,
			new ClockPreset("Bullet", 1, 0),
			new ClockPreset("Blitz", 5, 0),
			new ClockPreset("Rapid", 10, 0),
			new ClockPreset("Classical", 90, 0)
		};

		private bool _useClock, _useDual;
		private ClockPreset _clockPreset = ClockPreset.None;
		public ClockSettingsModel(ChessClockSetup clockSetup)
		{
			StartingClockSetup = clockSetup;
			SingleSettings = new SettingsModel(this, StartingClockSetup, Hue.Light);
			DualSettings = new SettingsModel(this, StartingClockSetup, Hue.Dark);
		}

		[SavedSetting]
		public bool UseClock
		{
			get => _useClock;
			set
			{
				_useClock = value;
				Notify(nameof(UseClock), nameof(UseDual));
			}
		}

		[SavedSetting]
		public bool UseDual
		{
			get => _useClock && _useDual;
			set
			{
				_useDual = value;
				Notify(nameof(UseDual), nameof(SingleClockHeader));
			}
		}

		[SavedSetting]
		public SettingsModel SingleSettings { get; private init; }

		[SavedSetting]
		public SettingsModel DualSettings { get; private init; }

		public IEnumerable<ClockPreset> Presets => _presets;

		public ClockPreset Preset
		{
			get => _clockPreset;
			set
			{
				if (value.TimeLimit > 0)
				{
					_clockPreset = value;
					SingleSettings.TimeLimit = _clockPreset.TimeLimit;
					SingleSettings.Increment = _clockPreset.Increment;
				}
			}
		}

		public string SingleClockHeader => _useDual ? "White Clock Settings" : "Clock Settings";

		public ChessClockSetup StartingClockSetup { get; private set; }

		public ChessClockSetup ResultingClockSetup
		{
			get
			{
				ChessClockSetup r = ChessClockSetup.Empty;
				if (_useClock)
				{
					SingleSettings.Apply(ref r);
					if (_useDual) DualSettings.Apply(ref r);
				}
				return r;
			}
		}

		public bool AreClockSettingsValid()
		{
			if (!_useClock) return true;
			if (!SingleSettings.IsValid) return false;
			return !_useDual || DualSettings.IsValid;
		}

		protected override void Execute(string? parameter)
		{

		}

		public class SettingsModel : ViewModel
		{
			private int _timeLimit, _increment;
			internal SettingsModel(ClockSettingsModel owner,  ChessClockSetup clockSetup, Hue hue)
			{
				Owner = owner;
				ClockSetup = clockSetup;
				Hue = hue;
				_timeLimit = Math.Max(1, (int)(hue == Hue.Light ? ClockSetup.WhiteMaxTime.TotalMinutes : ClockSetup.BlackMaxTime.TotalMinutes));
				_increment = (int)(hue == Hue.Light ? ClockSetup.WhiteIncrement.TotalSeconds : ClockSetup.BlackIncrement.TotalSeconds);
			}
			public ChessClockSetup ClockSetup { get; private init; }

			[SavedSetting]
			public int TimeLimit
			{
				get => _timeLimit;
				set
				{
					_timeLimit = value;
					Notify(nameof(TimeLimit));
					if (Other != null && !Owner.UseDual) Other.TimeLimit = _timeLimit;
				}
			}

			[SavedSetting]
			public int Increment
			{
				get => _increment;
				set
				{
					_increment = value;
					Notify(nameof(Increment));
					if (Other != null && !Owner.UseDual) Other.Increment = _increment;
				}
			}

			private Hue Hue { get; init; }
			internal void Apply(ref ChessClockSetup setup)
			{
				switch (Hue)
				{
					case Hue.Light:
						setup = setup with
						{
							WhiteMaxTime = TimeSpan.FromMinutes(_timeLimit),
							WhiteIncrement = TimeSpan.FromSeconds(_increment),
							BlackMaxTime = TimeSpan.FromMinutes(_timeLimit),
							BlackIncrement = TimeSpan.FromSeconds(_increment)
						};
						break;
					case Hue.Dark:
						setup = setup with { BlackMaxTime = TimeSpan.FromMinutes(_timeLimit), BlackIncrement = TimeSpan.FromSeconds(_increment) };
						break;
				}
			}

			internal bool IsValid => _timeLimit > 0;

			internal SettingsModel? Other => Hue == Hue.Light ? Owner.DualSettings : null;

			private ClockSettingsModel Owner { get; init; }
		}
	}
}
