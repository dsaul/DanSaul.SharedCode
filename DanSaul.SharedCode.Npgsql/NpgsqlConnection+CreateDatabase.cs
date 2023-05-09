using Npgsql;

namespace DanSaul.SharedCode.Npgsql
{
	public static class NpgsqlConnection_CreateDatabase
	{
		public static string? CreateDatabase(
			this NpgsqlConnection noDatabaseConnection, 
			string dbName, 
			string prefix = "zclient_dp_", 
			string suffixBeforeNumber = "_", 
			bool noNumberIteration = false,
			string databaseOwner = "postgres"
			) {

			if (string.IsNullOrWhiteSpace(dbName))
				throw new Exception("string.IsNullOrWhiteSpace(dbName) 1");

			dbName = RegexUtils.NotLettersNumbersUnderscoreRegex.Replace(dbName, "").ToLower(Culture.DevelopmentCulture);
			if (string.IsNullOrWhiteSpace(dbName))
				throw new Exception("string.IsNullOrWhiteSpace(dbName) 2");

			string? databaseName = $"{prefix}{dbName}";
			
			
			// Iterate up numbers until we find a database name that is not in use, don't go past 100 though.
			if (false == noNumberIteration) {
				for (var i = 0; i < 100; i++) {
					databaseName = $"{prefix}{dbName}{suffixBeforeNumber}{i}";

					if (false == noDatabaseConnection.DatabaseExists(databaseName))
						break;

					databaseName = null;
				}
			}

			// Create the named database.
			{
				string cmd = $"CREATE DATABASE {databaseName} WITH OWNER = {databaseOwner};";


				using NpgsqlCommand createDBCommand = new NpgsqlCommand(cmd, noDatabaseConnection);
				createDBCommand.ExecuteNonQuery();
			}

			return databaseName;
		}
	}
}
