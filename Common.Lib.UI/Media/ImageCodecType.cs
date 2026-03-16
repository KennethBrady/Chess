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

			public BitmapDecoder? Decoder(Stream imageData)
			{
				switch (ict)
				{
					case ImageCodecType.Bmp: return new BmpBitmapDecoder(imageData, BitmapCreateOptions.None, BitmapCacheOption.None);
					case ImageCodecType.Gif: return new GifBitmapDecoder(imageData, BitmapCreateOptions.None, BitmapCacheOption.None);
					case ImageCodecType.Jpg: return new JpegBitmapDecoder(imageData, BitmapCreateOptions.None, BitmapCacheOption.None);
					case ImageCodecType.Png: return new PngBitmapDecoder(imageData, BitmapCreateOptions.None, BitmapCacheOption.None);
					case ImageCodecType.Tiff: return new TiffBitmapDecoder(imageData, BitmapCreateOptions.None, BitmapCacheOption.None);
					case ImageCodecType.Wmp: return new WmpBitmapDecoder(imageData, BitmapCreateOptions.None, BitmapCacheOption.None);
				}
				return null;
			}

			public string FileExtension
			{
				get
				{
					switch (ict)
					{
						case ImageCodecType.Bmp: return Imaging.FileExtensions.BMP;
						case ImageCodecType.Gif: return Imaging.FileExtensions.GIF;
						case ImageCodecType.Jpg: return Imaging.FileExtensions.JPG;
						case ImageCodecType.Png: return Imaging.FileExtensions.PNG;
						case ImageCodecType.Tiff: return Imaging.FileExtensions.TIFF;
						case ImageCodecType.Exif: return Imaging.FileExtensions.EXIF;
						case ImageCodecType.Wmp: return Imaging.FileExtensions.WMP;
						case ImageCodecType.Svg: return Imaging.FileExtensions.SVG;
					}
					return string.Empty;
				}
			}
		}
	}
}
