using Common.Lib.Contracts;
using System.Windows;

namespace Common.Lib.UI.Animations
{
	//Experimental

	public abstract class Animation
	{
		protected record struct AnimInfo(Action<double> Update, Action Cleanup)
		{
			public bool IsEmpty { get; private init; }

			public static readonly AnimInfo Empty = new AnimInfo(Actions<double>.Empty, Actions.Empty) { IsEmpty = true };
		}

		/// <summary>
		/// Get the animation duration.
		/// </summary>
		protected virtual double Duration => 0.5;

		/// <summary>
		/// Get the millisecond delay prior to running the close animation.
		/// </summary>
		protected virtual int CloseDelay => 20;

		protected abstract AnimInfo CreateOpenAnimation(FrameworkElement forElement);

		protected abstract AnimInfo CreateCloseAnimation(FrameworkElement forElement);

		public void RunOpen(FrameworkElement element) => RunAnimation(CreateOpenAnimation(element));

		public void RunClose(FrameworkElement element, Action closeAction) => RunAnimation(CreateCloseAnimation(element), CloseDelay, closeAction);
		

		private async void RunAnimation(AnimInfo info, int delay = 0, Action? after = null)
		{
			if (info.IsEmpty) return;
			if (delay > 0) await Task.Delay(delay);
			double rel = 0;
			DateTime start = DateTime.Now;
			while (rel < 1.0)
			{
				await Task.Delay(10);
				rel = Math.Min(1.0, (DateTime.Now - start).TotalSeconds / Duration);
				info.Update(rel);
			}
			info.Cleanup();
			after?.Invoke();
		}

	}
}
