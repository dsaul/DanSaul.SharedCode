using Npgsql;
using System.Text;
using System.Data;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Serilog;

namespace SharedCode.DatabaseSchemas
{
	public record Contacts(Guid? Id, string? Json, string? SearchString, string? LastModifiedIso8601) : JSONTable(Id, Json, SearchString, LastModifiedIso8601)
	{
		public static Dictionary<Guid, Contacts> ForId(NpgsqlConnection connection, Guid id) {

			Dictionary<Guid, Contacts> ret = new Dictionary<Guid, Contacts>();

			string sql = @"SELECT * from ""contacts"" WHERE id = @id";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			cmd.Parameters.AddWithValue("@id", id);




			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					Contacts obj = Contacts.FromDataReader(reader);
					if (obj.Id == null) {
						continue;
					}
					ret.Add(obj.Id.Value, obj);
				}
			}

			return ret;
		}


		public static Dictionary<Guid, Contacts> All(NpgsqlConnection connection) {

			Dictionary<Guid, Contacts> ret = new Dictionary<Guid, Contacts>();

			string sql = @"SELECT * from ""contacts""";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					Contacts obj = Contacts.FromDataReader(reader);
					if (obj.Id == null) {
						continue;
					}
					ret.Add(obj.Id.Value, obj);
				}
			}

			return ret;


		}

		public static Dictionary<Guid, Contacts> ForIds(NpgsqlConnection connection, IEnumerable<Guid> ids) {

			Guid[] idsArr = ids.ToArray();

			Dictionary<Guid, Contacts> ret = new Dictionary<Guid, Contacts>();
			if (idsArr.Length == 0)
				return ret;

			List<string> valNames = new List<string>();
			for (int i = 0; i < idsArr.Length; i++) {
				valNames.Add($"@val{i}");
			}

			string sql = $"SELECT * from \"contacts\" WHERE id IN ({string.Join(", ", valNames)})";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			for (int i = 0; i < valNames.Count; i++) {
				cmd.Parameters.AddWithValue(valNames[i], idsArr[i]);
			}

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					Contacts obj = Contacts.FromDataReader(reader);
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



			string sql = $"DELETE FROM \"contacts\" WHERE \"id\" IN ({string.Join(", ", valNames)})";
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
			Dictionary<Guid, Contacts> updateObjects,
			out List<Guid> callerResponse,
			out Dictionary<Guid, Contacts> toSendToOthers,
			bool printDots = false) {

			callerResponse = new List<Guid>();
			toSendToOthers = new Dictionary<Guid, Contacts>();

			foreach (KeyValuePair<Guid, Contacts> kvp in updateObjects) {

				string sql = @"
					INSERT INTO
						public.""contacts""
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


		public static Contacts FromDataReader(NpgsqlDataReader reader) {

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

			return new Contacts(
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

				string str = tok.Value<string>();
				if (string.IsNullOrWhiteSpace(str)) {
					return null;
				}

				return str;
			}
		}

		[JsonIgnore]
		public string? Title
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root["title"];
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
		public Guid? CompanyId
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root["companyId"];
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
		public string? Notes
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root["notes"];
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
		public List<PhoneNumber> PhoneNumbers
		{
			get {
				List<PhoneNumber> ret = new List<PhoneNumber>();


				JObject? root = JsonObject;

				if (null == root) {
					return ret;
				}

				JArray? arr = root["phoneNumbers"] as JArray;
				if (null == arr) {
					return ret;
				}

				foreach (JObject addr in arr) {

					Guid? id = null;


					if (Guid.TryParse(addr.Value<string>("id"), out Guid tmp))
						id = tmp;



					ret.Add(new PhoneNumber(
						id,
						addr.Value<string>("label"),
						addr.Value<string>("value")));

				}

				return ret;
			}
		}

		[JsonIgnore]
		public string PhoneNumbersSearchString
		{
			get {
				StringBuilder sb = new StringBuilder();

				foreach (PhoneNumber number in PhoneNumbers) {
					sb.Append($" {number.Label}: {number.Value} ");
				}

				return sb.ToString();
			}
		}


		[JsonIgnore]
		public List<EMail> EMails
		{
			get {
				List<EMail> ret = new List<EMail>();


				JObject? root = JsonObject;

				if (null == root) {
					return ret;
				}

				JArray? arr = root["emails"] as JArray;
				if (null == arr) {
					return ret;
				}

				foreach (JObject addr in arr) {

					Guid? id = null;


					if (Guid.TryParse(addr.Value<string>("id"), out Guid tmp))
						id = tmp;



					ret.Add(new EMail(
						id,
						addr.Value<string>("label"),
						addr.Value<string>("value")));

				}

				return ret;
			}
		}

		[JsonIgnore]
		public string EMailsSearchString
		{
			get {
				StringBuilder sb = new StringBuilder();

				foreach (EMail email in EMails) {
					sb.Append($" {email.Label}: {email.Value} ");
				}

				return sb.ToString();
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






		public string GeneratedSearchString(NpgsqlConnection dpDB)
		{

			string? companyName = null;
			do {
				if (null == CompanyId)
					break;

				var resCompany = Companies.ForId(dpDB, CompanyId.Value);
				if (resCompany.Count == 0)
					break;

				Companies company = resCompany.FirstOrDefault().Value;
				companyName = company.Name;

			} while (false);
			
			
			return GenerateSearchString(Name, Title, companyName, Notes, PhoneNumbersSearchString, AddressesSearchString, EMailsSearchString);
		}

		public static string GenerateSearchString(
			string? name,
			string? title,
			string? companyName,
			string? notes,
			string? phoneNumbersSearchString,
			string? addressesSearchString,
			string? eMailsSearchString
			) {



			return $"{name} {title} {companyName} {notes} {phoneNumbersSearchString} {addressesSearchString} {eMailsSearchString}";
		}



		public static void VerifyRepairTable(NpgsqlConnection dpDB, bool insertDefaultContents = false) {

			if (dpDB.TableExists("contacts")) {
				Log.Debug($"----- Table \"contacts\" exists.");
			} else {
				Log.Debug($"----- Table \"contacts\" doesn't exist, creating.");

				using NpgsqlCommand cmd = new NpgsqlCommand(@"
					CREATE TABLE ""public"".""contacts"" (
						""id"" uuid DEFAULT uuid_generate_v1() NOT NULL,
						""json"" json DEFAULT '{}' NOT NULL,
						""search-string"" character varying DEFAULT '',
						""last-modified-ISO8601"" character varying DEFAULT timestamp_iso8601(now(), 'utc') NOT NULL,
						CONSTRAINT ""contacts_pk"" PRIMARY KEY(""id"")
					) WITH(oids = false);
					", dpDB);
				cmd.ExecuteNonQuery();
			}

			Console.Write("----- Table \"contacts\": ");
			Dictionary<Guid, Contacts> updateObjects = new Dictionary<Guid, Contacts>();
			{
				Dictionary<Guid, Contacts> all = Contacts.All(dpDB);

				foreach (KeyValuePair<Guid, Contacts> kvp in all) {

					JObject? root = kvp.Value.JsonObject;
					if (null == root)
						continue;

					JToken? lastModifiedInJSONTok = root["lastModifiedISO8601"];

					Contacts obj = new Contacts(
							Id: kvp.Key,
							Json: root.ToString(Newtonsoft.Json.Formatting.Indented),
							LastModifiedIso8601: null == lastModifiedInJSONTok ? kvp.Value.LastModifiedIso8601 : lastModifiedInJSONTok.Value<string>(),
							SearchString: kvp.Value.GeneratedSearchString(dpDB)
							);



					Console.Write(".");

					updateObjects.Add(kvp.Key, obj);
				}
			}
			Console.Write(" saving");
			Contacts.Upsert(dpDB, updateObjects, out _, out _, true);

			Log.Debug(" done.");
		}







	}
}
