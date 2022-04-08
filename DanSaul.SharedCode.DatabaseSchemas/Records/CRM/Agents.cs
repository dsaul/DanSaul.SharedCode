using Npgsql;
using System.Text;
using System.Data;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Serilog;

namespace SharedCode.DatabaseSchemas
{
	public record Agents(
		Guid? Id, 
		string? Json, 
		string? SearchString, 
		string? LastModifiedIso8601
		) : JSONTable(Id, Json, SearchString, LastModifiedIso8601)
	{
		public const string kJsonKeyPhoneId = "phoneId";



		public static Dictionary<Guid, Agents> ForId(NpgsqlConnection connection, Guid id) {

			Dictionary<Guid, Agents> ret = new Dictionary<Guid, Agents>();

			string sql = @"SELECT * from ""agents"" WHERE id = @id";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			cmd.Parameters.AddWithValue("@id", id);




			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					Agents obj = Agents.FromDataReader(reader);
					if (obj.Id == null) {
						continue;
					}
					ret.Add(obj.Id.Value, obj);
				}
			}

			return ret;
		}


		public static Dictionary<Guid, Agents> ForPhoneId(NpgsqlConnection connection, string phoneId) {

			Dictionary<Guid, Agents> ret = new Dictionary<Guid, Agents>();

			string sql = @"SELECT * from ""agents"" WHERE json->>'phoneId' = @phoneId";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			cmd.Parameters.AddWithValue("@phoneId", phoneId);

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					Agents obj = Agents.FromDataReader(reader);
					if (obj.Id == null) {
						continue;
					}
					ret.Add(obj.Id.Value, obj);
				}
			}

			return ret;


		}

		public static Dictionary<Guid, Agents> All(NpgsqlConnection connection) {

			Dictionary<Guid, Agents> ret = new Dictionary<Guid, Agents>();

			string sql = @"SELECT * from ""agents""";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					Agents obj = Agents.FromDataReader(reader);
					if (obj.Id == null) {
						continue;
					}
					ret.Add(obj.Id.Value, obj);
				}
			}

			return ret;


		}

		public static Dictionary<Guid, Agents> ForIds(NpgsqlConnection connection, IEnumerable<Guid> ids) {

			Guid[] idsArr = ids.ToArray();

			Dictionary<Guid, Agents> ret = new Dictionary<Guid, Agents>();
			if (idsArr.Length == 0)
				return ret;

			List<string> valNames = new List<string>();
			for (int i = 0; i < idsArr.Length; i++) {
				valNames.Add($"@val{i}");
			}

			string sql = $"SELECT * from \"agents\" WHERE id IN ({string.Join(", ", valNames)})";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			for (int i = 0; i < valNames.Count; i++) {
				cmd.Parameters.AddWithValue(valNames[i], idsArr[i]);
			}

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					Agents obj = Agents.FromDataReader(reader);
					if (obj.Id == null) {
						continue;
					}
					ret.Add(obj.Id.Value, obj);
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



			string sql = $"DELETE FROM \"agents\" WHERE \"id\" IN ({string.Join(", ", valNames)})";
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


		public static void Upsert(
			NpgsqlConnection connection,
			Dictionary<Guid, Agents> updateObjects,
			out List<Guid> callerResponse,
			out Dictionary<Guid, Agents> toSendToOthers,
			bool printDots = false
			) {

			callerResponse = new List<Guid>();
			toSendToOthers = new Dictionary<Guid, Agents>();

			foreach (KeyValuePair<Guid, Agents> kvp in updateObjects) {

				string sql = @"
					INSERT INTO
						public.""agents""
						(
							""id"",
							""json"",
							""search-string"",
							""last-modified-ISO8601""
						)
					VALUES
						(
							@id,
							CAST(@json AS json),
							@searchString, 
							@lastModifiedISO8601
						)
					ON CONFLICT (""id"") DO UPDATE
						SET
							""json"" = CAST(excluded.json AS json),
							""search-string"" = excluded.""search-string"",
							""last-modified-ISO8601"" = excluded.""last-modified-ISO8601""
					";

				using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
				cmd.Parameters.AddWithValue("@id", kvp.Key);
				cmd.Parameters.AddWithValue("@json", string.IsNullOrWhiteSpace(kvp.Value.Json) ? (object)DBNull.Value : kvp.Value.Json);
				cmd.Parameters.AddWithValue("@searchString", string.IsNullOrWhiteSpace(kvp.Value.SearchString) ? (object)DBNull.Value : kvp.Value.SearchString);
				cmd.Parameters.AddWithValue("@lastModifiedISO8601", string.IsNullOrWhiteSpace(kvp.Value.LastModifiedIso8601) ? (object)DBNull.Value : kvp.Value.LastModifiedIso8601);

				int rowsAffected = cmd.ExecuteNonQuery();

				if (rowsAffected == 0) {
					if (printDots)
						Console.Write("!");
					continue;
				}

				toSendToOthers.Add(kvp.Key, kvp.Value);
				callerResponse.Add(kvp.Key);

				if (printDots)
					Console.Write(".");
			}



		}


		public static Agents FromDataReader(NpgsqlDataReader reader) {

			Guid? id = default;
			string? json = default;
			string? searchString = default;
			string? lastModifiedIso8601 = default;


			if (!reader.IsDBNull("id")) {
				id = reader.GetGuid("id");
			}
			if (!reader.IsDBNull("json")) {
				json = reader.GetString("json");
			}
			if (!reader.IsDBNull("search-string")) {
				searchString = reader.GetString("search-string");
			}
			if (!reader.IsDBNull("last-modified-ISO8601")) {
				lastModifiedIso8601 = reader.GetString("last-modified-ISO8601");
			}

			return new Agents(
				Id: id,
				Json: json,
				SearchString: searchString,
				LastModifiedIso8601: lastModifiedIso8601
				);
		}

		[JsonIgnore]
		public string? Name
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root["name"];
				if (null == tok) {
					return null;
				}

				string str = tok.Value<string>();
				if (string.IsNullOrWhiteSpace(str)) {
					return null;
				}

				return str;
			}
		}

		[JsonIgnore]
		public string? Title
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root["title"];
				if (null == tok) {
					return null;
				}

				string str = tok.Value<string>();
				if (string.IsNullOrWhiteSpace(str)) {
					return null;
				}

				return str;
			}
		}

		[JsonIgnore]
		public Guid? EmploymentStatusId
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root["employmentStatusId"];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				string str = tok.Value<string>();
				if (string.IsNullOrWhiteSpace(str)) {
					return null;
				}

				if (!Guid.TryParse(str, out Guid guid)) {
					return null;
				}

				return guid;
			}
		}

		[JsonIgnore]
		public decimal? HourlyWage
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root["hourlyWage"];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				return tok.Value<decimal>();
			}
		}

		[JsonIgnore]
		public string? NotificationSMSNumber
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root["notificationSMSNumber"];
				if (null == tok) {
					return null;
				}

				string str = tok.Value<string>();
				if (string.IsNullOrWhiteSpace(str)) {
					return null;
				}

				return str;
			}
		}

		[JsonIgnore]
		public string? PhoneId
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyPhoneId];
				if (null == tok) {
					return null;
				}

				string str = tok.Value<string>();
				if (string.IsNullOrWhiteSpace(str)) {
					return null;
				}

				return str;
			}
		}

		[JsonIgnore]
		public string? PhonePasscode
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root["phonePasscode"];
				if (null == tok) {
					return null;
				}

				string str = tok.Value<string>();
				if (string.IsNullOrWhiteSpace(str)) {
					return null;
				}

				return str;
			}
		}

		[JsonIgnore]
		public string GeneratedSearchString
		{
			get {
				return GenerateSearchString(Name, Title, NotificationSMSNumber);
			}
		}

		public static string GenerateSearchString(string? name, string? title, string? notificationSMSNumber) {
			return $"{name} {title} {notificationSMSNumber}";
		}

		public static void VerifyRepairTable(NpgsqlConnection dpDB, bool insertDefaultContents = false) {

			if (dpDB.TableExists("agents")) {
				Log.Debug($"----- Table \"agents\" exists.");
			} else {
				Log.Debug($"----- Table \"agents\" doesn't exist, creating.");

				using NpgsqlCommand cmd = new NpgsqlCommand(@"
					CREATE TABLE ""public"".""agents"" (
						""id"" uuid DEFAULT uuid_generate_v1() NOT NULL,
						""json"" json DEFAULT '{}' NOT NULL,
						""search-string"" character varying DEFAULT '',
						""last-modified-ISO8601"" character varying DEFAULT timestamp_iso8601(now(), 'utc') NOT NULL,
						CONSTRAINT ""agents_pk"" PRIMARY KEY(""id"")
					) WITH(oids = false);
					", dpDB);
				cmd.ExecuteNonQuery();
			}


			// Repair

			Console.Write("----- Table \"agents\": ");
			Dictionary<Guid, Agents> updateObjects = new Dictionary<Guid, Agents>();
			{
				Dictionary<Guid, Agents> all = Agents.All(dpDB);

				foreach (KeyValuePair<Guid, Agents> kvp in all) {

					JObject? root = kvp.Value.JsonObject;
					if (null == root)
						continue;

					JToken? lastModifiedInJSONTok = root["lastModifiedISO8601"];

					Agents obj = new Agents(
						Id: kvp.Key,
						Json: root.ToString(Newtonsoft.Json.Formatting.Indented),
						LastModifiedIso8601: null == lastModifiedInJSONTok ? kvp.Value.LastModifiedIso8601 : lastModifiedInJSONTok.Value<string>(),
						SearchString: kvp.Value.GeneratedSearchString
						);


					Console.Write(".");

					updateObjects.Add(kvp.Key, obj);
				}
			}
			Console.Write(" saving");
			Agents.Upsert(dpDB, updateObjects, out _, out _, true);

			Log.Debug(" done.");
		}

	}
}
