namespace Common.Lib.UI.Settings
{
	internal interface ISettingsApplier
	{
		// This is the most relevant method
		void ApplyChanges();

		// These properties were added for unit testing purposes
		bool IsAttached { get; }
		int PropertyCount { get; }
		string CurrentSettings { get; }
	}
}
