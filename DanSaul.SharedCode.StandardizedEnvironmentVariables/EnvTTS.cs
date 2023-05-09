using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DanSaul.SharedCode.StandardizedEnvironmentVariables
{
	public static class EnvTTS
	{
		public static string? AWS_SHARED_CREDENTIALS_FILE
		{
			get {
				string? str = Environment.GetEnvironmentVariable("AWS_SHARED_CREDENTIALS_FILE");
				if (string.IsNullOrWhiteSpace(str)) {
					Log.Error("AWS_SHARED_CREDENTIALS_FILE empty or missing.");
					return null;
				}
				return str;
			}
		}
		public static string? AWS_PROFILE
		{
			get {
				string? str = Environment.GetEnvironmentVariable("AWS_PROFILE");
				if (string.IsNullOrWhiteSpace(str)) {
					Log.Error("AWS_PROFILE empty or missing.");
					return null;
				}
				return str;
			}
		}

		public static string? FFMPEG_PATH
		{
			get {
				string? str = Environment.GetEnvironmentVariable("FFMPEG_PATH");
				if (string.IsNullOrWhiteSpace(str)) {
					Log.Error("FFMPEG_PATH empty or missing.");
					return null;
				}
				return str;
			}
		}


		public static string? AWS_POLLY_ACCESS_KEY_ID_FILE
		{
			get {
				string? str = Environment.GetEnvironmentVariable("AWS_POLLY_ACCESS_KEY_ID_FILE");
				if (string.IsNullOrWhiteSpace(str)) {
					Log.Error("AWS_POLLY_ACCESS_KEY_ID_FILE empty or missing.");
					return null;
				}
				return str;
			}
		}

		public static string? AWS_POLLY_SECRET_ACCESS_KEY_FILE
		{
			get {
				string? str = Environment.GetEnvironmentVariable("AWS_POLLY_SECRET_ACCESS_KEY_FILE");
				if (string.IsNullOrWhiteSpace(str)) {
					Log.Error("AWS_POLLY_SECRET_ACCESS_KEY_FILE empty or missing.");
					return null;
				}
				return str;
			}
		}
	}
}
