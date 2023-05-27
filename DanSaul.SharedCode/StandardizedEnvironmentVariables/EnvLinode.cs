// (c) 2023 Dan Saul
using Serilog;

namespace DanSaul.SharedCode.StandardizedEnvironmentVariables
{
	public static class EnvLinode
	{
		public static string? LINODE_API_KEY_FILE
		{
			get
			{
				string? str = Environment.GetEnvironmentVariable("LINODE_API_KEY_FILE");
				if (string.IsNullOrWhiteSpace(str))
				{
					Log.Error("LINODE_API_KEY_FILE empty or missing.");
					return null;
				}
				return str;
			}
		}

		public static string? LINODE_API_KEY
		{
			get
			{
				string? e = LINODE_API_KEY_FILE;
				if (e == null)
					return default;
				return File.ReadAllText(e).Trim();
			}
		}
	}
}