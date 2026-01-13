namespace Common.Lib.UI.Settings
{
	internal class NonApplier : ISettingsApplier
	{
		internal static readonly NonApplier Instance = new();
		private NonApplier() { }
		void ISettingsApplier.ApplyChanges() { }
		bool ISettingsApplier.IsAttached => false;
		int ISettingsApplier.PropertyCount => 0;
		string ISettingsApplier.CurrentSettings => string.Empty;
	}

}
