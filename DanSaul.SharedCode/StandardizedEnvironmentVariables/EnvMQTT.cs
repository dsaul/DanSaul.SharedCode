using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DanSaul.SharedCode.StandardizedEnvironmentVariables
{
	public static class EnvMQTT
	{
		public static string? MQTT_HOST
		{
			get
			{
				string? str = Environment.GetEnvironmentVariable("MQTT_HOST");
				if (string.IsNullOrWhiteSpace(str))
				{
					Log.Error("MQTT_HOST empty or missing.");
					return null;
				}
				return str;
			}
		}
		public static int MQTT_PORT
		{
			get
			{
				string? str = Environment.GetEnvironmentVariable("MQTT_PORT");
				if (string.IsNullOrWhiteSpace(str))
				{
					Log.Error("MQTT_PORT empty or missing.");
					throw new InvalidOperationException();
				}

				if (false == int.TryParse(str, out int i))
				{
					Log.Error("MQTT_PORT invalid.");
					throw new InvalidOperationException();
				}

				return i;
			}
		}
		public static string? MQTT_USER_PATH
		{
			get
			{
				string? str = Environment.GetEnvironmentVariable("MQTT_USER_PATH");
				if (string.IsNullOrWhiteSpace(str))
				{
					Log.Error("MQTT_USER_PATH empty or missing.");
					return null;
				}
				return str;
			}
		}
		public static string MQTT_USER
		{
			get
			{
				string? path = MQTT_USER_PATH;
				if (string.IsNullOrWhiteSpace(path))
					throw new InvalidOperationException();
				return File.ReadAllText(path);
			}
		}
		public static string? MQTT_PASSWORD_PATH
		{
			get
			{
				string? str = Environment.GetEnvironmentVariable("MQTT_PASSWORD_PATH");
				if (string.IsNullOrWhiteSpace(str))
				{
					Log.Error("MQTT_PASSWORD_PATH empty or missing.");
					return null;
				}
				return str;
			}
		}
		public static string MQTT_PASSWORD
		{
			get
			{
				string? path = MQTT_PASSWORD_PATH;
				if (string.IsNullOrWhiteSpace(path))
					throw new InvalidOperationException();
				return File.ReadAllText(path);
			}
		}
	}
}
