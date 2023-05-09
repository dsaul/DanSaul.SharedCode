using Npgsql;
using System.Text;
using System.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using DanSaul.SharedCode;
using DanSaul.SharedCode.Npgsql;

namespace SharedCode.DatabaseSchemas
{
	public record ProjectStatus(Guid? Id, string? Json, string? SearchString, string? LastModifiedIso8601) : JSONTable(Id, Json, SearchString, LastModifiedIso8601)
	{
		public const string kJsonKeyName = "name";
		public const string kJsonKeyIsOpen = "isOpen";
		public const string kJsonKeyIsAwaitingPayment = "isAwaitingPayment";
		public const string kJsonKeyIsClosed = "isClosed";
		public const string kJsonKeyIsNewProjectStatus = "isNewProjectStatus";


		




		public static Dictionary<Guid, ProjectStatus> ForId(NpgsqlConnection connection, Guid id) {

			Dictionary<Guid, ProjectStatus> ret = new Dictionary<Guid, ProjectStatus>();

			string sql = @"SELECT * from ""project-status"" WHERE id = @id";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			cmd.Parameters.AddWithValue("@id", id);




			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					ProjectStatus obj = ProjectStatus.FromDataReader(reader);
					if (obj.Id == null) {
						continue;
					}
					ret.Add(obj.Id.Value, obj);
				}
			}

			return ret;
		}

		public static Dictionary<Guid, ProjectStatus> All(NpgsqlConnection connection) {

			Dictionary<Guid, ProjectStatus> ret = new Dictionary<Guid, ProjectStatus>();

			string sql = @"SELECT * from ""project-status""";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					ProjectStatus obj = ProjectStatus.FromDataReader(reader);
					if (obj.Id == null) {
						continue;
					}
					ret.Add(obj.Id.Value, obj);
				}
			}

			return ret;
		}


		public static Dictionary<Guid, ProjectStatus> ForIds(NpgsqlConnection connection, IEnumerable<Guid> ids) {

			Guid[] idsArr = ids.ToArray();

			Dictionary<Guid, ProjectStatus> ret = new Dictionary<Guid, ProjectStatus>();
			if (idsArr.Length == 0)
				return ret;

			List<string> valNames = new List<string>();
			for (int i = 0; i < idsArr.Length; i++) {
				valNames.Add($"@val{i}");
			}

			string sql = $"SELECT * from \"project-status\" WHERE id IN ({string.Join(", ", valNames)})";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			for (int i = 0; i < valNames.Count; i++) {
				cmd.Parameters.AddWithValue(valNames[i], idsArr[i]);
			}

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					ProjectStatus obj = ProjectStatus.FromDataReader(reader);
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



			string sql = $"DELETE FROM \"project-status\" WHERE \"id\" IN ({string.Join(", ", valNames)})";
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
			Dictionary<Guid, ProjectStatus> updateObjects,
			out List<Guid> callerResponse,
			out Dictionary<Guid, ProjectStatus> toSendToOthers,
			bool printDots = false
			) {

			callerResponse = new List<Guid>();
			toSendToOthers = new Dictionary<Guid, ProjectStatus>();

			foreach (KeyValuePair<Guid, ProjectStatus> kvp in updateObjects) {

				string sql = @"
					INSERT INTO
						public.""project-status""
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


		public static ProjectStatus FromDataReader(NpgsqlDataReader reader) {

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

			return new ProjectStatus(
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
		public bool? IsOpen
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyIsOpen];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				return tok.Value<bool>();
			}
		}

		[JsonIgnore]
		public bool? IsAwaitingPayment
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyIsAwaitingPayment];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				return tok.Value<bool>();
			}
		}

		[JsonIgnore]
		public bool? IsClosed
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyIsClosed];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				return tok.Value<bool>();
			}
		}

		[JsonIgnore]
		public bool? IsNewProjectStatus
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyIsNewProjectStatus];
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
				return GenerateSearchString(
					Name, 
					(IsOpen != null && IsOpen.Value) ? "Is Open" : "",
					(IsAwaitingPayment != null && IsAwaitingPayment.Value) ? "Is Awaiting Payment" : "",
					(IsClosed != null && IsClosed.Value) ? "Is Closed" : "",
					(IsNewProjectStatus != null && IsNewProjectStatus.Value) ? "New Project" : ""
					);
			}
		}

		public static string GenerateSearchString(
			string? nameStr, 
			string? isOpenStr, 
			string? isAwaitingPaymentStr,
			string? isClosedStr,
			string? isNewProjectStatusStr
			) {
			return $"{nameStr} {isOpenStr} {isAwaitingPaymentStr} {isClosedStr} {isNewProjectStatusStr}";
		}





		public static void VerifyRepairTable(NpgsqlConnection dpDB, bool insertDefaultContents = false) {

			if (dpDB.TableExists("project-status")) {
				Log.Debug($"----- Table \"project-status\" exists.");
			} else {
				Log.Debug($"----- Table \"project-status\" doesn't exist, creating.");

				using NpgsqlCommand cmd = new NpgsqlCommand(@"
					CREATE TABLE ""public"".""project-status"" (
						""id"" uuid DEFAULT uuid_generate_v1() NOT NULL,
						""json"" json DEFAULT '{}' NOT NULL,
						""search-string"" character varying DEFAULT '',
						""last-modified-ISO8601"" character varying DEFAULT timestamp_iso8601(now(), 'utc') NOT NULL,
						CONSTRAINT ""project_status_pk"" PRIMARY KEY(""id"")
					) WITH(oids = false);
					", dpDB);
				cmd.ExecuteNonQuery();
			}

			Console.Write("----- Table \"project-status\": ");

			if (insertDefaultContents) {
				Console.Write("------ Insert default contents. ");

				Dictionary<Guid, ProjectStatus> updateObjects = new Dictionary<Guid, ProjectStatus>();


				// Written Off
				{
					string lastModified = DateTime.UtcNow.ToString("o", Culture.DevelopmentCulture);
					Guid id = Guid.NewGuid();
					ProjectStatus entry = new ProjectStatus(
						Id: id,
						Json: new JObject {
							[kJsonKeyName] = "Written Off",
							[kJsonKeyIsOpen] = false,
							[kJsonKeyIsAwaitingPayment] = false,
							[kJsonKeyIsClosed] = true,
							[kJsonKeyIsNewProjectStatus] = false,
						}.ToString(),
						LastModifiedIso8601: lastModified,
						SearchString: ""
					);
					updateObjects.Add(id, entry);
				}

				// Arrears
				{
					string lastModified = DateTime.UtcNow.ToString("o", Culture.DevelopmentCulture);
					Guid id = Guid.NewGuid();
					ProjectStatus entry = new ProjectStatus(
						Id: id,
						Json: new JObject {
							[kJsonKeyName] = "Arrears",
							[kJsonKeyIsOpen] = false,
							[kJsonKeyIsAwaitingPayment] = true,
							[kJsonKeyIsClosed] = false,
							[kJsonKeyIsNewProjectStatus] = false,
						}.ToString(),
						LastModifiedIso8601: lastModified,
						SearchString: ""
					);
					updateObjects.Add(id, entry);
				}

				// Terminated
				{
					string lastModified = DateTime.UtcNow.ToString("o", Culture.DevelopmentCulture);
					Guid id = Guid.NewGuid();
					ProjectStatus entry = new ProjectStatus(
						Id: id,
						Json: new JObject {
							[kJsonKeyName] = "Terminated",
							[kJsonKeyIsOpen] = false,
							[kJsonKeyIsAwaitingPayment] = false,
							[kJsonKeyIsClosed] = true,
							[kJsonKeyIsNewProjectStatus] = false,
						}.ToString(),
						LastModifiedIso8601: lastModified,
						SearchString: ""
					);
					updateObjects.Add(id, entry);
				}

				// Awaiting Payment
				{
					string lastModified = DateTime.UtcNow.ToString("o", Culture.DevelopmentCulture);
					Guid id = Guid.NewGuid();
					ProjectStatus entry = new ProjectStatus(
						Id: id,
						Json: new JObject {
							[kJsonKeyName] = "Awaiting Payment",
							[kJsonKeyIsOpen] = false,
							[kJsonKeyIsAwaitingPayment] = true,
							[kJsonKeyIsClosed] = false,
							[kJsonKeyIsNewProjectStatus] = false,
						}.ToString(),
						LastModifiedIso8601: lastModified,
						SearchString: ""
					);
					updateObjects.Add(id, entry);
				}

				// Complete
				{
					string lastModified = DateTime.UtcNow.ToString("o", Culture.DevelopmentCulture);
					Guid id = Guid.NewGuid();
					ProjectStatus entry = new ProjectStatus(
						Id: id,
						Json: new JObject {
							[kJsonKeyName] = "Complete",
							[kJsonKeyIsOpen] = false,
							[kJsonKeyIsAwaitingPayment] = false,
							[kJsonKeyIsClosed] = true,
							[kJsonKeyIsNewProjectStatus] = false,
						}.ToString(),
						LastModifiedIso8601: lastModified,
						SearchString: ""
					);
					updateObjects.Add(id, entry);
				}

				// In Progress
				{
					string lastModified = DateTime.UtcNow.ToString("o", Culture.DevelopmentCulture);
					Guid id = Guid.NewGuid();
					ProjectStatus entry = new ProjectStatus(
						Id: id,
						Json: new JObject {
							[kJsonKeyName] = "In Progress",
							[kJsonKeyIsOpen] = true,
							[kJsonKeyIsAwaitingPayment] = false,
							[kJsonKeyIsClosed] = false,
							[kJsonKeyIsNewProjectStatus] = false,
						}.ToString(),
						LastModifiedIso8601: lastModified,
						SearchString: ""
					);
					updateObjects.Add(id, entry);
				}

				// Quoting
				{
					string lastModified = DateTime.UtcNow.ToString("o", Culture.DevelopmentCulture);
					Guid id = Guid.NewGuid();
					ProjectStatus entry = new ProjectStatus(
						Id: id,
						Json: new JObject {
							[kJsonKeyName] = "Quoting",
							[kJsonKeyIsOpen] = true,
							[kJsonKeyIsAwaitingPayment] = false,
							[kJsonKeyIsClosed] = false,
							[kJsonKeyIsNewProjectStatus] = false,
						}.ToString(),
						LastModifiedIso8601: lastModified,
						SearchString: ""
					);
					updateObjects.Add(id, entry);
				}

				// Rough In
				{
					string lastModified = DateTime.UtcNow.ToString("o", Culture.DevelopmentCulture);
					Guid id = Guid.NewGuid();
					ProjectStatus entry = new ProjectStatus(
						Id: id,
						Json: new JObject {
							[kJsonKeyName] = "Rough In",
							[kJsonKeyIsOpen] = true,
							[kJsonKeyIsAwaitingPayment] = false,
							[kJsonKeyIsClosed] = false,
							[kJsonKeyIsNewProjectStatus] = false,
						}.ToString(),
						LastModifiedIso8601: lastModified,
						SearchString: ""
					);
					updateObjects.Add(id, entry);
				}

				// Finishing
				{
					string lastModified = DateTime.UtcNow.ToString("o", Culture.DevelopmentCulture);
					Guid id = Guid.NewGuid();
					ProjectStatus entry = new ProjectStatus(
						Id: id,
						Json: new JObject {
							[kJsonKeyName] = "Finishing",
							[kJsonKeyIsOpen] = true,
							[kJsonKeyIsAwaitingPayment] = false,
							[kJsonKeyIsClosed] = false,
							[kJsonKeyIsNewProjectStatus] = false,
						}.ToString(),
						LastModifiedIso8601: lastModified,
						SearchString: ""
					);
					updateObjects.Add(id, entry);
				}

				// Created
				{
					string lastModified = DateTime.UtcNow.ToString("o", Culture.DevelopmentCulture);
					Guid id = Guid.NewGuid();
					ProjectStatus entry = new ProjectStatus(
						Id: id,
						Json: new JObject {
							[kJsonKeyName] = "Created",
							[kJsonKeyIsOpen] = true,
							[kJsonKeyIsAwaitingPayment] = false,
							[kJsonKeyIsClosed] = false,
							[kJsonKeyIsNewProjectStatus] = true,
						}.ToString(),
						LastModifiedIso8601: lastModified,
						SearchString: ""
					);
					updateObjects.Add(id, entry);
				}

				

				Console.Write(" saving");
				ProjectStatus.Upsert(dpDB, updateObjects, out _, out _, true);
			}


			{
				Console.Write("------ Repair Existing ");

				Dictionary<Guid, ProjectStatus> updateObjects = new Dictionary<Guid, ProjectStatus>();


				Dictionary<Guid, ProjectStatus> all = ProjectStatus.All(dpDB);

				foreach (KeyValuePair<Guid, ProjectStatus> kvp in all) {

					JObject? root = kvp.Value.JsonObject;
					if (null == root)
						continue;
					JToken? lastModifiedInJSONTok = root["lastModifiedISO8601"];

					ProjectStatus obj = new ProjectStatus(
							Id: kvp.Key,
							Json: root.ToString(Newtonsoft.Json.Formatting.Indented),
							LastModifiedIso8601: null == lastModifiedInJSONTok ? kvp.Value.LastModifiedIso8601 : lastModifiedInJSONTok.Value<string>(),
							SearchString: kvp.Value.GeneratedSearchString
							);



					Console.Write(".");

					updateObjects.Add(kvp.Key, obj);
				}

				Console.Write(" saving");
				ProjectStatus.Upsert(dpDB, updateObjects, out _, out _, true);
			}
			

			Log.Debug(" done.");
		}







	}
}
