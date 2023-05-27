// (c) 2023 Dan Saul
namespace DanSaul.SharedCode.StandardizedEnvironmentVariables
{
	public static class EnvHetzner
	{
		

		public static string HETZNER_API_KEY
		{
			get
			{
				string? str = Environment.GetEnvironmentVariable("HETZNER_API_KEY");
				if (string.IsNullOrWhiteSpace(str))
					throw new Exception("HETZNER_API_KEY not set");
				return str;
			}
		}

		public static string HETZNER_TARGET_ZONE
		{
			get
			{
				string? str = Environment.GetEnvironmentVariable("HETZNER_TARGET_ZONE");
				if (string.IsNullOrWhiteSpace(str))
					throw new Exception("HETZNER_TARGET_ZONE not set");
				return str;
			}
		}

		public static string HETZNER_TARGET_RECORD_NAME
		{
			get
			{
				string? str = Environment.GetEnvironmentVariable("HETZNER_TARGET_RECORD_NAME");
				if (string.IsNullOrWhiteSpace(str))
					throw new Exception("HETZNER_TARGET_RECORD_NAME not set");
				return str;
			}
		}

		public static string HETZNER_TARGET_RECORD_TYPE
		{
			get
			{
				string? str = Environment.GetEnvironmentVariable("HETZNER_TARGET_RECORD_TYPE");
				if (string.IsNullOrWhiteSpace(str))
					throw new Exception("HETZNER_TARGET_RECORD_TYPE not set");
				return str;
			}
		}

	}
}
