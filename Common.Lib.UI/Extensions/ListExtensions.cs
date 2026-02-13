using System.ComponentModel;
using System.Windows.Data;

namespace Common.Lib.UI.Extensions
{
	public static class ListExtensions
	{
		extension<T>(List<T> list)
		{
			public ICollectionView GetDefaultView => CollectionViewSource.GetDefaultView(list);
		}
	}
}
