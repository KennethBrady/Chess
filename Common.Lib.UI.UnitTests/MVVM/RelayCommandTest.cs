using Common.Lib.UI.MVVM;

namespace Common.Lib.UI.UnitTests.MVVM
{
	[TestClass]
	public class RelayCommandTest
	{
		[TestMethod]
		public void CanExecute()
		{
			string? lastParameter = null;
			int canExecuteCount = 0;
			const string Valid = "valid", Invalid = "invalid";
			RelayCommand<string> cmd = new RelayCommand<string>(s => { }, s =>
			{
				lastParameter = s;
				canExecuteCount++;
				return s != Invalid;
			});
			Assert.AreEqual(0, canExecuteCount);
			Assert.IsFalse(cmd.CanExecute(Invalid));
			Assert.AreEqual(1, canExecuteCount);
			Assert.AreEqual(Invalid, lastParameter);
			Assert.IsTrue(cmd.CanExecute(Valid));
			Assert.AreEqual(Valid, lastParameter);
		}

		[TestMethod]
		public void Execute()
		{
			string? lastParameter = null;
			int executeCount = 0;
			const string Valid = "valid", Invalid = "invalid";
			RelayCommand<string> cmd = new RelayCommand<string>(s => 
			{
				executeCount++;
				lastParameter = s;
				if (s == Invalid) return;
			}, s =>
			{
				lastParameter = s;
				return s != Invalid;
			});
			cmd.Execute(Invalid);
			Assert.AreEqual(1, executeCount);
			Assert.AreEqual(Invalid, lastParameter);
			cmd.Execute(Valid);
			Assert.AreEqual(2, executeCount);
			Assert.AreEqual(Valid, lastParameter);
		}
	}
}
