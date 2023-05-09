using Npgsql;
using System.Text;
using System.Data;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Serilog;
using DanSaul.SharedCode.Npgsql;
using DanSaul.SharedCode;

namespace SharedCode.DatabaseSchemas
{
	



	public record Labour(
		Guid? Id,
		string? Json,
		string? SearchString,
		string? LastModifiedIso8601
		) : JSONTable(Id, Json, SearchString, LastModifiedIso8601)
	{
		
		public const string kJsonKeyProjectId = "projectId";
		public const string kJsonKeyAgentId = "agentId";
		public const string kJsonKeyAssignmentId = "assignmentId";
		public const string kJsonKeyTypeId = "typeId";
		public const string kJsonKeyTimeMode = "timeMode";
		public const string kJsonKeyHours = "hours";
		public const string kJsonKeyStartISO8601 = "startISO8601";
		public const string kJsonKeyEndISO8601 = "endISO8601";
		public const string kJsonKeyIsActive = "isActive";
		public const string kJsonKeyLocationType = "locationType";
		public const string kJsonKeyIsExtra = "isExtra";
		public const string kJsonKeyIsBilled = "isBilled";
		public const string kJsonKeyIsPaidOut = "isPaidOut";
		public const string kJsonKeyIsEnteredThroughTelephoneCompanyAccess = "isEnteredThroughTelephoneCompanyAccess";
		public const string kJsonKeyExceptionTypeId = "exceptionTypeId";
		public const string kJsonKeyHolidayTypeId = "holidayTypeId";
		public const string kJsonKeyNonBillableTypeId = "nonBillableTypeId";
		public const string kJsonKeyNotes = "notes";
		public const string kJsonKeyBankedPayOutAmount = "bankedPayOutAmount";

		public const string kJsonValueTimeModeNone = "none";
		public const string kJsonValueTimeModeDateAndHours = "date-and-hours";
		public const string kJsonValueTimeModeStartStopTimestamp = "start-stop-timestamp";

		public const string kJsonValueLocationTypeNone = "none";
		public const string kJsonValueLocationTypeTravel = "travel";
		public const string kJsonValueLocationTypeOnSite = "on-site";
		public const string kJsonValueLocationTypeRemote = "remote";

		public static Dictionary<Guid, Labour> ForId(NpgsqlConnection connection, Guid id) {

			Dictionary<Guid, Labour> ret = new Dictionary<Guid, Labour>();

			string sql = @"SELECT * from ""labour"" WHERE id = @id";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			cmd.Parameters.AddWithValue("@id", id);




			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					Labour obj = Labour.FromDataReader(reader);
					if (obj.Id == null) {
						continue;
					}
					ret.Add(obj.Id.Value, obj);
				}
			}

			return ret;
		}

		public static Dictionary<Guid, Labour> ForAgentIDIsActive(NpgsqlConnection connection, Guid agentId, bool flag) {

			Dictionary<Guid, Labour> ret = new Dictionary<Guid, Labour>();

			string sql = @"SELECT * from ""labour"" WHERE (json->>'agentId')::uuid = @agentId AND (json->>'isActive')::boolean = @isActive";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			cmd.Parameters.AddWithValue("@agentId", agentId);
			cmd.Parameters.AddWithValue("@isActive", flag);


			

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					Labour obj = Labour.FromDataReader(reader);
					if (obj.Id == null) {
						continue;
					}
					ret.Add(obj.Id.Value, obj);
				}
			}

			return ret;
		}


		public static Dictionary<Guid, Labour> All(NpgsqlConnection connection) {

			Dictionary<Guid, Labour> ret = new Dictionary<Guid, Labour>();

			string sql = @"SELECT * from ""labour""";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					Labour obj = Labour.FromDataReader(reader);
					if (obj.Id == null) {
						continue;
					}
					ret.Add(obj.Id.Value, obj);
				}
			}

			return ret;


		}

		public static Dictionary<Guid, Labour> ForIds(NpgsqlConnection connection, IEnumerable<Guid> ids) {

			Guid[] idsArr = ids.ToArray();

			Dictionary<Guid, Labour> ret = new Dictionary<Guid, Labour>();
			if (idsArr.Length == 0)
				return ret;

			List<string> valNames = new List<string>();
			for (int i = 0; i < idsArr.Length; i++) {
				valNames.Add($"@val{i}");
			}

			string sql = $"SELECT * from \"labour\" WHERE id IN ({string.Join(", ", valNames)})";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			for (int i = 0; i < valNames.Count; i++) {
				cmd.Parameters.AddWithValue(valNames[i], idsArr[i]);
			}

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					Labour obj = Labour.FromDataReader(reader);
					if (obj.Id == null) {
						continue;
					}
					ret.Add(obj.Id.Value, obj);
				}
			}

			return ret;


		}

		public static Dictionary<Guid, Labour> ForProjectId(NpgsqlConnection connection, Guid projectId) {

			Dictionary<Guid, Labour> ret = new Dictionary<Guid, Labour>();

			string sql = @"SELECT * from ""labour"" WHERE json ->> 'projectId' = @projectId";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			cmd.Parameters.AddWithValue("@projectId", projectId.ToString());




			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					Labour obj = Labour.FromDataReader(reader);
					if (obj.Id == null) {
						continue;
					}
					ret.Add(obj.Id.Value, obj);
				}
			}

			return ret;
		}

		public static Dictionary<Guid, Labour> ForAgentId(NpgsqlConnection connection, Guid agentId) {

			Dictionary<Guid, Labour> ret = new Dictionary<Guid, Labour>();

			string sql = @"SELECT * from ""labour"" WHERE json ->> 'agentId' = @agentId";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			cmd.Parameters.AddWithValue("@agentId", agentId.ToString());




			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					Labour obj = Labour.FromDataReader(reader);
					if (obj.Id == null) {
						continue;
					}
					ret.Add(obj.Id.Value, obj);
				}
			}

			return ret;
		}

		public static Dictionary<Guid, Labour> ForProjects(NpgsqlConnection connection, IEnumerable<Projects> projects) {
			List<Guid> ids = new List<Guid>();
			foreach (Projects project in projects) {
				if (null == project || null == project.Id)
					continue;
				ids.Add(project.Id.Value);
			}
			return ForProjectIds(connection, ids);
		}

		public static Dictionary<Guid, Labour> ForProjectIds(NpgsqlConnection connection, IEnumerable<Guid> projectIds) {

			var needles = projectIds.ToArray();

			Dictionary<Guid, Labour> ret = new Dictionary<Guid, Labour>();
			if (needles.Length == 0)
				return ret;

			List<string> valNames = new List<string>();
			for (int i = 0; i < needles.Length; i++) {
				valNames.Add($"@val{i}");
			}
			
			string sql = $"SELECT * from \"labour\" WHERE (json ->> 'projectId')::uuid IN ({string.Join(", ", valNames)})";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			for (int i = 0; i < valNames.Count; i++) {
				cmd.Parameters.AddWithValue(valNames[i], needles[i]);
			}

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					Labour obj = Labour.FromDataReader(reader);
					if (null == obj || null == obj.Id)
						continue;
					ret.Add(obj.Id.Value, obj);
				}
			}

			return ret;


		}



		public static Dictionary<Guid, Labour> ForAssignmentIds(NpgsqlConnection connection, List<Guid> ids) {

			Dictionary<Guid, Labour> ret = new Dictionary<Guid, Labour>();
			if (ids.Count == 0)
				return ret;

			//List<string> valNames = new List<string>();
			//for (int i = 0; i < ids.Count; i++) {
			//	valNames.Add($"@val{i}");
			//}

			//string sql = $"SELECT * from \"labour\" WHERE json ->> 'assignmentId' IN ({string.Join(", ", valNames)})";
			//using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			//for (int i = 0; i < valNames.Count; i++) {
			//	cmd.Parameters.AddWithValue(valNames[i], ids[i]);
			//}


			// It seems we can't use prepared statements with json??


			List<string> validatedIds = new List<string>();
			foreach (Guid id in ids) {
				validatedIds.Add($"'{id}'");
			}


			string sql = $"SELECT * from \"labour\" WHERE json ->> 'assignmentId' IN ({string.Join(", ", validatedIds)})";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					Labour obj = Labour.FromDataReader(reader);
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



			string sql = $"DELETE FROM \"labour\" WHERE \"id\" IN ({string.Join(", ", valNames)})";
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
			Dictionary<Guid, Labour> updateObjects,
			out List<Guid> callerResponse,
			out Dictionary<Guid, Labour> toSendToOthers,
			bool printDots = false
			) {

			callerResponse = new List<Guid>();
			toSendToOthers = new Dictionary<Guid, Labour>();

			foreach (KeyValuePair<Guid, Labour> kvp in updateObjects) {

				string sql = @"
					INSERT INTO
						public.""labour""
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



		public static Labour FromDataReader(NpgsqlDataReader reader) {

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

			return new Labour(
				Id: id,
				Json: json,
				SearchString: searchString,
				LastModifiedIso8601: lastModifiedIso8601
				);
		}


		[JsonIgnore]
		public Guid? ProjectId
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyProjectId];
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
		public Guid? AgentId
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyAgentId];
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
		public Guid? AssignmentId
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyAssignmentId];
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
		public Guid? TypeId
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyTypeId];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				string? str = tok.Value<string>();
				if (string.IsNullOrWhiteSpace(str)) {
					return null;
				}

				return !Guid.TryParse(str, out Guid guid) ? null : (Guid?)guid;
			}
		}


		public enum TimeModeE
		{
			Unknown = 0,
			None,
			DateAndHours,
			StartStopTimestamp,
		}


		[JsonIgnore]
		public TimeModeE? TimeMode
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyTimeMode];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				string? str = tok.Value<string>();
				if (string.IsNullOrWhiteSpace(str)) {
					return null;
				}


				return str switch {
					"none" => TimeModeE.None,
					"date-and-hours" => TimeModeE.DateAndHours,
					"start-stop-timestamp" => TimeModeE.StartStopTimestamp,
					_ => TimeModeE.Unknown,
				};
			}
		}

		[JsonIgnore]
		public decimal? Hours
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyHours];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				return tok.Value<decimal>();
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

				JToken? tok = root[kJsonKeyStartISO8601];
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
		public string? EndISO8601
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyEndISO8601];
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
		public bool IsActive
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return false;
				}

				JToken? tok = root[kJsonKeyIsActive];
				if (null == tok || tok.Type == JTokenType.Null) {
					return false;
				}

				return tok.Value<bool>();
			}
		}

		public enum LocationTypeE
		{
			Unknown = 0,
			None,
			Travel,
			OnSite,
			Remote,
		}

		[JsonIgnore]
		public LocationTypeE? LocationType
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyLocationType];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				string? str = tok.Value<string>();
				if (string.IsNullOrWhiteSpace(str)) {
					return null;
				}


				return str switch {
					"none" => LocationTypeE.None,
					"travel" => LocationTypeE.Travel,
					"on-site" => LocationTypeE.OnSite,
					"remote" => LocationTypeE.Remote,
					_ => LocationTypeE.Unknown,
				};
			}
		}

		public bool GetIsExtra(NpgsqlConnection db, Dictionary<Guid, Projects>? projectsCache = null) {
			return IsLabourExtra(db, this, projectsCache);
		}

		public static bool IsLabourExtra(NpgsqlConnection db, Labour labour, Dictionary<Guid, Projects>? projectsCache = null) {

			JObject? root = labour.JsonObject;

			if (null == root) {
				return false;
			}

			Guid? projectId = labour.ProjectId;
			if (null != projectId) {

				// If we have a cache, use that instead of accessing the database.
				if (null != projectsCache) {

					Projects? project = projectsCache.GetValueOrDefault(projectId.Value);
					if (null != project && project.ForceLabourAsExtra) {
						return true;
					}

				} else {

					// Use the database.

					var res = Projects.ForId(db, projectId.Value);
					if (res.Count > 0) {
						Projects project = res.FirstOrDefault().Value;
						if (project.ForceLabourAsExtra) {
							return true;
						}
					}
				}



			}

			JToken? tok = root[kJsonKeyIsExtra];
			if (null == tok || tok.Type == JTokenType.Null) {
				return false;
			}

			return tok.Value<bool>();
		}


		[JsonIgnore]
		public bool IsBilled
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return false;
				}

				JToken? tok = root[kJsonKeyIsBilled];
				if (null == tok || tok.Type == JTokenType.Null) {
					return false;
				}

				return tok.Value<bool>();
			}
		}

		[JsonIgnore]
		public bool IsPaidOut
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return false;
				}

				JToken? tok = root[kJsonKeyIsPaidOut];
				if (null == tok || tok.Type == JTokenType.Null) {
					return false;
				}

				return tok.Value<bool>();
			}
		}


		[JsonIgnore]
		public bool IsEnteredThroughTelephoneCompanyAccess
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return false;
				}

				JToken? tok = root[kJsonKeyIsEnteredThroughTelephoneCompanyAccess];
				if (null == tok || tok.Type == JTokenType.Null) {
					return false;
				}

				return tok.Value<bool>();
			}
		}


		[JsonIgnore]
		public Guid? ExceptionTypeId
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyExceptionTypeId];
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
		public Guid? HolidayTypeId
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyHolidayTypeId];
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
		public Guid? NonBillableTypeId
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyNonBillableTypeId];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				string? str = tok.Value<string>();
				if (string.IsNullOrWhiteSpace(str)) {
					return null;
				}

				return !Guid.TryParse(str, out Guid guid) ? null : guid;
			}
		}


		[JsonIgnore]
		public string? Notes
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyNotes];
				if (null == tok) {
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
		public decimal? BankedPayOutAmount
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyBankedPayOutAmount];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				return tok.Value<decimal>();
			}
		}










		public HashSet<Guid> GetChildIDs(NpgsqlConnection connection) {
			HashSet<Guid> children = new HashSet<Guid>();

			Guid? projectId = ProjectId;
			if (null == projectId) {
				return children;
			}

			RecursiveChildrenOfProjectId(connection, ref children, projectId.Value);


			return children;
		}


		private static void RecursiveChildrenOfProjectId(NpgsqlConnection connection, ref HashSet<Guid> set, Guid projectId) {

			Dictionary<Guid, Projects> projectsWithParent = Projects.SelectForParentId(connection, projectId);
			foreach (Projects project in projectsWithParent.Values) {
				if (null == project || null == project.Id)
					continue;
				if (!set.Contains(project.Id.Value)) {
					set.Add(project.Id.Value);
					RecursiveChildrenOfProjectId(connection, ref set, project.Id.Value);
				}
			}


		}













		public string GeneratedSearchString(NpgsqlConnection connection)
		{
			string? projectStr = null;
			string? agentStr = null;

			do {
				if (null == ProjectId)
					break;

				var resProject = Projects.ForId(connection, ProjectId.Value);
				if (0 == resProject.Count)
					break;

				Projects project = resProject.FirstOrDefault().Value;

				projectStr = project.AddressesSearchString;

			} while (false);

			do {
				if (null == AgentId)
					break;

				var resAgent = Agents.ForId(connection, AgentId.Value);
				if (0 == resAgent.Count)
					break;

				Agents agent = resAgent.FirstOrDefault().Value;

				agentStr = agent.Name;

			} while (false);

			

			string isActiveStr = IsActive ? "Is Active" : "";
			string isExtraStr = GetIsExtra(connection) ? "Is Extra" : "";
			string isBilledStr = IsBilled ? "Is Billed" : "";
			string isPaidOutStr = IsPaidOut ? "Is Paid Out" : "";
			





			return GenerateSearchString(
				projectStr,
				agentStr,
				isActiveStr,
				isExtraStr,
				isBilledStr,
				isPaidOutStr,
				Notes
				);
		}

		public static string GenerateSearchString(
			string? projectStr, 
			string? agentStr, 
			string? isActiveStr,
			string? isExtraStr,
			string? isBilledStr,
			string? isPaidOutStr,
			string? notesStr
			) {
			return $"{projectStr} {agentStr} {isActiveStr} {isExtraStr} {isBilledStr} {isPaidOutStr} {notesStr}";
		}












		public static void VerifyRepairTable(NpgsqlConnection dpDB, bool insertDefaultContents = false) {

			if (dpDB.TableExists("labour")) {
				Log.Debug($"----- Table \"labour\" exists.");
			} else {
				Log.Debug($"----- Table \"labour\" doesn't exist, creating.");

				using NpgsqlCommand cmd = new NpgsqlCommand(@"
					CREATE TABLE ""public"".""labour"" (
						""id"" uuid DEFAULT uuid_generate_v1() NOT NULL,
						""json"" json DEFAULT '{}' NOT NULL,
						""search-string"" character varying DEFAULT '',
						""last-modified-ISO8601"" character varying DEFAULT timestamp_iso8601(now(), 'utc') NOT NULL,
						CONSTRAINT ""labour_pk"" PRIMARY KEY(""id"")
					) WITH(oids = false);
					", dpDB);
				cmd.ExecuteNonQuery();
			}


			Console.Write("----- Table \"labour\": ");
			Dictionary<Guid, Labour> updateObjects = new Dictionary<Guid, Labour>();
			{
				Dictionary<Guid, Labour> all = Labour.All(dpDB);

				foreach (KeyValuePair<Guid, Labour> kvp in all) {

					JObject? root = kvp.Value.JsonObject;
					if (null == root)
						continue;
					JToken? lastModifiedInJSONTok = root["lastModifiedISO8601"];

					string? lastModified = null == lastModifiedInJSONTok ? kvp.Value.LastModifiedIso8601 : lastModifiedInJSONTok.Value<string>();
					if (string.IsNullOrWhiteSpace(lastModified)) {
						lastModified = DateTime.UtcNow.ToString("o", Culture.DevelopmentCulture);
					}

					Labour obj = new Labour(
							Id: kvp.Key,
							Json: root.ToString(Newtonsoft.Json.Formatting.Indented),
							LastModifiedIso8601: lastModified,
							SearchString: kvp.Value.GeneratedSearchString(dpDB)
							);



					Console.Write(".");

					updateObjects.Add(kvp.Key, obj);
				}
			}
			Console.Write(" saving");
			Labour.Upsert(dpDB, updateObjects, out _, out _, true);

			Log.Debug(" done.");
		}


	}
}
