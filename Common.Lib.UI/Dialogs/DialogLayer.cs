using Common.Lib.UI.Animations;
using System.Windows;
using System.Windows.Controls;

namespace Common.Lib.UI.Dialogs
{
	public class DialogLayer : Canvas
	{
		private Stack<DialogView> _openDialogs = new();

		internal Task<IDialogResult<T>> PushDialog<T>(DialogView dialog, IDialogModelEx<T> model)
		{
			Canvas.SetZIndex(dialog, 1 + _openDialogs.Count);
			Animator.SetAnimation(dialog, dialog.Animation);
			Animator.SetDuration(dialog, 0.5);
			_openDialogs.Push(dialog);
			dialog.DataContext = model;
			TaskCompletionSource<IDialogResult<T>> src = new TaskCompletionSource<IDialogResult<T>>();
			IDialogResult<T>? dlgResult = null;
			void notify(AnimationPhase phase, FrameworkElement d)
			{
				switch(phase)
				{
					case AnimationPhase.Opening:
						Size s = new Size(d.ActualWidth, d.ActualHeight);
						PositionDialog((DialogView)d, s);
						break;
					case AnimationPhase.Closed:
						Children.Remove(dialog);
						if (dlgResult != null) src.SetResult(dlgResult);
						break;
				}
			}
			Animator.SetNotifier(dialog, notify);
			model.Closing = (result) =>
			{
				dlgResult = result;
				var dlg = _openDialogs.Pop();
				Animator.RunCloseAnimation(dlg);
			};
			Children.Add(dialog);
			return src.Task;
		}

		private void PositionDialog(DialogView dialog, Size size)
		{
			Point center = new Point(ActualWidth / 2, ActualHeight / 2);
			double ho2 = size.Height / 2, wo2 = size.Width / 2, x = 0, y = 0;
			switch (dialog.Placement)
			{
				case DialogPlacement.TopLeft: break;
				case DialogPlacement.CenterLeft: y = center.Y - size.Height / 2; break;
				case DialogPlacement.BottomLeft: y = ActualHeight - size.Height; break;
				case DialogPlacement.TopCenter: x = center.X - wo2; break;
				case DialogPlacement.Center: x = center.X - wo2; y = center.Y - ho2; break;
				case DialogPlacement.BottomCenter: x = center.X - wo2; y = ActualHeight - size.Height; break;
				case DialogPlacement.TopRight: x = ActualWidth - size.Width; break;
				case DialogPlacement.CenterRight: x = ActualWidth - size.Width; y = center.Y - ho2; break;
				case DialogPlacement.BottomRight: x = ActualWidth - size.Width; y = ActualHeight - size.Height; break;
				case DialogPlacement.Custom: x = dialog.CustomPlacement.X; y= dialog.CustomPlacement.Y; break;

			}
			dialog.SetInitialPosition(new Point(x, y));
		}
	}
}
