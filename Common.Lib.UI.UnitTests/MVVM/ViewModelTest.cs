using Common.Lib.UI.MVVM;

namespace Common.Lib.UI.UnitTests.MVVM
{
	[TestClass]
	public class ViewModelTest
	{
		[TestMethod]
		public void Notify()
		{
			MyVM m = new MyVM();
			List<string> changes = new();
			m.PropertyChanged += (_, e) =>
			{
				if (!string.IsNullOrEmpty(e.PropertyName)) changes.Add(e.PropertyName);
			};
			m.Value = 5;
			Assert.HasCount(1, changes);
			Assert.AreEqual("Value", changes[0]);
			Assert.AreEqual(5, m.Value);
		}

		[TestMethod]
		public void SuspendNotifications()
		{
			MyVM m = new MyVM();
			IViewModel vm = m as IViewModel;
			Assert.IsNotNull(vm);
			bool pcCalled = false;
			m.PropertyChanged += (_, _) => pcCalled = true;
			using var nc = vm.SuspendNotifications();
			m.Value = 6;
			Assert.IsFalse(pcCalled);
		}

		private class MyVM : ViewModel
		{
			private int _value;

			public int Value
			{
				get => _value;
				set
				{
					_value = value;
					Notify(nameof(Value));
				}
			}
		}
	}
}
