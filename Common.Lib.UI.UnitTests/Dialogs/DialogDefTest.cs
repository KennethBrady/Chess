using Common.Lib.UI.Dialogs;
using Common.Lib.UI.Windows;

namespace Common.Lib.UI.UnitTests.Dialogs
{
	[TestClass]
	public class DialogDefTest
	{
		[TestMethod]
		public void IsValid()
		{
			DialogDef dd = new();
			Assert.IsFalse(dd.IsViewValid);
			Assert.IsFalse(dd.IsModelValid);
			dd = new DialogDef(typeof(object), typeof(object), string.Empty);
			Assert.IsFalse(dd.IsViewValid);
			Assert.IsFalse(dd.IsModelValid);
			dd = new DialogDef(typeof(MyDialog), typeof(MyDlgModel), string.Empty);
			Assert.IsTrue(dd.IsViewValid);
			Assert.IsTrue(dd.IsModelValid);
		}

		private record struct MyRetVal;
		private class MyDlgModel : DialogModel<MyRetVal>
		{
			protected override void Execute(string? parameter) { }
		}

		private class MyDialog : DialogView
		{

		}
	}
}
