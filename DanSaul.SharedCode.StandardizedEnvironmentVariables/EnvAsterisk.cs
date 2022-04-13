using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedCode
{
	public static class EnvAsterisk
	{
		public static string? PBX_LOCAL_TTS_CACHE_BUCKET_DIRECTORY
		{
			get {
				string? str = Environment.GetEnvironmentVariable("PBX_LOCAL_TTS_CACHE_BUCKET_DIRECTORY");
				if (string.IsNullOrWhiteSpace(str)) {
					Log.Error("PBX_LOCAL_TTS_CACHE_BUCKET_DIRECTORY empty or missing.");
					return null;
				}
				return str;
			}
		}

		public static string? PBX_LOCAL_CLIENT_RECORDINGS_DIRECTORY
		{
			get {
				string? str = Environment.GetEnvironmentVariable("PBX_LOCAL_CLIENT_RECORDINGS_DIRECTORY");
				if (string.IsNullOrWhiteSpace(str)) {
					Log.Error("PBX_LOCAL_CLIENT_RECORDINGS_DIRECTORY empty or missing.");
					return null;
				}
				return str;
			}
		}

		public static string? ARI_TO_PBX_SSH_IDRSA_FILE
		{
			get {
				string? str = Environment.GetEnvironmentVariable("ARI_TO_PBX_SSH_IDRSA_FILE");
				if (string.IsNullOrWhiteSpace(str)) {
					Log.Error("ARI_TO_PBX_SSH_IDRSA_FILE empty or missing.");
					return null;
				}
				return str;
			}
		}

		public static string? PBX_FQDN_FILE
		{
			get {
				string? str = Environment.GetEnvironmentVariable("PBX_FQDN_FILE");
				if (string.IsNullOrWhiteSpace(str)) {
					Log.Error("PBX_FQDN empty or missing.");
					return null;
				}
				return str;
			}
		}

		public static string? PBX_FQDN
		{
			get {
				string? e = PBX_FQDN_FILE;
				if (e == null)
					return default;

				return File.ReadAllText(e);
			}
		}

		public static int? PBX_SSH_PORT
		{
			get {
				string? str = Environment.GetEnvironmentVariable("PBX_SSH_PORT");
				if (string.IsNullOrWhiteSpace(str)) {
					Log.Error("PBX_SSH_PORT empty or missing.");
					return null;
				}
				return int.Parse(str);
			}
		}

		public static string? PBX_SSH_USER
		{
			get {
				string? str = Environment.GetEnvironmentVariable("PBX_SSH_USER");
				if (string.IsNullOrWhiteSpace(str)) {
					Log.Error("PBX_SSH_USER empty or missing.");
					return null;
				}
				return str;
			}
		}

		public static string? PBX_LOCAL_OUTGOING_SPOOL_DIRECTORY
		{
			get {
				string? str = Environment.GetEnvironmentVariable("PBX_LOCAL_OUTGOING_SPOOL_DIRECTORY");
				if (string.IsNullOrWhiteSpace(str)) {
					Log.Error("PBX_LOCAL_OUTGOING_SPOOL_DIRECTORY empty or missing.");
					return null;
				}
				return str;
			}
		}

		public static string? PBX_LOCAL_OUTGOING_SPOOL_COMPLETED_DIRECTORY
		{
			get {
				string? str = Environment.GetEnvironmentVariable("PBX_LOCAL_OUTGOING_SPOOL_COMPLETED_DIRECTORY");
				if (string.IsNullOrWhiteSpace(str)) {
					Log.Error("PBX_LOCAL_OUTGOING_SPOOL_COMPLETED_DIRECTORY empty or missing.");
					return null;
				}
				return str;
			}
		}

		public static string? ARI_SPOOL_DIRECTORY
		{
			get
			{
				string? str = Environment.GetEnvironmentVariable("ARI_SPOOL_DIRECTORY");
				if (string.IsNullOrWhiteSpace(str))
				{
					Log.Error("ARI_SPOOL_DIRECTORY empty or missing.");
					return null;
				}
				return str;
			}
		}
	}
}
