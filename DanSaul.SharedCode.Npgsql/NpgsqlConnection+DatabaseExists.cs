using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedCode
{
	public static class NpgsqlConnection_DatabaseExists
	{
		public static bool DatabaseExists(this NpgsqlConnection noDatabaseConnection, string dbName) {
			string cmdText = $"SELECT 1 FROM pg_database WHERE datname='{dbName}'";
			using NpgsqlCommand cmd = new NpgsqlCommand(cmdText, noDatabaseConnection);
			bool dbExists = cmd.ExecuteScalar() != null;

			return dbExists;
		}
	}
}
