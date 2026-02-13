namespace Common.Lib.UI.MVVM
{
	public abstract class PopupModel : CommandModel
	{
		private bool _isOpen;

		public virtual bool IsOpen
		{
			get => _isOpen;
			set
			{
				_isOpen = value;
				Notify(nameof(IsOpen));
			}
		}

		protected override void Execute(string? parameter) { }
	}
}
