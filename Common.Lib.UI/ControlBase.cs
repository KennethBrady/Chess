using System.Windows;
using System.Windows.Controls;

namespace Common.Lib.UI
{
	public abstract class ControlBase : Control
	{
		public bool IsTemplateApplied { get; private set; }

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			IsTemplateApplied = true;
			UseTemplate();
			SizeChanged += (o, e) => OnSizeChanged(e.PreviousSize, e.NewSize);
		}

		protected abstract void UseTemplate();

		protected virtual void OnSizeChanged(Size oldSize, Size newSize) { }
	}
}
