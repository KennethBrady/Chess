using Common.Lib.UI.Media;
using System.IO;
using System.Windows;
using System.Windows.Media;
using IMG = Common.Lib.UI.Media.Imaging;

namespace Common.Lib.UI.UnitTests.Imaging
{
	[DeploymentItem("Imaging/Artifact.bin")]
	[DeploymentItem("Imaging/Artifact.png")]
	[DeploymentItem("Imaging/Artifact.bmp")]
	[DeploymentItem("Imaging/Artifact.tiff")]
	[DeploymentItem("Imaging/Artifact.jpg")]
	[DeploymentItem("Imaging/Artifact.gif")]
	[TestClass]
	public class ImagingTest
	{
		private const string DIB = "DeviceIndependentBitmap";

		private void SetupClipboard()
		{
			MemoryStream ms = new MemoryStream();
			using var fs = File.OpenRead("Artifact.bin");	// raw bytes corresponding to DIB image
			fs.CopyTo(ms);
			Clipboard.SetData(DIB, ms);
		}

		[STATestMethod]
		public void PasteFromClipboard()
		{
			SetupClipboard();
			var Import = IMG.ExtractImageFromClipboard();
			Assert.IsFalse(Import.IsEmpty);
			Assert.HasCount(1935718, Import.ImageData);
		}

		private static readonly ImageCodecType[] _supported = { ImageCodecType.Bmp, ImageCodecType.Gif, ImageCodecType.Png, ImageCodecType.Jpg, ImageCodecType.Tiff };
		[STATestMethod]
		public void ConversionsOccur()
		{
			SetupClipboard();
			foreach(ImageCodecType ict in _supported)
			{
				ImageImport imp = IMG.ExtractImageFromClipboard(ict);
				Assert.IsFalse(imp.IsEmpty, ict.ToString());
				Assert.AreEqual(ict, IMG.DetectCodec(imp.ImageData), ict.ToString());
				string fname = $"Artifact{ict.FileExtension}";
				byte[] data = File.ReadAllBytes(fname);
				Assert.IsTrue(data.SequenceEqual(imp.ImageData), ict.ToString());
				ImageSource src = imp.Source;
				Assert.IsNotNull(src);
				Assert.IsFalse(src.IsDefault);
			}
		}

		[TestMethod]
		public void DefaultImageSource()
		{
			var empty = ImageImport.Empty;
			ImageSource src = empty.Source;
			Assert.IsTrue(src.IsDefault);
		}
	}
}
