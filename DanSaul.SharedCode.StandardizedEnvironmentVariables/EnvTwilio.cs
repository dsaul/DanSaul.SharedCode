using Serilog;

namespace DanSaul.SharedCode.StandardizedEnvironmentVariables
{
	public static class EnvTwilio
	{
		public static string? TWILIO_AUTH_TOKEN_FILE
		{
			get {
				string? str = Environment.GetEnvironmentVariable("TWILIO_AUTH_TOKEN_FILE");
				if (string.IsNullOrWhiteSpace(str)) {
					Log.Error("TWILIO_AUTH_TOKEN_FILE empty or missing.");
					return null;
				}
				return str;
			}
		}


		public static string? TWILIO_AUTH_TOKEN
		{
			get {
				if (string.IsNullOrWhiteSpace(TWILIO_AUTH_TOKEN_FILE))
					return null;
				string? str = File.ReadAllText(TWILIO_AUTH_TOKEN_FILE);
				if (!string.IsNullOrWhiteSpace(str)) {
					return str;
				}
				return null;
			}
		}


		public static string? TWILIO_ACCOUNT_SID_FILE
		{
			get {
				string? str = Environment.GetEnvironmentVariable("TWILIO_ACCOUNT_SID_FILE");
				if (string.IsNullOrWhiteSpace(str)) {
					Log.Error("TWILIO_ACCOUNT_SID_FILE empty or missing.");
					return null;
				}
				return str;
			}
		}

		public static string? TWILIO_ACCOUNT_SID
		{
			get {
				if (string.IsNullOrWhiteSpace(TWILIO_ACCOUNT_SID_FILE))
					return null;
				string? str = File.ReadAllText(TWILIO_ACCOUNT_SID_FILE);
				if (!string.IsNullOrWhiteSpace(str)) {
					return str;
				}
				return null;
			}
		}
	}
}
