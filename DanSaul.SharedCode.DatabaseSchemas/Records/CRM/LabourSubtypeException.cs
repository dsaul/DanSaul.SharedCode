using Npgsql;
using System.Text;
using System.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;

namespace SharedCode.DatabaseSchemas
{
	public record LabourSubtypeException(Guid? Id, string? Json, string? SearchString, string? LastModifiedIso8601) : JSONTable(Id, Json, SearchString, LastModifiedIso8601)
	{

		public const string kJsonKeyName = "name";
		public const string kJsonKeyDescription = "description";
		public const string kJsonKeyIcon = "icon";







		public static Dictionary<Guid, LabourSubtypeException> ForId(NpgsqlConnection connection, Guid id) {

			Dictionary<Guid, LabourSubtypeException> ret = new Dictionary<Guid, LabourSubtypeException>();

			string sql = @"SELECT * from ""labour-subtype-exception"" WHERE id = @id";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			cmd.Parameters.AddWithValue("@id", id);




			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					LabourSubtypeException obj = LabourSubtypeException.FromDataReader(reader);
					if (obj.Id == null) {
						continue;
					}
					ret.Add(obj.Id.Value, obj);
				}
			}

			return ret;
		}

		public static Dictionary<Guid, LabourSubtypeException> All(NpgsqlConnection connection) {

			Dictionary<Guid, LabourSubtypeException> ret = new Dictionary<Guid, LabourSubtypeException>();

			string sql = @"SELECT * from ""labour-subtype-exception""";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					LabourSubtypeException obj = LabourSubtypeException.FromDataReader(reader);
					if (obj.Id == null) {
						continue;
					}
					ret.Add(obj.Id.Value, obj);
				}
			}

			return ret;
		}

		public static Dictionary<Guid, LabourSubtypeException> ForIds(NpgsqlConnection connection, IEnumerable<Guid> ids) {

			Guid[] idsArr = ids.ToArray();

			Dictionary<Guid, LabourSubtypeException> ret = new Dictionary<Guid, LabourSubtypeException>();
			if (idsArr.Length == 0)
				return ret;

			List<string> valNames = new List<string>();
			for (int i = 0; i < idsArr.Length; i++) {
				valNames.Add($"@val{i}");
			}

			string sql = $"SELECT * from \"labour-subtype-exception\" WHERE id IN ({string.Join(", ", valNames)})";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			for (int i = 0; i < valNames.Count; i++) {
				cmd.Parameters.AddWithValue(valNames[i], idsArr[i]);
			}

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					LabourSubtypeException obj = LabourSubtypeException.FromDataReader(reader);
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



			string sql = $"DELETE FROM \"labour-subtype-exception\" WHERE \"id\" IN ({string.Join(", ", valNames)})";
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
			Dictionary<Guid, LabourSubtypeException> updateObjects,
			out List<Guid> callerResponse,
			out Dictionary<Guid, LabourSubtypeException> toSendToOthers,
			bool printDots = false
			) {

			callerResponse = new List<Guid>();
			toSendToOthers = new Dictionary<Guid, LabourSubtypeException>();

			foreach (KeyValuePair<Guid, LabourSubtypeException> kvp in updateObjects) {

				string sql = @"
					INSERT INTO
						public.""labour-subtype-exception""
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


		public static LabourSubtypeException FromDataReader(NpgsqlDataReader reader) {

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

			return new LabourSubtypeException(
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
		public string? Description
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyDescription];
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

				JToken? tok = root[kJsonKeyIcon];
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

			if (dpDB.TableExists("labour-subtype-exception")) {
				Log.Debug($"----- Table \"labour-subtype-exception\" exists.");
			} else {
				Log.Debug($"----- Table \"labour-subtype-exception\" doesn't exist, creating.");

				using NpgsqlCommand cmd = new NpgsqlCommand(@"
					CREATE TABLE ""public"".""labour-subtype-exception"" (
						""id"" uuid DEFAULT uuid_generate_v1() NOT NULL,
						""json"" json DEFAULT '{}' NOT NULL,
						""search-string"" character varying DEFAULT '',
						""last-modified-ISO8601"" character varying DEFAULT timestamp_iso8601(now(), 'utc') NOT NULL,
						CONSTRAINT ""labour_subtype_exception_pk"" PRIMARY KEY(""id"")
					) WITH(oids = false);
					", dpDB);
				cmd.ExecuteNonQuery();
			}


			Console.Write("----- Table \"labour-subtype-exception\": ");


			if (insertDefaultContents) {
				Console.Write("------ Insert default contents. ");

				Dictionary<Guid, LabourSubtypeException> updateObjects = new Dictionary<Guid, LabourSubtypeException>();


				// Bereavement
				{
					string lastModified = DateTime.UtcNow.ToString("o", Culture.DevelopmentCulture);
					Guid id = Guid.NewGuid();
					LabourSubtypeException entry = new LabourSubtypeException(
						Id: id,
						Json: new JObject {
							[kJsonKeyName] = "Bereavement",
							[kJsonKeyDescription] = "Time off when someone has died.",
							[kJsonKeyIcon] = "nature_people",
						}.ToString(),
						LastModifiedIso8601: lastModified,
						SearchString: ""
					);
					updateObjects.Add(id, entry);
				}


				// Sick
				{
					string lastModified = DateTime.UtcNow.ToString("o", Culture.DevelopmentCulture);
					Guid id = Guid.NewGuid();
					LabourSubtypeException entry = new LabourSubtypeException(
						Id: id,
						Json: new JObject {
							[kJsonKeyName] = "Sick",
							[kJsonKeyDescription] = "Time off due to sickness.",
							[kJsonKeyIcon] = "local_hospital",
						}.ToString(),
						LastModifiedIso8601: lastModified,
						SearchString: ""
					);
					updateObjects.Add(id, entry);
				}


				// Personal Day
				{
					string lastModified = DateTime.UtcNow.ToString("o", Culture.DevelopmentCulture);
					Guid id = Guid.NewGuid();
					LabourSubtypeException entry = new LabourSubtypeException(
						Id: id,
						Json: new JObject {
							[kJsonKeyName] = "Personal Day",
							[kJsonKeyDescription] = "Time off for unspecified reasons.",
							[kJsonKeyIcon] = "bathtub",
						}.ToString(),
						LastModifiedIso8601: lastModified,
						SearchString: ""
					);
					updateObjects.Add(id, entry);
				}

				// Family Leave
				{
					string lastModified = DateTime.UtcNow.ToString("o", Culture.DevelopmentCulture);
					Guid id = Guid.NewGuid();
					LabourSubtypeException entry = new LabourSubtypeException(
						Id: id,
						Json: new JObject {
							[kJsonKeyName] = "Family Leave",
							[kJsonKeyDescription] = "Time off to deal with family issues, such as children being sick, etc...",
							[kJsonKeyIcon] = "child_friendly",
						}.ToString(),
						LastModifiedIso8601: lastModified,
						SearchString: ""
					);
					updateObjects.Add(id, entry);
				}

				// Weather
				{
					string lastModified = DateTime.UtcNow.ToString("o", Culture.DevelopmentCulture);
					Guid id = Guid.NewGuid();
					LabourSubtypeException entry = new LabourSubtypeException(
						Id: id,
						Json: new JObject {
							[kJsonKeyName] = "Weather",
							[kJsonKeyDescription] = "Time off due to bad weather.",
							[kJsonKeyIcon] = "ac_unit",
						}.ToString(),
						LastModifiedIso8601: lastModified,
						SearchString: ""
					);
					updateObjects.Add(id, entry);
				}


				Console.Write(" saving");
				LabourSubtypeException.Upsert(dpDB, updateObjects, out _, out _, true);

			}


			{

				Console.Write("------ Repair Existing ");
				Dictionary<Guid, LabourSubtypeException> updateObjects = new Dictionary<Guid, LabourSubtypeException>();


				Dictionary<Guid, LabourSubtypeException> all = LabourSubtypeException.All(dpDB);

				foreach (KeyValuePair<Guid, LabourSubtypeException> kvp in all) {

					JObject? root = kvp.Value.JsonObject;
					if (null == root)
						continue;
					JToken? lastModifiedInJSONTok = root["lastModifiedISO8601"];

					LabourSubtypeException obj = new LabourSubtypeException(
							Id: kvp.Key,
							Json: root.ToString(Newtonsoft.Json.Formatting.Indented),
							LastModifiedIso8601: null == lastModifiedInJSONTok ? kvp.Value.LastModifiedIso8601 : lastModifiedInJSONTok.Value<string>(),
							SearchString: kvp.Value.GeneratedSearchString
							);



					Console.Write(".");

					updateObjects.Add(kvp.Key, obj);
				}

				Console.Write(" saving");
				LabourSubtypeException.Upsert(dpDB, updateObjects, out _, out _, true);
			}
			

			Log.Debug(" done.");
		}




























	}
}
