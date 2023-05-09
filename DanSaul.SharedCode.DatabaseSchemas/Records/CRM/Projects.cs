using Npgsql;
using System.Text;
using System.Data;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Serilog;
using DanSaul.SharedCode.Npgsql;

namespace SharedCode.DatabaseSchemas
{
	public record Projects(Guid? Id, string? Json, string? SearchString, string? LastModifiedIso8601) : JSONTable(Id, Json, SearchString, LastModifiedIso8601)
	{
		public static Dictionary<Guid, Projects> ForId(NpgsqlConnection connection, Guid id) {

			Dictionary<Guid, Projects> ret = new Dictionary<Guid, Projects>();

			string sql = @"SELECT * from ""projects"" WHERE id = @id";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			cmd.Parameters.AddWithValue("@id", id);




			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					Projects obj = Projects.FromDataReader(reader);
					if (obj.Id == null) {
						continue;
					}
					ret.Add(obj.Id.Value, obj);
				}
			}

			return ret;
		}


		public static Dictionary<Guid, Projects> All(NpgsqlConnection connection) {

			Dictionary<Guid, Projects> ret = new Dictionary<Guid, Projects>();

			string sql = @"SELECT * from ""projects""";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					Projects obj = Projects.FromDataReader(reader);
					if (obj.Id == null) {
						continue;
					}
					ret.Add(obj.Id.Value, obj);
				}
			}

			return ret;


		}

		public static Dictionary<Guid, Projects> ForAddressPart(NpgsqlConnection connection, string addressPart) {

			Dictionary<Guid, Projects> ret = new Dictionary<Guid, Projects>();

			string addrPart = $"%{addressPart}%";

			string sql = @"
				SELECT 
					p.*
				FROM 
					""projects"" p, 
					json_array_elements(p.json->'addresses') obj
				WHERE 
					obj.value->>'value' ILIKE @addrPart
				";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			cmd.Parameters.AddWithValue("@addrPart", addrPart);

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					Projects obj = Projects.FromDataReader(reader);
					if (obj.Id == null) {
						continue;
					}
					ret.Add(obj.Id.Value, obj);
				}
			}

			return ret;


		}


		public static Dictionary<Guid, Projects> ForIds(NpgsqlConnection connection, IEnumerable<Guid> ids) {

			Guid[] idsArr = ids.ToArray();

			Dictionary<Guid, Projects> ret = new Dictionary<Guid, Projects>();
			if (idsArr.Length == 0)
				return ret;

			List<string> valNames = new List<string>();
			for (int i = 0; i < idsArr.Length; i++) {
				valNames.Add($"@val{i}");
			}



			string sql = $"SELECT * from \"projects\" WHERE id IN ({string.Join(", ", valNames)})";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			for (int i = 0; i < valNames.Count; i++) {
				cmd.Parameters.AddWithValue(valNames[i], idsArr[i]);
			}

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					Projects obj = Projects.FromDataReader(reader);
					if (obj.Id == null) {
						continue;
					}
					ret.Add(obj.Id.Value, obj);
				}
			}

			return ret;


		}

		public static Dictionary<Guid, Projects> SelectForParentId(NpgsqlConnection connection, Guid? parentId) {

			Dictionary<Guid, Projects> ret = new Dictionary<Guid, Projects>();
			if (parentId == null)
				return ret;

			string sql = $"SELECT * FROM \"projects\" WHERE json ->> 'parentId' = @parentId";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			cmd.Parameters.AddWithValue("@parentId", parentId.Value.ToString());


			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					Projects obj = Projects.FromDataReader(reader);
					if (obj.Id == null) {
						continue;
					}
					ret.Add(obj.Id.Value, obj);
				}
			}

			return ret;


		}


		public static void GetParentProjectMap(NpgsqlConnection connection, out Dictionary<Guid, List<Guid>> parentAsKey, out Dictionary<Guid, List<Guid>> childAsKey) {

			parentAsKey = new Dictionary<Guid, List<Guid>>();
			childAsKey = new Dictionary<Guid, List<Guid>>();

			string sql = $"SELECT id, (json ->> 'parentId')::uuid AS \"parentId\" FROM \"projects\" WHERE json ->> 'parentId' IS NOT NULL";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			//cmd.Parameters.AddWithValue("@parentId", parentId.Value.ToString());


			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {

					Guid? childId = null;
					if (!reader.IsDBNull("id")) {
						childId = reader.GetGuid("id");
					}

					Guid? parentId = null;
					if (!reader.IsDBNull("parentId")) {
						parentId = reader.GetGuid("parentId");
					}

					if (null == childId || null == parentId) {
						continue;
					}


					if (!parentAsKey.ContainsKey(parentId.Value)) {
						parentAsKey.Add(parentId.Value, new List<Guid>());
					}

					if (!childAsKey.ContainsKey(childId.Value)) {
						childAsKey.Add(childId.Value, new List<Guid>());
					}


					parentAsKey[parentId.Value].Add(childId.Value);
					childAsKey[childId.Value].Add(parentId.Value);

				}
			}



		}


		public static Dictionary<Guid, Projects> RecursiveChildProjectsOfId(NpgsqlConnection connection, Guid rootProject) {

			Dictionary<Guid, Projects> projects = new Dictionary<Guid, Projects>();


			// Find root.
			var res = Projects.ForId(connection, rootProject);
			if (res.Count == 0) {
				return projects;
			}

			Projects root = res.FirstOrDefault().Value;
			if (null == root) {
				return projects;
			}

			if (null != root.Id)
				projects.Add(root.Id.Value, root);

			void RecursiveFn(Projects project) {

				if (null == project) {
					return;
				}
			
				var children = SelectForParentId(connection, project.Id);
				if (children.Count == 0) {
					return; 
				}
			
				foreach (KeyValuePair<Guid, Projects> kvp in children) {

					bool found = projects.ContainsKey(kvp.Key);
					
					if (false == found) {
						projects.Add(kvp.Key, kvp.Value);
						RecursiveFn(kvp.Value);
					}
				}

			}

			RecursiveFn(root);
		
			return projects;
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



			string sql = $"DELETE FROM \"projects\" WHERE \"id\" IN ({string.Join(", ", valNames)})";
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
			Dictionary<Guid, Projects> updateObjects,
			out List<Guid> callerResponse,
			out Dictionary<Guid, Projects> toSendToOthers,
			bool printDots = false
			) {

			callerResponse = new List<Guid>();
			toSendToOthers = new Dictionary<Guid, Projects>();

			foreach (KeyValuePair<Guid, Projects> kvp in updateObjects) {

				string sql = @"
					INSERT INTO
						public.""projects""
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



		public static Projects FromDataReader(NpgsqlDataReader reader) {

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

			return new Projects(
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
		public List<Address> Addresses
		{
			get {
				List<Address> ret = new List<Address>();


				JObject? root = JsonObject;

				if (null == root) {
					return ret;
				}

				JArray? arr = root["addresses"] as JArray;
				if (null == arr) {
					return ret;
				}

				foreach (JObject addr in arr) {

					Guid? id = null;


					if (Guid.TryParse(addr.Value<string>("id"), out Guid tmp))
						id = tmp;



					ret.Add(new Address(
						id,
						addr.Value<string>("label"),
						addr.Value<string>("value")));

				}

				return ret;
			}
		}

		[JsonIgnore]
		public List<LabeledCompanyId> Companies
		{
			get {
				List<LabeledCompanyId> ret = new List<LabeledCompanyId>();


				JObject? root = JsonObject;

				if (null == root) {
					return ret;
				}

				JArray? arr = root["companies"] as JArray;
				if (null == arr) {
					return ret;
				}

				foreach (JObject addr in arr) {

					Guid? id = null;
					Guid? value = null;

					if (Guid.TryParse(addr.Value<string>("id"), out Guid tmp))
						id = tmp;
					if (Guid.TryParse(addr.Value<string>("value"), out tmp))
						value = tmp;



					ret.Add(new LabeledCompanyId(
						id,
						addr.Value<string>("label"),
						value));

				}

				return ret;
			}
		}

		[JsonIgnore]
		public List<LabeledContactId> Contacts
		{
			get {
				List<LabeledContactId> ret = new List<LabeledContactId>();


				JObject? root = JsonObject;

				if (null == root) {
					return ret;
				}

				JArray? arr = root["contacts"] as JArray;
				if (null == arr) {
					return ret;
				}

				foreach (JObject addr in arr) {

					Guid? id = null;
					Guid? value = null;

					if (Guid.TryParse(addr.Value<string>("id"), out Guid tmp))
						id = tmp;
					if (Guid.TryParse(addr.Value<string>("value"), out tmp))
						value = tmp;


					ret.Add(new LabeledContactId(
						id,
						addr.Value<string>("label"),
						value));

				}

				return ret;
			}
		}

		[JsonIgnore]
		public string AddressesSearchString
		{
			get {
				StringBuilder sb = new StringBuilder();

				foreach (Address address in Addresses) {
					sb.Append($" {address.Label}: {address.Value} ");
				}

				return sb.ToString();
			}
		}


		[JsonIgnore]
		public Guid? StatusId
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root["statusId"];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				string? str = tok.Value<string>();
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
		public Guid? ParentId
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root["parentId"];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				string? str = tok.Value<string>();
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
		public bool HasStartISO8601
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return false;
				}

				JToken? tok = root["hasStartISO8601"];
				if (null == tok || tok.Type == JTokenType.Null) {
					return false;
				}

				return tok.Value<bool>();
			}
		}

		public enum StartTimeModeE
		{
			None = 0,
			MorningFirstThing,
			MorningSecondThing,
			AfternoonFirstThing,
			AfternoonSecondThing,
			Time,
		};


		[JsonIgnore]
		public StartTimeModeE StartTimeMode
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return StartTimeModeE.None;
				}

				JToken? tok = root["startTimeMode"];
				if (null == tok || tok.Type == JTokenType.Null) {
					return StartTimeModeE.None;
				}

				string? str = tok.Value<string>();
				if (string.IsNullOrWhiteSpace(str)) {
					return StartTimeModeE.None;
				}

				return str switch {
					"morning-first-thing" => StartTimeModeE.MorningFirstThing,
					"morning-second-thing" => StartTimeModeE.MorningSecondThing,
					"afternoon-first-thing" => StartTimeModeE.AfternoonFirstThing,
					"afternoon-second-thing" => StartTimeModeE.AfternoonSecondThing,
					"time" => StartTimeModeE.Time,
					_ => StartTimeModeE.None,
				};
			}
		}

		[JsonIgnore]
		public string? StartISO8601
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root["startISO8601"];
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
		public bool HasEndISO8601
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return false;
				}

				JToken? tok = root["hasEndISO8601"];
				if (null == tok || tok.Type == JTokenType.Null) {
					return false;
				}

				return tok.Value<bool>();
			}
		}

		public enum EndTimeModeE
		{
			None = 0,
			Time,
		};

		[JsonIgnore]
		public EndTimeModeE EndTimeMode
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return EndTimeModeE.None;
				}

				JToken? tok = root["endTimeMode"];
				if (null == tok || tok.Type == JTokenType.Null) {
					return EndTimeModeE.None;
				}

				string? str = tok.Value<string>();
				if (string.IsNullOrWhiteSpace(str)) {
					return EndTimeModeE.None;
				}

				return str switch {
					"time" => EndTimeModeE.Time,
					_ => EndTimeModeE.None,
				};
			}
		}

		[JsonIgnore]
		public string? EndISO8601
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root["endISO8601"];
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
		public bool ForceLabourAsExtra
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return false;
				}

				JToken? tok = root["forceLabourAsExtra"];
				if (null == tok || tok.Type == JTokenType.Null) {
					return false;
				}

				return tok.Value<bool>();
			}
		}


		[JsonIgnore]
		public bool ForceAssignmentsToUseProjectSchedule
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return false;
				}

				JToken? tok = root["forceAssignmentsToUseProjectSchedule"];
				if (null == tok || tok.Type == JTokenType.Null) {
					return false;
				}

				return tok.Value<bool>();
			}
		}




		[JsonIgnore]
		public string GeneratedSearchString
		{
			get {
				return GenerateSearchString(Name, AddressesSearchString);
			}
		}

		public static string GenerateSearchString(string? name, string? addressesSearchString) {
			return $"{name} {addressesSearchString}";
		}


		public static void VerifyRepairTable(NpgsqlConnection dpDB, bool insertDefaultContents = false) {
			if (dpDB.TableExists("projects")) {
				Log.Debug($"----- Table \"projects\" exists.");
			} else {
				Log.Debug($"----- Table \"projects\" doesn't exist, creating.");

				using NpgsqlCommand cmd = new NpgsqlCommand(@"
					CREATE TABLE ""public"".""projects"" (
						""id"" uuid DEFAULT uuid_generate_v1() NOT NULL,
						""json"" json DEFAULT '{}' NOT NULL,
						""search-string"" character varying DEFAULT '',
						""last-modified-ISO8601"" character varying DEFAULT timestamp_iso8601(now(), 'utc') NOT NULL,
						CONSTRAINT ""projects_pk"" PRIMARY KEY(""id"")
					) WITH(oids = false);
					", dpDB);
				cmd.ExecuteNonQuery();

			}

			Console.Write("----- Table \"projects\": ");
			Dictionary<Guid, Projects> updateObjects = new Dictionary<Guid, Projects>();
			{
				Dictionary<Guid, Projects> all = Projects.All(dpDB);

				foreach (KeyValuePair<Guid, Projects> kvp in all) {

					JObject? root = kvp.Value.JsonObject;
					if (null == root)
						continue;
					JToken? lastModifiedInJSONTok = root["lastModifiedISO8601"];

					Projects obj = new Projects(
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
			Projects.Upsert(dpDB, updateObjects, out _, out _, true);

			Log.Debug(" done.");
		}









	}
}
