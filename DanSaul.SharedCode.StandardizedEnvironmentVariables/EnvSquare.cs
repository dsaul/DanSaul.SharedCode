using Serilog;

namespace DanSaul.SharedCode.StandardizedEnvironmentVariables
{
	public class EnvSquare
	{
		public static string? SQUARE_SANDBOX_ACCESS_TOKEN_FILE
		{
			get {
				string? str = System.Environment.GetEnvironmentVariable("SQUARE_SANDBOX_ACCESS_TOKEN_FILE");
				if (string.IsNullOrWhiteSpace(str)) {
					Log.Error("SQUARE_SANDBOX_ACCESS_TOKEN_FILE empty or missing.");
					return null;
				}
				return str;
			}
		}

		public static string? SQUARE_SANDBOX_ACCESS_TOKEN
		{
			get {
				if (string.IsNullOrWhiteSpace(SQUARE_SANDBOX_ACCESS_TOKEN_FILE))
					return null;
				return File.ReadAllText(SQUARE_SANDBOX_ACCESS_TOKEN_FILE);
			}
		}


		public static string? SQUARE_PRODUCTION_ACCESS_TOKEN_FILE
		{
			get {
				string? str = System.Environment.GetEnvironmentVariable("SQUARE_PRODUCTION_ACCESS_TOKEN_FILE");
				if (string.IsNullOrWhiteSpace(str)) {
					Log.Error("SQUARE_PRODUCTION_ACCESS_TOKEN_FILE empty or missing.");
					return null;
				}
				return str;
			}
		}

		public static string? SQUARE_PRODUCTION_ACCESS_TOKEN
		{
			get {
				if (string.IsNullOrWhiteSpace(SQUARE_PRODUCTION_ACCESS_TOKEN_FILE))
					return null;
				return File.ReadAllText(SQUARE_PRODUCTION_ACCESS_TOKEN_FILE);
			}
		}


		public static string? SQUARE_PRODUCTION_APPLICATION_ID_FILE
		{
			get {
				string? str = System.Environment.GetEnvironmentVariable("SQUARE_PRODUCTION_APPLICATION_ID_FILE");
				if (string.IsNullOrWhiteSpace(str)) {
					Log.Error("SQUARE_PRODUCTION_APPLICATION_ID_FILE empty or missing.");
					return null;
				}
				return str;
			}
		}

		public static string? SQUARE_PRODUCTION_APPLICATION_ID
		{
			get {
				if (string.IsNullOrWhiteSpace(SQUARE_PRODUCTION_APPLICATION_ID_FILE))
					return null;
				return File.ReadAllText(SQUARE_PRODUCTION_APPLICATION_ID_FILE);
			}
		}








	}
}
