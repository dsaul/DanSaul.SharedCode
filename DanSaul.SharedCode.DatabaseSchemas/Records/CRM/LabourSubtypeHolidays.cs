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
	public record LabourSubtypeHolidays(Guid? Id, string? Json, string? SearchString, string? LastModifiedIso8601) : JSONTable(Id, Json, SearchString, LastModifiedIso8601)
	{
		public const string kJsonKeyName = "name";
		public const string kJsonKeyIcon = "icon";
		public const string kJsonKeyDescription = "description";
		public const string kJsonKeyIsStaticDate = "isStaticDate";
		public const string kJsonKeyStaticDateMonth = "staticDateMonth";
		public const string kJsonKeyStaticDateDay = "staticDateDay";
		public const string kJsonKeyIsObservationDay = "isObservationDay";
		public const string kJsonKeyObservationDayStatic = "observationDayStatic";
		public const string kJsonKeyObservationDayStaticMonth = "observationDayStaticMonth";
		public const string kJsonKeyObservationDayStaticDay = "observationDayStaticDay";
		public const string kJsonKeyObservationDayActivateIfWeekend = "observationDayActivateIfWeekend";
		public const string kJsonKeyIsFirstMondayInMonthDate = "isFirstMondayInMonthDate";
		public const string kJsonKeyFirstMondayMonth = "firstMondayMonth";
		public const string kJsonKeyIsGoodFriday = "isGoodFriday";
		public const string kJsonKeyIsThirdMondayInMonthDate = "isThirdMondayInMonthDate";
		public const string kJsonKeyThirdMondayMonth = "thirdMondayMonth";
		public const string kJsonKeyIsSecondMondayInMonthDate = "isSecondMondayInMonthDate";
		public const string kJsonKeySecondMondayMonth = "secondMondayMonth";
		public const string kJsonKeyIsMondayBeforeDate = "isMondayBeforeDate";
		public const string kJsonKeyMondayBeforeDateMonth = "mondayBeforeDateMonth";
		public const string kJsonKeyMondayBeforeDateDay = "mondayBeforeDateDay";
		

		public static Dictionary<Guid, LabourSubtypeHolidays> ForId(NpgsqlConnection connection, Guid id) {

			Dictionary<Guid, LabourSubtypeHolidays> ret = new Dictionary<Guid, LabourSubtypeHolidays>();

			string sql = @"SELECT * from ""labour-subtype-holidays"" WHERE id = @id";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			cmd.Parameters.AddWithValue("@id", id);




			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					LabourSubtypeHolidays obj = LabourSubtypeHolidays.FromDataReader(reader);
					if (obj.Id == null) {
						continue;
					}
					ret.Add(obj.Id.Value, obj);
				}
			}

			return ret;
		}

		public static Dictionary<Guid, LabourSubtypeHolidays> All(NpgsqlConnection connection) {

			Dictionary<Guid, LabourSubtypeHolidays> ret = new Dictionary<Guid, LabourSubtypeHolidays>();

			string sql = @"SELECT * from ""labour-subtype-holidays""";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					LabourSubtypeHolidays obj = LabourSubtypeHolidays.FromDataReader(reader);
					if (obj.Id == null) {
						continue;
					}
					ret.Add(obj.Id.Value, obj);
				}
			}

			return ret;
		}

		public static Dictionary<Guid, LabourSubtypeHolidays> ForIds(NpgsqlConnection connection, IEnumerable<Guid> ids) {

			Guid[] idsArr = ids.ToArray();

			Dictionary<Guid, LabourSubtypeHolidays> ret = new Dictionary<Guid, LabourSubtypeHolidays>();
			if (idsArr.Length == 0)
				return ret;

			List<string> valNames = new List<string>();
			for (int i = 0; i < idsArr.Length; i++) {
				valNames.Add($"@val{i}");
			}

			string sql = $"SELECT * from \"labour-subtype-holidays\" WHERE id IN ({string.Join(", ", valNames)})";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			for (int i = 0; i < valNames.Count; i++) {
				cmd.Parameters.AddWithValue(valNames[i], idsArr[i]);
			}

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					LabourSubtypeHolidays obj = LabourSubtypeHolidays.FromDataReader(reader);
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



			string sql = $"DELETE FROM \"labour-subtype-holidays\" WHERE \"id\" IN ({string.Join(", ", valNames)})";
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
			Dictionary<Guid, LabourSubtypeHolidays> updateObjects,
			out List<Guid> callerResponse,
			out Dictionary<Guid, LabourSubtypeHolidays> toSendToOthers,
			bool printDots = false
			) {

			callerResponse = new List<Guid>();
			toSendToOthers = new Dictionary<Guid, LabourSubtypeHolidays>();

			foreach (KeyValuePair<Guid, LabourSubtypeHolidays> kvp in updateObjects) {

				string sql = @"
					INSERT INTO
						public.""labour-subtype-holidays""
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

		public static LabourSubtypeHolidays FromDataReader(NpgsqlDataReader reader) {

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

			return new LabourSubtypeHolidays(
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
		public bool? IsStaticDate
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyIsStaticDate];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				return tok.Value<bool>();
			}
		}

		[JsonIgnore]
		public decimal? StaticDateMonth
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyStaticDateMonth];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				return tok.Value<decimal>();
			}
		}

		[JsonIgnore]
		public decimal? StaticDateDay
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyStaticDateDay];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				return tok.Value<decimal>();
			}
		}

		[JsonIgnore]
		public bool? IsObservationDay
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyIsObservationDay];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				return tok.Value<bool>();
			}
		}

		[JsonIgnore]
		public bool? ObservationDayStatic
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyObservationDayStatic];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				return tok.Value<bool>();
			}
		}

		[JsonIgnore]
		public decimal? ObservationDayStaticMonth
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyObservationDayStaticMonth];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				return tok.Value<decimal>();
			}
		}

		[JsonIgnore]
		public decimal? ObservationDayStaticDay
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyObservationDayStaticDay];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				return tok.Value<decimal>();
			}
		}

		[JsonIgnore]
		public bool? ObservationDayActivateIfWeekend
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyObservationDayActivateIfWeekend];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				return tok.Value<bool>();
			}
		}

		[JsonIgnore]
		public bool? IsFirstMondayInMonthDate
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyIsFirstMondayInMonthDate];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				return tok.Value<bool>();
			}
		}

		[JsonIgnore]
		public decimal? FirstMondayMonth
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyFirstMondayMonth];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				return tok.Value<decimal>();
			}
		}

		[JsonIgnore]
		public bool? IsGoodFriday
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyIsGoodFriday];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				return tok.Value<bool>();
			}
		}

		[JsonIgnore]
		public bool? IsThirdMondayInMonthDate
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyIsThirdMondayInMonthDate];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				return tok.Value<bool>();
			}
		}

		[JsonIgnore]
		public decimal? ThirdMondayMonth
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyThirdMondayMonth];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				return tok.Value<decimal>();
			}
		}

		[JsonIgnore]
		public bool? IsSecondMondayInMonthDate
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyIsSecondMondayInMonthDate];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				return tok.Value<bool>();
			}
		}

		[JsonIgnore]
		public decimal? SecondMondayMonth
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeySecondMondayMonth];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				return tok.Value<decimal>();
			}
		}

		[JsonIgnore]
		public bool? IsMondayBeforeDate
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyIsMondayBeforeDate];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				return tok.Value<bool>();
			}
		}

		[JsonIgnore]
		public decimal? MondayBeforeDateMonth
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyMondayBeforeDateMonth];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				return tok.Value<decimal>();
			}
		}

		[JsonIgnore]
		public decimal? MondayBeforeDateDay
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyMondayBeforeDateDay];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				return tok.Value<decimal>();
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

			if (dpDB.TableExists("labour-subtype-holidays")) {
				Log.Debug($"----- Table \"labour-subtype-holidays\" exists.");
			} else {
				Log.Debug($"----- Table \"labour-subtype-holidays\" doesn't exist, creating.");

				using NpgsqlCommand cmd = new NpgsqlCommand(@"
					CREATE TABLE ""public"".""labour-subtype-holidays"" (
						""id"" uuid DEFAULT uuid_generate_v1() NOT NULL,
						""json"" json DEFAULT '{}' NOT NULL,
						""search-string"" character varying DEFAULT '',
						""last-modified-ISO8601"" character varying DEFAULT timestamp_iso8601(now(), 'utc') NOT NULL,
						CONSTRAINT ""labour_subtype_holidays_pk"" PRIMARY KEY(""id"")
					) WITH(oids = false);
					", dpDB);
				cmd.ExecuteNonQuery();
			}



			Console.Write("----- Table \"labour-subtype-holidays\": ");

			if (insertDefaultContents) {
				Console.Write("------ Insert default contents. ");

				Dictionary<Guid, LabourSubtypeHolidays> updateObjects = new Dictionary<Guid, LabourSubtypeHolidays>();

				// Christmas
				{
					string lastModified = DateTime.UtcNow.ToString("o", Culture.DevelopmentCulture);
					Guid id = Guid.NewGuid();
					LabourSubtypeHolidays entry = new LabourSubtypeHolidays(
						Id: id,
						Json: new JObject {
							[kJsonKeyName] = "Christmas",
							[kJsonKeyIcon] = "fa-calendar-day",
							[kJsonKeyDescription] = "Stat Holiday, held on December 25.",
							[kJsonKeyIsStaticDate] = true,
							[kJsonKeyStaticDateMonth] = 12,
							[kJsonKeyStaticDateDay] = 25,
							[kJsonKeyIsObservationDay] = false,
							[kJsonKeyObservationDayStatic] = false,
							[kJsonKeyObservationDayStaticMonth] = null,
							[kJsonKeyObservationDayStaticDay] = null,
							[kJsonKeyObservationDayActivateIfWeekend] = false,
							[kJsonKeyIsFirstMondayInMonthDate] = false,
							[kJsonKeyFirstMondayMonth] = null,
							[kJsonKeyIsGoodFriday] = false,
							[kJsonKeyIsThirdMondayInMonthDate] = false,
							[kJsonKeyThirdMondayMonth] = null,
							[kJsonKeyIsSecondMondayInMonthDate] = false,
							[kJsonKeySecondMondayMonth] = null,
							[kJsonKeyIsMondayBeforeDate] = false,
							[kJsonKeyMondayBeforeDateMonth] = null,
							[kJsonKeyMondayBeforeDateDay] = null,
						}.ToString(),
						LastModifiedIso8601: lastModified,
						SearchString: ""
					);
					updateObjects.Add(id, entry);
				}


				// Good Friday
				{
					string lastModified = DateTime.UtcNow.ToString("o", Culture.DevelopmentCulture);
					Guid id = Guid.NewGuid();
					LabourSubtypeHolidays entry = new LabourSubtypeHolidays(
						Id: id,
						Json: new JObject {
							[kJsonKeyName] = "Good Friday",
							[kJsonKeyIcon] = "fa-calendar-day",
							[kJsonKeyDescription] = "Stat Holiday, 2 days before Easter Sunday.",
							[kJsonKeyIsStaticDate] = false,
							[kJsonKeyStaticDateMonth] = null,
							[kJsonKeyStaticDateDay] = null,
							[kJsonKeyIsObservationDay] = false,
							[kJsonKeyObservationDayStatic] = false,
							[kJsonKeyObservationDayStaticMonth] = null,
							[kJsonKeyObservationDayStaticDay] = null,
							[kJsonKeyObservationDayActivateIfWeekend] = false,
							[kJsonKeyIsFirstMondayInMonthDate] = false,
							[kJsonKeyFirstMondayMonth] = null,
							[kJsonKeyIsGoodFriday] = true,
							[kJsonKeyIsThirdMondayInMonthDate] = false,
							[kJsonKeyThirdMondayMonth] = null,
							[kJsonKeyIsSecondMondayInMonthDate] = false,
							[kJsonKeySecondMondayMonth] = null,
							[kJsonKeyIsMondayBeforeDate] = false,
							[kJsonKeyMondayBeforeDateMonth] = null,
							[kJsonKeyMondayBeforeDateDay] = null,
						}.ToString(),
						LastModifiedIso8601: lastModified,
						SearchString: ""
					);
					updateObjects.Add(id, entry);
				}

				// Victoria Day
				{
					string lastModified = DateTime.UtcNow.ToString("o", Culture.DevelopmentCulture);
					Guid id = Guid.NewGuid();
					LabourSubtypeHolidays entry = new LabourSubtypeHolidays(
						Id: id,
						Json: new JObject {
							[kJsonKeyName] = "Victoria Day",
							[kJsonKeyIcon] = "fa-calendar-day",
							[kJsonKeyDescription] = "Federal Holiday, held on last Monday preceding May 25.",
							[kJsonKeyIsStaticDate] = false,
							[kJsonKeyStaticDateMonth] = null,
							[kJsonKeyStaticDateDay] = null,
							[kJsonKeyIsObservationDay] = false,
							[kJsonKeyObservationDayStatic] = false,
							[kJsonKeyObservationDayStaticMonth] = null,
							[kJsonKeyObservationDayStaticDay] = null,
							[kJsonKeyObservationDayActivateIfWeekend] = true,
							[kJsonKeyIsFirstMondayInMonthDate] = false,
							[kJsonKeyFirstMondayMonth] = null,
							[kJsonKeyIsGoodFriday] = false,
							[kJsonKeyIsThirdMondayInMonthDate] = false,
							[kJsonKeyThirdMondayMonth] = null,
							[kJsonKeyIsSecondMondayInMonthDate] = false,
							[kJsonKeySecondMondayMonth] = null,
							[kJsonKeyIsMondayBeforeDate] = true,
							[kJsonKeyMondayBeforeDateMonth] = 5,
							[kJsonKeyMondayBeforeDateDay] = 25,
						}.ToString(),
						LastModifiedIso8601: lastModified,
						SearchString: ""
					);
					updateObjects.Add(id, entry);
				}

				// Rememberance Day
				{
					string lastModified = DateTime.UtcNow.ToString("o", Culture.DevelopmentCulture);
					Guid id = Guid.NewGuid();
					LabourSubtypeHolidays entry = new LabourSubtypeHolidays(
						Id: id,
						Json: new JObject {
							[kJsonKeyName] = "Rememberance Day",
							[kJsonKeyIcon] = "fa-calendar-day",
							[kJsonKeyDescription] = "Stat Holiday, except NS MB ON QC, held on November 11th.",
							[kJsonKeyIsStaticDate] = true,
							[kJsonKeyStaticDateMonth] = 11,
							[kJsonKeyStaticDateDay] = 11,
							[kJsonKeyIsObservationDay] = false,
							[kJsonKeyObservationDayStatic] = false,
							[kJsonKeyObservationDayStaticMonth] = null,
							[kJsonKeyObservationDayStaticDay] = null,
							[kJsonKeyObservationDayActivateIfWeekend] = false,
							[kJsonKeyIsFirstMondayInMonthDate] = false,
							[kJsonKeyFirstMondayMonth] = null,
							[kJsonKeyIsGoodFriday] = false,
							[kJsonKeyIsThirdMondayInMonthDate] = false,
							[kJsonKeyThirdMondayMonth] = null,
							[kJsonKeyIsSecondMondayInMonthDate] = false,
							[kJsonKeySecondMondayMonth] = null,
							[kJsonKeyIsMondayBeforeDate] = false,
							[kJsonKeyMondayBeforeDateMonth] = null,
							[kJsonKeyMondayBeforeDateDay] = null,
						}.ToString(),
						LastModifiedIso8601: lastModified,
						SearchString: ""
					);
					updateObjects.Add(id, entry);
				}


				// New Year's Day
				{
					string lastModified = DateTime.UtcNow.ToString("o", Culture.DevelopmentCulture);
					Guid id = Guid.NewGuid();
					LabourSubtypeHolidays entry = new LabourSubtypeHolidays(
						Id: id,
						Json: new JObject {
							[kJsonKeyName] = "New Year's Day",
							[kJsonKeyIcon] = "fa-calendar-day",
							[kJsonKeyDescription] = "Stat Holiday, held on January 1.",
							[kJsonKeyIsStaticDate] = true,
							[kJsonKeyStaticDateMonth] = 1,
							[kJsonKeyStaticDateDay] = 1,
							[kJsonKeyIsObservationDay] = false,
							[kJsonKeyObservationDayStatic] = false,
							[kJsonKeyObservationDayStaticMonth] = null,
							[kJsonKeyObservationDayStaticDay] = null,
							[kJsonKeyObservationDayActivateIfWeekend] = false,
							[kJsonKeyIsFirstMondayInMonthDate] = false,
							[kJsonKeyFirstMondayMonth] = null,
							[kJsonKeyIsGoodFriday] = null,
							[kJsonKeyIsThirdMondayInMonthDate] = false,
							[kJsonKeyThirdMondayMonth] = null,
							[kJsonKeyIsSecondMondayInMonthDate] = false,
							[kJsonKeySecondMondayMonth] = null,
							[kJsonKeyIsMondayBeforeDate] = false,
							[kJsonKeyMondayBeforeDateMonth] = null,
							[kJsonKeyMondayBeforeDateDay] = null,
						}.ToString(),
						LastModifiedIso8601: lastModified,
						SearchString: ""
					);
					updateObjects.Add(id, entry);
				}

				// Boxing Day
				{
					string lastModified = DateTime.UtcNow.ToString("o", Culture.DevelopmentCulture);
					Guid id = Guid.NewGuid();
					LabourSubtypeHolidays entry = new LabourSubtypeHolidays(
						Id: id,
						Json: new JObject {
							[kJsonKeyName] = "Boxing Day",
							[kJsonKeyIcon] = "fa-calendar-day",
							[kJsonKeyDescription] = "Boxing Day, held on December 26.",
							[kJsonKeyIsStaticDate] = true,
							[kJsonKeyStaticDateMonth] = 12,
							[kJsonKeyStaticDateDay] = 26,
							[kJsonKeyIsObservationDay] = false,
							[kJsonKeyObservationDayStatic] = false,
							[kJsonKeyObservationDayStaticMonth] = null,
							[kJsonKeyObservationDayStaticDay] = null,
							[kJsonKeyObservationDayActivateIfWeekend] = false,
							[kJsonKeyIsFirstMondayInMonthDate] = false,
							[kJsonKeyFirstMondayMonth] = null,
							[kJsonKeyIsGoodFriday] = false,
							[kJsonKeyIsThirdMondayInMonthDate] = false,
							[kJsonKeyThirdMondayMonth] = null,
							[kJsonKeyIsSecondMondayInMonthDate] = false,
							[kJsonKeySecondMondayMonth] = null,
							[kJsonKeyIsMondayBeforeDate] = false,
							[kJsonKeyMondayBeforeDateMonth] = null,
							[kJsonKeyMondayBeforeDateDay] = null,
						}.ToString(),
						LastModifiedIso8601: lastModified,
						SearchString: ""
					);
					updateObjects.Add(id, entry);
				}



				// Louis Riel Day (MB) Family Day (BC, AB, ON, NB, SK) Heritage Day (NS) Islander Day (PE)
				{
					string lastModified = DateTime.UtcNow.ToString("o", Culture.DevelopmentCulture);
					Guid id = Guid.NewGuid();
					LabourSubtypeHolidays entry = new LabourSubtypeHolidays(
						Id: id,
						Json: new JObject {
							[kJsonKeyName] = "Louis Riel Day (MB) Family Day (BC, AB, ON, NB, SK) Heritage Day (NS) Islander Day (PE)",
							[kJsonKeyIcon] = "fa-calendar-day",
							[kJsonKeyDescription] = "Stat Holiday, except QC, NL, and Territories, held on the 3rd Monday in Feburary.",
							[kJsonKeyIsStaticDate] = false,
							[kJsonKeyStaticDateMonth] = null,
							[kJsonKeyStaticDateDay] = null,
							[kJsonKeyIsObservationDay] = false,
							[kJsonKeyObservationDayStatic] = false,
							[kJsonKeyObservationDayStaticMonth] = null,
							[kJsonKeyObservationDayStaticDay] = null,
							[kJsonKeyObservationDayActivateIfWeekend] = false,
							[kJsonKeyIsFirstMondayInMonthDate] = false,
							[kJsonKeyFirstMondayMonth] = null,
							[kJsonKeyIsGoodFriday] = false,
							[kJsonKeyIsThirdMondayInMonthDate] = true,
							[kJsonKeyThirdMondayMonth] = 2,
							[kJsonKeyIsSecondMondayInMonthDate] = false,
							[kJsonKeySecondMondayMonth] = null,
							[kJsonKeyIsMondayBeforeDate] = false,
							[kJsonKeyMondayBeforeDateMonth] = null,
							[kJsonKeyMondayBeforeDateDay] = null,
						}.ToString(),
						LastModifiedIso8601: lastModified,
						SearchString: ""
					);
					updateObjects.Add(id, entry);
				}

				// Canada Day
				{
					string lastModified = DateTime.UtcNow.ToString("o", Culture.DevelopmentCulture);
					Guid id = Guid.NewGuid();
					LabourSubtypeHolidays entry = new LabourSubtypeHolidays(
						Id: id,
						Json: new JObject {
							[kJsonKeyName] = "Canada Day",
							[kJsonKeyIcon] = "fa-calendar-day",
							[kJsonKeyDescription] = "Federal Holiday, held on July 1.",
							[kJsonKeyIsStaticDate] = true,
							[kJsonKeyStaticDateMonth] = 7,
							[kJsonKeyStaticDateDay] = 1,
							[kJsonKeyIsObservationDay] = false,
							[kJsonKeyObservationDayStatic] = false,
							[kJsonKeyObservationDayStaticMonth] = null,
							[kJsonKeyObservationDayStaticDay] = null,
							[kJsonKeyObservationDayActivateIfWeekend] = false,
							[kJsonKeyIsFirstMondayInMonthDate] = false,
							[kJsonKeyFirstMondayMonth] = null,
							[kJsonKeyIsGoodFriday] = false,
							[kJsonKeyIsThirdMondayInMonthDate] = false,
							[kJsonKeyThirdMondayMonth] = null,
							[kJsonKeyIsSecondMondayInMonthDate] = false,
							[kJsonKeySecondMondayMonth] = null,
							[kJsonKeyIsMondayBeforeDate] = false,
							[kJsonKeyMondayBeforeDateMonth] = null,
							[kJsonKeyMondayBeforeDateDay] = null,
						}.ToString(),
						LastModifiedIso8601: lastModified,
						SearchString: ""
					);
					updateObjects.Add(id, entry);
				}

				// Labour Day
				{
					string lastModified = DateTime.UtcNow.ToString("o", Culture.DevelopmentCulture);
					Guid id = Guid.NewGuid();
					LabourSubtypeHolidays entry = new LabourSubtypeHolidays(
						Id: id,
						Json: new JObject {
							[kJsonKeyName] = "Labour Day",
							[kJsonKeyIcon] = "fa-calendar-day",
							[kJsonKeyDescription] = "Stat Holiday, held on the 1st Monday in September.",
							[kJsonKeyIsStaticDate] = false,
							[kJsonKeyStaticDateMonth] = null,
							[kJsonKeyStaticDateDay] = null,
							[kJsonKeyIsObservationDay] = false,
							[kJsonKeyObservationDayStatic] = false,
							[kJsonKeyObservationDayStaticMonth] = null,
							[kJsonKeyObservationDayStaticDay] = null,
							[kJsonKeyObservationDayActivateIfWeekend] = false,
							[kJsonKeyIsFirstMondayInMonthDate] = true,
							[kJsonKeyFirstMondayMonth] = 9,
							[kJsonKeyIsGoodFriday] = false,
							[kJsonKeyIsThirdMondayInMonthDate] = false,
							[kJsonKeyThirdMondayMonth] = null,
							[kJsonKeyIsSecondMondayInMonthDate] = false,
							[kJsonKeySecondMondayMonth] = null,
							[kJsonKeyIsMondayBeforeDate] = false,
							[kJsonKeyMondayBeforeDateMonth] = null,
							[kJsonKeyMondayBeforeDateDay] = null,
						}.ToString(),
						LastModifiedIso8601: lastModified,
						SearchString: ""
					);
					updateObjects.Add(id, entry);
				}

				// Civic Holiday
				{
					string lastModified = DateTime.UtcNow.ToString("o", Culture.DevelopmentCulture);
					Guid id = Guid.NewGuid();
					LabourSubtypeHolidays entry = new LabourSubtypeHolidays(
						Id: id,
						Json: new JObject {
							[kJsonKeyName] = "Civic Holiday",
							[kJsonKeyIcon] = "fa-calendar-day",
							[kJsonKeyDescription] = "Stat Holiday, except NS MB and PEI, held on the 1st Monday in August.",
							[kJsonKeyIsStaticDate] = false,
							[kJsonKeyStaticDateMonth] = null,
							[kJsonKeyStaticDateDay] = null,
							[kJsonKeyIsObservationDay] = false,
							[kJsonKeyObservationDayStatic] = false,
							[kJsonKeyObservationDayStaticMonth] = null,
							[kJsonKeyObservationDayStaticDay] = null,
							[kJsonKeyObservationDayActivateIfWeekend] = false,
							[kJsonKeyIsFirstMondayInMonthDate] = true,
							[kJsonKeyFirstMondayMonth] = 8,
							[kJsonKeyIsGoodFriday] = false,
							[kJsonKeyIsThirdMondayInMonthDate] = false,
							[kJsonKeyThirdMondayMonth] = null,
							[kJsonKeyIsSecondMondayInMonthDate] = false,
							[kJsonKeySecondMondayMonth] = null,
							[kJsonKeyIsMondayBeforeDate] = false,
							[kJsonKeyMondayBeforeDateMonth] = null,
							[kJsonKeyMondayBeforeDateDay] = null,
						}.ToString(),
						LastModifiedIso8601: lastModified,
						SearchString: ""
					);
					updateObjects.Add(id, entry);
				}


				// Thanksgiving
				{
					string lastModified = DateTime.UtcNow.ToString("o", Culture.DevelopmentCulture);
					Guid id = Guid.NewGuid();
					LabourSubtypeHolidays entry = new LabourSubtypeHolidays(
						Id: id,
						Json: new JObject {
							[kJsonKeyName] = "Thanksgiving",
							[kJsonKeyIcon] = "fa-calendar-day",
							[kJsonKeyDescription] = "Stat Holiday, except Atlantic Provinces, held on the 2nd Monday in October.",
							[kJsonKeyIsStaticDate] = false,
							[kJsonKeyStaticDateMonth] = null,
							[kJsonKeyStaticDateDay] = null,
							[kJsonKeyIsObservationDay] = false,
							[kJsonKeyObservationDayStatic] = false,
							[kJsonKeyObservationDayStaticMonth] = null,
							[kJsonKeyObservationDayStaticDay] = null,
							[kJsonKeyObservationDayActivateIfWeekend] = false,
							[kJsonKeyIsFirstMondayInMonthDate] = false,
							[kJsonKeyFirstMondayMonth] = null,
							[kJsonKeyIsGoodFriday] = false,
							[kJsonKeyIsThirdMondayInMonthDate] = false,
							[kJsonKeyThirdMondayMonth] = null,
							[kJsonKeyIsSecondMondayInMonthDate] = true,
							[kJsonKeySecondMondayMonth] = 10,
							[kJsonKeyIsMondayBeforeDate] = false,
							[kJsonKeyMondayBeforeDateMonth] = null,
							[kJsonKeyMondayBeforeDateDay] = null,
						}.ToString(),
						LastModifiedIso8601: lastModified,
						SearchString: ""
					);
					updateObjects.Add(id, entry);
				}















				Console.Write(" saving");
				LabourSubtypeHolidays.Upsert(dpDB, updateObjects, out _, out _, true);
			}


			{
				Console.Write("------ Repair Existing ");
				Dictionary<Guid, LabourSubtypeHolidays> updateObjects = new Dictionary<Guid, LabourSubtypeHolidays>();

				Dictionary<Guid, LabourSubtypeHolidays> all = LabourSubtypeHolidays.All(dpDB);

				foreach (KeyValuePair<Guid, LabourSubtypeHolidays> kvp in all) {

					JObject? root = kvp.Value.JsonObject;
					if (null == root) {
						continue;
					}
					JToken? lastModifiedInJSONTok = root["lastModifiedISO8601"];

					LabourSubtypeHolidays obj = new LabourSubtypeHolidays(
							Id: kvp.Key,
							Json: root.ToString(Newtonsoft.Json.Formatting.Indented),
							LastModifiedIso8601: null == lastModifiedInJSONTok ? kvp.Value.LastModifiedIso8601 : lastModifiedInJSONTok.Value<string>(),
							SearchString: kvp.Value.GeneratedSearchString
							);



					Console.Write(".");

					updateObjects.Add(kvp.Key, obj);
				}

				Console.Write(" saving");
				LabourSubtypeHolidays.Upsert(dpDB, updateObjects, out _, out _, true);
			}
			

			Log.Debug(" done.");
		}






	}
}
