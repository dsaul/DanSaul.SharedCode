using Npgsql;
using System.Text;
using System.Data;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Serilog;
using DanSaul.SharedCode.Npgsql;

namespace SharedCode.DatabaseSchemas
{
	public record Assignments(Guid? Id, string? Json, string? SearchString, string? LastModifiedIso8601) : JSONTable(Id, Json, SearchString, LastModifiedIso8601)
	{
		public static Dictionary<Guid, Assignments> ForId(NpgsqlConnection connection, Guid id) {

			Dictionary<Guid, Assignments> ret = new();

			string sql = @"SELECT * from ""assignments"" WHERE id = @id";
			using NpgsqlCommand cmd = new(sql, connection);
			cmd.Parameters.AddWithValue("@id", id);




			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					Assignments obj = Assignments.FromDataReader(reader);

					if (obj.Id == null) {
						continue;
					}
					ret.Add(obj.Id.Value, obj);
				}
			}

			return ret;
		}


		public static Dictionary<Guid, Assignments> All(NpgsqlConnection connection) {

			Dictionary<Guid, Assignments> ret = new();

			string sql = @"SELECT * from ""assignments""";
			using NpgsqlCommand cmd = new(sql, connection);

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					Assignments obj = Assignments.FromDataReader(reader);
					if (obj.Id == null) {
						continue;
					}
					ret.Add(obj.Id.Value, obj);
				}
			}

			return ret;


		}
		public static Dictionary<Guid, Assignments> ForIds(NpgsqlConnection connection, IEnumerable<Guid> ids) {

			Guid[] idsArr = ids.ToArray();

			Dictionary<Guid, Assignments> ret = new();
			if (idsArr.Length == 0)
				return ret;

			List<string> valNames = new();
			for (int i = 0; i < idsArr.Length; i++) {
				valNames.Add($"@val{i}");
			}

			string sql = $"SELECT * from \"assignments\" WHERE id IN ({string.Join(", ", valNames)})";
			using NpgsqlCommand cmd = new(sql, connection);
			for (int i = 0; i < valNames.Count; i++) {
				cmd.Parameters.AddWithValue(valNames[i], idsArr[i]);
			}

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					Assignments obj = Assignments.FromDataReader(reader);
					if (obj.Id == null) {
						continue;
					}
					ret.Add(obj.Id.Value, obj);
				}
			}

			return ret;


		}


		public static List<Guid> Delete(NpgsqlConnection connection, List<Guid> idsToDelete) {

			List<Guid> toSendToOthers = new();
			if (idsToDelete.Count == 0) {
				return toSendToOthers;
			}

			List<string> valNames = new();
			for (int i = 0; i < idsToDelete.Count; i++) {
				valNames.Add($"@val{i}");
			}



			string sql = $"DELETE FROM \"assignments\" WHERE \"id\" IN ({string.Join(", ", valNames)})";
			using NpgsqlCommand cmd = new(sql, connection);
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
			Dictionary<Guid, Assignments> updateObjects,
			out List<Guid> callerResponse,
			out Dictionary<Guid, Assignments> toSendToOthers,
			bool printDots = false
			) {

			callerResponse = new List<Guid>();
			toSendToOthers = new Dictionary<Guid, Assignments>();

			foreach (KeyValuePair<Guid, Assignments> kvp in updateObjects) {

				string sql = @"
					INSERT INTO
						public.""assignments""
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

				using NpgsqlCommand cmd = new(sql, connection);
				cmd.Parameters.AddWithValue("@id", kvp.Key);

				string json = "{}";
				if (kvp.Value.Json != null) {
					json = kvp.Value.Json;
				}

				object lastModified = DBNull.Value;
				if (false == string.IsNullOrWhiteSpace(kvp.Value.LastModifiedIso8601)) {
					lastModified = kvp.Value.LastModifiedIso8601;
				}

				cmd.Parameters.AddWithValue("@json", json);
				cmd.Parameters.AddWithValue("@searchString", string.IsNullOrWhiteSpace(kvp.Value.SearchString) ? (object)DBNull.Value : kvp.Value.SearchString);
				cmd.Parameters.AddWithValue("@lastModifiedISO8601", lastModified);

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



		public static Assignments FromDataReader(NpgsqlDataReader reader) {

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

			return new Assignments(
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

				JToken? tok = root["projectId"];
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
		public HashSet<Guid> AgentIds
		{
			get {
				HashSet<Guid> res = new();

				if (null == Json || null == JsonObject) {
					return res;
				}

				JToken? agentIdTok = JsonObject["agentId"];
				if (null != agentIdTok) {
					string? agentIdStr = agentIdTok.Value<string>();
					if (!string.IsNullOrWhiteSpace(agentIdStr)) {

						if (Guid.TryParse(agentIdStr, out Guid parsed))
							res.Add(parsed);

					}
				}

				if (JsonObject["agentIds"] is JArray agentIds)
				{
					foreach (JToken item in agentIds)
					{
						string? str = item.Value<string>();
						if (!string.IsNullOrWhiteSpace(str))
						{
							if (Guid.TryParse(str, out Guid parsed))
								res.Add(parsed);
						}
					}
				}

				return res;
			}
		}


		[JsonIgnore]
		public string? WorkRequested
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root["workRequested"];
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
		[Obsolete("Use project notes instead.")]
		public string? WorkPerformed
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root["workPerformed"];
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
		[Obsolete("Use project notes instead.")]
		public string? InternalComments
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root["internalComments"];
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


		public void GetSchedule(
			NpgsqlConnection dpDB,
			out bool? usingProjectSchedule,
			out bool? hasStartISO8601,
			out string? startTimeMode,
			out string? startISO8601,
			out bool? hasEndISO8601,
			out string? endTimeMode,
			out string? endISO8601
			) {


			JObject? assignmentRoot = JsonObject;
			if (null == assignmentRoot) {
				usingProjectSchedule = null;
				hasStartISO8601 = null;
				startTimeMode = null;
				startISO8601 = null;
				hasEndISO8601 = null;
				endTimeMode = null;
				endISO8601 = null;
				return;
			}

			// go through project id for schedule if applicable
			do {
				usingProjectSchedule = null;

				JToken? projectIdTok = assignmentRoot["projectId"];
				if (projectIdTok == null)
					break;

				string? projectIdStr = projectIdTok.Value<string>();
				if (string.IsNullOrWhiteSpace(projectIdStr))
					break;

				// If there is a project ID we need to confirm that 
				// the project doesn't specify that we use its schedule.

				var projects = Projects.ForIds(dpDB, new List<Guid> { Guid.Parse(projectIdStr) });
				if (projects.Count == 0)
					break;

				Projects project = projects.First().Value;
				if (null == project)
					break;

				JObject? projectRoot = project.JsonObject;
				if (null == projectRoot)
					break;

				JToken? forceAssignmentsToUseProjectScheduleTok = projectRoot["forceAssignmentsToUseProjectSchedule"];
				if (null == forceAssignmentsToUseProjectScheduleTok)
					break;

				bool forceAssignmentsToUseProjectSchedule = forceAssignmentsToUseProjectScheduleTok.Value<bool>();
				if (false == forceAssignmentsToUseProjectSchedule)
					break;

				// apply schedule to above vars

				usingProjectSchedule = true;

				JToken? projectHasStartISO8601 = projectRoot["hasStartISO8601"];
				hasStartISO8601 = null == projectHasStartISO8601 ? (bool?)null : projectHasStartISO8601.Value<bool>();

				JToken? projectStartTimeMode = projectRoot["startTimeMode"];
				startTimeMode = null == projectStartTimeMode ? (string?)null : projectStartTimeMode.Value<string>();

				JToken? projectStartISO8601 = projectRoot["startISO8601"];
				startISO8601 = null == projectStartISO8601 ? (string?)null : projectStartISO8601.Value<string>();

				JToken? projectHasEndISO8601 = projectRoot["hasEndISO8601"];
				hasEndISO8601 = null == projectHasEndISO8601 ? (bool?)null : projectHasEndISO8601.Value<bool>();

				JToken? projectEndTimeMode = projectRoot["endTimeMode"];
				endTimeMode = null == projectEndTimeMode ? (string?)null : projectEndTimeMode.Value<string>();

				JToken? projectEndISO8601 = projectRoot["endISO8601"];
				endISO8601 = null == projectEndISO8601 ? (string?)null : projectEndISO8601.Value<string>();

				return;

			} while (false);


			// If we're not using the project schedule, use the assignments.
			JToken? assignmentHasStartISO8601 = assignmentRoot["hasStartISO8601"];
			hasStartISO8601 = null == assignmentHasStartISO8601 ? (bool?)null : assignmentHasStartISO8601.Value<bool>();

			JToken? assignmentStartTimeMode = assignmentRoot["startTimeMode"];
			startTimeMode = null == assignmentStartTimeMode ? (string?)null : assignmentStartTimeMode.Value<string>();

			JToken? assignmentStartISO8601 = assignmentRoot["startISO8601"];
			startISO8601 = null == assignmentStartISO8601 ? (string?)null : assignmentStartISO8601.Value<string>();

			JToken? assignmentHasEndISO8601 = assignmentRoot["hasEndISO8601"];
			hasEndISO8601 = null == assignmentHasEndISO8601 ? (bool?)null : assignmentHasEndISO8601.Value<bool>();

			JToken? assignmentEndTimeMode = assignmentRoot["endTimeMode"];
			endTimeMode = null == assignmentEndTimeMode ? (string?)null : assignmentEndTimeMode.Value<string>();


			JToken? assignmentEndISO8601 = assignmentRoot["endISO8601"];
			endISO8601 = assignmentEndISO8601 == null ? (string?)null : assignmentEndISO8601.Value<string>();
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













		public string GetAgentIdsSearchString(NpgsqlConnection db) {
			StringBuilder sb = new();

			foreach (Guid agentId in AgentIds) {

				Agents agent = Agents.ForId(db, agentId).FirstOrDefault().Value;


				sb.Append($" {agent.Name} ");
			}

			return sb.ToString();
		}

		public string GenerateSearchString(NpgsqlConnection dpDB) {

			string projectStr = "";

			do {
				if (null == ProjectId)
					break;

				var projectRes = Projects.ForId(dpDB, ProjectId.Value);
				if (0 == projectRes.Count)
					break;

				Projects project = projectRes.FirstOrDefault().Value;
				projectStr = $"{project.Name} {project.AddressesSearchString}";

			} while (false);

			return GenerateSearchString(
				dpDB, 
				projectStr,
				GetAgentIdsSearchString(dpDB),
				WorkRequested,
#pragma warning disable CS0618 // Type or member is obsolete
				WorkPerformed,
				InternalComments
#pragma warning restore CS0618 // Type or member is obsolete
				);
		}
		public static string GenerateSearchString(
			NpgsqlConnection dpDB, 
			string? projectStr, 
			string? agentIDsStr, 
			string? workRequested,
			string? workPerformed,
			string? internalComments
			) {
			return $"{projectStr} {agentIDsStr} {workRequested} {workPerformed} {internalComments}";
		}


		public static void VerifyRepairTable(NpgsqlConnection dpDB, bool insertDefaultContents = false) {


			if (dpDB.TableExists("assignments")) {
				Log.Debug($"----- Table \"assignments\" exists.");
			} else {
				Log.Debug($"----- Table \"assignments\" doesn't exist, creating.");

				using NpgsqlCommand cmd = new(@"
					CREATE TABLE ""public"".""assignments"" (
						""id"" uuid DEFAULT uuid_generate_v1() NOT NULL,
						""json"" json DEFAULT '{}' NOT NULL,
						""search-string"" character varying DEFAULT '',
						""last-modified-ISO8601"" character varying DEFAULT timestamp_iso8601(now(), 'utc') NOT NULL,
						CONSTRAINT ""assignments_pk"" PRIMARY KEY(""id"")
					) WITH(oids = false);
					", dpDB);
				cmd.ExecuteNonQuery();
			}




			Console.Write("----- Table \"assignments\": ");
			Dictionary<Guid, Assignments> updateObjects = new();
			{
				Dictionary<Guid, Assignments> all = Assignments.All(dpDB);

				foreach (KeyValuePair<Guid, Assignments> kvp in all) {

					JObject? root = kvp.Value.JsonObject;
					if (null == root)
						continue;

					JToken? lastModifiedInJSONTok = root["lastModifiedISO8601"];

					Assignments obj = new(
							Id: kvp.Key,
							Json: root.ToString(Newtonsoft.Json.Formatting.Indented),
							LastModifiedIso8601: null == lastModifiedInJSONTok ? kvp.Value.LastModifiedIso8601 : lastModifiedInJSONTok.Value<string>(),
							SearchString: kvp.Value.GenerateSearchString(dpDB)
							);


					Console.Write(".");

					updateObjects.Add(kvp.Key, obj);
				}
			}
			Console.Write(" saving");
			Assignments.Upsert(dpDB, updateObjects, out _, out _, true);

			Log.Debug(" done.");
		}














	}
}
