using Npgsql;
using System.Text;
using System.Data;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Serilog;

namespace SharedCode.DatabaseSchemas
{
	public record AssignmentStatus(Guid? Id, string? Json, string? SearchString, string? LastModifiedIso8601) : JSONTable(Id, Json, SearchString, LastModifiedIso8601)
	{
		public const string kJsonKeyName = "name";
		public const string kJsonKeyIsOpen = "isOpen";
		public const string kJsonKeyIsReOpened = "isReOpened";
		public const string kJsonKeyIsAssigned = "isAssigned";
		public const string kJsonKeyIsWaitingOnClient = "isWaitingOnClient";
		public const string kJsonKeyIsWaitingOnVendor = "isWaitingOnVendor";
		public const string kJsonKeyIsBillable = "isBillable";
		public const string kJsonKeyIsBillableReview = "isBillableReview";
		public const string kJsonKeyIsToBeScheduled = "isToBeScheduled";
		public const string kJsonKeyIsNonBillable = "isNonBillable"; 
		public const string kJsonKeyIsInProgress = "isInProgress";
		public const string kJsonKeyIsScheduled = "isScheduled";
		public const string kJsonKeyIsDefault = "isDefault";


		public static Dictionary<Guid, AssignmentStatus> ForId(NpgsqlConnection connection, Guid id) {

			Dictionary<Guid, AssignmentStatus> ret = new Dictionary<Guid, AssignmentStatus>();

			string sql = @"SELECT * from ""assignment-status"" WHERE id = @id";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			cmd.Parameters.AddWithValue("@id", id);




			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					AssignmentStatus obj = AssignmentStatus.FromDataReader(reader);
					if (obj.Id == null) {
						continue;
					}
					ret.Add(obj.Id.Value, obj);
				}
			}

			return ret;
		}


		public static Dictionary<Guid, AssignmentStatus> ForIsOpen(NpgsqlConnection connection, bool isOpen) {

			Dictionary<Guid, AssignmentStatus> ret = new Dictionary<Guid, AssignmentStatus>();

			string sql = @"select * from ""assignment-status"" WHERE (json ->> 'isOpen')::boolean = @isOpen";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			cmd.Parameters.AddWithValue("@isOpen", isOpen);




			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					AssignmentStatus obj = AssignmentStatus.FromDataReader(reader);
					if (obj.Id == null) {
						continue;
					}
					ret.Add(obj.Id.Value, obj);
				}
			}

			return ret;
		}

		public static Dictionary<Guid, AssignmentStatus> ForIsBillableReview(NpgsqlConnection connection, bool isBillableReview) {

			Dictionary<Guid, AssignmentStatus> ret = new Dictionary<Guid, AssignmentStatus>();

			string sql = @"select * from ""assignment-status"" WHERE (json ->> 'isBillableReview')::boolean = @isBillableReview";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			cmd.Parameters.AddWithValue("@isBillableReview", isBillableReview);




			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					AssignmentStatus obj = AssignmentStatus.FromDataReader(reader);
					if (obj.Id == null) {
						continue;
					}
					ret.Add(obj.Id.Value, obj);
				}
			}

			return ret;
		}

		public static Dictionary<Guid, AssignmentStatus> All(NpgsqlConnection connection) {

			Dictionary<Guid, AssignmentStatus> ret = new Dictionary<Guid, AssignmentStatus>();

			string sql = @"SELECT * from ""assignment-status""";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					AssignmentStatus obj = AssignmentStatus.FromDataReader(reader);
					if (obj.Id == null) {
						continue;
					}
					ret.Add(obj.Id.Value, obj);
				}
			}

			return ret;


		}

		public static Dictionary<Guid, AssignmentStatus> ForIds(NpgsqlConnection connection, IEnumerable<Guid> ids) {

			Guid[] idsArr = ids.ToArray();

			Dictionary<Guid, AssignmentStatus> ret = new Dictionary<Guid, AssignmentStatus>();
			if (idsArr.Length == 0)
				return ret;

			List<string> valNames = new List<string>();
			for (int i = 0; i < idsArr.Length; i++) {
				valNames.Add($"@val{i}");
			}

			string sql = $"SELECT * from \"assignment-status\" WHERE id IN ({string.Join(", ", valNames)})";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			for (int i = 0; i < valNames.Count; i++) {
				cmd.Parameters.AddWithValue(valNames[i], idsArr[i]);
			}

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					AssignmentStatus obj = AssignmentStatus.FromDataReader(reader);
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



			string sql = $"DELETE FROM \"assignment-status\" WHERE \"id\" IN ({string.Join(", ", valNames)})";
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
			Dictionary<Guid, AssignmentStatus> updateObjects,
			out List<Guid> callerResponse,
			out Dictionary<Guid, AssignmentStatus> toSendToOthers,
			bool printDots = false) {

			callerResponse = new List<Guid>();
			toSendToOthers = new Dictionary<Guid, AssignmentStatus>();

			foreach (KeyValuePair<Guid, AssignmentStatus> kvp in updateObjects) {

				string sql = @"
					INSERT INTO
						public.""assignment-status""
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


		public static AssignmentStatus FromDataReader(NpgsqlDataReader reader) {

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

			return new AssignmentStatus(
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

				bool b = tok.Value<bool>();
				return b;
			}
		}

		[JsonIgnore]
		public bool? IsReOpened
		{
			get {
				JObject? root = JsonObject;
				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyIsReOpened];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				bool b = tok.Value<bool>();
				return b;
			}
		}

		[JsonIgnore]
		public bool? IsAssigned
		{
			get {
				JObject? root = JsonObject;
				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyIsAssigned];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				bool b = tok.Value<bool>();
				return b;
			}
		}

		[JsonIgnore]
		public bool? IsWaitingOnClient
		{
			get {
				JObject? root = JsonObject;
				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyIsWaitingOnClient];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				bool b = tok.Value<bool>();
				return b;
			}
		}

		[JsonIgnore]
		public bool? IsWaitingOnVendor
		{
			get {
				JObject? root = JsonObject;
				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyIsWaitingOnVendor];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				bool b = tok.Value<bool>();
				return b;
			}
		}

		[JsonIgnore]
		public bool? IsBillable
		{
			get {
				JObject? root = JsonObject;
				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyIsBillable];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				bool b = tok.Value<bool>();
				return b;
			}
		}

		[JsonIgnore]
		public bool? IsBillableReview
		{
			get {
				JObject? root = JsonObject;
				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyIsBillableReview];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				bool b = tok.Value<bool>();
				return b;
			}
		}

		[JsonIgnore]
		public bool? IsToBeScheduled
		{
			get {
				JObject? root = JsonObject;
				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyIsToBeScheduled];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				bool b = tok.Value<bool>();
				return b;
			}
		}

		[JsonIgnore]
		public bool? IsNonBillable
		{
			get {
				JObject? root = JsonObject;
				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyIsNonBillable];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				bool b = tok.Value<bool>();
				return b;
			}
		}

		[JsonIgnore]
		public bool? IsInProgress
		{
			get {
				JObject? root = JsonObject;
				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyIsInProgress];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				bool b = tok.Value<bool>();
				return b;
			}
		}

		[JsonIgnore]
		public bool? IsScheduled
		{
			get {
				JObject? root = JsonObject;
				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyIsScheduled];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				bool b = tok.Value<bool>();
				return b;
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

				bool b = tok.Value<bool>();
				return b;
			}
		}

		[JsonIgnore]
		public string GeneratedSearchString
		{
			get {
				return GenerateSearchString(
					Name, 
					IsOpen, 
					IsReOpened, 
					IsAssigned,
					IsWaitingOnClient,
					IsWaitingOnVendor,
					IsBillable,
					IsBillableReview,
					IsInProgress,
					IsNonBillable,
					IsScheduled,
					IsToBeScheduled,
					IsDefault);
			}
		}

		public static string GenerateSearchString(
			string? name, 
			bool? isOpen, 
			bool? isReOpened, 
			bool? isAssigned,
			bool? isWaitingOnClient,
			bool? isWaitingOnVendor,
			bool? isBillable,
			bool? isBillableReview,
			bool? isInProgress,
			bool? isNonBillable,
			bool? isScheduled,
			bool? isToBeScheduled,
			bool? isDefault
			) {

			string isOpenStr = (null != isOpen && isOpen.Value) ? "Is Open" : "";
			string isReOpenedStr = (null != isReOpened && isReOpened.Value) ? "Is Re-Opened" : "";
			string isAssignedStr = (null != isAssigned && isAssigned.Value) ? "Is Assigned" : "";
			string isWaitingOnClientStr = (null != isWaitingOnClient && isWaitingOnClient.Value) ? "Is Waiting on Client" : "";
			string isWaitingOnVendorStr = (null != isWaitingOnVendor && isWaitingOnVendor.Value) ? "Is Waiting on Vendor" : "";
			string isBillableStr = (null != isBillable && isBillable.Value) ? "Is Billable" : "";
			string isBillableReviewStr = (null != isBillableReview && isBillableReview.Value) ? "Is Billable Review" : "";
			string isInProgressStr = (null != isInProgress && isInProgress.Value) ? "Is In Progress" : "";
			string isNonBillableStr = (null != isNonBillable && isNonBillable.Value) ? "Is Non-Billable" : "";
			string isScheduledStr = (null != isScheduled && isScheduled.Value) ? "Is Scheduled" : "";
			string isToBeScheduledStr = (null != isToBeScheduled && isToBeScheduled.Value) ? "Is To Be Scheduled" : "";
			string isDefaultStr = (null != isDefault && isDefault.Value) ? "Is Default" : "";


			return $"{name} {isOpenStr} {isReOpenedStr} {isAssignedStr} {isWaitingOnClientStr} {isWaitingOnVendorStr} {isBillableStr} {isBillableReviewStr} {isInProgressStr} {isNonBillableStr} {isScheduledStr} {isToBeScheduledStr} {isDefaultStr}";
		}

		public static void VerifyRepairTable(NpgsqlConnection dpDB, bool insertDefaultContents = false) {


			if (dpDB.TableExists("assignment-status")) {
				Log.Debug($"----- Table \"assignment-status\" exists.");
			} else {
				Log.Debug($"----- Table \"assignment-status\" doesn't exist, creating.");

				using NpgsqlCommand cmd = new NpgsqlCommand(@"
					CREATE TABLE ""public"".""assignment-status"" (
						""id"" uuid DEFAULT uuid_generate_v1() NOT NULL,
						""json"" json DEFAULT '{}' NOT NULL,
						""search-string"" character varying DEFAULT '',
						""last-modified-ISO8601"" character varying DEFAULT timestamp_iso8601(now(), 'utc') NOT NULL,
						CONSTRAINT ""assignment_status_pk"" PRIMARY KEY(""id"")
					) WITH(oids = false);
					", dpDB);
				cmd.ExecuteNonQuery();
			}

			// Repair

			Console.Write("----- Table \"assignment-status\": ");
			


			if (insertDefaultContents) {
				Console.Write("------ Insert default contents. ");

				Dictionary<Guid, AssignmentStatus> updateObjects = new Dictionary<Guid, AssignmentStatus>();

				// Billable
				{
					string lastModified = DateTime.UtcNow.ToString("o", Culture.DevelopmentCulture);
					Guid id = Guid.NewGuid();
					AssignmentStatus entry = new AssignmentStatus(
						Id: id,
						Json: new JObject {
							[kJsonKeyName] = "Billable",
							[kJsonKeyIsOpen] = false,
							[kJsonKeyIsReOpened] = false,
							[kJsonKeyIsAssigned] = false,
							[kJsonKeyIsWaitingOnClient] = false,
							[kJsonKeyIsWaitingOnVendor] = false,
							[kJsonKeyIsBillable] = true,
							[kJsonKeyIsBillableReview] = false,
							[kJsonKeyIsToBeScheduled] = false,
							[kJsonKeyIsNonBillable] = false,
							[kJsonKeyIsInProgress] = false,
							[kJsonKeyIsScheduled] = false,
							[kJsonKeyIsDefault] = true,
						}.ToString(),
						LastModifiedIso8601: lastModified,
						SearchString: ""
					);
					updateObjects.Add(id, entry);
				}


				// Assigned
				{
					string lastModified = DateTime.UtcNow.ToString("o", Culture.DevelopmentCulture);
					Guid id = Guid.NewGuid();
					AssignmentStatus entry = new AssignmentStatus(
						Id: id,
						Json: new JObject {
							[kJsonKeyName] = "Assigned",
							[kJsonKeyIsOpen] = true,
							[kJsonKeyIsReOpened] = false,
							[kJsonKeyIsAssigned] = true,
							[kJsonKeyIsWaitingOnClient] = false,
							[kJsonKeyIsWaitingOnVendor] = false,
							[kJsonKeyIsBillable] = false,
							[kJsonKeyIsBillableReview] = false,
							[kJsonKeyIsToBeScheduled] = false,
							[kJsonKeyIsNonBillable] = false,
							[kJsonKeyIsInProgress] = false,
							[kJsonKeyIsScheduled] = false,
							[kJsonKeyIsDefault] = true,
						}.ToString(),
						LastModifiedIso8601: lastModified,
						SearchString: ""
					);
					updateObjects.Add(id, entry);
				}

				// Waiting on Vendor
				{
					string lastModified = DateTime.UtcNow.ToString("o", Culture.DevelopmentCulture);
					Guid id = Guid.NewGuid();
					AssignmentStatus entry = new AssignmentStatus(
						Id: id,
						Json: new JObject {
							[kJsonKeyName] = "Waiting on Vendor",
							[kJsonKeyIsOpen] = true,
							[kJsonKeyIsReOpened] = false,
							[kJsonKeyIsAssigned] = false,
							[kJsonKeyIsWaitingOnClient] = false,
							[kJsonKeyIsWaitingOnVendor] = true,
							[kJsonKeyIsBillable] = false,
							[kJsonKeyIsBillableReview] = false,
							[kJsonKeyIsToBeScheduled] = false,
							[kJsonKeyIsNonBillable] = false,
							[kJsonKeyIsInProgress] = false,
							[kJsonKeyIsScheduled] = false,
							[kJsonKeyIsDefault] = true,
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
					AssignmentStatus entry = new AssignmentStatus(
						Id: id,
						Json: new JObject {
							[kJsonKeyName] = "In Progress",
							[kJsonKeyIsOpen] = true,
							[kJsonKeyIsReOpened] = false,
							[kJsonKeyIsAssigned] = true,
							[kJsonKeyIsWaitingOnClient] = false,
							[kJsonKeyIsWaitingOnVendor] = false,
							[kJsonKeyIsBillable] = false,
							[kJsonKeyIsBillableReview] = false,
							[kJsonKeyIsToBeScheduled] = false,
							[kJsonKeyIsNonBillable] = false,
							[kJsonKeyIsInProgress] = true,
							[kJsonKeyIsScheduled] = false,
							[kJsonKeyIsDefault] = true,
						}.ToString(),
						LastModifiedIso8601: lastModified,
						SearchString: ""
					);
					updateObjects.Add(id, entry);
				}

				// Billable Review
				{
					string lastModified = DateTime.UtcNow.ToString("o", Culture.DevelopmentCulture);
					Guid id = Guid.NewGuid();
					AssignmentStatus entry = new AssignmentStatus(
						Id: id,
						Json: new JObject {
							[kJsonKeyName] = "Billable Review",
							[kJsonKeyIsOpen] = true,
							[kJsonKeyIsReOpened] = false,
							[kJsonKeyIsAssigned] = false,
							[kJsonKeyIsWaitingOnClient] = false,
							[kJsonKeyIsWaitingOnVendor] = false,
							[kJsonKeyIsBillable] = false,
							[kJsonKeyIsBillableReview] = true,
							[kJsonKeyIsToBeScheduled] = false,
							[kJsonKeyIsNonBillable] = false,
							[kJsonKeyIsInProgress] = false,
							[kJsonKeyIsScheduled] = false,
							[kJsonKeyIsDefault] = true,
						}.ToString(),
						LastModifiedIso8601: lastModified,
						SearchString: ""
					);
					updateObjects.Add(id, entry);
				}

				// Waiting on Client
				{
					string lastModified = DateTime.UtcNow.ToString("o", Culture.DevelopmentCulture);
					Guid id = Guid.NewGuid();
					AssignmentStatus entry = new AssignmentStatus(
						Id: id,
						Json: new JObject {
							[kJsonKeyName] = "Waiting on Client",
							[kJsonKeyIsOpen] = true,
							[kJsonKeyIsReOpened] = false,
							[kJsonKeyIsAssigned] = false,
							[kJsonKeyIsWaitingOnClient] = true,
							[kJsonKeyIsWaitingOnVendor] = false,
							[kJsonKeyIsBillable] = false,
							[kJsonKeyIsBillableReview] = false,
							[kJsonKeyIsToBeScheduled] = false,
							[kJsonKeyIsNonBillable] = false,
							[kJsonKeyIsInProgress] = false,
							[kJsonKeyIsScheduled] = false,
							[kJsonKeyIsDefault] = true,
						}.ToString(),
						LastModifiedIso8601: lastModified,
						SearchString: ""
					);
					updateObjects.Add(id, entry);
				}


				// To Be Scheduled
				{
					string lastModified = DateTime.UtcNow.ToString("o", Culture.DevelopmentCulture);
					Guid id = Guid.NewGuid();
					AssignmentStatus entry = new AssignmentStatus(
						Id: id,
						Json: new JObject {
							[kJsonKeyName] = "To Be Scheduled",
							[kJsonKeyIsOpen] = true,
							[kJsonKeyIsReOpened] = false,
							[kJsonKeyIsAssigned] = false,
							[kJsonKeyIsWaitingOnClient] = false,
							[kJsonKeyIsWaitingOnVendor] = false,
							[kJsonKeyIsBillable] = false,
							[kJsonKeyIsBillableReview] = false,
							[kJsonKeyIsToBeScheduled] = true,
							[kJsonKeyIsNonBillable] = false,
							[kJsonKeyIsInProgress] = false,
							[kJsonKeyIsScheduled] = false,
							[kJsonKeyIsDefault] = true,
						}.ToString(),
						LastModifiedIso8601: lastModified,
						SearchString: ""
					);
					updateObjects.Add(id, entry);
				}

				// To Be Picked
				{
					string lastModified = DateTime.UtcNow.ToString("o", Culture.DevelopmentCulture);
					Guid id = Guid.NewGuid();
					AssignmentStatus entry = new AssignmentStatus(
						Id: id,
						Json: new JObject {
							[kJsonKeyName] = "To Be Picked",
							[kJsonKeyIsOpen] = true,
							[kJsonKeyIsReOpened] = false,
							[kJsonKeyIsAssigned] = false,
							[kJsonKeyIsWaitingOnClient] = false,
							[kJsonKeyIsWaitingOnVendor] = false,
							[kJsonKeyIsBillable] = false,
							[kJsonKeyIsBillableReview] = false,
							[kJsonKeyIsToBeScheduled] = false,
							[kJsonKeyIsNonBillable] = false,
							[kJsonKeyIsInProgress] = false,
							[kJsonKeyIsScheduled] = false,
							[kJsonKeyIsDefault] = true,
						}.ToString(),
						LastModifiedIso8601: lastModified,
						SearchString: ""
					);
					updateObjects.Add(id, entry);
				}

				// Re-opened
				{
					string lastModified = DateTime.UtcNow.ToString("o", Culture.DevelopmentCulture);
					Guid id = Guid.NewGuid();
					AssignmentStatus entry = new AssignmentStatus(
						Id: id,
						Json: new JObject {
							[kJsonKeyName] = "Re-opened",
							[kJsonKeyIsOpen] = true,
							[kJsonKeyIsReOpened] = true,
							[kJsonKeyIsAssigned] = false,
							[kJsonKeyIsWaitingOnClient] = false,
							[kJsonKeyIsWaitingOnVendor] = false,
							[kJsonKeyIsBillable] = false,
							[kJsonKeyIsBillableReview] = false,
							[kJsonKeyIsToBeScheduled] = false,
							[kJsonKeyIsNonBillable] = false,
							[kJsonKeyIsInProgress] = false,
							[kJsonKeyIsScheduled] = false,
							[kJsonKeyIsDefault] = true,
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
					AssignmentStatus entry = new AssignmentStatus(
						Id: id,
						Json: new JObject {
							[kJsonKeyName] = "Non Billable",
							[kJsonKeyIsOpen] = false,
							[kJsonKeyIsReOpened] = false,
							[kJsonKeyIsAssigned] = false,
							[kJsonKeyIsWaitingOnClient] = false,
							[kJsonKeyIsWaitingOnVendor] = false,
							[kJsonKeyIsBillable] = false,
							[kJsonKeyIsBillableReview] = false,
							[kJsonKeyIsToBeScheduled] = false,
							[kJsonKeyIsNonBillable] = true,
							[kJsonKeyIsInProgress] = false,
							[kJsonKeyIsScheduled] = false,
							[kJsonKeyIsDefault] = true,
						}.ToString(),
						LastModifiedIso8601: lastModified,
						SearchString: ""
					);
					updateObjects.Add(id, entry);
				}

				// Scheduled
				{
					string lastModified = DateTime.UtcNow.ToString("o", Culture.DevelopmentCulture);
					Guid id = Guid.NewGuid();
					AssignmentStatus entry = new AssignmentStatus(
						Id: id,
						Json: new JObject {
							[kJsonKeyName] = "Scheduled",
							[kJsonKeyIsOpen] = true,
							[kJsonKeyIsReOpened] = false,
							[kJsonKeyIsAssigned] = false,
							[kJsonKeyIsWaitingOnClient] = false,
							[kJsonKeyIsWaitingOnVendor] = false,
							[kJsonKeyIsBillable] = false,
							[kJsonKeyIsBillableReview] = false,
							[kJsonKeyIsToBeScheduled] = false,
							[kJsonKeyIsNonBillable] = false,
							[kJsonKeyIsInProgress] = false,
							[kJsonKeyIsScheduled] = true,
							[kJsonKeyIsDefault] = true,
						}.ToString(),
						LastModifiedIso8601: lastModified,
						SearchString: ""
					);
					updateObjects.Add(id, entry);
				}

				// 
				{
					string lastModified = DateTime.UtcNow.ToString("o", Culture.DevelopmentCulture);
					Guid id = Guid.NewGuid();
					AssignmentStatus entry = new AssignmentStatus(
						Id: id,
						Json: new JObject {
							[kJsonKeyName] = "Closed",
							[kJsonKeyIsOpen] = false,
							[kJsonKeyIsReOpened] = false,
							[kJsonKeyIsAssigned] = false,
							[kJsonKeyIsWaitingOnClient] = false,
							[kJsonKeyIsWaitingOnVendor] = false,
							[kJsonKeyIsBillable] = false,
							[kJsonKeyIsBillableReview] = false,
							[kJsonKeyIsToBeScheduled] = false,
							[kJsonKeyIsNonBillable] = false,
							[kJsonKeyIsInProgress] = false,
							[kJsonKeyIsScheduled] = false,
							[kJsonKeyIsDefault] = true,
						}.ToString(),
						LastModifiedIso8601: lastModified,
						SearchString: ""
					);
					updateObjects.Add(id, entry);
				}



				Console.Write(" saving");
				AssignmentStatus.Upsert(dpDB, updateObjects, out _, out _, true);
			}
			



			{
				Dictionary<Guid, AssignmentStatus> updateObjects = new Dictionary<Guid, AssignmentStatus>();

				Console.Write("------ Repair Existing ");
				Dictionary<Guid, AssignmentStatus> all = AssignmentStatus.All(dpDB);

				foreach (KeyValuePair<Guid, AssignmentStatus> kvp in all) {

					JObject? root = kvp.Value.JsonObject;
					if (null == root)
						continue;
					
					JToken? lastModifiedInJSONTok = root["lastModifiedISO8601"];

					AssignmentStatus obj = new AssignmentStatus(
							Id: kvp.Key,
							Json: root.ToString(Formatting.Indented),
							LastModifiedIso8601: null == lastModifiedInJSONTok ? kvp.Value.LastModifiedIso8601 : lastModifiedInJSONTok.Value<string>(),
							SearchString: kvp.Value.GeneratedSearchString
							);


					Console.Write(".");

					updateObjects.Add(kvp.Key, obj);
				}

				Console.Write(" saving");
				AssignmentStatus.Upsert(dpDB, updateObjects, out _, out _, true);

			}
			

			Log.Debug(" done.");
		}






	}
}
