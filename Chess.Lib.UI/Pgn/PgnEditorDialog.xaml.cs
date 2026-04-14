using Common.Lib.UI.Dialogs;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Chess.Lib.UI.Pgn
{
	/// <summary>
	/// Interaction logic for PgnEditorDialog2.xaml
	/// </summary>
	public partial class PgnEditorDialog : DialogView
	{
		public PgnEditorDialog()
		{
			InitializeComponent();
		}
	}

	internal class EmptyTagsConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			if (values.Length == 2 && values[0] is bool ronly && values[1] is bool hasEmpty)
			{
				if (!ronly || hasEmpty) return Visibility.Visible;
				return Visibility.Collapsed;
			}
			return values;
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
