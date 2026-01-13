using Common.Lib.UI;
using Common.Lib.UI.Dialogs;
using Common.Lib.UI.Settings;

namespace Common.Lib.UI.UnitTests.Settings
{
	[TestClass]
	public class SettingsTest
	{

		[TestMethod]
		public void Applier()
		{
			TestDialogModel model = new TestDialogModel();
			string originalSettings = TestSettings.Default.MyTestDialogSettings;
			var applier = SettingsApplier.Attach(model, TestSettings.Default, TestDialogModel.SettingsKey);
			Assert.IsTrue(applier.IsAttached);
			Assert.AreEqual(3, applier.PropertyCount);
			Assert.AreEqual("B✖False★D✖0★S✖", applier.CurrentSettings);
			model.B = true;
			model.D = 5.0;
			model.S = "Hi";
			Assert.AreEqual("B✖True★D✖5★S✖Hi", applier.CurrentSettings);
			Assert.AreEqual(originalSettings, TestSettings.Default.MyTestDialogSettings);
			applier.ApplyChanges();
			Assert.AreEqual("B✖True★D✖5★S✖Hi", TestSettings.Default.MyTestDialogSettings);
		}

		[TestMethod]
		public void IsDefault()
		{
			Assert.IsFalse(TestSettings.Default.IsDefault);
		}

		public record struct TestReturnType(bool B, double D);
		private class TestDialogModel : DialogModel<TestReturnType>
		{
			internal const string SettingsKey = "MyTestDialogSettings";
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

			internal void Close(bool accept)
			{
				if (accept) Accept(new TestReturnType(_b, _d));
			}

			protected override void Execute(string? parameter)
			{
				
			}
		}

	}


}
