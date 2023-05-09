using Serilog;
using SharpPcap.LibPcap;
using System.Net;

namespace DanSaul.SharedCode.Extensions.SharpPCap
{
	public static class LibPcapLiveDevice_Extensions
	{
		public static LibPcapLiveDevice? DeviceForIpAddress(this LibPcapLiveDeviceList deviceList, IPAddress? ip)
		{
			if (ip == null)
				return null;

			foreach (var dev in LibPcapLiveDeviceList.Instance)
			{
				var addrs = dev.Addresses;
				foreach (var entry in addrs)
				{
					Sockaddr sa = entry.Addr;
					IPAddress? ipa = sa.ipAddress;
					if (ipa == null)
						continue;



					if (ipa.ToString() == ip.ToString())
					{
						Log.Information("Using interface \"{desc}\" IP {ipa}", dev.Description, ipa);
						return dev;
					}
					else
					{
						Log.Information("Skipped Interface \"{desc}\" IP {ipa}", dev.Description, ipa);
					}
				}
			}
			return null;
		}
	}
}