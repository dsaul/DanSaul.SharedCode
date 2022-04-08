using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Databases.Records.Billing;
using System.Globalization;
using SharedCode.Extensions;
using Serilog;

namespace SharedCode.DatabaseSchemas
{
	public record ProjectNotes(Guid? Id, string? Json, string? SearchString, string? LastModifiedIso8601) : JSONTable(Id, Json, SearchString, LastModifiedIso8601)
	{
		public enum ContentTypes
		{
			Unknown = 0,
			StyledText,
			Checkbox,
			Image,
			Video
		}







		public static Dictionary<Guid, ProjectNotes> ForId(NpgsqlConnection connection, Guid id) {

			Dictionary<Guid, ProjectNotes> ret = new Dictionary<Guid, ProjectNotes>();

			string sql = @"SELECT * from ""project-notes"" WHERE id = @id";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			cmd.Parameters.AddWithValue("@id", id);




			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					ProjectNotes obj = ProjectNotes.FromDataReader(reader);
					if (obj.Id == null) {
						continue;
					}
					ret.Add(obj.Id.Value, obj);
				}
			}

			return ret;
		}


		public static Dictionary<Guid, ProjectNotes> ForAssignmentId(NpgsqlConnection connection, Guid? assignmentId) {

			Dictionary<Guid, ProjectNotes> ret = new Dictionary<Guid, ProjectNotes>();

			if (assignmentId == null)
				return ret;

			string sql = @"SELECT * from ""project-notes"" WHERE json ->> 'assignmentId' = @assignmentId";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			cmd.Parameters.AddWithValue("@assignmentId", assignmentId.Value.ToString());




			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					ProjectNotes obj = ProjectNotes.FromDataReader(reader);
					if (obj.Id == null) {
						continue;
					}
					ret.Add(obj.Id.Value, obj);
				}
			}

			return ret;
		}


		public static Dictionary<Guid, ProjectNotes> ForProjectId(NpgsqlConnection connection, Guid projectId) {

			Dictionary<Guid, ProjectNotes> ret = new Dictionary<Guid, ProjectNotes>();

			string sql = @"SELECT * from ""project-notes"" WHERE json ->> 'projectId' = @projectId";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			cmd.Parameters.AddWithValue("@projectId", projectId.ToString());




			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					ProjectNotes obj = ProjectNotes.FromDataReader(reader);
					if (obj.Id == null) {
						continue;
					}
					ret.Add(obj.Id.Value, obj);
				}
			}

			return ret;
		}






		





		public static Dictionary<Guid, ProjectNotes> All(NpgsqlConnection connection) {

			Dictionary<Guid, ProjectNotes> ret = new Dictionary<Guid, ProjectNotes>();

			string sql = @"SELECT * from ""project-notes""";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					ProjectNotes obj = ProjectNotes.FromDataReader(reader);
					if (obj.Id == null) {
						continue;
					}
					ret.Add(obj.Id.Value, obj);
				}
			}

			return ret;


		}

		public static Dictionary<Guid, ProjectNotes> ForIds(NpgsqlConnection connection, IEnumerable<Guid> ids) {

			Guid[] idsArr = ids.ToArray();

			Dictionary<Guid, ProjectNotes> ret = new Dictionary<Guid, ProjectNotes>();
			if (idsArr.Length == 0)
				return ret;

			List<string> valNames = new List<string>();
			for (int i = 0; i < idsArr.Length; i++) {
				valNames.Add($"@val{i}");
			}

			string sql = $"SELECT * from \"project-notes\" WHERE id IN ({string.Join(", ", valNames)})";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			for (int i = 0; i < valNames.Count; i++) {
				cmd.Parameters.AddWithValue(valNames[i], idsArr[i]);
			}

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					ProjectNotes obj = ProjectNotes.FromDataReader(reader);
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



			string sql = $"DELETE FROM \"project-notes\" WHERE \"id\" IN ({string.Join(", ", valNames)})";
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
			Dictionary<Guid, ProjectNotes> updateObjects,
			out List<Guid> callerResponse,
			out Dictionary<Guid, ProjectNotes> toSendToOthers,
			bool printDots = false
			) {

			callerResponse = new List<Guid>();
			toSendToOthers = new Dictionary<Guid, ProjectNotes>();

			foreach (KeyValuePair<Guid, ProjectNotes> kvp in updateObjects) {

				string sql = @"
					INSERT INTO
						public.""project-notes""
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


		public static ProjectNotes FromDataReader(NpgsqlDataReader reader) {

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

			return new ProjectNotes(
				Id: id,
				Json: json,
				SearchString: searchString,
				LastModifiedIso8601: lastModifiedIso8601
				);
		}

		[JsonIgnore]
		public string? OriginalISO8601
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root["originalISO8601"];
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
		public Guid? OriginalBillingId
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root["originalBillingId"];
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
		public Guid? AssignmentId
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root["assignmentId"];
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
		public ContentTypes ContentType
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return ContentTypes.Unknown;
				}

				JToken? tok = root["contentType"];
				if (null == tok || tok.Type == JTokenType.Null) {
					return ContentTypes.Unknown;
				}

				string str = tok.Value<string>();
				if (string.IsNullOrWhiteSpace(str)) {
					return ContentTypes.Unknown;
				}

				return str switch {
					"styled-text" => ContentTypes.StyledText,
					"checkbox" => ContentTypes.Checkbox,
					"image" => ContentTypes.Image,
					"video" => ContentTypes.Video,
					_ => ContentTypes.Unknown,
				};
			}
		}


		[JsonIgnore]
		public string? StyledTextHTML
		{
			get {
				if (ContentType != ContentTypes.StyledText)
					return null;
				
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? contentTok = root["content"];
				if (null == contentTok || contentTok.Type == JTokenType.Null) {
					return null;
				}

				JToken? htmlTok = root["html"];
				if (null == htmlTok || htmlTok.Type == JTokenType.Null) {
					return null;
				}

				string str = htmlTok.Value<string>();
				if (string.IsNullOrWhiteSpace(str)) {
					return null;
				}

				return str;
			}
		}

		[JsonIgnore]
		public string? CheckboxText
		{
			get {
				if (ContentType != ContentTypes.Checkbox)
					return null;

				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? contentTok = root["content"];
				if (null == contentTok || contentTok.Type == JTokenType.Null) {
					return null;
				}

				JToken? textTok = root["text"];
				if (null == textTok || textTok.Type == JTokenType.Null) {
					return null;
				}

				string str = textTok.Value<string>();
				if (string.IsNullOrWhiteSpace(str)) {
					return null;
				}

				return str;
			}
		}

		[JsonIgnore]
		public bool? CheckboxState
		{
			get {
				if (ContentType != ContentTypes.Checkbox)
					return null;

				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? contentTok = root["content"];
				if (null == contentTok || contentTok.Type == JTokenType.Null) {
					return null;
				}

				JToken? checkboxStateTok = root["checkboxState"];
				if (null == checkboxStateTok || checkboxStateTok.Type == JTokenType.Null) {
					return null;
				}

				bool b = checkboxStateTok.Value<bool>();

				return b;
			}
		}


		[JsonIgnore]
		public string? VideoURI
		{
			get {
				if (ContentType != ContentTypes.Video)
					return null;

				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? contentTok = root["content"];
				if (null == contentTok || contentTok.Type == JTokenType.Null) {
					return null;
				}

				JToken? uriTok = root["uri"];
				if (null == uriTok || uriTok.Type == JTokenType.Null) {
					return null;
				}

				string str = uriTok.Value<string>();
				if (string.IsNullOrWhiteSpace(str)) {
					return null;
				}

				return str;
			}
		}

		[JsonIgnore]
		public string? ImageURI
		{
			get {
				if (ContentType != ContentTypes.Image)
					return null;

				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? contentTok = root["content"];
				if (null == contentTok || contentTok.Type == JTokenType.Null) {
					return null;
				}

				JToken? uriTok = root["uri"];
				if (null == uriTok || uriTok.Type == JTokenType.Null) {
					return null;
				}

				string str = uriTok.Value<string>();
				if (string.IsNullOrWhiteSpace(str)) {
					return null;
				}

				return str;
			}
		}


		[JsonIgnore]
		public bool? InternalOnly
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root["internalOnly"];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}


				return tok.Value<bool>();
			}
		}


		[JsonIgnore]
		public bool? Resolved
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root["resolved"];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}


				return tok.Value<bool>();
			}
		}


		[JsonIgnore]
		public bool? NoLongerRelevant
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root["noLongerRelevant"];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}


				return tok.Value<bool>();
			}
		}


		public string GetLaTeXString(NpgsqlConnection dbBilling) {

			StringBuilder sb = new StringBuilder();

			Guid? originalBillingId = OriginalBillingId;
			Guid? lastModifiedBillingId = LastModifiedBillingId;

			List<Guid> selectList = new List<Guid>();
			if (null != originalBillingId)
				selectList.Add(originalBillingId.Value);
			if (null != lastModifiedBillingId)
				selectList.Add(lastModifiedBillingId.Value);

			BillingContacts? originalContact = null;
			BillingContacts? lastModifiedContact = null;

			var idResults = BillingContacts.ForIds(dbBilling, selectList);
			if (null != originalBillingId)
				idResults.TryGetValue(originalBillingId.Value, out originalContact);
			if (null != lastModifiedBillingId)
				idResults.TryGetValue(lastModifiedBillingId.Value, out lastModifiedContact);

			CultureInfo culture = new CultureInfo("en-CA");

			string? originalISO8601 = OriginalISO8601;
			string? originalIso8601Formatted = null;
			DateTime? originalISO8601DB = null;
			
			if (DateTime.TryParse(originalISO8601, out DateTime tmp)) {
				originalISO8601DB = tmp;

				DateTime originalISO8601Local = originalISO8601DB.Value.ToLocalTime();
				originalIso8601Formatted = originalISO8601Local.ToString("MMM d, yyyy, h:mm tt", culture);
			}

			string? lastModifiedISO8601 = LastModifiedIso8601;
			string? lastModifiedIso8601Formatted = null;
			DateTime? lastModifiedISO8601DB = null;

			if (DateTime.TryParse(lastModifiedISO8601, out tmp)) {
				lastModifiedISO8601DB = tmp;

				DateTime lastModifiedISO8601Local = lastModifiedISO8601DB.Value.ToLocalTime();
				lastModifiedIso8601Formatted = lastModifiedISO8601Local.ToString("MMM d, yyyy, h:mm tt", culture);
			}

			string fullName = "No Name";
			if (null != originalContact && !string.IsNullOrWhiteSpace(originalContact.FullName)) {
				fullName = originalContact.FullName;
			}

			sb.Append($"{fullName} {originalIso8601Formatted} ");

			if (originalISO8601DB != lastModifiedISO8601DB) {

				sb.Append("Modified ");

				if (originalContact != lastModifiedContact) {

					string modFullName = "No Name";
					if (null != lastModifiedContact && !string.IsNullOrWhiteSpace(lastModifiedContact.FullName)) {
						modFullName = lastModifiedContact.FullName;
					}

					sb.Append($"{modFullName} ");
				}

				sb.Append($"{lastModifiedIso8601Formatted} ");


			}

			sb.Append("---\n");

			var noteType = ContentType;

			switch (noteType) {
				case ProjectNotes.ContentTypes.Checkbox:

					if (CheckboxState == null) {
						sb.Append("[?] ");
					} else if (CheckboxState.Value) {
						sb.Append("[x] ");
					} else {
						sb.Append("[ ] ");
					}

					sb.Append(CheckboxText);

					break;
				case ProjectNotes.ContentTypes.Image:


					sb.Append($"Image: {ImageURI}");

					break;
				case ProjectNotes.ContentTypes.StyledText:
					sb.Append($"{StyledTextHTML}");
					break;
				case ProjectNotes.ContentTypes.Video:
					sb.Append($"Video: {VideoURI}");
					break;
			}

			return sb.ToString();

		}

		[JsonIgnore]
		public string GeneratedSearchString
		{
			get {
				return GenerateSearchString();
			}
		}

		public static string GenerateSearchString() {
			return $"";
#warning TODO: implement
		}





		public static void VerifyRepairTable(NpgsqlConnection dpDB, bool insertDefaultContents = false) {

			if (dpDB.TableExists("project-notes")) {
				Log.Debug($"----- Table \"project-notes\" exists.");
			} else {
				Log.Debug($"----- Table \"project-notes\" doesn't exist, creating.");

				using NpgsqlCommand cmd = new NpgsqlCommand(@"
					CREATE TABLE ""public"".""project-notes"" (
						""id"" uuid DEFAULT uuid_generate_v1() NOT NULL,
						""json"" json DEFAULT '{}' NOT NULL,
						""search-string"" character varying DEFAULT '',
						""last-modified-ISO8601"" character varying DEFAULT timestamp_iso8601(now(), 'utc') NOT NULL,
						CONSTRAINT ""project_notes_pk"" PRIMARY KEY(""id"")
					) WITH(oids = false);
					", dpDB);
				cmd.ExecuteNonQuery();
			}

			Console.Write("----- Table \"project-notes\": ");
			Dictionary<Guid, ProjectNotes> updateObjects = new Dictionary<Guid, ProjectNotes>();
			{
				Dictionary<Guid, ProjectNotes> all = ProjectNotes.All(dpDB);

				foreach (KeyValuePair<Guid, ProjectNotes> kvp in all) {

					JObject? root = kvp.Value.JsonObject;
					if (null == root)
						continue;
					JToken? lastModifiedInJSONTok = root["lastModifiedISO8601"];

					ProjectNotes obj = new ProjectNotes(
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
			ProjectNotes.Upsert(dpDB, updateObjects, out _, out _, true);

			Log.Debug(" done.");
		}















	}
}
