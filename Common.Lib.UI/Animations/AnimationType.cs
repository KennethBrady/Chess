using System.Windows;

namespace Common.Lib.UI.Animations
{
	public enum AnimationType
	{
		None,
		Fade,
		SlideVertical,
		SlideHorizontal,
		ExpandFromCenter
	}

	public enum AnimationPhase { Opening, Opened, Closing, Closed };

	public enum AnimationDirection { Open, Close };

	public record struct AnimationInfo(AnimationType Type, AnimationDirection Direction, double Duration)
	{
		internal Duration ToDuration => new Duration(TimeSpan.FromSeconds(Duration));
	}


}
