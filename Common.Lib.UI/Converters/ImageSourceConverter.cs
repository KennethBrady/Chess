using Common.Lib.UI.Media;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Common.Lib.UI.Converters
{
	public class ImageSourceConverter : IValueConverter
	{
		public const string FileMode = "file";
		public const string MemMode = "mem";

		public static BitmapImage? ConvertToImage(string imageFilePath, string mode = FileMode)
		{
			if (!File.Exists(imageFilePath)) return null;
			if (!Imaging.HasCodec(imageFilePath)) return null;
			if (string.IsNullOrEmpty(mode)) mode = "file";
			BitmapImage img = new BitmapImage();
			img.BeginInit();
			switch (mode)
			{
				case FileMode:
					img.UriSource = new Uri(imageFilePath);
					img.CacheOption = BitmapCacheOption.OnLoad;
					img.EndInit();
					break;
				case MemMode:
					{
						using MemoryStream m = new MemoryStream(File.ReadAllBytes(imageFilePath));
						img.StreamSource = m;
						img.CacheOption = BitmapCacheOption.OnLoad;
						img.EndInit();
						break;
					}
				default: throw new ArgumentException($"Unknown mode: {mode}");
			}
			img.Freeze();
			return img;
		}

		public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is string fpath) return ConvertToImage(fpath);
			return null;
		}

		public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
