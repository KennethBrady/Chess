namespace Common.Lib.UI.Dialogs
{
	public enum AnimationType
	{
		None,
		Fade,
		SlideFromTop,
		SlideFromBottom,
		SlideFromLeft,
		SlideFromRight,
		ExpandFromCenter
	}

	public enum AnimationPhase { Opening, Opened, Closing, Closed };

	public enum CloseIndicatorType { None, ReducedOpacity };
}
