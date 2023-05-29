// (c) 2023 Dan Saul
using Serilog;

namespace DanSaul.SharedCode.StandardizedEnvironmentVariables
{
	public static class EnvAsterisk
	{
		#region ASTERISK_HOST_FILE
		public static string ASTERISK_HOST_FILE
		{
			get
			{
				string? str = Environment.GetEnvironmentVariable("ASTERISK_HOST_FILE");
				if (string.IsNullOrWhiteSpace(str))
					throw new InvalidOperationException("ASTERISK_HOST_FILE empty or missing.");
				return str;
			}
		}
		public static string ASTERISK_HOST
		{
			get
			{
				string? env = ASTERISK_HOST_FILE;
				if (string.IsNullOrWhiteSpace(env))
					throw new InvalidOperationException("ASTERISK_HOST_FILE empty or missing.");
				return File.ReadAllText(env);
			}
		}
		#endregion
		#region ASTERISK_AMI_USER_FILE
		public static string ASTERISK_AMI_USER_FILE
		{
			get
			{
				string? str = Environment.GetEnvironmentVariable("ASTERISK_AMI_USER_FILE");
				if (string.IsNullOrWhiteSpace(str))
					throw new InvalidOperationException("ASTERISK_AMI_USER_FILE empty or missing.");
				return str;
			}
		}
		public static string ASTERISK_AMI_USER
		{
			get
			{
				string? env = ASTERISK_AMI_USER_FILE;
				if (string.IsNullOrWhiteSpace(env))
					throw new InvalidOperationException("ASTERISK_AMI_USER empty or missing.");
				return File.ReadAllText(env);
			}
		}
		#endregion
		#region ASTERISK_AMI_PASS_FILE
		public static string ASTERISK_AMI_PASS_FILE
		{
			get
			{
				string? str = Environment.GetEnvironmentVariable("ASTERISK_AMI_PASS_FILE");
				if (string.IsNullOrWhiteSpace(str))
					throw new InvalidOperationException("ASTERISK_AMI_PASS_FILE empty or missing.");
				return str;
			}
		}
		public static string ASTERISK_AMI_PASS
		{
			get
			{
				string? env = ASTERISK_AMI_PASS_FILE;
				if (string.IsNullOrWhiteSpace(env))
					throw new InvalidOperationException("ASTERISK_AMI_PASS_FILE empty or missing.");
				return File.ReadAllText(env);
			}
		}
		#endregion
		#region ASTERISK_AMI_PORT_FILE
		public static string ASTERISK_AMI_PORT_FILE
		{
			get
			{
				string? str = Environment.GetEnvironmentVariable("ASTERISK_AMI_PORT_FILE");
				if (string.IsNullOrWhiteSpace(str))
					throw new InvalidOperationException("ASTERISK_AMI_PORT_FILE empty or missing.");
				return str;
			}
		}
		public static int ASTERISK_AMI_PORT
		{
			get
			{
				string? env = ASTERISK_AMI_PORT_FILE;
				if (string.IsNullOrWhiteSpace(env))
					throw new InvalidOperationException("ASTERISK_AMI_PORT_FILE empty or missing.");
				string str = File.ReadAllText(env);
				if (string.IsNullOrWhiteSpace(str))
					throw new InvalidOperationException("ASTERISK_AMI_PORT_FILE empty or missing.");

				if (int.TryParse(str, out int _parsed))
					return _parsed;
				throw new InvalidOperationException("ASTERISK_AMI_PORT_FILE unable to parse.");
			}
		}
		#endregion
		#region ASTERISK_DEBUG_SSH_ENABLE
		public static bool ASTERISK_DEBUG_SSH_ENABLE
		{
			get
			{
				string? payload = Environment.GetEnvironmentVariable("ASTERISK_DEBUG_SSH_ENABLE");
				if (string.IsNullOrWhiteSpace(payload))
					return false;

				if (bool.TryParse(payload, out bool parsed))
					return parsed;

				return false;
			}
		}
		#endregion
		#region ASTERISK_DEBUG_SSH_PORT_FILE
		public static string ASTERISK_DEBUG_SSH_PORT_FILE
		{
			get
			{
				string? str = Environment.GetEnvironmentVariable("ASTERISK_DEBUG_SSH_PORT_FILE");
				if (string.IsNullOrWhiteSpace(str))
					throw new InvalidOperationException("ASTERISK_DEBUG_SSH_PORT_FILE empty or missing.");
				return str;
			}
		}
		public static int ASTERISK_DEBUG_SSH_PORT
		{
			get
			{
				string? env = ASTERISK_DEBUG_SSH_PORT_FILE;
				if (string.IsNullOrWhiteSpace(env))
					throw new InvalidOperationException("ASTERISK_DEBUG_SSH_PORT_FILE empty or missing.");
				string str = File.ReadAllText(env);
				if (string.IsNullOrWhiteSpace(str))
					throw new InvalidOperationException("ASTERISK_DEBUG_SSH_PORT_FILE empty or missing.");

				if (int.TryParse(str, out int _parsed))
					return _parsed;
				throw new InvalidOperationException("ASTERISK_DEBUG_SSH_PORT_FILE unable to parse.");
			}
		}
		#endregion
		#region ASTERISK_DEBUG_SSH_USER_FILE
		public static string ASTERISK_DEBUG_SSH_USER_FILE
		{
			get
			{
				string? str = Environment.GetEnvironmentVariable("ASTERISK_DEBUG_SSH_USER_FILE");
				if (string.IsNullOrWhiteSpace(str))
					throw new InvalidOperationException("ASTERISK_DEBUG_SSH_USER_FILE empty or missing.");
				return str;
			}
		}

		public static string ASTERISK_DEBUG_SSH_USER
		{
			get
			{
				string? env = ASTERISK_DEBUG_SSH_USER_FILE;
				if (string.IsNullOrWhiteSpace(env))
					throw new InvalidOperationException("ASTERISK_DEBUG_SSH_USER_FILE empty or missing.");
				return File.ReadAllText(env);
			}
		}
		#endregion
		#region ASTERISK_EXTERNAL_IP_ADDRESS
		public static string ASTERISK_EXTERNAL_IP_ADDRESS_FILE
		{
			get
			{
				string? str = Environment.GetEnvironmentVariable("ASTERISK_EXTERNAL_IP_ADDRESS_FILE");
				if (string.IsNullOrWhiteSpace(str))
					throw new InvalidOperationException("ASTERISK_EXTERNAL_IP_ADDRESS_FILE empty or missing.");
				return str;
			}
		}

		public static string ASTERISK_EXTERNAL_IP_ADDRESS
		{
			get
			{
				string? env = ASTERISK_EXTERNAL_IP_ADDRESS_FILE;
				if (string.IsNullOrWhiteSpace(env))
					throw new InvalidOperationException("ASTERISK_EXTERNAL_IP_ADDRESS_FILE empty or missing.");
				return File.ReadAllText(env);
			}
		}
		#endregion
		#region ASTERISK_SIP_PORT_FILE
		public static string ASTERISK_SIP_PORT_FILE
		{
			get
			{
				string? str = Environment.GetEnvironmentVariable("ASTERISK_SIP_PORT_FILE");
				if (string.IsNullOrWhiteSpace(str))
					throw new InvalidOperationException("ASTERISK_SIP_PORT_FILE empty or missing.");
				return str;
			}
		}
		public static int ASTERISK_SIP_PORT
		{
			get
			{
				string? env = ASTERISK_SIP_PORT_FILE;
				if (string.IsNullOrWhiteSpace(env))
					throw new InvalidOperationException("ASTERISK_SIP_PORT_FILE empty or missing.");
				string str = File.ReadAllText(env);
				if (string.IsNullOrWhiteSpace(str))
					throw new InvalidOperationException("ASTERISK_SIP_PORT_FILE empty or missing.");

				if (int.TryParse(str, out int _parsed))
					return _parsed;
				throw new InvalidOperationException("ASTERISK_SIP_PORT_FILE unable to parse.");
			}
		}
		#endregion
		#region ASTERISK_HOME_AREA_CODE
		public static string ASTERISK_HOME_AREA_CODE
		{
			get
			{
				string? payload = Environment.GetEnvironmentVariable("ASTERISK_HOME_AREA_CODE");
				if (string.IsNullOrWhiteSpace(payload))
					throw new InvalidOperationException("ASTERISK_HOME_AREA_CODE empty or missing.");
				return payload;
			}
		}
		#endregion
		#region ASTERISK_RECORDINGS_DIRECTORY
		public static string ASTERISK_RECORDINGS_DIRECTORY
		{
			get
			{
				string? payload = Environment.GetEnvironmentVariable("ASTERISK_RECORDINGS_DIRECTORY");
				if (string.IsNullOrWhiteSpace(payload))
					throw new InvalidOperationException("ASTERISK_RECORDINGS_DIRECTORY empty or missing.");
				return payload;
			}
		}
		#endregion
		#region ASTERISK_AGI_HOST_FILE
		public static string ASTERISK_AGI_HOST_FILE
		{
			get
			{
				string? str = Environment.GetEnvironmentVariable("ASTERISK_AGI_HOST_FILE");
				if (string.IsNullOrWhiteSpace(str))
					throw new InvalidOperationException("ASTERISK_AGI_HOST_FILE empty or missing.");
				return str;
			}
		}
		public static string ASTERISK_AGI_HOST
		{
			get
			{
				string? env = ASTERISK_AGI_HOST_FILE;
				if (string.IsNullOrWhiteSpace(env))
					throw new InvalidOperationException("ASTERISK_AGI_HOST_FILE empty or missing.");
				return File.ReadAllText(env);
			}
		}
		#endregion




		#region Obsolete
		[Obsolete]
		public static string? PBX_LOCAL_TTS_CACHE_BUCKET_DIRECTORY
		{
			get
			{
				string? str = Environment.GetEnvironmentVariable("PBX_LOCAL_TTS_CACHE_BUCKET_DIRECTORY");
				if (string.IsNullOrWhiteSpace(str))
				{
					throw new InvalidOperationException("PBX_LOCAL_TTS_CACHE_BUCKET_DIRECTORY empty or missing.");
				}
				return str;
			}
		}

		[Obsolete]
		public static string? PBX_LOCAL_CLIENT_RECORDINGS_DIRECTORY
		{
			get
			{
				string? str = Environment.GetEnvironmentVariable("PBX_LOCAL_CLIENT_RECORDINGS_DIRECTORY");
				if (string.IsNullOrWhiteSpace(str))
				{
					throw new InvalidOperationException("PBX_LOCAL_CLIENT_RECORDINGS_DIRECTORY empty or missing.");
				}
				return str;
			}
		}

		[Obsolete]
		public static string? ARI_TO_PBX_SSH_IDRSA_FILE
		{
			get
			{
				string? str = Environment.GetEnvironmentVariable("ARI_TO_PBX_SSH_IDRSA_FILE");
				if (string.IsNullOrWhiteSpace(str))
				{
					throw new InvalidOperationException("ARI_TO_PBX_SSH_IDRSA_FILE empty or missing.");
				}
				return str;
			}
		}
		[Obsolete]
		public static string? PBX_FQDN_FILE
		{
			get
			{
				string? str = Environment.GetEnvironmentVariable("PBX_FQDN_FILE");
				if (string.IsNullOrWhiteSpace(str))
				{
					throw new InvalidOperationException("PBX_FQDN empty or missing.");
				}
				return str;
			}
		}
		[Obsolete]
		public static string? PBX_FQDN
		{
			get
			{
				string? path = PBX_FQDN_FILE;
				if (string.IsNullOrWhiteSpace(path))
					throw new InvalidOperationException("PBX_FQDN_FILE empty or missing.");

				return File.ReadAllText(path);
			}
		}
		[Obsolete]
		public static int? PBX_SSH_PORT
		{
			get
			{
				string? str = Environment.GetEnvironmentVariable("PBX_SSH_PORT");
				if (string.IsNullOrWhiteSpace(str))
				{
					throw new InvalidOperationException("PBX_SSH_PORT empty or missing.");
				}
				return int.Parse(str);
			}
		}
		[Obsolete]
		public static string? PBX_SSH_USER
		{
			get
			{
				string? str = Environment.GetEnvironmentVariable("PBX_SSH_USER");
				if (string.IsNullOrWhiteSpace(str))
				{
					throw new InvalidOperationException("PBX_SSH_USER empty or missing.");
				}
				return str;
			}
		}
		[Obsolete]
		public static string? ARI_OUTGOING_SPOOL_DIRECTORY
		{
			get
			{
				string? str = Environment.GetEnvironmentVariable("ARI_OUTGOING_SPOOL_DIRECTORY");
				if (string.IsNullOrWhiteSpace(str))
				{
					throw new InvalidOperationException("ARI_OUTGOING_SPOOL_DIRECTORY empty or missing.");
				}
				return str;
			}
		}
		[Obsolete]
		public static string? ARI_OUTGOING_SPOOL_COMPLETED_DIRECTORY
		{
			get
			{
				string? str = Environment.GetEnvironmentVariable("ARI_OUTGOING_SPOOL_COMPLETED_DIRECTORY");
				if (string.IsNullOrWhiteSpace(str))
				{
					throw new InvalidOperationException("ARI_OUTGOING_SPOOL_COMPLETED_DIRECTORY empty or missing.");
				}
				return str;
			}
		}

		#endregion
	}
}
