using Common.Lib.UI.Images;
using System.Windows.Media.Imaging;

namespace Common.Lib.UI.UnitTests.Images
{
	[TestClass]
	public class ImageProviderTest
	{
		[TestMethod]
		public void Load()
		{
			ImageType[] allTypes = (ImageType[])Enum.GetValues(typeof(ImageType));
			foreach(var it in allTypes)
			{
				var img = ImageProvider.Load(it);
				Assert.IsNotNull(img);
				Assert.IsInstanceOfType(img, typeof(BitmapImage));
			}
		}
	}
}
