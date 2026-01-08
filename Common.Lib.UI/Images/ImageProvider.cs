using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Common.Lib.UI.Images
{
	public enum ImageType
	{
		Delete, DeleteFaded, Maximize, Minimize, Restore, Close
	}

	internal static class ImageTypeEx
	{
		extension(ImageType it)
		{
			internal string Name
			{
				get
				{
					switch (it)
					{
						case ImageType.Delete: return "delete";
						case ImageType.DeleteFaded: return "delte-faded";
						case ImageType.Maximize: return "maximize-dark";
						case ImageType.Minimize: return "minimize-dark";
						case ImageType.Restore: return "restore-dark";
						case ImageType.Close: return "close-dark";
						default: throw new UnreachableException();
					}
				}
			}

			internal string FileName => it.Name + ".png";
		}
	}

	/// <summary>
	/// Supply common images
	/// </summary>
	/// <remarks>
	/// Ideally, Images.xaml could be shared via merged dictionaries, but I've not been able to get this working
	/// </remarks>
	public static class ImageProvider
	{
		public static BitmapImage Load(ImageType type)
		{
			string fpath = Path.Combine(Environment.CurrentDirectory, $@"Images\{type.FileName}");
			fpath = Path.GetFullPath(fpath);
			bool exists = File.Exists(fpath);
			BitmapImage img = new BitmapImage();
			img.BeginInit();
			img.UriSource = new Uri(fpath, UriKind.Absolute);
			img.CacheOption = BitmapCacheOption.OnLoad;
			img.EndInit();
			img.Freeze();
			return img;
		}

		public static readonly DependencyProperty ImageTypeProperty = DependencyProperty.RegisterAttached("Image", typeof(ImageType?),
			typeof(ImageProvider), new PropertyMetadata(null, HandleImageTypeChanged));

		private static void HandleImageTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is Image image && e.NewValue is ImageType it) image.Source = Load(it);
		}

		public static ImageType? GetImageType(Image image) => (ImageType?)image.GetValue(ImageTypeProperty);

		public static void SetImageType(Image image, ImageType? type) => image.SetValue(ImageTypeProperty, type);
	}
}
