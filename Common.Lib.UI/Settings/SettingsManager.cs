using Common.Lib.Extensions;
using Common.Lib.UI.MVVM;
using System.Configuration;
using System.Reflection;

namespace Common.Lib.UI.Settings
{
	internal static class SettingsManager
	{
		// TODO: Find a more robust delimiting strategy, as this will fail for strings containing these characters.
		public const char PairDelimiter = '|';
		public const char ValueDelimiter = '=';

		/// <summary>
		/// Apply settings to a model
		/// </summary>
		/// <param name="model"></param>
		/// <param name="settings"></param>
		/// <param name="key"></param>
		internal static void ApplySettings(IViewModel model, ApplicationSettingsBase settings, string key)
		{
			if (settings.IsDefault || !settings.CanApply(key)) return;
			string settingsValues = (string)settings[key];
			ApplySettings(model, settingsValues.ToDictionary());
		}

		/// <summary>
		/// Extract settings from a model and store it in the application settings
		/// </summary>
		/// <param name="model"></param>
		/// <param name="settings"></param>
		/// <param name="key"></param>
		internal static void ExtractAndSaveSettings(IViewModel model, ApplicationSettingsBase settings, string key)
		{
			if (settings.IsDefault || !settings.CanApply(key)) return;
			settings[key] = ExtractSettings(model).Serialize();
		}

		private static Dictionary<string,string> ExtractSettings(IViewModel settings)
		{
			Dictionary<string, string> r = new();
			void add(IViewModel ism, string propertyPrefix)
			{
				foreach (PropertyInfo pinfo in ism.GetType().SettingsProperties())
				{
					object? value = pinfo.GetValue(ism, null);
					string key = string.IsNullOrEmpty(propertyPrefix) ? pinfo.Name : $"{propertyPrefix}.{pinfo.Name}";
					if (pinfo.PropertyType.IsAssignableTo(typeof(IViewModel)))
					{
						IViewModel? sub = value as IViewModel;
						if (sub != null) add(sub, key);
					} else
					{
						string? sval = value?.ToString();
						if (sval is null) sval = string.Empty;
						r.Add(key, sval);
					}
				}
			}
			add(settings, string.Empty);
			return r;
		}

		private static void ApplySettings(IViewModel settings, Dictionary<string,string> values)
		{
			void apply(IViewModel ism, string prefix)
			{
				using var susp = ism.SuspendNotifications();
				foreach(PropertyInfo pinfo in ism.GetType().SettingsProperties())
				{
					string key = string.IsNullOrEmpty(prefix) ? pinfo.Name : $"{prefix}.{pinfo.Name}";
					if (pinfo.PropertyType.IsAssignableTo(typeof(IViewModel)))
					{
						IViewModel? sub = pinfo.GetValue(ism) as IViewModel;
						if (sub != null) apply(sub, key);
					} else
					{
						if (values.ContainsKey(key))
						{
							string value = values[key];
							try
							{
								pinfo.SetValue(ism, value.ConvertTo(pinfo.PropertyType));
							}
							catch (Exception ex)
							{
								System.Diagnostics.Debug.WriteLine($"Setting assignment failed: {ex.Message}");
							}
						}
					}
				}
			}
			apply(settings, string.Empty);
		}
	}

	internal static class SettingsSerializer
	{
		extension(Dictionary<string,string> settings)
		{
			internal string Serialize() => string.Join(SettingsManager.PairDelimiter, settings.Select(nvp => $"{nvp.Key}{SettingsManager.ValueDelimiter}{nvp.Value}"));
		}

		extension(string settings)
		{
			internal Dictionary<string,string> ToDictionary()
			{
				Dictionary<string, string> r = new();
				string[] pairs = settings.Split(SettingsManager.PairDelimiter);
				foreach(string pair in pairs)
				{
					string[] nvp = pair.Split(SettingsManager.ValueDelimiter);
					if (nvp.Length == 2 && !r.ContainsKey(nvp[0])) r.Add(nvp[0], nvp[1]);
				}
				return r;
			}
		}
	}
}
