// (c) 2023 Dan Saul
using Npgsql;
using Serilog;

namespace DanSaul.SharedCode.Npgsql
{
	public static class NpgsqlConnection_EnsureUUIDExtension
	{
		public static void EnsureUUIDExtension(this NpgsqlConnection connection) {

			using NpgsqlCommand createUuidCommand = new(@"
					CREATE EXTENSION IF NOT EXISTS ""uuid-ossp"";
					", connection);
			createUuidCommand.ExecuteNonQuery();

			Log.Debug("----- Done.");
		}
	}
}
