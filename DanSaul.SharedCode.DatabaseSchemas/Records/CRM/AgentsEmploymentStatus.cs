using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SharedCode.Extensions;
using System.Globalization;
using Serilog;

namespace SharedCode.DatabaseSchemas
{
	public record AgentsEmploymentStatus(
		Guid? Id,
		string? Json,
		string? SearchString,
		string? LastModifiedIso8601
		) : JSONTable(Id, Json, SearchString, LastModifiedIso8601)
	{

		public const string kJsonKeyName = "name";
		public const string kJsonKeyShouldBeListedInScheduler = "shouldBeListedInScheduler";
		public const string kJsonKeyIsDefault = "isDefault";
		public const string kJsonKeyIsContractor = "isContractor";
		public const string kJsonKeyIsEmployee = "isEmployee";
		public const string kJsonKeyIsActive = "isActive";

		public static Dictionary<Guid, AgentsEmploymentStatus> ForId(NpgsqlConnection connection, Guid id) {

			Dictionary<Guid, AgentsEmploymentStatus> ret = new Dictionary<Guid, AgentsEmploymentStatus>();

			string sql = @"SELECT * from ""agents-employment-status"" WHERE id = @id";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			cmd.Parameters.AddWithValue("@id", id);




			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					AgentsEmploymentStatus obj = AgentsEmploymentStatus.FromDataReader(reader);
					if (obj.Id == null) {
						continue;
					}
					ret.Add(obj.Id.Value, obj);
				}
			}

			return ret;
		}


		public static Dictionary<Guid, AgentsEmploymentStatus> All(NpgsqlConnection connection) {

			Dictionary<Guid, AgentsEmploymentStatus> ret = new Dictionary<Guid, AgentsEmploymentStatus>();

			string sql = @"SELECT * from ""agents-employment-status""";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					AgentsEmploymentStatus obj = AgentsEmploymentStatus.FromDataReader(reader);
					if (obj.Id == null) {
						continue;
					}
					ret.Add(obj.Id.Value, obj);
				}
			}

			return ret;


		}

		public static Dictionary<Guid, AgentsEmploymentStatus> ForIds(NpgsqlConnection connection, IEnumerable<Guid> ids) {

			Guid[] idsArr = ids.ToArray();

			Dictionary<Guid, AgentsEmploymentStatus> ret = new Dictionary<Guid, AgentsEmploymentStatus>();
			if (idsArr.Length == 0)
				return ret;

			List<string> valNames = new List<string>();
			for (int i = 0; i < idsArr.Length; i++) {
				valNames.Add($"@val{i}");
			}

			string sql = $"SELECT * from \"agents-employment-status\" WHERE id IN ({string.Join(", ", valNames)})";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			for (int i = 0; i < valNames.Count; i++) {
				cmd.Parameters.AddWithValue(valNames[i], idsArr[i]);
			}

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					AgentsEmploymentStatus obj = AgentsEmploymentStatus.FromDataReader(reader);
					if (obj.Id == null) {
						continue;
					}
					ret.Add(obj.Id.Value, obj);
				}
			}

			return ret;


		}

		public static Dictionary<Guid, AgentsEmploymentStatus> GetDefaultCurrentEmployee(NpgsqlConnection connection) {
			Dictionary<Guid, AgentsEmploymentStatus> ret = new Dictionary<Guid, AgentsEmploymentStatus>();

			string sql = @"SELECT *
				FROM 
					""agents-employment-status""
				WHERE 
					(json->>'isDefault')::boolean = true
				AND
					(json->>'isActive')::boolean = true
				AND
					(json->>'isEmployee')::boolean = true";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					AgentsEmploymentStatus obj = AgentsEmploymentStatus.FromDataReader(reader);
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



			string sql = $"DELETE FROM \"agents-employment-status\" WHERE \"id\" IN ({string.Join(", ", valNames)})";
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
			Dictionary<Guid, AgentsEmploymentStatus> updateObjects,
			out List<Guid> callerResponse,
			out Dictionary<Guid, AgentsEmploymentStatus> toSendToOthers,
			bool printDots = false
			) {

			callerResponse = new List<Guid>();
			toSendToOthers = new Dictionary<Guid, AgentsEmploymentStatus>();

			foreach (KeyValuePair<Guid, AgentsEmploymentStatus> kvp in updateObjects) {

				string sql = @"
					INSERT INTO
						public.""agents-employment-status""
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


		public static AgentsEmploymentStatus FromDataReader(NpgsqlDataReader reader) {

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

			return new AgentsEmploymentStatus(
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

				JToken? tok = root[kJsonKeyName];
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
		public bool? ShouldBeListedInScheduler
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyShouldBeListedInScheduler];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				return tok.Value<bool>();
			}
		}

		[JsonIgnore]
		public bool? IsDefault
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyIsDefault];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				return tok.Value<bool>();
			}
		}

		[JsonIgnore]
		public bool? IsContractor
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyIsContractor];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				return tok.Value<bool>();
			}
		}

		[JsonIgnore]
		public bool? IsEmployee
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyIsEmployee];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				return tok.Value<bool>();
			}
		}

		[JsonIgnore]
		public bool? IsActive
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyIsActive];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				return tok.Value<bool>();
			}
		}


		[JsonIgnore]
		public string GeneratedSearchString
		{
			get {
				return GenerateSearchString(Name);
			}
		}

		public static string GenerateSearchString(string? name) {
			return $"{name}";
		}

		public static void VerifyRepairTable(NpgsqlConnection dpDB, bool insertDefaultContents = false) {


			if (dpDB.TableExists("agents-employment-status")) {
				Log.Debug($"----- Table \"agents-employment-status\" exists.");
			} else {
				Log.Debug($"----- Table \"agents-employment-status\" doesn't exist, creating.");

				using NpgsqlCommand cmd = new NpgsqlCommand(@"
					CREATE TABLE ""public"".""agents-employment-status"" (
						""id"" uuid DEFAULT uuid_generate_v1() NOT NULL,
						""json"" json DEFAULT '{}' NOT NULL,
						""search-string"" character varying DEFAULT '',
						""last-modified-ISO8601"" character varying DEFAULT timestamp_iso8601(now(), 'utc') NOT NULL,
						CONSTRAINT ""agents_employment_status_pk"" PRIMARY KEY(""id"")
					) WITH(oids = false);
					", dpDB);
				cmd.ExecuteNonQuery();
			}

			// Repair

			Console.Write("----- Table \"agents-employment-status\": ");
			

			if (insertDefaultContents) {
				Console.Write("------ Insert default contents. ");

				Dictionary<Guid, AgentsEmploymentStatus> updateObjects = new Dictionary<Guid, AgentsEmploymentStatus>();

				// Non Employee
				{
					string lastModified = DateTime.UtcNow.ToString("o", SharedCode.Culture.Konstants.DevelopmentCulture);
					Guid id = Guid.NewGuid();
					AgentsEmploymentStatus entry = new AgentsEmploymentStatus(
						Id: id,
						Json: new JObject {
							[kJsonKeyName] = "Non Employee",
							[kJsonKeyShouldBeListedInScheduler] = false,
							[kJsonKeyIsDefault] = true,
							[kJsonKeyIsContractor] = false,
							[kJsonKeyIsEmployee] = false,
							[kJsonKeyIsActive] = false,
						}.ToString(),
						LastModifiedIso8601: lastModified,
						SearchString: ""
					);
					updateObjects.Add(id, entry);
				}

				// Former Employee
				{
					string lastModified = DateTime.UtcNow.ToString("o", SharedCode.Culture.Konstants.DevelopmentCulture);
					Guid id = Guid.NewGuid();
					AgentsEmploymentStatus entry = new AgentsEmploymentStatus(
						Id: id,
						Json: new JObject {
							[kJsonKeyName] = "Former Employee",
							[kJsonKeyShouldBeListedInScheduler] = false,
							[kJsonKeyIsDefault] = true,
							[kJsonKeyIsContractor] = false,
							[kJsonKeyIsEmployee] = true,
							[kJsonKeyIsActive] = false,
						}.ToString(),
						LastModifiedIso8601: lastModified,
						SearchString: ""
					);
					updateObjects.Add(id, entry);
				}

				// Current Contractor
				{
					string lastModified = DateTime.UtcNow.ToString("o", SharedCode.Culture.Konstants.DevelopmentCulture);
					Guid id = Guid.NewGuid();
					AgentsEmploymentStatus entry = new AgentsEmploymentStatus(
						Id: id,
						Json: new JObject {
							[kJsonKeyName] = "Current Contractor",
							[kJsonKeyShouldBeListedInScheduler] = true,
							[kJsonKeyIsDefault] = true,
							[kJsonKeyIsContractor] = true,
							[kJsonKeyIsEmployee] = false,
							[kJsonKeyIsActive] = true,
						}.ToString(),
						LastModifiedIso8601: lastModified,
						SearchString: ""
					);
					updateObjects.Add(id, entry);
				}

				// Current Employee
				{
					string lastModified = DateTime.UtcNow.ToString("o", SharedCode.Culture.Konstants.DevelopmentCulture);
					Guid id = Guid.NewGuid();
					AgentsEmploymentStatus entry = new AgentsEmploymentStatus(
						Id: id,
						Json: new JObject {
							[kJsonKeyName] = "Current Employee",
							[kJsonKeyShouldBeListedInScheduler] = true,
							[kJsonKeyIsDefault] = true,
							[kJsonKeyIsContractor] = false,
							[kJsonKeyIsEmployee] = true,
							[kJsonKeyIsActive] = true,
						}.ToString(),
						LastModifiedIso8601: lastModified,
						SearchString: ""
					);
					updateObjects.Add(id, entry);
				}

				// Former Contractor
				{
					string lastModified = DateTime.UtcNow.ToString("o", SharedCode.Culture.Konstants.DevelopmentCulture);
					Guid id = Guid.NewGuid();
					AgentsEmploymentStatus entry = new AgentsEmploymentStatus(
						Id: id,
						Json: new JObject {
							[kJsonKeyName] = "Former Contractor",
							[kJsonKeyShouldBeListedInScheduler] = false,
							[kJsonKeyIsDefault] = true,
							[kJsonKeyIsContractor] = true,
							[kJsonKeyIsEmployee] = false,
							[kJsonKeyIsActive] = false,
						}.ToString(),
						LastModifiedIso8601: lastModified,
						SearchString: ""
					);
					updateObjects.Add(id, entry);
				}

				// Non Field Employee
				{
					string lastModified = DateTime.UtcNow.ToString("o", SharedCode.Culture.Konstants.DevelopmentCulture);
					Guid id = Guid.NewGuid();
					AgentsEmploymentStatus entry = new AgentsEmploymentStatus(
						Id: id,
						Json: new JObject {
							[kJsonKeyName] = "Non Field Employee",
							[kJsonKeyShouldBeListedInScheduler] = false,
							[kJsonKeyIsDefault] = true,
							[kJsonKeyIsContractor] = false,
							[kJsonKeyIsEmployee] = true,
							[kJsonKeyIsActive] = true,
						}.ToString(),
						LastModifiedIso8601: lastModified,
						SearchString: ""
					);
					updateObjects.Add(id, entry);
				}

				Console.Write(" saving");
				AgentsEmploymentStatus.Upsert(dpDB, updateObjects, out _, out _, true);
			}
			


			{
				Console.Write("------ Repair Existing ");
				Dictionary<Guid, AgentsEmploymentStatus> updateObjects = new Dictionary<Guid, AgentsEmploymentStatus>();


				Dictionary<Guid, AgentsEmploymentStatus> all = AgentsEmploymentStatus.All(dpDB);

				foreach (KeyValuePair<Guid, AgentsEmploymentStatus> kvp in all) {

					JObject? root = kvp.Value.JsonObject;
					if (null == root)
						continue;

					JToken? lastModifiedInJSONTok = root["lastModifiedISO8601"];

					AgentsEmploymentStatus obj = new AgentsEmploymentStatus(
							Id: kvp.Key,
							Json: root.ToString(Newtonsoft.Json.Formatting.Indented),
							LastModifiedIso8601: null == lastModifiedInJSONTok ? kvp.Value.LastModifiedIso8601 : lastModifiedInJSONTok.Value<string>(),
							SearchString: kvp.Value.GeneratedSearchString
							);



					Console.Write(".");

					updateObjects.Add(kvp.Key, obj);
				}

				Console.Write(" saving");
				AgentsEmploymentStatus.Upsert(dpDB, updateObjects, out _, out _, true);
			}

			

			Log.Debug(" done.");
		}

	}
}
