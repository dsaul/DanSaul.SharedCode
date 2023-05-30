using System.Net;

namespace DanSaul.SharedCode.Utility
{
    public static class DiscoverWANIP
	{
		public static async Task<IPAddress?> Run()
		{
			using HttpClient client = new HttpClient();
			string str = await client.GetStringAsync("https://api.ipify.org");
			if (!IPAddress.TryParse(str, out IPAddress? _parsed))
				return null;
			return _parsed;
		}
	}
}
