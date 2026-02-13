using System.Net.NetworkInformation;

namespace Common.Lib.IO
{
	public static class Network
	{
		public static bool IsOnline => IsNetworkAvailable(10000000);

		public static bool IsNetworkAvailable(long minimumSpeed = 10000000)
		{
			if (!NetworkInterface.GetIsNetworkAvailable())
				return false;

			foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
			{
				// discard because of standard reasons
				if ((ni.OperationalStatus != OperationalStatus.Up) ||
						(ni.NetworkInterfaceType == NetworkInterfaceType.Loopback) ||
						(ni.NetworkInterfaceType == NetworkInterfaceType.Tunnel))
					continue;

				// this allow to filter modems, serial, etc.

				// I use 10000000 as a minimum speed for most cases
				if (ni.Speed < minimumSpeed)
					continue;

				// discard virtual cards (virtual box, virtual pc, etc.)
				if ((ni.Description.IndexOf("virtual", StringComparison.OrdinalIgnoreCase) >= 0) ||
						(ni.Name.IndexOf("virtual", StringComparison.OrdinalIgnoreCase) >= 0))
					continue;

				// discard "Microsoft Loopback Adapter", it will not show as NetworkInterfaceType.Loopback but as Ethernet Card.
				if (ni.Description.Equals("Microsoft Loopback Adapter", StringComparison.OrdinalIgnoreCase))
					continue;

				return true;
			}
			return false;
		}

		#region Ping Methods

		public static bool CanPing(string server)
		{
			return CanPing(server, TimeSpan.FromSeconds(5));
		}

		public static bool CanPing(string server, int msTimeout)
		{
			return CanPing(server, TimeSpan.FromMilliseconds(msTimeout));
		}

		public static bool CanPing(string server, TimeSpan timeout)
		{
			Ping ping = new Ping();
			try
			{
				PingReply rep = ping.Send(server);
				return rep.Status == IPStatus.Success;
			}
			catch
			{
				return false;
			}
		}

		public static Task<bool> CanPingAsync(string server)
		{
			return CanPingAsync(server, TimeSpan.FromSeconds(5));
		}

		public static Task<bool> CanPingAsync(string server, int msTimeout)
		{
			return CanPingAsync(server, TimeSpan.FromMilliseconds(msTimeout));
		}

		public static Task<bool> CanPingAsync(string server, TimeSpan timeout)
		{
			return Task<bool>.Factory.StartNew(() => CanPing(server, timeout));
		}

		#endregion
	}
}
