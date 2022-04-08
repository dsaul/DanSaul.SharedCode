using Serilog;

namespace SharedCode
{
	public static class EnvAmazonS3
	{
		public static string? S3_SECRET_KEY_FILE
		{
			get
			{
				string? str = Environment.GetEnvironmentVariable("S3_SECRET_KEY_FILE");
				if (string.IsNullOrWhiteSpace(str))
				{
					Log.Error("S3_SECRET_KEY_FILE empty or missing.");
					return null;
				}
				return str;
			}
		}

		public static string? S3_SECRET_KEY
		{
			get
			{
				string? e = S3_SECRET_KEY_FILE;
				if (e == null)
					return default;
				return File.ReadAllText(e);
			}
		}

		public static string? S3_ACCESS_KEY_FILE
		{
			get
			{
				string? str = Environment.GetEnvironmentVariable("S3_ACCESS_KEY_FILE");
				if (string.IsNullOrWhiteSpace(str))
				{
					Log.Error("S3_ACCESS_KEY_FILE empty or missing.");
					return null;
				}
				return str;
			}
		}

		public static string? S3_ACCESS_KEY
		{
			get
			{
				string? e = S3_ACCESS_KEY_FILE;
				if (e == null)
					return default;
				return File.ReadAllText(e);
			}
		}

		public static string? S3_PBX_SERVICE_URI_FILE
		{
			get
			{
				string? str = Environment.GetEnvironmentVariable("S3_PBX_SERVICE_URI_FILE");
				if (string.IsNullOrWhiteSpace(str))
				{
					Log.Error("S3_PBX_SERVICE_URI_FILE empty or missing.");
					return null;
				}
				return str;
			}
		}
	}
}