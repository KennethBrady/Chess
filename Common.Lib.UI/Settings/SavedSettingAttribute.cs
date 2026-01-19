using System.Reflection;

namespace Common.Lib.UI.Settings
{
	/// <summary>
	/// Decorates a model property to indicate that it should be persisted as a setting.
	/// This can apply properties with simple types, or to properties which are themselves models with persisted properties.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class SavedSettingAttribute : Attribute { }

	internal static class SettingsEx
	{
		extension(Type type)
		{
			/// <summary>
			/// Get all properties decorated with SavedSettingAttribute.
			/// </summary>
			/// <returns></returns>
			internal List<PropertyInfo> SettingsProperties() =>
				type.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => p.CanRead && p.CanWrite && p.IsSetting).ToList();
			
		}

		extension(PropertyInfo pinfo)
		{
			internal bool IsSetting => pinfo.GetCustomAttribute(typeof(SavedSettingAttribute), false) != null;
		}
	}
}
