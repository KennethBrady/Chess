using Common.Lib.Contracts;
using System.Windows;
using System.Windows.Media;

namespace Common.Lib.UI.Animations
{
	/// <summary>
	/// Provides simple animations
	/// </summary>
	/// <remarks>
	/// When a control is registered for animation by setting the Animation attached property to a value other than None, 
	/// the animation will occur just once when that control first changes size.
	/// </remarks>
	public static class Animator
	{
		#region Attached Properties

		public static readonly DependencyProperty AnimationProperty = DependencyProperty.RegisterAttached("Animation", typeof(AnimationType),
			typeof(Animator), new PropertyMetadata(AnimationType.None, HandleAnimationChanged));

		public static readonly DependencyProperty DurationProperty = DependencyProperty.RegisterAttached("Duration", typeof(double),
			typeof(Animator), new PropertyMetadata(0.5));

		public static readonly DependencyProperty NotifierProperty = DependencyProperty.RegisterAttached("Notifier", typeof(Action<AnimationPhase, FrameworkElement>),
			typeof(Animator), new PropertyMetadata(Actions<AnimationPhase, FrameworkElement>.Empty));


		public static AnimationType GetAnimation(FrameworkElement e) => (AnimationType)e.GetValue(AnimationProperty);

		public static void SetAnimation(FrameworkElement e, AnimationType newValue) => e.SetValue(AnimationProperty, newValue);

		public static double GetDuration(FrameworkElement e) => (double)e.GetValue(DurationProperty);

		public static void SetDuration(FrameworkElement e, double newValue) => e.SetValue(DurationProperty, newValue);

		public static Action<AnimationPhase, FrameworkElement> GetNotifier(FrameworkElement e) => (Action<AnimationPhase, FrameworkElement>)e.GetValue(NotifierProperty);

		public static void SetNotifier(FrameworkElement e, Action<AnimationPhase, FrameworkElement> action) => e.SetValue(NotifierProperty, action);

		#endregion

		private static void HandleAnimationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (e.NewValue is AnimationType a && d is FrameworkElement fe)
			{
				if (a == AnimationType.None) fe.SizeChanged -= Element_SizeChanged;
				else
				{
					fe.Visibility = Visibility.Hidden;
					fe.SizeChanged += Element_SizeChanged;
				}
			}
		}

		public static void RunCloseAnimation(FrameworkElement element) =>
			RunCloseAnimation(element, GetAnimation(element), GetDuration(element), GetNotifier(element));

		public static void RunCloseAnimation(FrameworkElement element, AnimationType type, double duration, Action<AnimationPhase, FrameworkElement>? notifier = null)
		{
			AnimInfo? anim = null;
			switch (type)
			{
				case AnimationType.Empty: anim = CreateEmpty(element); break;
				case AnimationType.Fade: anim = CreateFadeOut(element); break;
				case AnimationType.SlideFromLeft: anim = CreateSlideOutLeft(element); break;
				case AnimationType.SlideFromRight: anim = CreateSlideOutRight(element); break;
				case AnimationType.SlideFromTop: anim = CreateSlideOutFromTop(element); break;
				case AnimationType.SlideFromBottom: anim = CreateSlideOutFromBottom(element); break;
				case AnimationType.ExpandFromCenter: anim = CreateExpandOutFromCenter(element); break;
			}
			if (anim.HasValue)
			{
				RunAnimation(anim.Value, false, element);
			}
		}

		private static void Element_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			FrameworkElement fe = (FrameworkElement)sender;
			fe.SizeChanged -= Element_SizeChanged;
			RunOpenAnimation(fe);
		}

		private static async void RunOpenAnimation(FrameworkElement fe)
		{
			AnimInfo? anim = null;
			switch (GetAnimation(fe))
			{
				case AnimationType.Empty: anim = CreateEmpty(fe); break;
				case AnimationType.Fade: anim = CreateFadeIn(fe); break;
				case AnimationType.SlideFromLeft: anim = CreateSlideInLeft(fe); break;
				case AnimationType.SlideFromRight: anim = CreateSlideInRight(fe); break;
				case AnimationType.SlideFromTop: anim = CreateSlideInFromTop(fe); break;
				case AnimationType.SlideFromBottom: anim = CreateSlideInFromBottom(fe); break;
				case AnimationType.ExpandFromCenter: anim = CreateExpandFromCenter(fe); break;
			}
			if (anim.HasValue)
			{
				fe.Visibility = Visibility.Visible;
				RunAnimation(anim.Value, true, fe);
			}
		}

		private static async void RunAnimation(AnimInfo anim, bool isOpenAnimation, FrameworkElement fe)
		{
			double dur = GetDuration(fe);
			if (GetAnimation(fe) == AnimationType.Empty) dur = 0.01;
			AnimationPhase ap = isOpenAnimation ? AnimationPhase.Opening : AnimationPhase.Closing;
			GetNotifier(fe)?.Invoke(ap, fe);
			double rel = 0;
			DateTime start = DateTime.Now;
			while (rel < 1.0)
			{
				await Task.Delay(10);
				rel = Math.Min(1.0, (DateTime.Now - start).TotalSeconds / dur);
				anim.Update(rel);
			}
			ap = isOpenAnimation ? AnimationPhase.Opened : AnimationPhase.Closed;
			GetNotifier(fe)?.Invoke(ap, fe);
			anim.Cleanup();
		}

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
				b.GradientStops[1].Offset = 1-rel;
				b.GradientStops[2].Offset = 1-rel;
			}
			void reset() => fe.OpacityMask = null;
			fe.OpacityMask = b;
			return new AnimInfo(adjust, reset);
		}
	}
}
