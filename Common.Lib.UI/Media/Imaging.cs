using System.IO;
using System.Windows.Media.Imaging;

namespace Common.Lib.UI.Media
{
	public enum ImageCodecType
	{
		Unknown = 0,
		Bmp = 1,
		Gif = 2,
		Jpg = 3,
		Png = 4,
		Tiff = 5,
		Exif = 6,
		Wmp = 7,
		Svg = 8
	}

	public static class ImageCodecTypeExtensions
	{
		extension(ImageCodecType ict)
		{
			public BitmapEncoder? Encoder()
			{
				switch (ict)
				{
					case ImageCodecType.Bmp: return new BmpBitmapEncoder();
					case ImageCodecType.Gif: return new GifBitmapEncoder();
					case ImageCodecType.Jpg: return new JpegBitmapEncoder();
					case ImageCodecType.Png: return new PngBitmapEncoder();
					case ImageCodecType.Tiff: return new TiffBitmapEncoder();
					case ImageCodecType.Wmp: return new WmpBitmapEncoder();
				}
				return null;
			}

			public string FileExtension
			{
				get
				{
					switch (ict)
					{
						case ImageCodecType.Bmp: return Imaging.Extensions.BMP;
						case ImageCodecType.Gif: return Imaging.Extensions.GIF;
						case ImageCodecType.Jpg: return Imaging.Extensions.JPG;
						case ImageCodecType.Png: return Imaging.Extensions.PNG;
						case ImageCodecType.Tiff: return Imaging.Extensions.TIFF;
						case ImageCodecType.Exif: return Imaging.Extensions.EXIF;
						case ImageCodecType.Wmp: return Imaging.Extensions.WMP;
						case ImageCodecType.Svg: return Imaging.Extensions.SVG;
					}
					return string.Empty;
				}
			}
		}
	}

	public static class Imaging
	{
		public static class Extensions
		{
			public const string BMP = ".bmp";
			public const string PNG = ".png";
			public const string JPG = ".jpg";
			public const string JPEG = ".jpeg";
			public const string GIF = ".gif";
			public const string EXIF = ".exif";
			public const string TIFF = ".tiff";
			public const string WMP = ".wmp";
			public const string SVG = ".svg";

			private static readonly string[] _all = { BMP, PNG, JPG, JPEG, GIF, EXIF, TIFF, WMP, SVG };
			public static IEnumerable<string> All => _all;

			public static ImageCodecType CodecFor(string? extension)
			{
				switch (extension?.ToLower())
				{
					case BMP: return ImageCodecType.Bmp;
					case PNG: return ImageCodecType.Png;
					case JPEG:
					case JPG: return ImageCodecType.Jpg;
					case GIF: return ImageCodecType.Gif;
					case EXIF: return ImageCodecType.Exif;
					case TIFF: return ImageCodecType.Tiff;
					case WMP: return ImageCodecType.Wmp;
				}
				return ImageCodecType.Unknown;
			}
		}

		public static bool HasCodec(string filePath) => Extensions.All.Contains(Path.GetExtension(filePath));

	}
}
