

using Common.Lib.UI.Controls.Extensions;

namespace Common.Lib.UI.Controls.Models
{
	internal interface IStatusBarModel
	{
		StatusBar? StatusBar { get; set; }
	}

	public class StatusBarModel : CommandModel, IStatusBarModel
	{
		private bool _isOpen;
		private List<object> _entries = new();

		public StatusBarModel() { }

		public IReadOnlyList<object> Entries => _entries.AsReadOnly();

		public void AddEntry(object entry)
		{
			Bar?.CancelFade();
			_entries.Insert(0, entry);
			Notify(nameof(LastEntry), nameof(PastEntries), nameof(HasPastEntries), nameof(HasEntries), nameof(HasPastEntries));
			Bar?.InitFade();
		}

		public IEnumerable<object> PastEntries => _entries.Skip(1);

		public bool HasPastEntries => _entries.Count > 1;

		public bool HasEntries => _entries.Count > 0;

		public object? LastEntry => _entries.FirstOrDefault();

		public bool IsOpen
		{
			get => _isOpen;
			set
			{
				if (_isOpen != value)
				{
					_isOpen = value;
					Notify(nameof(IsOpen));
				}
			}
		}

		public FadeInfo Fading { get; set; } = FadeInfo.Empty;

		public virtual string ToString(object value)
		{
			if (value is null) return string.Empty;
			string? r = value.ToString();
			return r == null ? string.Empty : r;
		}

		protected override bool CanExecute(string? parameter)
		{
			switch (parameter)
			{
				case "show": return true;
			}
			return false;
		}

		protected override void Execute(string? parameter)
		{
			switch (parameter)
			{
				case "show": IsOpen = !IsOpen; break;
			}
		}



		StatusBar? IStatusBarModel.StatusBar { get; set; }
		private StatusBar? Bar => ((IStatusBarModel)this).StatusBar;
	}
}
