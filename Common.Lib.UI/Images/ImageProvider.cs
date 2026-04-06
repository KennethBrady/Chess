using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Common.Lib.UI.Images
{
	public enum ImageType
	{
		Delete, DeleteFaded, Maximize, Minimize, Restore, Close, Help_Open, Help_Closed,
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
						case ImageType.DeleteFaded: return "delete-faded";
						case ImageType.Maximize: return "maximize-dark";
						case ImageType.Minimize: return "minimize-dark";
						case ImageType.Restore: return "restore-dark";
						case ImageType.Close: return "close-dark";
						case ImageType.Help_Open: return "help_open";
						case ImageType.Help_Closed: return "help_closed";
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
			string resname = $"Common.Lib.UI.Images.{type.FileName}";
			var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resname);
			BitmapImage img =new BitmapImage();
			img.BeginInit();
			img.StreamSource = stream;
			img.CacheOption = BitmapCacheOption.None;
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
