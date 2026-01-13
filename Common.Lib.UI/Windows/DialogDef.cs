using Common.Lib.UI.Dialogs;

namespace Common.Lib.UI.Windows
{
	public record struct DialogDef(Type DialogType, Type ModelType, string SettingsKey = "")
	{
		internal static readonly Type DialogViewType = typeof(DialogView);
		internal static readonly Type IDialogModelExType = typeof(IDialogModelEx);

		private bool IsEmpty => DialogType ==null || ModelType == null;
		internal bool IsViewTypeValid => !IsEmpty && DialogType.IsAssignableTo(DialogViewType);
		internal bool IsModelTypeValid => !IsEmpty && ModelType.IsAssignableTo(IDialogModelExType);
		internal bool HasSettingsKey => !string.IsNullOrEmpty(SettingsKey);
	}
}
