using Npgsql;
using System.Text;
using System.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using DanSaul.SharedCode.Npgsql;
using DanSaul.SharedCode;

namespace SharedCode.DatabaseSchemas
{
	public record LabourSubtypeNonBillable(Guid? Id, string? Json, string? SearchString, string? LastModifiedIso8601) : JSONTable(Id, Json, SearchString, LastModifiedIso8601)
	{
		public const string kJsonKeyName = "name";
		public const string kJsonKeyDescription = "description";
		public const string kJsonKeyIcon = "icon";
		
		public static Dictionary<Guid, LabourSubtypeNonBillable> ForId(NpgsqlConnection connection, Guid id) {

			Dictionary<Guid, LabourSubtypeNonBillable> ret = new Dictionary<Guid, LabourSubtypeNonBillable>();

			string sql = @"SELECT * from ""labour-subtype-non-billable"" WHERE id = @id";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			cmd.Parameters.AddWithValue("@id", id);




			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					LabourSubtypeNonBillable obj = LabourSubtypeNonBillable.FromDataReader(reader);
					if (obj.Id == null) {
						continue;
					}
					ret.Add(obj.Id.Value, obj);
				}
			}

			return ret;
		}

		public static Dictionary<Guid, LabourSubtypeNonBillable> All(NpgsqlConnection connection) {

			Dictionary<Guid, LabourSubtypeNonBillable> ret = new Dictionary<Guid, LabourSubtypeNonBillable>();

			string sql = @"SELECT * from ""labour-subtype-non-billable""";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					LabourSubtypeNonBillable obj = LabourSubtypeNonBillable.FromDataReader(reader);
					if (obj.Id == null) {
						continue;
					}
					ret.Add(obj.Id.Value, obj);
				}
			}

			return ret;
		}

		public static Dictionary<Guid, LabourSubtypeNonBillable> ForIds(NpgsqlConnection connection, IEnumerable<Guid> ids) {

			Guid[] idsArr = ids.ToArray();

			Dictionary<Guid, LabourSubtypeNonBillable> ret = new Dictionary<Guid, LabourSubtypeNonBillable>();
			if (idsArr.Length == 0)
				return ret;

			List<string> valNames = new List<string>();
			for (int i = 0; i < idsArr.Length; i++) {
				valNames.Add($"@val{i}");
			}

			string sql = $"SELECT * from \"labour-subtype-non-billable\" WHERE id IN ({string.Join(", ", valNames)})";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			for (int i = 0; i < valNames.Count; i++) {
				cmd.Parameters.AddWithValue(valNames[i], idsArr[i]);
			}

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					LabourSubtypeNonBillable obj = LabourSubtypeNonBillable.FromDataReader(reader);
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



			string sql = $"DELETE FROM \"labour-subtype-non-billable\" WHERE \"id\" IN ({string.Join(", ", valNames)})";
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
			Dictionary<Guid, LabourSubtypeNonBillable> updateObjects,
			out List<Guid> callerResponse,
			out Dictionary<Guid, LabourSubtypeNonBillable> toSendToOthers,
			bool printDots = false
			) {

			callerResponse = new List<Guid>();
			toSendToOthers = new Dictionary<Guid, LabourSubtypeNonBillable>();

			foreach (KeyValuePair<Guid, LabourSubtypeNonBillable> kvp in updateObjects) {

				string sql = @"
					INSERT INTO
						public.""labour-subtype-non-billable""
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




		public static LabourSubtypeNonBillable FromDataReader(NpgsqlDataReader reader) {

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

			return new LabourSubtypeNonBillable(
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

				string? str = tok.Value<string>();
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

				string? str = tok.Value<string>();
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

				string? str = tok.Value<string>();
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

			if (dpDB.TableExists("labour-subtype-non-billable")) {
				Log.Debug($"----- Table \"labour-subtype-non-billable\" exists.");
			} else {
				Log.Debug($"----- Table \"labour-subtype-non-billable\" doesn't exist, creating.");

				using NpgsqlCommand cmd = new NpgsqlCommand(@"
					CREATE TABLE ""public"".""labour-subtype-non-billable"" (
						""id"" uuid DEFAULT uuid_generate_v1() NOT NULL,
						""json"" json DEFAULT '{}' NOT NULL,
						""search-string"" character varying DEFAULT '',
						""last-modified-ISO8601"" character varying DEFAULT timestamp_iso8601(now(), 'utc') NOT NULL,
						CONSTRAINT ""labour_subtype_non_billable_pk"" PRIMARY KEY(""id"")
					) WITH(oids = false);
					", dpDB);
				cmd.ExecuteNonQuery();
			}

			Console.Write("----- Table \"labour-subtype-non-billable\": ");


			if (insertDefaultContents) {
				Console.Write("------ Insert default contents. ");

				Dictionary<Guid, LabourSubtypeNonBillable> updateObjects = new Dictionary<Guid, LabourSubtypeNonBillable>();

				// Other
				{
					string lastModified = DateTime.UtcNow.ToString("o", Culture.DevelopmentCulture);
					Guid id = Guid.NewGuid();
					LabourSubtypeNonBillable entry = new LabourSubtypeNonBillable(
						Id: id,
						Json: new JObject {
							[kJsonKeyName] = "Other",
							[kJsonKeyDescription] = "Some other non billable task.",
							[kJsonKeyIcon] = "info",
						}.ToString(),
						LastModifiedIso8601: lastModified,
						SearchString: ""
					);
					updateObjects.Add(id, entry);
				}


				// Training
				{
					string lastModified = DateTime.UtcNow.ToString("o", Culture.DevelopmentCulture);
					Guid id = Guid.NewGuid();
					LabourSubtypeNonBillable entry = new LabourSubtypeNonBillable(
						Id: id,
						Json: new JObject {
							[kJsonKeyName] = "Training",
							[kJsonKeyDescription] = "Time spent learning something.",
							[kJsonKeyIcon] = "info",
						}.ToString(),
						LastModifiedIso8601: lastModified,
						SearchString: ""
					);
					updateObjects.Add(id, entry);
				}

				// Programming
				{
					string lastModified = DateTime.UtcNow.ToString("o", Culture.DevelopmentCulture);
					Guid id = Guid.NewGuid();
					LabourSubtypeNonBillable entry = new LabourSubtypeNonBillable(
						Id: id,
						Json: new JObject {
							[kJsonKeyName] = "Programming",
							[kJsonKeyDescription] = "The writing of computer programs or other technical work.",
							[kJsonKeyIcon] = "info",
						}.ToString(),
						LastModifiedIso8601: lastModified,
						SearchString: ""
					);
					updateObjects.Add(id, entry);
				}

				// Meeting
				{
					string lastModified = DateTime.UtcNow.ToString("o", Culture.DevelopmentCulture);
					Guid id = Guid.NewGuid();
					LabourSubtypeNonBillable entry = new LabourSubtypeNonBillable(
						Id: id,
						Json: new JObject {
							[kJsonKeyName] = "Meeting",
							[kJsonKeyDescription] = "A company meeting that cannot be billed to a 3rd party.",
							[kJsonKeyIcon] = "people",
						}.ToString(),
						LastModifiedIso8601: lastModified,
						SearchString: ""
					);
					updateObjects.Add(id, entry);
				}

				// Administration
				{
					string lastModified = DateTime.UtcNow.ToString("o", Culture.DevelopmentCulture);
					Guid id = Guid.NewGuid();
					LabourSubtypeNonBillable entry = new LabourSubtypeNonBillable(
						Id: id,
						Json: new JObject {
							[kJsonKeyName] = "Administration",
							[kJsonKeyDescription] = "Clerical tasks, including drafting documents, telephones, scheduling, project management.",
							[kJsonKeyIcon] = "info",
						}.ToString(),
						LastModifiedIso8601: lastModified,
						SearchString: ""
					);
					updateObjects.Add(id, entry);
				}










				Console.Write(" saving");
				LabourSubtypeNonBillable.Upsert(dpDB, updateObjects, out _, out _, true);
			}

			
			{
				Console.Write("------ Repair Existing ");
				Dictionary<Guid, LabourSubtypeNonBillable> updateObjects = new Dictionary<Guid, LabourSubtypeNonBillable>();

				Dictionary<Guid, LabourSubtypeNonBillable> all = LabourSubtypeNonBillable.All(dpDB);

				foreach (KeyValuePair<Guid, LabourSubtypeNonBillable> kvp in all) {

					JObject? root = kvp.Value.JsonObject;
					if (null == root)
						continue;
					JToken? lastModifiedInJSONTok = root["lastModifiedISO8601"];

					LabourSubtypeNonBillable obj = new LabourSubtypeNonBillable(
							Id: kvp.Key,
							Json: root.ToString(Newtonsoft.Json.Formatting.Indented),
							LastModifiedIso8601: null == lastModifiedInJSONTok ? kvp.Value.LastModifiedIso8601 : lastModifiedInJSONTok.Value<string>(),
							SearchString: kvp.Value.GeneratedSearchString
							);



					Console.Write(".");

					updateObjects.Add(kvp.Key, obj);
				}


				Console.Write(" saving");
				LabourSubtypeNonBillable.Upsert(dpDB, updateObjects, out _, out _, true);

			}
			

			Log.Debug(" done.");
		}




















	}
}
