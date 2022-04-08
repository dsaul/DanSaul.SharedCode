using Npgsql;
using System.Text;
using System.Data;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Serilog;

namespace SharedCode.DatabaseSchemas
{
	public record LabourTypes(Guid? Id, string? Json, string? SearchString, string? LastModifiedIso8601) : JSONTable(Id, Json, SearchString, LastModifiedIso8601)
	{
		public const string kJsonKeyName = "name";
		public const string kJsonKeyIcon = "icon";
		public const string kJsonKeyDescription = "description";
		public const string kJsonKeyDefault = "default";
		public const string kJsonKeyIsBillable = "isBillable";
		public const string kJsonKeyIsHoliday = "isHoliday";
		public const string kJsonKeyIsNonBillable = "isNonBillable";
		public const string kJsonKeyIsException = "isException";
		public const string kJsonKeyIsPayOutBanked = "isPayOutBanked";

		public static Dictionary<Guid, LabourTypes> ForId(NpgsqlConnection connection, Guid id) {

			Dictionary<Guid, LabourTypes> ret = new Dictionary<Guid, LabourTypes>();

			string sql = @"SELECT * from ""labour-types"" WHERE id = @id";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			cmd.Parameters.AddWithValue("@id", id);




			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					LabourTypes obj = LabourTypes.FromDataReader(reader);
					if (obj.Id == null) {
						continue;
					}
					ret.Add(obj.Id.Value, obj);
				}
			}

			return ret;
		}


		public static Dictionary<Guid, LabourTypes> ForBillableAndDefault(NpgsqlConnection connection) {

			Dictionary<Guid, LabourTypes> ret = new Dictionary<Guid, LabourTypes>();
			
			string sql = @"SELECT * FROM ""labour-types"" WHERE (json->>'default')::boolean = true AND (json->>'isBillable')::boolean = true";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			
			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					LabourTypes obj = LabourTypes.FromDataReader(reader);
					if (obj.Id == null) {
						continue;
					}
					ret.Add(obj.Id.Value, obj);
				}
			}

			return ret;
		}

		public static Dictionary<Guid, LabourTypes> All(NpgsqlConnection connection) {

			Dictionary<Guid, LabourTypes> ret = new Dictionary<Guid, LabourTypes>();

			string sql = @"SELECT * from ""labour-types""";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					LabourTypes obj = LabourTypes.FromDataReader(reader);
					if (obj.Id == null) {
						continue;
					}
					ret.Add(obj.Id.Value, obj);
				}
			}

			return ret;
		}

		public static Dictionary<Guid, LabourTypes> ForIds(NpgsqlConnection connection, IEnumerable<Guid> ids) {

			Guid[] idsArr = ids.ToArray();

			Dictionary<Guid, LabourTypes> ret = new Dictionary<Guid, LabourTypes>();
			if (idsArr.Length == 0)
				return ret;

			List<string> valNames = new List<string>();
			for (int i = 0; i < idsArr.Length; i++) {
				valNames.Add($"@val{i}");
			}

			string sql = $"SELECT * from \"labour-types\" WHERE id IN ({string.Join(", ", valNames)})";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			for (int i = 0; i < valNames.Count; i++) {
				cmd.Parameters.AddWithValue(valNames[i], idsArr[i]);
			}

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					LabourTypes obj = LabourTypes.FromDataReader(reader);
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



			string sql = $"DELETE FROM \"labour-types\" WHERE \"id\" IN ({string.Join(", ", valNames)})";
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
			Dictionary<Guid, LabourTypes> updateObjects,
			out List<Guid> callerResponse,
			out Dictionary<Guid, LabourTypes> toSendToOthers,
			bool printDots = false
			) {

			callerResponse = new List<Guid>();
			toSendToOthers = new Dictionary<Guid, LabourTypes>();

			foreach (KeyValuePair<Guid, LabourTypes> kvp in updateObjects) {

				string sql = @"
					INSERT INTO
						public.""labour-types""
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



		public static LabourTypes FromDataReader(NpgsqlDataReader reader) {

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

			return new LabourTypes(
				Id: id,
				Json: json,
				SearchString: searchString,
				LastModifiedIso8601: lastModifiedIso8601
				);
		}

		[JsonIgnore]
		public bool IsBillable
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return false;
				}

				JToken? tok = root["isBillable"];
				if (null == tok || tok.Type == JTokenType.Null) {
					return false;
				}

				return tok.Value<bool>();
			}
		}

		[JsonIgnore]
		public bool IsHoliday
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return false;
				}

				JToken? tok = root["isHoliday"];
				if (null == tok || tok.Type == JTokenType.Null) {
					return false;
				}

				return tok.Value<bool>();
			}
		}

		[JsonIgnore]
		public bool IsNonBillable
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return false;
				}

				JToken? tok = root["isNonBillable"];
				if (null == tok || tok.Type == JTokenType.Null) {
					return false;
				}

				return tok.Value<bool>();
			}
		}

		[JsonIgnore]
		public bool IsException
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return false;
				}

				JToken? tok = root["isException"];
				if (null == tok || tok.Type == JTokenType.Null) {
					return false;
				}

				return tok.Value<bool>();
			}
		}

		[JsonIgnore]
		public bool IsPayOutBanked
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return false;
				}

				JToken? tok = root["isPayOutBanked"];
				if (null == tok || tok.Type == JTokenType.Null) {
					return false;
				}

				return tok.Value<bool>();
			}
		}


		[JsonIgnore]
		public bool Default
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return false;
				}

				JToken? tok = root["default"];
				if (null == tok || tok.Type == JTokenType.Null) {
					return false;
				}

				return tok.Value<bool>();
			}
		}

		[JsonIgnore]
		public string? Description
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root["description"];
				if (null == tok || tok.Type == JTokenType.Null) {
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
		public string? Icon
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root["icon"];
				if (null == tok || tok.Type == JTokenType.Null) {
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
		public string? Name
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root["name"];
				if (null == tok || tok.Type == JTokenType.Null) {
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
				return GenerateSearchString(Name, Description);
			}
		}

		public static string GenerateSearchString(string? name, string? description) {
			return $"{name} {description}";
		}





		public static void VerifyRepairTable(NpgsqlConnection dpDB, bool insertDefaultContents = false) {

			if (dpDB.TableExists("labour-types")) {
				Log.Debug($"----- Table \"labour-types\" exists.");
			} else {
				Log.Debug($"----- Table \"labour-types\" doesn't exist, creating.");

				using NpgsqlCommand cmd = new NpgsqlCommand(@"
					CREATE TABLE ""public"".""labour-types"" (
						""id"" uuid DEFAULT uuid_generate_v1() NOT NULL,
						""json"" json DEFAULT '{}' NOT NULL,
						""search-string"" character varying DEFAULT '',
						""last-modified-ISO8601"" character varying DEFAULT timestamp_iso8601(now(), 'utc') NOT NULL,
						CONSTRAINT ""labour_types_pk"" PRIMARY KEY(""id"")
					) WITH(oids = false);
					", dpDB);
				cmd.ExecuteNonQuery();
			}

			Console.Write("----- Table \"labour-types\": ");

			if (insertDefaultContents) {
				Console.Write("------ Insert default contents. ");

				Dictionary<Guid, LabourTypes> updateObjects = new Dictionary<Guid, LabourTypes>();


				// Billable
				{
					string lastModified = DateTime.UtcNow.ToString("o", Culture.DevelopmentCulture);
					Guid id = Guid.NewGuid();
					LabourTypes entry = new LabourTypes(
						Id: id,
						Json: new JObject {
							[kJsonKeyName] = "Billable",
							[kJsonKeyIcon] = "attach_money",
							[kJsonKeyDescription] = "Time that will be billed to a client.",
							[kJsonKeyDefault] = true,
							[kJsonKeyIsBillable] = true,
							[kJsonKeyIsHoliday] = false,
							[kJsonKeyIsNonBillable] = false,
							[kJsonKeyIsException] = false,
							[kJsonKeyIsPayOutBanked] = false,
						}.ToString(),
						LastModifiedIso8601: lastModified,
						SearchString: ""
					);
					updateObjects.Add(id, entry);
				}

				// Exception
				{
					string lastModified = DateTime.UtcNow.ToString("o", Culture.DevelopmentCulture);
					Guid id = Guid.NewGuid();
					LabourTypes entry = new LabourTypes(
						Id: id,
						Json: new JObject {
							[kJsonKeyName] = "Exception",
							[kJsonKeyIcon] = "info",
							[kJsonKeyDescription] = "Non planned events.",
							[kJsonKeyDefault] = true,
							[kJsonKeyIsBillable] = false,
							[kJsonKeyIsHoliday] = false,
							[kJsonKeyIsNonBillable] = false,
							[kJsonKeyIsException] = true,
							[kJsonKeyIsPayOutBanked] = false,
						}.ToString(),
						LastModifiedIso8601: lastModified,
						SearchString: ""
					);
					updateObjects.Add(id, entry);
				}

				// Holiday
				{
					string lastModified = DateTime.UtcNow.ToString("o", Culture.DevelopmentCulture);
					Guid id = Guid.NewGuid();
					LabourTypes entry = new LabourTypes(
						Id: id,
						Json: new JObject {
							[kJsonKeyName] = "Holiday",
							[kJsonKeyIcon] = "beach_access",
							[kJsonKeyDescription] = "Re-occuring standard days off, paid and not paid.",
							[kJsonKeyDefault] = true,
							[kJsonKeyIsBillable] = false,
							[kJsonKeyIsHoliday] = true,
							[kJsonKeyIsNonBillable] = false,
							[kJsonKeyIsException] = false,
							[kJsonKeyIsPayOutBanked] = false,
						}.ToString(),
						LastModifiedIso8601: lastModified,
						SearchString: ""
					);
					updateObjects.Add(id, entry);
				}


				// Pay Out Banked
				{
					string lastModified = DateTime.UtcNow.ToString("o", Culture.DevelopmentCulture);
					Guid id = Guid.NewGuid();
					LabourTypes entry = new LabourTypes(
						Id: id,
						Json: new JObject {
							[kJsonKeyName] = "Pay Out Banked",
							[kJsonKeyIcon] = "360",
							[kJsonKeyDescription] = "",
							[kJsonKeyDefault] = true,
							[kJsonKeyIsBillable] = false,
							[kJsonKeyIsHoliday] = false,
							[kJsonKeyIsNonBillable] = false,
							[kJsonKeyIsException] = false,
							[kJsonKeyIsPayOutBanked] = true,
						}.ToString(),
						LastModifiedIso8601: lastModified,
						SearchString: ""
					);
					updateObjects.Add(id, entry);
				}

				// Non Billable
				{
					string lastModified = DateTime.UtcNow.ToString("o", Culture.DevelopmentCulture);
					Guid id = Guid.NewGuid();
					LabourTypes entry = new LabourTypes(
						Id: id,
						Json: new JObject {
							[kJsonKeyName] = "Non Billable",
							[kJsonKeyIcon] = "money_off",
							[kJsonKeyDescription] = "Time that will be billed to your company.",
							[kJsonKeyDefault] = true,
							[kJsonKeyIsBillable] = false,
							[kJsonKeyIsHoliday] = false,
							[kJsonKeyIsNonBillable] = true,
							[kJsonKeyIsException] = false,
							[kJsonKeyIsPayOutBanked] = false,
						}.ToString(),
						LastModifiedIso8601: lastModified,
						SearchString: ""
					);
					updateObjects.Add(id, entry);
				}














				Console.Write(" saving");
				LabourTypes.Upsert(dpDB, updateObjects, out _, out _, true);
			}

			{
				Console.Write("------ Repair Existing ");
				Dictionary<Guid, LabourTypes> updateObjects = new Dictionary<Guid, LabourTypes>();


				Dictionary<Guid, LabourTypes> all = LabourTypes.All(dpDB);

				foreach (KeyValuePair<Guid, LabourTypes> kvp in all) {

					JObject? root = kvp.Value.JsonObject;
					if (null == root)
						continue;

					JToken? lastModifiedInJSONTok = root["lastModifiedISO8601"];

					LabourTypes obj = new LabourTypes(
							Id: kvp.Key,
							Json: root.ToString(Newtonsoft.Json.Formatting.Indented),
							LastModifiedIso8601: null == lastModifiedInJSONTok ? kvp.Value.LastModifiedIso8601 : lastModifiedInJSONTok.Value<string>(),
							SearchString: kvp.Value.GeneratedSearchString
							);

					Console.Write(".");

					updateObjects.Add(kvp.Key, obj);
				}

				Console.Write(" saving");
				LabourTypes.Upsert(dpDB, updateObjects, out _, out _, true);
			}
			

			Log.Debug(" done.");
		}




	}
}
