using Common.Lib.UI.Images;
using System.Globalization;
using System.Windows.Data;

namespace Common.Lib.UI.Converters
{
	public class ImageTypeConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is ImageType it) return ImageProvider.Load(it);
			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
