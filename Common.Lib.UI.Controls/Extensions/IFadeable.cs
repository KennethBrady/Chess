using System.Windows;
using System.Windows.Media.Animation;

namespace Common.Lib.UI.Controls.Extensions
{
	public interface IFadeable
	{
		UIElement FadeTarget { get; }
		event EventHandler BeginFade;
		void ResetFade();
		event EventHandler InterruptFade;
	}

	public record struct FadeInfo(TimeSpan Lag, TimeSpan Duration, double StartValue = 0, double EndValue = 1.0)
	{
		public static readonly FadeInfo Empty = new FadeInfo(TimeSpan.Zero, TimeSpan.Zero, 0, 0);
		public bool IsEmpty => Lag.TotalSeconds == 0 && Duration.TotalSeconds == 0;
	}


	public static class Fader
	{
		public static readonly DependencyProperty FadeProperty = DependencyProperty.RegisterAttached("Fade", typeof(FadeInfo),
			typeof(Fader), new PropertyMetadata(FadeInfo.Empty, HandleFadeChanged));

		private static void HandleFadeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is UIElement uie && d is IFadeable fadeable && e.NewValue is FadeInfo fade)
			{
				if (fade.IsEmpty) fadeable.BeginFade -= Fadeable_BeginFade; else fadeable.BeginFade += Fadeable_BeginFade;
			}
		}


		public static FadeInfo GetFade(UIElement uie) => (FadeInfo)uie.GetValue(FadeProperty);

		public static void SetFade(UIElement uie, FadeInfo fade) => uie.SetValue(FadeProperty, fade);

		private static void Fadeable_BeginFade(object? sender, EventArgs e)
		{
			if (sender == null) return;
			UIElement uie = (UIElement)sender;
			FadeInfo info = GetFade(uie);
			if (info.IsEmpty) return;
			new _Fader((IFadeable)uie, info);
		}

		private class _Fader
		{
			internal _Fader(IFadeable fadeable, FadeInfo fadeInfo)
			{
				Fadeable = fadeable;
				FadeInfo = fadeInfo;
				Fadeable.InterruptFade += Fadeable_InterruptFade;
				Animation = new DoubleAnimation(FadeInfo.StartValue, FadeInfo.EndValue, FadeInfo.Duration);
				RunFade();
			}

			private async void RunFade()
			{
				if (FadeInfo.Lag.TotalSeconds > 0) await Task.Delay(FadeInfo.Lag);
				if (IsCancelled) return;
				IsAnimating = true;
				Animation.Completed += Animation_Completed;
				Element.BeginAnimation(UIElement.OpacityProperty, Animation, HandoffBehavior.SnapshotAndReplace);
			}

			private void Animation_Completed(object? sender, EventArgs e)
			{
				if (IsCancelled) return;
				IsAnimating = false;
				Fadeable.ResetFade();
			}

			private void Fadeable_InterruptFade(object? sender, EventArgs e)
			{
				IsCancelled = true;
				Element.BeginAnimation(UIElement.OpacityProperty, null);
			}

			private bool IsCancelled { get; set; }
			private bool IsAnimating { get; set; }
			private FadeInfo FadeInfo { get; init; }
			private UIElement Element => Fadeable.FadeTarget;
			private IFadeable Fadeable { get; set; }
			private DoubleAnimation Animation { get; init; }
		}

	}
}
