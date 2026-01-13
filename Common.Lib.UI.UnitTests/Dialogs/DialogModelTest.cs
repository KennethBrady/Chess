using Common.Lib.UI.Dialogs;

namespace Common.Lib.UI.UnitTests.Dialogs
{
	[TestClass]
	public class DialogModelTest
	{
		[TestMethod]
		public void Accept()
		{
			TestDialogModel tdm = new TestDialogModel();
			Assert.IsNull(tdm.FinalResult);
			IDialogModelEx<TestType> dex = tdm;
			IDialogResult<TestType>? res = null;
			dex.Closing += (result) =>
			{
				res = result;
			};
			tdm.Close(true);
			Assert.IsNotNull(res);
			Assert.AreSame(res, tdm.FinalResult);
			Assert.IsTrue(res.Accepted);
		}

		[TestMethod]
		public void Cancel()
		{
			const string TC = "TestComplete";
			TestDialogModel tdm = new TestDialogModel();
			Assert.IsNull(tdm.FinalResult);
			IDialogModelEx<TestType> dex = tdm;
			IDialogResult<TestType>? res = null;
			dex.Closing += (result) =>
			{
				res = result;
			};
			tdm.Close(false, TC);
			Assert.IsNotNull(res);
			Assert.AreSame(res, tdm.FinalResult);
			Assert.IsFalse(res.Accepted);
			Assert.IsTrue(tdm.OnClosedInvoked);
			IDialogResultCancelled<TestType>? rc = res as IDialogResultCancelled<TestType>;
			Assert.IsNotNull(rc);
			Assert.AreEqual(TC, rc.Reason);
		}

		public record struct TestType();
		private class TestDialogModel : DialogModel<TestType>
		{
			public bool OnClosedInvoked { get; private set; }
			internal void Close(bool accept, string reason = "")
			{
				if (accept) Accept(new TestType()); else Cancel(reason);
			}

			protected override void Execute(string? parameter) { }

			protected override void OnClosed(IDialogResult<TestType> result)
			{
				base.OnClosed(result);
				OnClosedInvoked = true;
			}
		}
	}
}
