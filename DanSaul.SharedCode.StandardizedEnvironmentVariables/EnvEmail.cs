using Serilog;

namespace DanSaul.SharedCode.StandardizedEnvironmentVariables
{
	public static class EnvEmail
	{
		public static string? SMTP_HOST_FQDN_FILE
		{
			get {
				string? str = Environment.GetEnvironmentVariable("SMTP_HOST_FQDN_FILE");
				if (string.IsNullOrWhiteSpace(str)) {
					Log.Error("SMTP_HOST_FQDN_FILE empty or missing.");
					return null;
				}
				return str;
			}
		}

		public static string? SMTP_HOST_FQDN
		{
			get {
				string? path = SMTP_HOST_FQDN_FILE;
				if (string.IsNullOrWhiteSpace(path))
					return null;
				return File.ReadAllText(path);
			}
		}


		public static string? SMTP_HOST_PORT_FILE
		{
			get {
				string? str = Environment.GetEnvironmentVariable("SMTP_HOST_PORT_FILE");
				if (string.IsNullOrWhiteSpace(str)) {
					Log.Error("SMTP_HOST_PORT_FILE empty or missing.");
					return null;
				}
				return str;
			}
		}

		public static int? SMTP_HOST_PORT
		{
			get {
				string? path = SMTP_HOST_PORT_FILE;
				if (string.IsNullOrWhiteSpace(path)) {
					Log.Error("SMTP_HOST_PORT_FILE empty or missing.");
					return null;
				}

				string str = File.ReadAllText(path);
				if (string.IsNullOrWhiteSpace(str)) {
					Log.Error("SMTP_HOST_PORT_FILE empty or missing.");
					return null;
				}
				try {
					return int.Parse(str);
				}
				catch (Exception e) {
					Log.Error(e, "Unable to parse {OriginalString}", str);
					return null;
				}
			}
		}


		public static string? SMTP_USERNAME_FILE
		{
			get {
				string? str = Environment.GetEnvironmentVariable("SMTP_USERNAME_FILE");
				if (string.IsNullOrWhiteSpace(str)) {
					Log.Error("SMTP_USERNAME_FILE empty or missing.");
					return null;
				}
				return str;
			}
		}

		public static string? SMTP_USERNAME
		{
			get {
				if (string.IsNullOrWhiteSpace(SMTP_USERNAME_FILE))
					return null;
				if (!File.Exists(SMTP_USERNAME_FILE))
					return null;

				return File.ReadAllText(SMTP_USERNAME_FILE);
			}
		}

		public static string? SMTP_PASSWORD_FILE
		{
			get {
				string? str = Environment.GetEnvironmentVariable("SMTP_PASSWORD_FILE");
				if (string.IsNullOrWhiteSpace(str)) {
					Log.Error("SMTP_PASSWORD_FILE empty or missing.");
					return null;
				}
				return str;
			}
		}

		public static string? SMTP_PASSWORD
		{
			get {
				if (string.IsNullOrWhiteSpace(SMTP_PASSWORD_FILE))
					return null;
				if (!File.Exists(SMTP_PASSWORD_FILE))
					return null;

				return File.ReadAllText(SMTP_PASSWORD_FILE);
			}
		}
	}
}
