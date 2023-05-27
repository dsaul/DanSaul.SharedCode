// (c) 2023 Dan Saul
using Serilog;

namespace DanSaul.SharedCode.StandardizedEnvironmentVariables
{
	public static class EnvVAPID
	{
		public static string? VAPID_SUBJECT_PATH
		{
			get
			{
				string? str = Environment.GetEnvironmentVariable("VAPID_SUBJECT_PATH");
				if (string.IsNullOrWhiteSpace(str))
				{
					Log.Error("VAPID_SUBJECT_PATH empty or missing.");
					return null;
				}
				return str;
			}
		}
		public static string VAPID_SUBJECT
		{
			get
			{
				string? path = VAPID_SUBJECT_PATH;
				if (string.IsNullOrWhiteSpace(path))
					throw new InvalidOperationException();
				return File.ReadAllText(path);
			}
		}
		public static string? VAPID_PRIVATE_KEY_PATH
		{
			get
			{
				string? str = Environment.GetEnvironmentVariable("VAPID_PRIVATE_KEY_PATH");
				if (string.IsNullOrWhiteSpace(str))
				{
					Log.Error("VAPID_PRIVATE_KEY_PATH empty or missing.");
					return null;
				}
				return str;
			}
		}
		public static string VAPID_PRIVATE_KEY
		{
			get
			{
				string? path = VAPID_PRIVATE_KEY_PATH;
				if (string.IsNullOrWhiteSpace(path))
					throw new InvalidOperationException();
				return File.ReadAllText(path);
			}
		}

		public static string? VAPID_PUBLIC_KEY_PATH
		{
			get
			{
				string? str = Environment.GetEnvironmentVariable("VAPID_PUBLIC_KEY_PATH");
				if (string.IsNullOrWhiteSpace(str))
				{
					Log.Error("VAPID_PUBLIC_KEY_PATH empty or missing.");
					return null;
				}
				return str;
			}
		}
		public static string VAPID_PUBLIC_KEY
		{
			get
			{
				string? path = VAPID_PUBLIC_KEY_PATH;
				if (string.IsNullOrWhiteSpace(path))
					throw new InvalidOperationException();
				return File.ReadAllText(path);
			}
		}
	}
}
