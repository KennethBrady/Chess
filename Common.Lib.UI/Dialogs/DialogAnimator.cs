using Common.Lib.UI.Animations;
using System.Windows;
using System.Windows.Media.Animation;

namespace Common.Lib.UI.Dialogs
{
	internal static class DialogAnimator
	{
		internal static void RunOpenAnimation(DialogView view)
		{
			Storyboard? sb = Storyboards.StoryboardFor(view, new AnimationInfo(view.Animation, AnimationDirection.Open, view.AnimationDuration));
			if (sb != null)
			{
				view.Visibility = Visibility.Visible;
				view.BeginStoryboard(sb);
			}
		}

		internal static void RunCloseAnimation(DialogView view, Action after)
		{
			Storyboard? sb = Storyboards.StoryboardFor(view, new AnimationInfo(view.Animation, AnimationDirection.Close, view.AnimationDuration));
			if (sb != null)
			{
				sb.Completed += (_, _) => after();
				view.BeginStoryboard(sb);
			}
		}
	}
}
