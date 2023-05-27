// (c) 2023 Dan Saul
using Serilog;

namespace DanSaul.SharedCode.StandardizedEnvironmentVariables
{
    public class EnvOnCallResponder
    {
		public static string? ON_CALL_RESPONDER_SMS_FROM_E164_FILE
		{
			get {
				string? str = Environment.GetEnvironmentVariable("ON_CALL_RESPONDER_SMS_FROM_E164_FILE");
				if (string.IsNullOrWhiteSpace(str)) {
					Log.Error("ON_CALL_RESPONDER_SMS_FROM_E164_FILE empty or missing.");
					return null;
				}
				return str;
			}
		}

		public static string? ON_CALL_RESPONDER_SMS_FROM_E164
		{
			get {
				string? path = ON_CALL_RESPONDER_SMS_FROM_E164_FILE;
				if (string.IsNullOrWhiteSpace(path))
					return null;
				return File.ReadAllText(path);
			}
		}

		public static string? ON_CALL_RESPONDER_SUPPORT_CONTACT_LOCATION_FILE
		{
			get {
				string? str = Environment.GetEnvironmentVariable("ON_CALL_RESPONDER_SUPPORT_CONTACT_LOCATION_FILE");
				if (string.IsNullOrWhiteSpace(str)) {
					Log.Error("ON_CALL_RESPONDER_SUPPORT_CONTACT_LOCATION_FILE empty or missing.");
					return null;
				}
				return str;
			}
		}

		public static string? ON_CALL_RESPONDER_SUPPORT_CONTACT_LOCATION
		{
			get {
				string? path = ON_CALL_RESPONDER_SUPPORT_CONTACT_LOCATION_FILE;
				if (string.IsNullOrWhiteSpace(path))
					return null;
				return File.ReadAllText(path);
			}
		}

		public static string? ON_CALL_RESPONDER_MESSAGE_ACCESS_BASE_URI_FILE
		{
			get {
				string? str = Environment.GetEnvironmentVariable("ON_CALL_RESPONDER_MESSAGE_ACCESS_BASE_URI_FILE");
				if (string.IsNullOrWhiteSpace(str)) {
					Log.Error("ON_CALL_RESPONDER_MESSAGE_ACCESS_BASE_URI_FILE empty or missing.");
					return null;
				}
				return str;
			}
		}

		public static string? ON_CALL_RESPONDER_MESSAGE_ACCESS_BASE_URI
		{
			get {
				string? path = ON_CALL_RESPONDER_MESSAGE_ACCESS_BASE_URI_FILE;
				if (string.IsNullOrWhiteSpace(path))
					return null;
				return File.ReadAllText(path);
			}
		}

		public static string? ON_CALL_RESPONDER_NOTIFICATION_EMAIL_FROM_ADDRESS_FILE
		{
			get {
				string? str = Environment.GetEnvironmentVariable("ON_CALL_RESPONDER_NOTIFICATION_EMAIL_FROM_ADDRESS_FILE");
				if (string.IsNullOrWhiteSpace(str)) {
					Log.Error("ON_CALL_RESPONDER_NOTIFICATION_EMAIL_FROM_ADDRESS_FILE empty or missing.");
					return null;
				}
				return str;
			}
		}

		public static string? ON_CALL_RESPONDER_NOTIFICATION_EMAIL_FROM_ADDRESS
		{
			get {
				string? path = ON_CALL_RESPONDER_NOTIFICATION_EMAIL_FROM_ADDRESS_FILE;
				if (string.IsNullOrWhiteSpace(path))
					return null;
				return File.ReadAllText(path);
			}
		}


	}
}
