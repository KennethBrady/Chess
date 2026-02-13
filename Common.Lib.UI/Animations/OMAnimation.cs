using Common.Lib.UI.Dialogs;
using System.Windows;
using System.Windows.Media;

namespace Common.Lib.UI.Animations
{
	/// <summary>
	/// Opacity-mask based animations
	/// </summary>
	public class OMAnimation : Animation
	{
		public OMAnimation(AnimationType type)
		{
			Type = type;
		}

		public AnimationType Type { get; init; }

		protected override AnimInfo CreateOpenAnimation(FrameworkElement forElement)
		{
			switch(Type)
			{
				case AnimationType.None: return AnimInfo.Empty;
				case AnimationType.Fade: return CreateFadeIn(forElement);
				case AnimationType.SlideFromLeft: return CreateSlideInLeft(forElement);
				case AnimationType.SlideFromTop: return CreateSlideInFromTop(forElement);
				case AnimationType.SlideFromBottom: return CreateSlideInFromBottom(forElement);
				case AnimationType.SlideFromRight: return CreateSlideInRight(forElement);
				case AnimationType.ExpandFromCenter: return CreateExpandFromCenter(forElement);
				default: return AnimInfo.Empty;
			}
		}

		protected override AnimInfo CreateCloseAnimation(FrameworkElement forElement)
		{
			switch(Type)
			{
				case AnimationType.Fade: return CreateFadeOut(forElement);
				case AnimationType.SlideFromLeft: return CreateSlideOutLeft(forElement);
				case AnimationType.SlideFromTop: return CreateSlideOutFromTop(forElement);
				case AnimationType.SlideFromBottom: return CreateSlideOutFromBottom(forElement);
				case AnimationType.SlideFromRight: return CreateSlideOutRight(forElement);
				case AnimationType.ExpandFromCenter: return CreateExpandOutFromCenter(forElement);
				default: return AnimInfo.Empty;
			}
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
