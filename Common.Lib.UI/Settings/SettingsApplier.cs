using Common.Lib.Extensions;
using Common.Lib.UI.MVVM;
using System.ComponentModel;
using System.Configuration;
using System.Reflection;

namespace Common.Lib.UI.Settings
{
	internal static class SettingsApplier
	{
		internal static ISettingsApplier Attach(IViewModel model, ApplicationSettingsBase settings, string settingsKey)
		{
			if (string.IsNullOrEmpty(settingsKey)) return NonApplier.Instance;
			return CanAttach(settings, settingsKey) ? new SettingsSetter(model, settings, settingsKey) : NonApplier.Instance;
		}

		private static object? Convert(string value, Type toType) => value.ConvertTo(toType);
		private static bool CanAttach(ApplicationSettingsBase settings, string key)
		{
			if (settings.IsDefault) return false;
			try
			{
				_ = settings[key];
				return true;
			}
			catch { return false; }
		}

		private class SettingsSetter : ISettingsApplier
		{
			private record struct PropertySetting(PropertyInfo Property, string CurrentValue)
			{
				internal string Name => Property.Name;
			}

			// Is it too optimistic to assume that these characters will never be used as user input?
			private const string PairDelim = "★", ValueDelim = "✖";
			private Dictionary<string, PropertySetting> _pSettings = new();
			internal SettingsSetter(IViewModel model, ApplicationSettingsBase settings, string settingsKey)
			{
				Model = model;
				Settings = settings;
				SettingsKey = settingsKey;
				Attach();
			}

			public string SettingsKey { get; private init; }

			private IViewModel Model { get; set; }
			private ApplicationSettingsBase Settings { get; init; }

			public bool IsAttached { get; set; }
			int ISettingsApplier.PropertyCount => _pSettings.Count;

			private void Attach()
			{
				Dictionary<string, string> setValues = new();
				string settingsValue = (string)Settings[SettingsKey];

				string[] nvps = settingsValue.Split(PairDelim);
				foreach (string nvp in nvps)
				{
					string[] parts = nvp.Split(ValueDelim);
					if (parts.Length == 2 && !setValues.ContainsKey(parts[0])) setValues.Add(parts[0], parts[1]);
				}
				using var noNotify = Model.SuspendNotifications();
				foreach (PropertyInfo p in Model.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
				{
					if (!p.CanWrite || !p.CanRead) continue;
					if (p.GetCustomAttribute(typeof(SavedSettingAttribute), true) is SavedSettingAttribute sa)
					{
						if (!setValues.ContainsKey(p.Name))
						{
							object? value = p.GetValue(Model);
							string? sval = value?.ToString();
							if (sval == null) sval = string.Empty;
							_pSettings.Add(p.Name, new PropertySetting(p, sval));
						}
						else
						{
							_pSettings.Add(p.Name, new PropertySetting(p, setValues[p.Name]));
							object? value = Convert(setValues[p.Name], p.PropertyType);
							if (value == null) continue;
							try
							{
								p.SetValue(Model, value);
							}
							catch (Exception ex)
							{
								System.Diagnostics.Debug.WriteLine($"Failed to set {p.Name}: {ex.Message}");
							}
						}
					}

				}
				Model.PropertyChanged += Model_PropertyChanged;
				IsAttached = true;
			}

			public string CurrentSettings => string.Join(PairDelim, _pSettings.Values.Select(ps => $"{ps.Name}{ValueDelim}{ps.CurrentValue}"));

			private void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
			{
				if (e.PropertyName == null) return;
				if (_pSettings.ContainsKey(e.PropertyName))
				{
					var info = _pSettings[e.PropertyName];
					object? nuValue = info.Property.GetValue(Model, null);
					string? sval = nuValue?.ToString();
					if (sval != null) _pSettings[e.PropertyName] = info with { CurrentValue = sval };
				}
			}

			void ISettingsApplier.ApplyChanges()
			{
				if (IsAttached) Settings[SettingsKey] = CurrentSettings;
			}
		}
	}
}
