using DanSaul.SharedCode.Npgsql;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;
using Serilog;
using System.Data;
using System.Text;

namespace SharedCode.DatabaseSchemas
{
	[Obsolete]
	public record OnCallAutoAttendants(Guid? Id, string? Json, string? SearchString, string? LastModifiedIso8601) : JSONTable(Id, Json, SearchString, LastModifiedIso8601)
	{

		public const string kJsonKeyName = "name";
		public const string kJsonKeyAgentOnCallPriorityCalendars = "agentOnCallPriorityCalendars";
		public const string kJsonKeyNoAgentResponseNotificationNumber = "noAgentResponseNotificationNumber";
		public const string kJsonKeyNoAgentResponseNotificationEMail = "noAgentResponseNotificationEMail";
		public const string kJsonKeyMarkedHandledNotificationEMail = "markedHandledNotificationEMail";
		public const string kJsonKeyFailoverNumber = "failoverNumber";
		public const string kJsonKeyRecordings = "recordings";
		public const string kJsonKeyRecordingsKeyIntro = "intro";
		public const string kJsonKeyRecordingsKeyIntroKeyType = "type";
		public const string kJsonKeyRecordingsKeyIntroKeyText = "text";
		public const string kJsonKeyRecordingsKeyIntroKeyRecordingId = "recordingId";
		public const string kJsonKeyRecordingsKeyAskForCallbackNumber = "askForCallbackNumber";
		public const string kJsonKeyRecordingsKeyAskForCallbackNumberKeyType = "type";
		public const string kJsonKeyRecordingsKeyAskForCallbackNumberKeyText = "text";
		public const string kJsonKeyRecordingsKeyAskForCallbackNumberKeyRecordingId = "recordingId";
		public const string kJsonKeyRecordingsKeyAskForMessage = "askForMessage";
		public const string kJsonKeyRecordingsKeyAskForMessageKeyType = "type";
		public const string kJsonKeyRecordingsKeyAskForMessageKeyText = "text";
		public const string kJsonKeyRecordingsKeyAskForMessageKeyRecordingId = "recordingId";
		public const string kJsonKeyRecordingsKeyThankYouAfter = "thankYouAfter";
		public const string kJsonKeyRecordingsKeyThankYouAfterKeyType = "type";
		public const string kJsonKeyRecordingsKeyThankYouAfterKeyText = "text";
		public const string kJsonKeyRecordingsKeyThankYouAfterKeyRecordingId = "recordingId";

		public const string kJsonKeyCallAttemptsToEachCalendarBeforeGivingUp = "callAttemptsToEachCalendarBeforeGivingUp";
		public const string kJsonKeyMinutesBetweenCallAttempts = "minutesBetweenCallAttempts";
		[Obsolete]
		public static Dictionary<Guid, OnCallAutoAttendants> ForId(NpgsqlConnection connection, Guid id) {

			Dictionary<Guid, OnCallAutoAttendants> ret = new Dictionary<Guid, OnCallAutoAttendants>();

			string sql = @"SELECT * from ""on-call-auto-attendants"" WHERE id = @id";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			cmd.Parameters.AddWithValue("@id", id);




			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					OnCallAutoAttendants obj = OnCallAutoAttendants.FromDataReader(reader);
					if (obj.Id == null) {
						continue;
					}
					ret.Add(obj.Id.Value, obj);
				}
			}

			return ret;
		}

		[Obsolete]
		public static Dictionary<Guid, OnCallAutoAttendants> All(NpgsqlConnection connection) {

			Dictionary<Guid, OnCallAutoAttendants> ret = new Dictionary<Guid, OnCallAutoAttendants>();

			string sql = @"SELECT * from ""on-call-auto-attendants""";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					OnCallAutoAttendants obj = OnCallAutoAttendants.FromDataReader(reader);
					if (obj.Id == null) {
						continue;
					}
					ret.Add(obj.Id.Value, obj);
				}
			}

			return ret;


		}
		[Obsolete]
		public static Dictionary<Guid, OnCallAutoAttendants> ForIds(NpgsqlConnection connection, IEnumerable<Guid> ids) {

			Guid[] idsArr = ids.ToArray();

			Dictionary<Guid, OnCallAutoAttendants> ret = new Dictionary<Guid, OnCallAutoAttendants>();
			if (idsArr.Length == 0)
				return ret;

			List<string> valNames = new List<string>();
			for (int i = 0; i < idsArr.Length; i++) {
				valNames.Add($"@val{i}");
			}

			string sql = $"SELECT * from \"on-call-auto-attendants\" WHERE id IN ({string.Join(", ", valNames)})";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			for (int i = 0; i < valNames.Count; i++) {
				cmd.Parameters.AddWithValue(valNames[i], idsArr[i]);
			}

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					OnCallAutoAttendants obj = OnCallAutoAttendants.FromDataReader(reader);
					if (obj.Id == null) {
						continue;
					}
					ret.Add(obj.Id.Value, obj);
				}
			}

			return ret;


		}
		[Obsolete]
		public static List<Guid> Delete(NpgsqlConnection connection, List<Guid> idsToDelete) {


			List<Guid> toSendToOthers = new List<Guid>();
			if (idsToDelete.Count == 0) {
				return toSendToOthers;
			}


			List<string> valNames = new List<string>();
			for (int i = 0; i < idsToDelete.Count; i++) {
				valNames.Add($"@val{i}");
			}



			string sql = $"DELETE FROM \"on-call-auto-attendants\" WHERE \"id\" IN ({string.Join(", ", valNames)})";
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

		[Obsolete]
		public static void Upsert(
			NpgsqlConnection connection,
			Dictionary<Guid, OnCallAutoAttendants> updateObjects,
			out List<Guid> callerResponse,
			out Dictionary<Guid, OnCallAutoAttendants> toSendToOthers,
			bool printDots = false
			) {

			callerResponse = new List<Guid>();
			toSendToOthers = new Dictionary<Guid, OnCallAutoAttendants>();

			foreach (KeyValuePair<Guid, OnCallAutoAttendants> kvp in updateObjects) {

				string sql = @"
					INSERT INTO
						public.""on-call-auto-attendants""
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
		[Obsolete]
		public static OnCallAutoAttendants FromDataReader(NpgsqlDataReader reader) {

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

			return new OnCallAutoAttendants(
				Id: id,
				Json: json,
				SearchString: searchString,
				LastModifiedIso8601: lastModifiedIso8601
				);
		}








		[Obsolete]

		[JsonIgnore]
		public string GeneratedSearchString
		{
			get {
				return GenerateSearchString();
			}
		}
		[Obsolete]
		public static string GenerateSearchString() {
			return $"";
		}

		[Obsolete]
		public static void VerifyRepairTable(NpgsqlConnection dpDB, bool insertDefaultContents = false) {

			if (dpDB.TableExists("on-call-auto-attendants")) {
				Log.Debug($"----- Table \"on-call-auto-attendants\" exists.");
			} else {
				Log.Debug($"----- Table \"on-call-auto-attendants\" doesn't exist, creating.");

				using NpgsqlCommand cmd = new NpgsqlCommand(@"
					CREATE TABLE ""public"".""on-call-auto-attendants"" (
						""id"" uuid DEFAULT uuid_generate_v1() NOT NULL,
						""json"" json DEFAULT '{}' NOT NULL,
						""search-string"" character varying DEFAULT '',
						""last-modified-ISO8601"" character varying DEFAULT timestamp_iso8601(now(), 'utc') NOT NULL,
						CONSTRAINT ""on_call_auto_attendants_pk"" PRIMARY KEY(""id"")
					) WITH(oids = false);
					", dpDB);
				cmd.ExecuteNonQuery();
			}

		}
		[Obsolete]
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
		[Obsolete]
		[JsonIgnore]
		public List<Guid> AgentOnCallPriorityCalendars
		{
			get {
				List<Guid> ret = new List<Guid>();

				JObject? root = JsonObject;

				if (null == root) {
					return ret;
				}

				JArray? arr = root[kJsonKeyAgentOnCallPriorityCalendars] as JArray;
				if (null == arr || arr.Type == JTokenType.Null) {
					return ret;
				}

				foreach (JToken tok in arr) {


					string? str = tok.Value<string>();

					Guid? id = null;
					if (Guid.TryParse(str, out Guid tmp))
						id = tmp;

					if (null != id) {
						ret.Add(id.Value);
					}
					
				}

				return ret;
			}
		}
		[Obsolete]
		[JsonIgnore]
		public string? NoAgentResponseNotificationNumber
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyNoAgentResponseNotificationNumber];
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
		[Obsolete]
		[JsonIgnore]
		public string? NoAgentResponseNotificationEMail
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyNoAgentResponseNotificationEMail];
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
		[Obsolete]
		[JsonIgnore]
		public string? MarkedHandledNotificationEMail
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyMarkedHandledNotificationEMail];
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
		[Obsolete]
		[JsonIgnore]
		public string? FailoverNumber
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyFailoverNumber];
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
		[Obsolete]
		public enum RecordingTypeE
		{
			Polly,
			Recording,
		};
		[Obsolete]
		[JsonIgnore]
		public RecordingTypeE? RecordingsIntroType
		{
			get {
				JObject? root = JsonObject;
				if (null == root) {
					return null;
				}

				JObject? recRoot = root[kJsonKeyRecordings] as JObject;
				if (null == recRoot) {
					return null;
				}

				JObject? introRoot = recRoot[kJsonKeyRecordingsKeyIntro] as JObject;
				if (null == introRoot) {
					return null;
				}

				JToken? tok = introRoot[kJsonKeyRecordingsKeyIntroKeyType];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				string? str = tok.Value<string>();
				if (string.IsNullOrWhiteSpace(str)) {
					return null;
				}

				RecordingTypeE res;
				if (!Enum.TryParse<RecordingTypeE>(str, true, out res)) {
					return null;
				}

				return res;
			}
		}
		[Obsolete]
		[JsonIgnore]
		public string? RecordingsIntroText
		{
			get {
				JObject? root = JsonObject;
				if (null == root) {
					return null;
				}

				JObject? recRoot = root[kJsonKeyRecordings] as JObject;
				if (null == recRoot) {
					return null;
				}

				JObject? introRoot = recRoot[kJsonKeyRecordingsKeyIntro] as JObject;
				if (null == introRoot) {
					return null;
				}

				JToken? tok = introRoot[kJsonKeyRecordingsKeyIntroKeyText];
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
		[Obsolete]
		[JsonIgnore]
		public Guid? RecordingsIntroRecordingId
		{
			get {
				JObject? root = JsonObject;
				if (null == root) {
					return null;
				}

				JObject? recRoot = root[kJsonKeyRecordings] as JObject;
				if (null == recRoot) {
					return null;
				}

				JObject? introRoot = recRoot[kJsonKeyRecordingsKeyIntro] as JObject;
				if (null == introRoot) {
					return null;
				}

				JToken? tok = introRoot[kJsonKeyRecordingsKeyIntroKeyRecordingId];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				string? str = tok.Value<string>();
				if (string.IsNullOrWhiteSpace(str)) {
					return null;
				}

				if (!Guid.TryParse(str, out Guid result)) {
					return null;
				}

				return result;
			}
		}
		[Obsolete]
		[JsonIgnore]
		public RecordingTypeE? RecordingsAskForCallbackNumberType
		{
			get {
				JObject? root = JsonObject;
				if (null == root) {
					return null;
				}

				JObject? recRoot = root[kJsonKeyRecordings] as JObject;
				if (null == recRoot) {
					return null;
				}

				JObject? introRoot = recRoot[kJsonKeyRecordingsKeyAskForCallbackNumber] as JObject;
				if (null == introRoot) {
					return null;
				}

				JToken? tok = introRoot[kJsonKeyRecordingsKeyAskForCallbackNumberKeyType];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				string? str = tok.Value<string>();
				if (string.IsNullOrWhiteSpace(str)) {
					return null;
				}

				RecordingTypeE res;
				if (!Enum.TryParse<RecordingTypeE>(str, true, out res)) {
					return null;
				}

				return res;
			}
		}
		[Obsolete]
		[JsonIgnore]
		public string? RecordingsAskForCallbackNumberText
		{
			get {
				JObject? root = JsonObject;
				if (null == root) {
					return null;
				}

				JObject? recRoot = root[kJsonKeyRecordings] as JObject;
				if (null == recRoot) {
					return null;
				}

				JObject? introRoot = recRoot[kJsonKeyRecordingsKeyAskForCallbackNumber] as JObject;
				if (null == introRoot) {
					return null;
				}

				JToken? tok = introRoot[kJsonKeyRecordingsKeyAskForCallbackNumberKeyText];
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
		[Obsolete]
		[JsonIgnore]
		public Guid? RecordingsAskForCallbackNumberRecordingId
		{
			get {
				JObject? root = JsonObject;
				if (null == root) {
					return null;
				}

				JObject? recRoot = root[kJsonKeyRecordings] as JObject;
				if (null == recRoot) {
					return null;
				}

				JObject? introRoot = recRoot[kJsonKeyRecordingsKeyAskForCallbackNumber] as JObject;
				if (null == introRoot) {
					return null;
				}

				JToken? tok = introRoot[kJsonKeyRecordingsKeyAskForCallbackNumberKeyRecordingId];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				string? str = tok.Value<string>();
				if (string.IsNullOrWhiteSpace(str)) {
					return null;
				}

				if (!Guid.TryParse(str, out Guid result)) {
					return null;
				}

				return result;
			}
		}

		[Obsolete]
		[JsonIgnore]
		public RecordingTypeE? RecordingsAskForMessageType
		{
			get {
				JObject? root = JsonObject;
				if (null == root) {
					return null;
				}

				JObject? recRoot = root[kJsonKeyRecordings] as JObject;
				if (null == recRoot) {
					return null;
				}

				JObject? introRoot = recRoot[kJsonKeyRecordingsKeyAskForMessage] as JObject;
				if (null == introRoot) {
					return null;
				}

				JToken? tok = introRoot[kJsonKeyRecordingsKeyAskForMessageKeyType];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				string? str = tok.Value<string>();
				if (string.IsNullOrWhiteSpace(str)) {
					return null;
				}

				RecordingTypeE res;
				if (!Enum.TryParse<RecordingTypeE>(str, true, out res)) {
					return null;
				}

				return res;
			}
		}
		[Obsolete]
		[JsonIgnore]
		public string? RecordingsAskForMessageText
		{
			get {
				JObject? root = JsonObject;
				if (null == root) {
					return null;
				}

				JObject? recRoot = root[kJsonKeyRecordings] as JObject;
				if (null == recRoot) {
					return null;
				}

				JObject? introRoot = recRoot[kJsonKeyRecordingsKeyAskForMessage] as JObject;
				if (null == introRoot) {
					return null;
				}

				JToken? tok = introRoot[kJsonKeyRecordingsKeyAskForMessageKeyText];
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
		[Obsolete]
		[JsonIgnore]
		public Guid? RecordingsAskForMessageRecordingId
		{
			get {
				JObject? root = JsonObject;
				if (null == root) {
					return null;
				}

				JObject? recRoot = root[kJsonKeyRecordings] as JObject;
				if (null == recRoot) {
					return null;
				}

				JObject? introRoot = recRoot[kJsonKeyRecordingsKeyAskForMessage] as JObject;
				if (null == introRoot) {
					return null;
				}

				JToken? tok = introRoot[kJsonKeyRecordingsKeyAskForMessageKeyRecordingId];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				string? str = tok.Value<string>();
				if (string.IsNullOrWhiteSpace(str)) {
					return null;
				}

				if (!Guid.TryParse(str, out Guid result)) {
					return null;
				}

				return result;
			}
		}

		[Obsolete]
		[JsonIgnore]
		public RecordingTypeE? RecordingsThankYouAfterType
		{
			get {
				JObject? root = JsonObject;
				if (null == root) {
					return null;
				}

				JObject? recRoot = root[kJsonKeyRecordings] as JObject;
				if (null == recRoot) {
					return null;
				}

				JObject? introRoot = recRoot[kJsonKeyRecordingsKeyThankYouAfter] as JObject;
				if (null == introRoot) {
					return null;
				}

				JToken? tok = introRoot[kJsonKeyRecordingsKeyThankYouAfterKeyType];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				string? str = tok.Value<string>();
				if (string.IsNullOrWhiteSpace(str)) {
					return null;
				}

				RecordingTypeE res;
				if (!Enum.TryParse<RecordingTypeE>(str, true, out res)) {
					return null;
				}

				return res;
			}
		}
		[Obsolete]
		[JsonIgnore]
		public string? RecordingsThankYouAfterText
		{
			get {
				JObject? root = JsonObject;
				if (null == root) {
					return null;
				}

				JObject? recRoot = root[kJsonKeyRecordings] as JObject;
				if (null == recRoot) {
					return null;
				}

				JObject? introRoot = recRoot[kJsonKeyRecordingsKeyThankYouAfter] as JObject;
				if (null == introRoot) {
					return null;
				}

				JToken? tok = introRoot[kJsonKeyRecordingsKeyThankYouAfterKeyText];
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
		[Obsolete]
		[JsonIgnore]
		public Guid? RecordingsThankYouAfterKeyRecordingId
		{
			get {
				JObject? root = JsonObject;
				if (null == root) {
					return null;
				}

				JObject? recRoot = root[kJsonKeyRecordings] as JObject;
				if (null == recRoot) {
					return null;
				}

				JObject? introRoot = recRoot[kJsonKeyRecordingsKeyThankYouAfter] as JObject;
				if (null == introRoot) {
					return null;
				}

				JToken? tok = introRoot[kJsonKeyRecordingsKeyThankYouAfterKeyRecordingId];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				string? str = tok.Value<string>();
				if (string.IsNullOrWhiteSpace(str)) {
					return null;
				}

				if (!Guid.TryParse(str, out Guid result)) {
					return null;
				}

				return result;
			}
		}












		[Obsolete]

		[JsonIgnore]
		public decimal? CallAttemptsToEachCalendarBeforeGivingUp
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyCallAttemptsToEachCalendarBeforeGivingUp];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				return tok.Value<decimal>();
			}
		}
		[Obsolete]
		[JsonIgnore]
		public decimal? MinutesBetweenCallAttempts
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyMinutesBetweenCallAttempts];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				return tok.Value<decimal>();
			}
		}




		

	}
}
