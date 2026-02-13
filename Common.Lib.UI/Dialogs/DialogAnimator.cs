using System.Windows;
using System.Windows.Media;

namespace Common.Lib.UI.Dialogs
{
	internal static class DialogAnimator
	{
		internal static void BeginOpenAnimation(DialogView view)
		{
			AnimInfo? anim = null;
			switch (view.Animation)
			{
				case AnimationType.Fade: anim = CreateFadeIn(view); break;
				case AnimationType.SlideFromLeft:anim = CreateSlideInLeft(view); break;
				case AnimationType.SlideFromRight: anim = CreateSlideInRight(view); break;
				case AnimationType.SlideFromTop: anim = CreateSlideInFromTop(view); break;
				case AnimationType.SlideFromBottom: anim = CreateSlideInFromBottom(view); break;
				case AnimationType.ExpandFromCenter: anim = CreateExpandFromCenter(view); break;
			}
			if(anim.HasValue)
			{
				view.Visibility = Visibility.Visible;
				RunAnimation(view, anim.Value);
			}
		}

		internal static void RunCloseAnimation(DialogView view, Action after)
		{
			AnimInfo? anim = null;
			switch(view.Animation)
			{
				case AnimationType.Fade: anim = CreateFadeOut(view); break;
				case AnimationType.SlideFromLeft: anim = CreateSlideOutLeft(view); break;
				case AnimationType.SlideFromRight: anim = CreateSlideOutRight(view);break;
				case AnimationType.SlideFromTop: anim = CreateSlideOutFromTop(view); break;
				case AnimationType.SlideFromBottom: anim = CreateSlideOutFromBottom(view); break;
				case AnimationType.ExpandFromCenter: anim = CreateExpandOutFromCenter(view); break;
			}
			if (anim.HasValue) RunAnimation(view, anim.Value, after, 20);
		}

		private static async void RunAnimation(DialogView view, AnimInfo anim, Action? after = null, int delay = 0)
		{
			// For reasons I don't understand, this brief delay allows the close animation to be fully visible.
			// Without it, the dialogs suddenly disapper.
			if (delay > 0) await Task.Delay(delay);
			double rel = 0;
			DateTime start = DateTime.Now;
			while (rel < 1.0)
			{
				await Task.Delay(10);
				rel = Math.Min(1.0, (DateTime.Now - start).TotalSeconds / view.AnimationDuration);
				anim.Update(rel);
			}
			anim.Cleanup();
			after?.Invoke();
		}

		//TODO: Make these implementations public and re-usable.
		private record struct AnimInfo(Action<double> Update, Action Cleanup);

		private static AnimInfo CreateEmpty(FrameworkElement fe)
		{
			return new AnimInfo(d => { }, () => { });
		}

		private static AnimInfo CreateFadeIn(FrameworkElement fe)
		{
			SolidColorBrush b = new SolidColorBrush(Colors.Transparent);
			fe.OpacityMask = b;
			void update(double rel) => b.Color = Color.FromArgb((byte)(rel * 255), 255, 255, 255);
			void cleanup() => fe.OpacityMask = null;
			return new AnimInfo(update, cleanup);
		}

		private static AnimInfo CreateFadeOut(FrameworkElement fe)
		{
			SolidColorBrush b = new SolidColorBrush(Colors.White);
			fe.OpacityMask = b;
			void update(double rel) => b.Color = Color.FromArgb((byte)((1 - rel) * 255), 255, 255, 255);
			void cleanup() => fe.OpacityMask = null;
			return new AnimInfo(update, cleanup);
		}

		private static AnimInfo CreateSlideInLeft(FrameworkElement fe)
		{
			LinearGradientBrush b = new LinearGradientBrush { StartPoint = new Point(0, 0.5), EndPoint = new Point(1.0, 0.5) };
			b.GradientStops.Add(new GradientStop(Colors.White, 0.0));
			b.GradientStops.Add(new GradientStop(Colors.White, 0.0));
			b.GradientStops.Add(new GradientStop(Colors.Transparent, 0.0));
			b.GradientStops.Add(new GradientStop(Colors.Transparent, 1.0));
			void adjust(double rel)
			{
				b.GradientStops[1].Offset = rel;
				b.GradientStops[2].Offset = rel;
			}
			void cleanup() => fe.OpacityMask = null;
			fe.OpacityMask = b;
			return new AnimInfo(adjust, cleanup);
		}

		private static AnimInfo CreateSlideOutLeft(FrameworkElement fe)
		{
			LinearGradientBrush b = new LinearGradientBrush { StartPoint = new Point(0, 0.5), EndPoint = new Point(1.0, 0.5) };
			b.GradientStops.Add(new GradientStop(Colors.White, 0.0));
			b.GradientStops.Add(new GradientStop(Colors.White, 1.0));
			b.GradientStops.Add(new GradientStop(Colors.Transparent, 1.0));
			b.GradientStops.Add(new GradientStop(Colors.Transparent, 1.0));
			void update(double rel)
			{
				b.GradientStops[1].Offset = 1 - rel;
				b.GradientStops[2].Offset = 1 - rel;
			}
			void cleanup() => fe.OpacityMask = null;
			fe.OpacityMask = b;
			return new AnimInfo(update, cleanup);
		}

		private static AnimInfo CreateSlideInRight(FrameworkElement fe)
		{
			LinearGradientBrush b = new LinearGradientBrush { StartPoint = new Point(0, 0.5), EndPoint = new Point(1.0, 0.5) };
			b.GradientStops.Add(new GradientStop(Colors.Transparent, 0.0));
			b.GradientStops.Add(new GradientStop(Colors.Transparent, 1.0));
			b.GradientStops.Add(new GradientStop(Colors.White, 1.0));
			b.GradientStops.Add(new GradientStop(Colors.White, 1.0));
			void adjust(double rel)
			{
				b.GradientStops[1].Offset = 1 - rel;
				b.GradientStops[2].Offset = 1 - rel;
			}
			void cleanup() => fe.OpacityMask = null;
			fe.OpacityMask = b;
			return new AnimInfo(adjust, cleanup);
		}

		private static AnimInfo CreateSlideOutRight(FrameworkElement fe)
		{
			LinearGradientBrush b = new LinearGradientBrush { StartPoint = new Point(0, 0.5), EndPoint = new Point(1.0, 0.5) };
			b.GradientStops.Add(new GradientStop(Colors.Transparent, 0.0));
			b.GradientStops.Add(new GradientStop(Colors.Transparent, 1.0));
			b.GradientStops.Add(new GradientStop(Colors.White, 1.0));
			b.GradientStops.Add(new GradientStop(Colors.White, 1.0));
			void adjust(double rel)
			{
				b.GradientStops[1].Offset = rel;
				b.GradientStops[2].Offset = rel;
			}
			void cleanup() => fe.OpacityMask = null;
			fe.OpacityMask = b;
			return new AnimInfo(adjust, cleanup);
		}

		private static AnimInfo CreateSlideInFromTop(FrameworkElement fe)
		{
			LinearGradientBrush b = new LinearGradientBrush { StartPoint = new Point(0.5, 0), EndPoint = new Point(0.5, 1.0) };
			b.GradientStops.Add(new GradientStop { Color = Colors.White, Offset = 0.0 });
			b.GradientStops.Add(new GradientStop { Color = Colors.White, Offset = 0.0 });
			b.GradientStops.Add(new GradientStop { Color = Colors.Transparent, Offset = 0.0 });
			b.GradientStops.Add(new GradientStop { Color = Colors.Transparent, Offset = 1.0 });
			void adjust(double rel)
			{
				b.GradientStops[1].Offset = rel;
				b.GradientStops[2].Offset = rel;
			}
			void cleanup() => fe.OpacityMask = null;
			fe.OpacityMask = b;
			return new AnimInfo(adjust, cleanup);
		}

		private static AnimInfo CreateSlideOutFromTop(FrameworkElement fe)
		{
			LinearGradientBrush b = new LinearGradientBrush { StartPoint = new Point(0.5, 0), EndPoint = new Point(0.5, 1.0) };
			b.GradientStops.Add(new GradientStop { Color = Colors.White, Offset = 0.0 });
			b.GradientStops.Add(new GradientStop { Color = Colors.White, Offset = 1.0 });
			b.GradientStops.Add(new GradientStop { Color = Colors.Transparent, Offset = 1.0 });
			b.GradientStops.Add(new GradientStop { Color = Colors.Transparent, Offset = 1.0 });
			void adjust(double rel)
			{
				b.GradientStops[1].Offset = 1 - rel;
				b.GradientStops[2].Offset = 1 - rel;
			}
			void cleanup() => fe.OpacityMask = null;
			fe.OpacityMask = b;
			return new AnimInfo(adjust, cleanup);
		}

		private static AnimInfo CreateSlideInFromBottom(FrameworkElement fe)
		{
			LinearGradientBrush b = new LinearGradientBrush { StartPoint = new Point(0.5, 0), EndPoint = new Point(0.5, 1.0) };
			b.GradientStops.Add(new GradientStop { Color = Colors.Transparent, Offset = 0.0 });
			b.GradientStops.Add(new GradientStop { Color = Colors.Transparent, Offset = 1.0 });
			b.GradientStops.Add(new GradientStop { Color = Colors.White, Offset = 1.0 });
			b.GradientStops.Add(new GradientStop { Color = Colors.White, Offset = 1.0 });
			void adjust(double rel)
			{
				b.GradientStops[1].Offset = 1 - rel;
				b.GradientStops[2].Offset = 1 - rel;
			}
			void cleanup() => fe.OpacityMask = null;
			fe.OpacityMask = b;
			return new AnimInfo(adjust, cleanup);
		}

		private static AnimInfo CreateSlideOutFromBottom(FrameworkElement fe)
		{
			LinearGradientBrush b = new LinearGradientBrush { StartPoint = new Point(0.5, 0), EndPoint = new Point(0.5, 1.0) };
			b.GradientStops.Add(new GradientStop { Color = Colors.Transparent, Offset = 0.0 });
			b.GradientStops.Add(new GradientStop { Color = Colors.Transparent, Offset = 0.0 });
			b.GradientStops.Add(new GradientStop { Color = Colors.White, Offset = 0.0 });
			b.GradientStops.Add(new GradientStop { Color = Colors.White, Offset = 1.0 });
			void adjust(double rel)
			{
				b.GradientStops[1].Offset = rel;
				b.GradientStops[2].Offset = rel;
			}
			void cleanup() => fe.OpacityMask = null;
			fe.OpacityMask = b;
			return new AnimInfo(adjust, cleanup);
		}

		private static AnimInfo CreateExpandFromCenter(FrameworkElement fe)
		{
			RadialGradientBrush b = new RadialGradientBrush
			{
				MappingMode = BrushMappingMode.Absolute,
				Center = new Point(fe.ActualWidth / 2, fe.ActualHeight / 2),
				GradientOrigin = new Point(fe.ActualWidth / 2, fe.ActualHeight / 2),
				RadiusX = fe.ActualWidth / 1.4,
				RadiusY = fe.ActualHeight / 1.4
			};
			b.GradientStops.Add(new GradientStop { Color = Colors.White, Offset = 0 });
			b.GradientStops.Add(new GradientStop { Color = Colors.White, Offset = 0 });
			b.GradientStops.Add(new GradientStop { Color = Colors.Transparent, Offset = 0 });
			b.GradientStops.Add(new GradientStop { Color = Colors.Transparent, Offset = 1 });
			void adjust(double rel)
			{
				b.GradientStops[1].Offset = rel;
				b.GradientStops[2].Offset = rel;
			}
			void reset() => fe.OpacityMask = null;
			fe.OpacityMask = b;
			return new AnimInfo(adjust, reset);
		}
		private static AnimInfo CreateExpandOutFromCenter(FrameworkElement fe)
		{
			RadialGradientBrush b = new RadialGradientBrush
			{
				MappingMode = BrushMappingMode.Absolute,
				Center = new Point(fe.ActualWidth / 2, fe.ActualHeight / 2),
				GradientOrigin = new Point(fe.ActualWidth / 2, fe.ActualHeight / 2),
				RadiusX = fe.ActualWidth / 1.4,
				RadiusY = fe.ActualHeight / 1.4
			};

			b.GradientStops.Add(new GradientStop { Color = Colors.White, Offset = 0 });
			b.GradientStops.Add(new GradientStop { Color = Colors.White, Offset = 1 });
			b.GradientStops.Add(new GradientStop { Color = Colors.Transparent, Offset = 1 });
			b.GradientStops.Add(new GradientStop { Color = Colors.Transparent, Offset = 1 });
			void adjust(double rel)
			{
				b.GradientStops[1].Offset = 1 - rel;
				b.GradientStops[2].Offset = 1 - rel;
			}
			void reset() => fe.OpacityMask = null;
			fe.OpacityMask = b;
			return new AnimInfo(adjust, reset);
		}

	}
}
