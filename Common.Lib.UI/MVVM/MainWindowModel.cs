using Common.Lib.UI.Windows;

namespace Common.Lib.UI.MVVM
{
	public class MainWindowModel : AppWindowModel
	{
		private static MainWindowModel Empty = new MainWindowModel();

		public static MainWindowModel Instance { get; set; } = Empty;

		private MainWindowModel() : base(AppWindow.Default) { }

		protected MainWindowModel(IAppWindow window) : base(window)
		{
			if (!ReferenceEquals(Instance, Empty)) throw new ApplicationException("Attempted re-creation of singleton model.");
			Instance = this;
		}

		protected override void Execute(string? parameter) { }
	}
}
