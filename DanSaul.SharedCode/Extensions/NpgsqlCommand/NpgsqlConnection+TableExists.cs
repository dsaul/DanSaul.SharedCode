// (c) 2023 Dan Saul
using Npgsql;

namespace DanSaul.SharedCode.Npgsql
{
	public static class NpgsqlConnection_TableExists
	{
		public static bool TableExists(this NpgsqlConnection db, string tableName) {
			string sql = @"
				SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name  = @tableName
			";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, db);
			cmd.Parameters.AddWithValue("@tableName", tableName);
			return cmd.ExecuteScalar() != null;
		}
	}
}
