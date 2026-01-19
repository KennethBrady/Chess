using Common.Lib.UI;
using Common.Lib.UI.Dialogs;
using Common.Lib.UI.MVVM;
using Common.Lib.UI.Settings;

namespace Common.Lib.UI.UnitTests.Settings
{
	[TestClass]
	public class SettingsTest
	{
		[TestMethod]
		public void IsDefault()
		{
			Assert.IsFalse(TestSettings.Default.IsDefault);
		}

		[TestMethod]
		public void GetAndApplySettings()
		{
			TestDialogModel tdm = new TestDialogModel();
			Assert.IsEmpty(TestSettings.Default.TestDialogSettings);
			SettingsManager.ExtractAndSaveSettings(tdm, TestSettings.Default, TestDialogModel.SettingsKey);
			Assert.IsNotEmpty(TestSettings.Default.TestDialogSettings);
			tdm.S = "Hello";
			tdm.Sub1.S1 = "Yes";
			tdm.Sub1.S2 = "No";
			tdm.Sub2.S1 = "Up";
			tdm.Sub2.S2 = "Down";
			SettingsManager.ExtractAndSaveSettings(tdm, TestSettings.Default, TestDialogModel.SettingsKey);
			TestDialogModel tdm2 = new TestDialogModel();
			bool pccalled = false;
			tdm2.PropertyChanged += (_, _) => pccalled = true;
			SettingsManager.ApplySettings(tdm2, TestSettings.Default, TestDialogModel.SettingsKey);
			Assert.IsFalse(pccalled, "Notifications disabled");
			Assert.AreEqual("Hello", tdm2.S);
			Assert.AreEqual("Yes", tdm2.Sub1.S1);
			Assert.AreEqual("No", tdm2.Sub1.S2);
			Assert.AreEqual("Up", tdm2.Sub2.S1);
			Assert.AreEqual("Down", tdm2.Sub2.S2);
		}

		public record struct TestReturnType(bool B, double D);
		private class TestDialogModel : DialogModel<TestReturnType>
		{
			internal const string SettingsKey = "TestDialogSettings";
			private bool _b;
			private double _d;
			private string _s = string.Empty;

			[SavedSetting]
			public bool B
			{
				get => _b;
				set
				{
					_b = value;
					Notify(nameof(B));
				}
			}

			[SavedSetting]
			public double D
			{
				get => _d;
				set
				{
					_d = value;
					Notify(nameof(D));
				}
			}

			[SavedSetting]
			public string S
			{
				get => _s;
				set
				{
					_s = value;
					Notify(nameof(S));
				}
			}

			[SavedSetting]
			public SubModel Sub1 { get; init; } = new SubModel(1);

			[SavedSetting]
			public SubModel Sub2 { get; init; } = new SubModel(2);

			internal void Close(bool accept)
			{
				if (accept) Accept(new TestReturnType(_b, _d));
			}

			protected override void Execute(string? parameter)
			{

			}

			public class SubModel : ViewModel
			{
				private string _s1 = string.Empty, _s2 = string.Empty;

				internal SubModel(int order)
				{
					Order = order;
				}

				private int Order { get; init; }

				[SavedSetting]
				public string S1
				{
					get => _s1;
					set
					{
						_s1 = value;
						Notify(nameof(S1));
					}
				}

				[SavedSetting]
				public string S2
				{
					get => _s2;
					set
					{
						_s2 = value;
						Notify(nameof(S2));
					}
				}
			}
		}
	}
}
