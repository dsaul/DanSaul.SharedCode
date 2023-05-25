using Serilog;

namespace DanSaul.SharedCode.StandardizedEnvironmentVariables
{
	public static class EnvAmazonS3
	{
		public static string S3_SECRET_KEY_FILE
		{
			get
			{
				string? str = Environment.GetEnvironmentVariable("S3_SECRET_KEY_FILE");
				if (string.IsNullOrWhiteSpace(str))
					throw new InvalidOperationException("S3_SECRET_KEY_FILE empty or missing.");
				return str;
			}
		}

		public static string S3_SECRET_KEY
		{
			get
			{
				string? path = S3_SECRET_KEY_FILE;
				if (string.IsNullOrWhiteSpace(path))
					throw new InvalidOperationException("S3_SECRET_KEY_FILE empty or missing.");
				return File.ReadAllText(path);
			}
		}

		public static string S3_ACCESS_KEY_FILE
		{
			get
			{
				string? str = Environment.GetEnvironmentVariable("S3_ACCESS_KEY_FILE");
				if (string.IsNullOrWhiteSpace(str))
					throw new InvalidOperationException("S3_ACCESS_KEY_FILE empty or missing.");
				return str;
			}
		}

		public static string S3_ACCESS_KEY
		{
			get
			{
				string? path = S3_ACCESS_KEY_FILE;
				if (string.IsNullOrWhiteSpace(path))
					throw new InvalidOperationException();
				return File.ReadAllText(path);
			}
		}

		public static bool S3_FORCE_PATH_STYLE
		{
			get
			{
				string? str = Environment.GetEnvironmentVariable("S3_FORCE_PATH_STYLE");
				if (string.IsNullOrWhiteSpace(str))
					throw new InvalidOperationException("S3_FORCE_PATH_STYLE empty or missing.");
				if (bool.TryParse(str, out bool parsed))
					return parsed;
				throw new InvalidOperationException("S3_FORCE_PATH_STYLE unable to parse.");
			}
		}


		public static string S3_SERVICE_URI_FILE
		{
			get
			{
				string? str = Environment.GetEnvironmentVariable("S3_SERVICE_URI_FILE");
				if (string.IsNullOrWhiteSpace(str))
					throw new InvalidOperationException("S3_SERVICE_URI_FILE empty or missing.");
				return str;
			}
		}

		public static string S3_SERVICE_URI
		{
			get
			{
				string? path = S3_SERVICE_URI_FILE;
				if (string.IsNullOrWhiteSpace(path))
					throw new InvalidOperationException("S3_SERVICE_URI_FILE empty or missing.");
				return File.ReadAllText(path);
			}
		}


























		[Obsolete]
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




		[Obsolete]
		public static string? S3_DISPATCH_PULSE_SERVICE_URI_FILE
		{
			get
			{
				string? str = Environment.GetEnvironmentVariable("S3_DISPATCH_PULSE_SERVICE_URI_FILE");
				if (string.IsNullOrWhiteSpace(str))
				{
					Log.Error("S3_DISPATCH_PULSE_SERVICE_URI_FILE empty or missing.");
					return null;
				}
				return str;
			}
		}
		[Obsolete]
		public static string? S3_DISPATCH_PULSE_SERVICE_URI
		{
			get
			{
				string? path = S3_DISPATCH_PULSE_SERVICE_URI_FILE;
				if (string.IsNullOrWhiteSpace(path))
					return null;
				return File.ReadAllText(path);
			}
		}


		[Obsolete]
		public static string? S3_PBX_ACCESS_KEY_FILE
		{
			get
			{
				string? str = Environment.GetEnvironmentVariable("S3_PBX_ACCESS_KEY_FILE");
				if (string.IsNullOrWhiteSpace(str))
				{
					Log.Error("S3_PBX_ACCESS_KEY_FILE empty or missing.");
					return null;
				}
				return str;
			}
		}
		[Obsolete]
		public static string? S3_PBX_ACCESS_KEY
		{
			get
			{
				string? e = S3_PBX_ACCESS_KEY_FILE;
				if (e == null)
					return default;
				return File.ReadAllText(e);
			}
		}
		[Obsolete]
		public static string? S3_PBX_SECRET_KEY_FILE
		{
			get
			{
				string? str = Environment.GetEnvironmentVariable("S3_PBX_SECRET_KEY_FILE");
				if (string.IsNullOrWhiteSpace(str))
				{
					Log.Error("S3_PBX_SECRET_KEY_FILE empty or missing.");
					return null;
				}
				return str;
			}
		}
		[Obsolete]
		public static string? S3_PBX_SECRET_KEY
		{
			get
			{
				string? e = S3_PBX_SECRET_KEY_FILE;
				if (e == null)
					return default;
				return File.ReadAllText(e);
			}
		}
		[Obsolete]
		public static string? S3_PBX_SERVICE_URI
		{
			get
			{
				string? e = S3_PBX_SERVICE_URI_FILE;
				if (e == null)
					return default;
				return File.ReadAllText(e);
			}
		}






		[Obsolete]
		public static string? S3_CARD_ON_FILE_BUCKET_NAME_FILE
		{
			get
			{
				string? str = Environment.GetEnvironmentVariable("S3_CARD_ON_FILE_BUCKET_NAME_FILE");
				if (string.IsNullOrWhiteSpace(str))
				{
					Log.Error("S3_CARD_ON_FILE_BUCKET_NAME_FILE empty or missing.");
					return null;
				}
				return str;
			}
		}
		[Obsolete]
		public static string? S3_CARD_ON_FILE_BUCKET_NAME
		{
			get
			{
				string? path = S3_CARD_ON_FILE_BUCKET_NAME_FILE;
				if (string.IsNullOrWhiteSpace(path))
					return null;
				return File.ReadAllText(path);
			}
		}




		[Obsolete]
		public static string? S3_CARD_ON_FILE_AUTHORIZATION_FORMS_ACCESS_KEY_FILE
		{
			get
			{
				string? str = Environment.GetEnvironmentVariable("S3_CARD_ON_FILE_AUTHORIZATION_FORMS_ACCESS_KEY_FILE");
				if (string.IsNullOrWhiteSpace(str))
				{
					Log.Error("S3_CARD_ON_FILE_AUTHORIZATION_FORMS_ACCESS_KEY_FILE empty or missing.");
					return null;
				}
				return str;
			}
		}
		[Obsolete]
		public static string? S3_CARD_ON_FILE_AUTHORIZATION_FORMS_ACCESS_KEY
		{
			get
			{
				string? path = S3_CARD_ON_FILE_AUTHORIZATION_FORMS_ACCESS_KEY_FILE;
				if (string.IsNullOrWhiteSpace(path))
					return null;
				return File.ReadAllText(path);
			}
		}

		[Obsolete]
		public static string? S3_CARD_ON_FILE_AUTHORIZATION_FORMS_SECRET_KEY_FILE
		{
			get
			{
				string? str = Environment.GetEnvironmentVariable("S3_CARD_ON_FILE_AUTHORIZATION_FORMS_SECRET_KEY_FILE");
				if (string.IsNullOrWhiteSpace(str))
				{
					Log.Error("S3_CARD_ON_FILE_AUTHORIZATION_FORMS_SECRET_KEY_FILE empty or missing.");
					return null;
				}
				return str;
			}
		}

		[Obsolete]
		public static string? S3_CARD_ON_FILE_AUTHORIZATION_FORMS_SECRET_KEY
		{
			get
			{
				string? path = S3_CARD_ON_FILE_AUTHORIZATION_FORMS_SECRET_KEY_FILE;
				if (string.IsNullOrWhiteSpace(path))
					return null;
				return File.ReadAllText(path);
			}
		}

		public static string S3_BUCKET_MMS
		{
			get
			{
				string? str = Environment.GetEnvironmentVariable("S3_BUCKET_MMS");
				if (string.IsNullOrWhiteSpace(str))
				{
					throw new Exception("S3_BUCKET_MMS empty or missing.");
				}
				return str;
			}
		}

		









	}
}