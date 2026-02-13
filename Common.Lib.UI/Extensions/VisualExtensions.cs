using System.Windows.Media;

namespace Common.Lib.UI.Extensions
{
	public static class VisualExtensions
	{
		extension(Visual visual)
		{
			public Visual? FindParent(Predicate<Visual> test)
			{
				var parent = VisualTreeHelper.GetParent(visual);
				while (parent != null)
				{
					if (parent is Visual v && test(v)) return v;
					parent = VisualTreeHelper.GetParent(parent);
				}
				return null;
			}


		}
	}
}
