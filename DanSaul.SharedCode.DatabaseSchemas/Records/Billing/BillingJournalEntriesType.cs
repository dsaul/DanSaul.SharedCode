using Npgsql;
using System.Text;
using System.Data;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Serilog;
using DanSaul.SharedCode.DatabaseSchemas.Properties;

namespace SharedCode.DatabaseSchemas
{
	public record BillingJournalEntriesType(
		Guid? Uuid,
		string? Type,
		string? Json
		)
	{
		public static Dictionary<Guid, BillingJournalEntriesType> ForId(NpgsqlConnection connection, Guid id) {

			Dictionary<Guid, BillingJournalEntriesType> ret = new Dictionary<Guid, BillingJournalEntriesType>();

			string sql = @"SELECT * from ""billing-journal-entries-type"" WHERE uuid = @uuid";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			cmd.Parameters.AddWithValue("@uuid", id);




			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					BillingJournalEntriesType obj = BillingJournalEntriesType.FromDataReader(reader);
					if (obj.Uuid == null) {
						continue;
					}
					ret.Add(obj.Uuid.Value, obj);
				}
			}

			return ret;
		}

		public static Dictionary<Guid, BillingJournalEntriesType> ForIds(NpgsqlConnection connection, IEnumerable<Guid> ids) {

			Guid[] idsArr = ids.ToArray();

			Dictionary<Guid, BillingJournalEntriesType> ret = new Dictionary<Guid, BillingJournalEntriesType>();
			if (idsArr.Length == 0)
				return ret;

			List<string> valNames = new List<string>();
			for (int i = 0; i < idsArr.Length; i++) {
				valNames.Add($"@val{i}");
			}

			string sql = $"SELECT * from \"billing-journal-entries-type\" WHERE uuid IN ({string.Join(", ", valNames)})";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			for (int i = 0; i < valNames.Count; i++) {
				cmd.Parameters.AddWithValue(valNames[i], idsArr[i]);
			}

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					BillingJournalEntriesType obj = BillingJournalEntriesType.FromDataReader(reader);
					if (obj.Uuid == null) {
						continue;
					}
					ret.Add(obj.Uuid.Value, obj);
				}
			}

			return ret;


		}


		public static Dictionary<Guid, BillingJournalEntriesType> All(NpgsqlConnection connection) {

			Dictionary<Guid, BillingJournalEntriesType> ret = new Dictionary<Guid, BillingJournalEntriesType>();

			string sql = @"SELECT * from ""billing-journal-entries-type""";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					BillingJournalEntriesType obj = BillingJournalEntriesType.FromDataReader(reader);
					if (obj.Uuid == null) {
						continue;
					}
					ret.Add(obj.Uuid.Value, obj);
				}
			}

			return ret;


		}

		public static List<Guid> Delete(NpgsqlConnection connection, List<Guid> idsToDelete) {

			List<Guid> toSendToOthers = new List<Guid>();
			if (idsToDelete.Count == 0) {
				return toSendToOthers;
			}

			List<string> valNames = new List<string>();
			for (int i = 0; i < idsToDelete.Count; i++) {
				valNames.Add($"@val{i}");
			}



			string sql = $"DELETE FROM \"billing-journal-entries-type\" WHERE \"uuid\" IN ({string.Join(", ", valNames)})";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			for (int i = 0; i < valNames.Count; i++) {
				cmd.Parameters.AddWithValue(valNames[i], idsToDelete[i]);
			}



			int rowsAffected = cmd.ExecuteNonQuery();
			if (rowsAffected == 0) {
				return toSendToOthers;
			}

			toSendToOthers.AddRange(idsToDelete);
			return toSendToOthers;



		}


		public static void Upsert(NpgsqlConnection connection, Dictionary<Guid, BillingJournalEntriesType> updateObjects, out List<Guid> callerResponse, out Dictionary<Guid, BillingJournalEntriesType> toSendToOthers) {

			callerResponse = new List<Guid>();
			toSendToOthers = new Dictionary<Guid, BillingJournalEntriesType>();

			foreach (KeyValuePair<Guid, BillingJournalEntriesType> kvp in updateObjects) {

				string sql = @"
					INSERT INTO
						""billing-journal-entries-type""
						(
							""uuid"",
							""type"",
							""json""
						)
					VALUES
						(
							@uuid,
							@type,
							CAST(@json AS json)
						)
					ON CONFLICT (""uuid"") DO UPDATE
						SET
							""type"" = excluded.""type"",
							""json"" = CAST(excluded.""json"" AS json)
					";

				using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
				cmd.Parameters.AddWithValue("@uuid", kvp.Key);
				cmd.Parameters.AddWithValue("@type", string.IsNullOrWhiteSpace(kvp.Value.Type) ? (object)DBNull.Value : kvp.Value.Type);
				cmd.Parameters.AddWithValue("@json", string.IsNullOrWhiteSpace(kvp.Value.Json) ? (object)DBNull.Value : kvp.Value.Json);

				int rowsAffected = cmd.ExecuteNonQuery();

				if (rowsAffected == 0) {
					continue;
				}

				toSendToOthers.Add(kvp.Key, kvp.Value);
				callerResponse.Add(kvp.Key);


			}



		}



		public static BillingJournalEntriesType FromDataReader(NpgsqlDataReader reader) {

			Guid? uuid = default;
			string? type = default;
			string? json = default;

			if (!reader.IsDBNull("uuid")) {
				uuid = reader.GetGuid("uuid");
			}
			if (!reader.IsDBNull("type")) {
				type = reader.GetString("type");
			}
			if (!reader.IsDBNull("json")) {
				json = reader.GetString("json");
			}


			return new BillingJournalEntriesType(
				Uuid: uuid,
				Type: type,
				Json: json
				);
		}

		[JsonIgnore]
		public JObject? JsonObject
		{
			get {
				if (string.IsNullOrWhiteSpace(Json))
					return null;
				return JsonConvert.DeserializeObject(Json, new JsonSerializerSettings() { DateParseHandling = DateParseHandling.None }) as JObject;
			}
		}


		public static void VerifyRepairTable(NpgsqlConnection db, bool insertDefaultContents = false) {

			if (db.TableExists("billing-journal-entries-type")) {
				Log.Debug($"----- Table \"billing-journal-entries-type\" exists.");
			} else {
				Log.Information($"----- Table \"billing-journal-entries-type\" doesn't exist, creating.");

				using NpgsqlCommand cmd = new NpgsqlCommand(@"
					CREATE TABLE ""public"".""billing-journal-entries-type"" (
						""uuid"" uuid DEFAULT public.uuid_generate_v1() NOT NULL,
						""type"" character varying(255) NOT NULL,
						""json"" json DEFAULT '{}'::json NOT NULL,
						CONSTRAINT ""billing_journal_entries_type"" PRIMARY KEY(""uuid"")
					) WITH(oids = false);
					", db);
				cmd.ExecuteNonQuery();
			}


			if (insertDefaultContents) {
				Log.Information("Insert Default Contents");



				NpgsqlCommand command = new NpgsqlCommand(SQLUtility.RemoveCommentsFromSQLString(Resources.SQLInsertDefaultBillingJournalEntriesType, true), db);
				command.ExecuteNonQuery();



			}





		}









	}
}
