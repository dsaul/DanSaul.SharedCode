using Npgsql;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
