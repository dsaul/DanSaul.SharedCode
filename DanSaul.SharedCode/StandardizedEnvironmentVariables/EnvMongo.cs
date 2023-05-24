
using Serilog;

namespace DanSaul.SharedCode.StandardizedEnvironmentVariables
{
	public static class EnvMongo
	{
		public static string? MONGO_URI_PATH
		{
			get
			{
				string? str = Environment.GetEnvironmentVariable("MONGO_URI_PATH");
				if (string.IsNullOrWhiteSpace(str))
				{
					Log.Error("MONGO_URI_PATH empty or missing.");
					return null;
				}
				return str;
			}
		}
		public static string MONGO_URI
		{
			get
			{
				string? path = MONGO_URI_PATH;
				if (string.IsNullOrWhiteSpace(path))
					throw new InvalidOperationException();
				return File.ReadAllText(path);
			}
		}
	}
}
