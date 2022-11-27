using Npgsql;
using System.Text;
using System.Data;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Serilog;

namespace SharedCode.DatabaseSchemas
{
	public record BillingSessions(
		Guid? Uuid,
		Guid? ContactId,
		string? AgentDescription,
		string? IpAddress,
		DateTime? CreatedUtc,
		DateTime? LastAccessUtc,
		string? Json
		)
	{
		public static Dictionary<Guid, BillingSessions> ForId(NpgsqlConnection connection, Guid id) {

			Dictionary<Guid, BillingSessions> ret = new Dictionary<Guid, BillingSessions>();

			string sql = @"SELECT * from ""billing-sessions"" WHERE uuid = @uuid";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			cmd.Parameters.AddWithValue("@uuid", id);




			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					BillingSessions obj = BillingSessions.FromDataReader(reader);
					if (obj.Uuid == null) {
						continue;
					}
					ret.Add(obj.Uuid.Value, obj);
				}
			}

			return ret;
		}

		public static Dictionary<Guid, BillingSessions> ForIds(NpgsqlConnection connection, IEnumerable<Guid> ids) {

			Guid[] idsArr = ids.ToArray();

			Dictionary<Guid, BillingSessions> ret = new Dictionary<Guid, BillingSessions>();
			if (idsArr.Length == 0)
				return ret;

			List<string> valNames = new List<string>();
			for (int i = 0; i < idsArr.Length; i++) {
				valNames.Add($"@val{i}");
			}

			string sql = $"SELECT * from \"billing-sessions\" WHERE uuid IN ({string.Join(", ", valNames)})";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			for (int i = 0; i < valNames.Count; i++) {
				cmd.Parameters.AddWithValue(valNames[i], idsArr[i]);
			}

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					BillingSessions obj = BillingSessions.FromDataReader(reader);
					if (obj.Uuid == null) {
						continue;
					}
					ret.Add(obj.Uuid.Value, obj);
				}
			}

			return ret;


		}

		public static Dictionary<Guid, BillingSessions> ForContactId(NpgsqlConnection connection, Guid? contactId) {

			Dictionary<Guid, BillingSessions> ret = new Dictionary<Guid, BillingSessions>();

			if (null == contactId) {
				return ret;
			}

			string sql = @"SELECT * from ""billing-sessions"" WHERE ""contact-id"" = @contactId";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			cmd.Parameters.AddWithValue("@contactId", contactId);




			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					BillingSessions obj = BillingSessions.FromDataReader(reader);
					if (obj.Uuid == null) {
						continue;
					}
					ret.Add(obj.Uuid.Value, obj);
				}
			}

			return ret;
		}

		public static Dictionary<Guid, BillingSessions> ForContactIds(NpgsqlConnection connection, List<Guid> ids) {

			Dictionary<Guid, BillingSessions> ret = new Dictionary<Guid, BillingSessions>();
			if (ids.Count == 0) {
				return ret;
			}

			List<string> valNames = new List<string>();
			for (int i = 0; i < ids.Count; i++) {
				valNames.Add($"@val{i}");
			}

			string sql = $"SELECT * FROM \"billing-sessions\" WHERE \"contact-id\" IN ({string.Join(", ", valNames)})";

			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			for (int i = 0; i < valNames.Count; i++) {
				cmd.Parameters.AddWithValue(valNames[i], ids[i]);
			}

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					BillingSessions obj = BillingSessions.FromDataReader(reader);
					if (obj.Uuid == null) {
						continue;
					}
					ret.Add(obj.Uuid.Value, obj);
				}
			}


			return ret;
		}



		public static Dictionary<Guid, BillingSessions> All(NpgsqlConnection connection) {

			Dictionary<Guid, BillingSessions> ret = new Dictionary<Guid, BillingSessions>();

			string sql = @"SELECT * from ""billing-sessions""";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					BillingSessions obj = BillingSessions.FromDataReader(reader);
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



			string sql = $"DELETE FROM \"billing-sessions\" WHERE \"uuid\" IN ({string.Join(", ", valNames)})";
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


		public static List<Guid> DeleteForContactIds(NpgsqlConnection connection, List<Guid> idsToDelete) {

			List<Guid> toSendToOthers = new List<Guid>();
			if (idsToDelete.Count == 0) {
				return toSendToOthers;
			}

			List<string> valNames = new List<string>();
			for (int i = 0; i < idsToDelete.Count; i++) {
				valNames.Add($"@val{i}");
			}



			string sql = $"DELETE FROM \"billing-sessions\" WHERE \"contact-id\" IN ({string.Join(", ", valNames)})";
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


		public static void Upsert(NpgsqlConnection connection, Dictionary<Guid, BillingSessions> updateObjects, out List<Guid> callerResponse, out Dictionary<Guid, BillingSessions> toSendToOthers) {

			callerResponse = new List<Guid>();
			toSendToOthers = new Dictionary<Guid, BillingSessions>();

			foreach (KeyValuePair<Guid, BillingSessions> kvp in updateObjects) {

				string sql = @"
					INSERT INTO
						""billing-sessions""
						(
							""uuid"",
							""contact-id"",
							""agent-description"",
							""ip-address"",
							""created-utc"",
							""last-access-utc"",
							""json""
						)
					VALUES
						(
							@uuid,
							@contactId,
							@agentDescription,
							@ipAddress,
							@createdUtc,
							@lastAccessUtc,
							CAST(@json AS json)
						)
					ON CONFLICT (""uuid"") DO UPDATE
						SET
							""contact-id"" = excluded.""contact-id"",
							""agent-description"" = excluded.""agent-description"",
							""ip-address"" = excluded.""ip-address"",
							""created-utc"" = excluded.""created-utc"",
							""last-access-utc"" = excluded.""last-access-utc"",
							""json"" = CAST(excluded.""json"" AS json)
					";

				using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
				cmd.Parameters.AddWithValue("@uuid", kvp.Key);
				cmd.Parameters.AddWithValue("@contactId", null == kvp.Value.ContactId ? (object)DBNull.Value : kvp.Value.ContactId);
				cmd.Parameters.AddWithValue("@agentDescription", string.IsNullOrWhiteSpace(kvp.Value.AgentDescription) ? (object)DBNull.Value : kvp.Value.AgentDescription);
				cmd.Parameters.AddWithValue("@ipAddress", string.IsNullOrWhiteSpace(kvp.Value.IpAddress) ? (object)DBNull.Value : kvp.Value.IpAddress);
				cmd.Parameters.AddWithValue("@createdUtc", null == kvp.Value.CreatedUtc ? (object)DBNull.Value : kvp.Value.CreatedUtc);
				cmd.Parameters.AddWithValue("@lastAccessUtc", null == kvp.Value.LastAccessUtc ? (object)DBNull.Value : kvp.Value.LastAccessUtc);
				cmd.Parameters.AddWithValue("@json", string.IsNullOrWhiteSpace(kvp.Value.Json) ? (object)DBNull.Value : kvp.Value.Json);

				int rowsAffected = cmd.ExecuteNonQuery();

				if (rowsAffected == 0) {
					continue;
				}

				toSendToOthers.Add(kvp.Key, kvp.Value);
				callerResponse.Add(kvp.Key);


			}



		}



		public static BillingSessions FromDataReader(NpgsqlDataReader reader) {

			Guid? uuid = default;
			Guid? contactId = default;
			string? agentDescription = default;
			string? ipAddress = default;
			DateTime? createdUtc = default;
			DateTime? lastAccessUtc = default;
			string? json = default;

			Dictionary<string, int> columns = new Dictionary<string, int>();
			for (int i = 0; i < reader.FieldCount; i++) {
				columns.Add(reader.GetName(i), i);
			}


			if (!reader.IsDBNull("uuid")) {
				uuid = reader.GetGuid("uuid");
			}
			if (!reader.IsDBNull("contact-id")) {
				contactId = reader.GetGuid("contact-id");
			}
			if (!reader.IsDBNull("agent-description")) {
				agentDescription = reader.GetString("agent-description");
			}
			if (!reader.IsDBNull("ip-address")) {
				ipAddress = reader.GetString("ip-address");
			}
			if (!reader.IsDBNull("created-utc")) {
				createdUtc = reader.GetDateTime(columns["created-utc"]);
			}
			if (!reader.IsDBNull("last-access-utc")) {
				lastAccessUtc = reader.GetDateTime(columns["last-access-utc"]);
			}
			if (!reader.IsDBNull("json")) {
				json = reader.GetString("json");
			}



			return new BillingSessions(
				Uuid: uuid,
				ContactId: contactId,
				AgentDescription: agentDescription,
				IpAddress: ipAddress,
				CreatedUtc: createdUtc,
				LastAccessUtc: lastAccessUtc,
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

			if (db.TableExists("billing-sessions")) {
				Log.Debug($"----- Table \"billing-sessions\" exists.");
			} else {
				Log.Information($"----- Table \"billing-sessions\" doesn't exist, creating.");

				using NpgsqlCommand cmd = new NpgsqlCommand(@"
					CREATE TABLE ""public"".""billing-sessions"" (
						""uuid"" uuid DEFAULT public.uuid_generate_v1() NOT NULL,
						""contact-id"" uuid NOT NULL,
						""agent-description"" character varying(255) NOT NULL,
						""ip-address"" character varying(255),
						""created-utc"" timestamp without time zone DEFAULT now() NOT NULL,
						""last-access-utc"" timestamp without time zone DEFAULT now() NOT NULL,
						""json"" json DEFAULT '{}'::json NOT NULL,
						CONSTRAINT ""billing_sessions_pk"" PRIMARY KEY(""uuid"")
					) WITH(oids = false);
					", db);
				cmd.ExecuteNonQuery();
			}


			if (insertDefaultContents) {
				// None
			}





		}















	}
}
