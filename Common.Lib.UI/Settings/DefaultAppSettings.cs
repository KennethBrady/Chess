using System.Configuration;

namespace Common.Lib.UI.Settings
{
	internal class DefaultAppSettings : ApplicationSettingsBase { }

	internal static class AppSettingsEx
	{
		extension(ApplicationSettingsBase asb)
		{
			//NOTE: leave public to prevent CS0570 in unit tests
			//See https://developercommunity.visualstudio.com/t/internal-extension-property-causes-CS0/11000258?ref=native&refTime=1768161556485&refUserId=2c86525d-7eb9-414e-b2d8-4a020136cb19
			public bool IsDefault => asb is DefaultAppSettings;
		}
	}
}
