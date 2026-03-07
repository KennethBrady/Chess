using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Common.Lib.UI.Animations
{
	public static class Storyboards
	{
		public static Storyboard? StoryboardFor(FrameworkElement view, AnimationInfo info)
		{
			bool isOpen= info.Direction == AnimationDirection.Open;
			switch(info.Type)
			{
				case AnimationType.Fade: return isOpen ? CreateFadeIn(view, info.ToDuration) : CreateFadeOut(view, info.ToDuration);
				case AnimationType.SlideHorizontal: return isOpen ? 
						CreateSlideInHorizontally(view, info.ToDuration) : CreateSlideOutHorizontally(view, info.ToDuration);
				case AnimationType.SlideVertical: return isOpen ? 
						CreateSlideInVertically(view, info.ToDuration) : CreateSlideOutVertically(view, info.ToDuration);
				case AnimationType.ExpandFromCenter: return isOpen ?
						CreateExpandIn(view, info.ToDuration) : CreateExpandOut(view, info.ToDuration);
			}
			return null;
		}

		private static Storyboard CreateFadeIn(FrameworkElement view, Duration duration)
		{
			DoubleAnimation a = new DoubleAnimation { Duration = duration, From = 0.0, To = 1.0 };
			Storyboard.SetTarget(a, view);
			Storyboard.SetTargetProperty(a, new PropertyPath("Opacity"));
			Storyboard b = new Storyboard();
			b.Children.Add(a);
			return b;
		}

		private static Storyboard CreateFadeOut(FrameworkElement view, Duration duration)
		{
			DoubleAnimation a = new DoubleAnimation { Duration = duration, From = 1.0, To = 0.0 };
			Storyboard.SetTarget(a, view);
			Storyboard.SetTargetProperty(a, new PropertyPath("Opacity"));
			Storyboard b = new Storyboard();
			b.Children.Add(a);
			return b;
		}

		private static Storyboard CreateSlideInHorizontally(FrameworkElement view, Duration d)
		{
			DoubleAnimation a = new DoubleAnimation { Duration = d, To = view.DesiredSize.Width, From = 0 };
			Storyboard.SetTarget(a, view);
			Storyboard.SetTargetProperty(a, new PropertyPath("Width"));
			Storyboard b = new Storyboard();
			b.Children.Add(a);
			CaptureMinMaxWidth(view, b);
			CaptureCanvasPlacements(view, b, true, false, d, AnimationDirection.Open);
			return b;
		}

		private static Storyboard CreateSlideOutHorizontally(FrameworkElement view, Duration d)
		{
			DoubleAnimation a = new DoubleAnimation { Duration = d, From = view.DesiredSize.Width, To = 0 };
			Storyboard.SetTarget(a, view);
			Storyboard.SetTargetProperty(a, new PropertyPath("Width"));
			Storyboard b = new Storyboard();
			b.Children.Add(a);
			CaptureMinMaxWidth(view, b);
			CaptureCanvasPlacements(view, b, true, false, d, AnimationDirection.Close);
			return b;
		}

		private static Storyboard CreateSlideInVertically(FrameworkElement view, Duration d)
		{
			DoubleAnimation a = new DoubleAnimation { Duration = d, From = 0, To = view.DesiredSize.Height };
			Storyboard.SetTarget(a, view);
			Storyboard.SetTargetProperty(a, new PropertyPath("Height"));
			Storyboard b = new Storyboard();
			b.Children.Add(a);
			CaptureMinMaxWidth(view, b);
			CaptureCanvasPlacements(view, b, false, true, d, AnimationDirection.Open);
			return b;
		}

		private static Storyboard CreateSlideOutVertically(FrameworkElement view, Duration d)
		{
			DoubleAnimation a = new DoubleAnimation { Duration = d, From = view.DesiredSize.Height, To = 0 };
			Storyboard.SetTarget(a, view);
			Storyboard.SetTargetProperty(a, new PropertyPath("Height"));
			Storyboard b = new Storyboard();
			b.Children.Add(a);
			CaptureCanvasPlacements(view, b, false, true, d, AnimationDirection.Close);
			return b;
		}

		private static Storyboard CreateExpandIn(FrameworkElement view, Duration d)
		{
			DoubleAnimation a = new DoubleAnimation { Duration = d, From = 0, To = view.DesiredSize.Width };
			Storyboard.SetTarget(a, view);
			Storyboard.SetTargetProperty(a, new PropertyPath("Width"));
			Storyboard b = new Storyboard();
			b.Children.Add(a);
			a = new DoubleAnimation { Duration = d, From = 0, To = view.DesiredSize.Height };
			Storyboard.SetTarget(a, view);
			Storyboard.SetTargetProperty(a, new PropertyPath("Height"));
			b.Children.Add(a);
			CaptureMinMaxWidth(view, b);
			CaptureCanvasPlacements(view, b, true, true, d, AnimationDirection.Open);
			return b;
		}

		private static Storyboard CreateExpandOut(FrameworkElement view, Duration d)
		{
			DoubleAnimation a = new DoubleAnimation { Duration = d, From = view.DesiredSize.Width, To = 0 };
			Storyboard.SetTarget(a, view);
			Storyboard.SetTargetProperty(a, new PropertyPath("Width"));
			Storyboard b = new Storyboard();
			b.Children.Add(a);
			a = new DoubleAnimation { Duration = d, From = view.DesiredSize.Height, To = 0 };
			Storyboard.SetTarget(a, view);
			Storyboard.SetTargetProperty(a, new PropertyPath("Height"));
			b.Children.Add(a);
			CaptureMinMaxWidth(view, b);
			CaptureCanvasPlacements(view, b, true, true, d, AnimationDirection.Close);
			return b;
		}

		private static void CaptureCanvasPlacements(FrameworkElement view, Storyboard b, bool horizontal, bool vertical, Duration d, AnimationDirection dir)
		{
			if (horizontal && Canvas.GetLeft(view) != double.NaN)
			{
				double l0 = Canvas.GetLeft(view), l1 = l0 + view.DesiredSize.Width / 2;
				DoubleAnimation a = new DoubleAnimation { Duration = d };
				switch(dir)
				{
					case AnimationDirection.Open: a.To = l0; a.From = l1; break;
					case AnimationDirection.Close: a.To = l1; a.From = l0; break;
				}
				Storyboard.SetTarget(a, view);
				Storyboard.SetTargetProperty(a, new PropertyPath("(Canvas.Left)"));
				b.Children.Add(a);
			}
			if (vertical && Canvas.GetTop(view) != double.NaN)
			{
				double t0 = Canvas.GetTop(view), t1 = t0 + view.DesiredSize.Height / 2;
				DoubleAnimation a = new DoubleAnimation { Duration = d };
				switch(dir)
				{
					case AnimationDirection.Open: a.From = t1; a.To = t0; break;
					case AnimationDirection.Close: a.From = t0; a.To = t1; break;
				}
				Storyboard.SetTarget(a, view);
				Storyboard.SetTargetProperty(a, new PropertyPath("(Canvas.Top)"));
				b.Children.Add(a);
			}
		}

		private static void CaptureMinMaxWidth(FrameworkElement view, Storyboard b)
		{
			if (view.MinWidth > 0 || view.MinWidth > 0 || view.MaxHeight < double.PositiveInfinity || view.MaxWidth < double.PositiveInfinity)
			{
				double minw = view.MinWidth, maxw = view.MaxWidth, minh = view.MinHeight, maxh = view.MaxHeight;
				b.Completed += (_, _) =>
				{
					view.MinWidth = minw; view.MaxWidth = maxw; view.MinHeight = minh; view.MaxHeight = maxh;
				};
				view.MinWidth = 0; view.MaxWidth = double.PositiveInfinity; view.MinHeight = 0; view.MaxHeight = double.PositiveInfinity;
			}
		}


	}
}
