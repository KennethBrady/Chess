using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Common.Lib.UI.Media
{
	public record struct ImageImport(ImageCodecType Type, byte[] ImageData, ImageSource Source)
	{
		public static readonly ImageImport Empty = new ImageImport(ImageCodecType.Unknown, Array.Empty<byte>(), Imaging.Default);

		public bool IsEmpty => ImageData == null || ImageData.Length == 0 || Source == null || Source.IsDefault;
	}

	public static class ImagingExtensions
	{
		extension(ImageSource source)
		{
			public bool IsDefault => ReferenceEquals(source, Imaging.Default);
		}
	}

	public static class Imaging
	{ 
		public static readonly ImageSource Default = new BitmapImage();

		/// <summary>
		/// Detect the codec for the beginning bytes a file.
		/// </summary>
		/// <param name="image"></param>
		/// <returns>The codec, based on known file headers.</returns>
		/// <remarks>
		/// EXIF, WMP and SVG are not handled
		/// </remarks>
		public static ImageCodecType DetectCodec(byte[] image)
		{
			if (image.Length < 24) return ImageCodecType.Unknown;
			// Ignoring OS/2 BMP format
			if (image[0] == 0x42 && image[1] == 0x4d && image[6] == 0x00 && image[8] == 0x00 && image[14] == 0x28) return ImageCodecType.Bmp;
			if (image[0] == 0xff && image[1] == 0xd8 && image[6] == 0x4a && image[7] == 0x46 && image[8] == 0x49 && image[9] == 0x46) return ImageCodecType.Jpg;
			if (image[0] == 0x89 && image[1] == 0x50 && image[2] == 0x4e && image[3] == 0x47 && image[4] == 0x0d && image[5] == 0x0a && image[12] == 0x49
				&& image[13] == 0x48 && image[14] == 0x44 && image[15] == 0x52) return ImageCodecType.Png;
			if ((image[0] == 0x49 && image[1] == 0x49) || (image[0] == 0x4d && image[1] == 0x4d)) return ImageCodecType.Tiff;
			if (image[0] == 0x47 && image[1] == 0x49 && image[2] == 0x46 && image[3] == 0x38 && image[4] == 0x39 && image[5] == 0x61) return ImageCodecType.Gif;
			return ImageCodecType.Unknown;
		}

		public static ImageImport ExtractImageFromClipboard(ImageCodecType convertTo = ImageCodecType.Bmp)
		{
			MemoryStream? bmp = ExtractDIBFromClipboard();
			if (bmp == null) return ImageImport.Empty;
			if (convertTo == ImageCodecType.Bmp) return new ImageImport(convertTo, bmp.ToArray(), BitmapFrame.Create(bmp));
			BitmapEncoder? enc = convertTo.Encoder();
			if (enc == null) return ImageImport.Empty;
			enc.Frames.Add(BitmapFrame.Create(bmp));
			using MemoryStream ms = new MemoryStream();
			enc.Save(ms);
			ms.Seek(0, SeekOrigin.Begin);
			return new ImageImport(convertTo, ms.ToArray(), BitmapFrame.Create(ms));
		}

		public static ImageSource FromData(byte[] data, ImageCodecType type)
		{
			using MemoryStream ms = new MemoryStream(data);
			BitmapDecoder? dec = type.Decoder(ms);
			if (dec == null || dec.Frames.Count == 0) return Default;
			return dec.Frames[0];
		}

		private const string DIB = "DeviceIndependentBitmap";
		public static bool HasCodec(string filePath) => FileExtensions.All.Contains(Path.GetExtension(filePath));

		// Modified from https://thomaslevesque.com/2009/02/05/wpf-paste-an-image-from-the-clipboard/
		private static MemoryStream? ExtractDIBFromClipboard()
		{
			MemoryStream? ms = Clipboard.GetData(DIB) as MemoryStream;
			if (ms == null) return null;
			byte[] dibBuffer = new byte[ms.Length];
			ms.Read(dibBuffer, 0, dibBuffer.Length);

			BITMAPINFOHEADER infoHeader =
					FromByteArray<BITMAPINFOHEADER>(dibBuffer);

			int fileHeaderSize = Marshal.SizeOf(typeof(BITMAPFILEHEADER));
			int infoHeaderSize = infoHeader.biSize;
			int fileSize = fileHeaderSize + infoHeader.biSize + infoHeader.biSizeImage;

			BITMAPFILEHEADER fileHeader = new BITMAPFILEHEADER();
			fileHeader.bfType = BITMAPFILEHEADER.BM;
			fileHeader.bfSize = fileSize;
			fileHeader.bfReserved1 = 0;
			fileHeader.bfReserved2 = 0;
			fileHeader.bfOffBits = fileHeaderSize + infoHeaderSize + infoHeader.biClrUsed * 4;

			byte[] fileHeaderBytes =
					ToByteArray<BITMAPFILEHEADER>(fileHeader);

			MemoryStream msBitmap = new MemoryStream();
			msBitmap.Write(fileHeaderBytes, 0, fileHeaderSize);
			msBitmap.Write(dibBuffer, 0, dibBuffer.Length);
			msBitmap.Seek(0, SeekOrigin.Begin);
			return msBitmap;
		}

		private static T FromByteArray<T>(byte[] bytes) where T : struct
		{
			IntPtr ptr = IntPtr.Zero;
			try
			{
				int size = Marshal.SizeOf(typeof(T));
				ptr = Marshal.AllocHGlobal(size);
				Marshal.Copy(bytes, 0, ptr, size);
				object? obj = Marshal.PtrToStructure(ptr, typeof(T));
				return obj is T t ? t : default;
			}
			finally
			{
				if (ptr != IntPtr.Zero)
					Marshal.FreeHGlobal(ptr);
			}
		}

		private static byte[] ToByteArray<T>(T obj) where T : struct
		{
			IntPtr ptr = IntPtr.Zero;
			try
			{
				int size = Marshal.SizeOf(typeof(T));
				ptr = Marshal.AllocHGlobal(size);
				Marshal.StructureToPtr(obj, ptr, true);
				byte[] bytes = new byte[size];
				Marshal.Copy(ptr, bytes, 0, size);
				return bytes;
			}
			finally
			{
				if (ptr != IntPtr.Zero)
					Marshal.FreeHGlobal(ptr);
			}
		}

		#region FILEHEADER structures

		[StructLayout(LayoutKind.Sequential, Pack = 2)]
		private struct BITMAPFILEHEADER
		{
			public static readonly short BM = 0x4d42; // BM

			public short bfType;
			public int bfSize;
			public short bfReserved1;
			public short bfReserved2;
			public int bfOffBits;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct BITMAPINFOHEADER
		{
			public int biSize;
			public int biWidth;
			public int biHeight;
			public short biPlanes;
			public short biBitCount;
			public int biCompression;
			public int biSizeImage;
			public int biXPelsPerMeter;
			public int biYPelsPerMeter;
			public int biClrUsed;
			public int biClrImportant;
		}

		#endregion

		#region FileExtensions

		public static class FileExtensions
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

		#endregion
	}
}
