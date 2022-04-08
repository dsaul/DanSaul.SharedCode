using API.Hubs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;
using SharedCode.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using SharedCode.Cal;
using Ical.Net;
using Ical.Net.DataTypes;
using Ical.Net.CalendarComponents;
using Serilog;

namespace SharedCode.DatabaseSchemas
{
	public record Calendars(Guid? Id, string? Json, string? SearchString, string? LastModifiedIso8601) : JSONTable(Id, Json, SearchString, LastModifiedIso8601)
	{

		public const string kJsonKeyName = "name";
		public const string kJsonKeyICalFileURI = "iCalFileURI";
		public const string kJsonKeyICalFileLastRetrievedISO8601 = "iCalFileLastRetrievedISO8601";
		public const string kJsonKeyICalFileLastData = "iCalFileLastData";
		public const string kJsonKeyOccurancesRoughlyAroundThisMonth = "occurancesRoughlyAroundThisMonth";

		public const string kJsonKeyOccurancesKeyStartISO8601 = "startISO8601";
		public const string kJsonKeyOccurancesKeyEndISO8601 = "endISO8601";
		public const string kJsonKeyOccurancesKeyDurationTotalSeconds = "durationTotalSeconds";
		public const string kJsonKeyOccurancesKeyDescription = "description";
		public const string kJsonKeyOccurancesKeyPhoneNumber = "phoneNumber";

		public static Dictionary<Guid, Calendars> ForId(NpgsqlConnection connection, Guid id) {

			Dictionary<Guid, Calendars> ret = new Dictionary<Guid, Calendars>();

			string sql = @"SELECT * from ""calendars"" WHERE id = @id";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			cmd.Parameters.AddWithValue("@id", id);




			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					Calendars obj = Calendars.FromDataReader(reader);
					if (obj.Id == null) {
						continue;
					}
					ret.Add(obj.Id.Value, obj);
				}
			}

			return ret;
		}


		public static Dictionary<Guid, Calendars> All(NpgsqlConnection connection) {

			Dictionary<Guid, Calendars> ret = new Dictionary<Guid, Calendars>();

			string sql = @"SELECT * from ""calendars""";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					Calendars obj = Calendars.FromDataReader(reader);
					if (obj.Id == null) {
						continue;
					}
					ret.Add(obj.Id.Value, obj);
				}
			}

			return ret;


		}

		public static Dictionary<Guid, Calendars> ForIds(NpgsqlConnection connection, IEnumerable<Guid> ids) {

			Guid[] idsArr = ids.ToArray();

			Dictionary<Guid, Calendars> ret = new Dictionary<Guid, Calendars>();
			if (idsArr.Length == 0)
				return ret;

			List<string> valNames = new List<string>();
			for (int i = 0; i < idsArr.Length; i++) {
				valNames.Add($"@val{i}");
			}

			string sql = $"SELECT * from \"calendars\" WHERE id IN ({string.Join(", ", valNames)})";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			for (int i = 0; i < valNames.Count; i++) {
				cmd.Parameters.AddWithValue(valNames[i], idsArr[i]);
			}

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					Calendars obj = Calendars.FromDataReader(reader);
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



			string sql = $"DELETE FROM \"calendars\" WHERE \"id\" IN ({string.Join(", ", valNames)})";
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
			Dictionary<Guid, Calendars> updateObjects,
			out List<Guid> callerResponse,
			out Dictionary<Guid, Calendars> toSendToOthers,
			bool printDots = false
			) {

			callerResponse = new List<Guid>();
			toSendToOthers = new Dictionary<Guid, Calendars>();

			foreach (KeyValuePair<Guid, Calendars> kvp in updateObjects) {

				string sql = @"
					INSERT INTO
						public.""calendars""
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

		public static Calendars FromDataReader(NpgsqlDataReader reader) {

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

			return new Calendars(
				Id: id,
				Json: json,
				SearchString: searchString,
				LastModifiedIso8601: lastModifiedIso8601
				);
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
		public string? ICalFileURI
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyICalFileURI];
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
		public string? ICalFileLastRetrievedISO8601
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyICalFileLastRetrievedISO8601];
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
		public string? ICalFileLastData
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyICalFileLastData];
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
		public HashSet<CalendarOnCallPhoneNumber>OnCallPhoneNumbersRightNow
		{
			get {
				var calendar = Calendar.Load(ICalFileLastData);

				return CalendarUtils.CalendarOnCallPhoneNumbersRightNow(calendar);
			}
		}

		[JsonIgnore]
		public HashSet<string> OnCallEMailsRightNow
		{
			get {
				var calendar = Calendar.Load(ICalFileLastData);

				return CalendarUtils.CalendarOnCallEMailsRightNow(calendar);
			}
		}





		public static async Task RetrieveCalendar(NpgsqlConnection dpDB, IdempotencyResponse response, Guid calendarId) {

			var resCals = Calendars.ForId(dpDB, calendarId);
			if (0 == resCals.Count) {
				response.IsError = true;
				response.ErrorMessage = "Unable to find calendar.";
				return;
			}

			Calendars cal = resCals.FirstOrDefault().Value;
			if (null == cal.JsonObject) {
				response.IsError = true;
				response.ErrorMessage = "Calendar json data is empty.";
				return;
			}
			if (null == cal.Id) {
				response.IsError = true;
				response.ErrorMessage = "Calendar id is empty.";
				return;
			}

			try {
				
				
				using HttpClient httpClient = new HttpClient();
				httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/88.0.4324.182 Safari/537.36");
				HttpResponseMessage? httpResp = null;

				try {
					httpResp = await httpClient.GetAsync(cal.ICalFileURI);
				} catch (InvalidOperationException) {
					if (null != httpResp) {
						httpResp.Dispose();
						httpResp = null;
					}
					throw new Exception("Unable to fetch calendar, the web link to the calendar isn't valid.");
				}
				
				if (httpResp.StatusCode != HttpStatusCode.OK) {
					response.IsError = true;
					response.ErrorMessage = $"Unable to fetch calendar {httpResp.StatusCode}.";
					return;
				}

				using HttpContent httpContent = httpResp.Content;
				string icalStr = await httpContent.ReadAsStringAsync();
				if (string.IsNullOrWhiteSpace(icalStr)) {
					response.IsError = true;
					response.ErrorMessage = $"Empty calendar recieved.";
					return;
				}

				JObject? modJson = cal.JsonObject.DeepClone() as JObject;
				if (null != modJson) {
					modJson[Calendars.kJsonKeyICalFileLastRetrievedISO8601] =
						DateTime.UtcNow.ToString("o", SharedCode.Culture.Konstants.DevelopmentCulture);
					modJson[Calendars.kJsonKeyICalFileLastData] = icalStr;


					Calendar iCal = Calendar.Load(icalStr);
					HashSet<Occurrence> occurances = CalendarUtils.OccurancesRoughlyAroundThisMonth(iCal);

					JArray occurancesJA = new JArray();
					foreach (Occurrence occ in occurances) {

						Period p = occ.Period;
						DateTime dtStartUtc = p.StartTime.AsUtc;
						DateTime dtEndUtc = p.EndTime.AsUtc;
						TimeSpan duration = p.Duration;


						string dtStartISO8601 = dtStartUtc.ToString("o", SharedCode.Culture.Konstants.DevelopmentCulture);
						string dtEndISO8601 = dtEndUtc.ToString("o", SharedCode.Culture.Konstants.DevelopmentCulture);
						double totalSeconds = duration.TotalSeconds;

						JObject obj = new JObject();
						obj[kJsonKeyOccurancesKeyStartISO8601] = dtStartISO8601;
						obj[kJsonKeyOccurancesKeyEndISO8601] = dtEndISO8601;
						obj[kJsonKeyOccurancesKeyDurationTotalSeconds] = totalSeconds;


						CalendarEvent? evt = occ.Source as CalendarEvent;
						if (null != evt) {
							string description = evt.Summary.Trim();
							string number = evt.Location.Trim();

							obj[kJsonKeyOccurancesKeyDescription] = description;
							obj[kJsonKeyOccurancesKeyPhoneNumber] = number;
						}

						occurancesJA.Add(obj);

					}
					modJson[kJsonKeyOccurancesRoughlyAroundThisMonth] = occurancesJA;







					Calendars calMod = cal with
					{
						Json = modJson.ToString()
					};

					Calendars.Upsert(dpDB, new Dictionary<Guid, Calendars>
					{
					{ cal.Id.Value, calMod }
				}, out _, out _);


				}

				if (null != httpResp) {
					httpResp.Dispose();
					httpResp = null;
				}
			} catch (Exception e) {
				response.IsError = true;
				response.ErrorMessage = e.Message;
				return;
			}
			


			

			

			

			

		}



		public static void VerifyRepairTable(NpgsqlConnection dpDB, bool insertDefaultContents = false) {

			if (dpDB.TableExists("calendars")) {
				Log.Debug($"----- Table \"calendars\" exists.");
			} else {
				Log.Debug($"----- Table \"calendars\" doesn't exist, creating.");

				using NpgsqlCommand cmd = new NpgsqlCommand(@"
					CREATE TABLE ""public"".""calendars"" (
						""id"" uuid DEFAULT uuid_generate_v1() NOT NULL,
						""json"" json DEFAULT '{}' NOT NULL,
						""search-string"" character varying DEFAULT '',
						""last-modified-ISO8601"" character varying DEFAULT timestamp_iso8601(now(), 'utc') NOT NULL,
						CONSTRAINT ""calendars_pk"" PRIMARY KEY(""id"")
					) WITH(oids = false);
					", dpDB);
				cmd.ExecuteNonQuery();

#warning TODO: implement
			}
		}

	}
}
