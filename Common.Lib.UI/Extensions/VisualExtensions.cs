using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Common.Lib.UI.Extensions
{
	public static class VisualExtensions
	{
		private static IEnumerable<T> VisualChildrenOf<T>(Visual visual)
		{
			int nChildren = VisualTreeHelper.GetChildrenCount(visual);
			for (int i = 0; i < nChildren; ++i)
			{
				Visual v = (Visual)VisualTreeHelper.GetChild(visual, i);
				switch (v)
				{
					case T t: yield return t; break;
					case null: continue;
					default: foreach (var t in VisualChildrenOf<T>(v)) yield return t; break;
				}
			}
		}

		extension<T>(DependencyObject o) where T : Visual
		{
			public T? GetVisualParent()
			{
				while (o != null && o is not T)
				{
					o = VisualTreeHelper.GetParent(o);
				}
				return o as T;
			}
		}

		extension<T>(Visual visual) where T : Visual
		{
			public T? FindParent(Predicate<T> test)
			{
				var parent = VisualTreeHelper.GetParent(visual);
				while (parent != null)
				{
					if (parent is T t && test(t)) return t;
					parent = VisualTreeHelper.GetParent(parent);
				}
				return default;
			}

			public T? FindParent()
			{
				var parent = VisualTreeHelper.GetParent(visual);
				while (parent != null)
				{
					if (parent is T t) return t;
					parent = VisualTreeHelper.GetParent(parent);
				}
				return default;
			}


			public IEnumerable<T> FindVisualChildren() => VisualChildrenOf<T>(visual);

			public  T? FindLogicalChild()
			{
				foreach (Visual o in LogicalTreeHelper.GetChildren(visual))
				{
					if (o is T t) return t;
					T? r = o.FindLogicalChild<T>();
					if (r != null) return r;
				}
				return null;
			}

		}

		extension<T>(ItemsControl items)
		{
			public IEnumerable<T> FindVisualChildren() => VisualChildrenOf<T>(items);
			
		}
	}
}
