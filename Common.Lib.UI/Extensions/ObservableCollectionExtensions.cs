using System.Collections.ObjectModel;

namespace Common.Lib.UI.Extensions
{
	public static class ObservableCollectionExtensions
	{
		extension<T>(ObservableCollection<T> collection)
		{
			public void InsertSorted(T item, Comparison<T> comparison)
			{
				if (collection.Count == 0) collection.Add(item);
				else
				{
					for (int i = 0; i < collection.Count; ++i)
					{
						int result = comparison(collection[i], item);
						if (result >= 1)
						{
							collection.Insert(i, item);
							return;
						}
					}
					collection.Add(item);
				}
			}
		}
	}
}
