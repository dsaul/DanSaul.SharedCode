using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedCode.Asterisk
{
	public static class Konstants
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
