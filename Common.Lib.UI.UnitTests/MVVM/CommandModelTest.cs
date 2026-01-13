namespace Common.Lib.UI.UnitTests.MVVM
{
	[TestClass]
	public class CommandModelTest
	{
		[TestMethod]
		public void Command()
		{
			const string ARG = "arg";
			TestCommandModel tcm = new();
			int cecCount = 0;
			tcm.Command.CanExecuteChanged += (_, _) =>
			{
				cecCount++;
			};
			Assert.IsNotNull(tcm.Command);
			Assert.HasCount(0, tcm.CommandParameters);
			Assert.HasCount(0, tcm.CanExecuteParameters);
			tcm.Command.Execute(ARG);
			Assert.HasCount(1, tcm.CommandParameters);
			Assert.AreEqual(ARG, tcm.CommandParameters[0]);
			Assert.IsFalse(tcm.Command.CanExecute(ARG));
			Assert.HasCount(1, tcm.CanExecuteParameters);
			Assert.AreEqual(ARG, tcm.CanExecuteParameters[0]);
			Assert.AreEqual(0, cecCount);
			tcm.RaiseCE();
			Assert.AreEqual(1, cecCount);
		}

		private class TestCommandModel : CommandModel
		{
			internal TestCommandModel() { }

			public List<string> CommandParameters { get; init; } = new();
			public List<string> CanExecuteParameters { get; init; } = new();

			internal void RaiseCE() => RaiseCanExecuteChanged();

			protected override bool CanExecute(string? parameter)
			{
				if (parameter != null) CanExecuteParameters.Add(parameter);
				return false;
			}

			protected override void Execute(string? parameter)
			{
				if (parameter != null) CommandParameters.Add(parameter);
			}
		}
	}
}
