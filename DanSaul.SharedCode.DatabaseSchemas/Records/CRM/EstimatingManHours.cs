using Npgsql;
using System.Text;
using System.Data;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Serilog;
using DanSaul.SharedCode.Npgsql;

namespace SharedCode.DatabaseSchemas
{
	public record EstimatingManHours(
		Guid? Id, 
		string? Json, 
		string? SearchString, 
		string? LastModifiedIso8601
		) : JSONTable(Id, Json, SearchString, LastModifiedIso8601)
	{
		public static Dictionary<Guid, EstimatingManHours> ForId(NpgsqlConnection connection, Guid id) {

			Dictionary<Guid, EstimatingManHours> ret = new Dictionary<Guid, EstimatingManHours>();

			string sql = @"SELECT * from ""estimating-man-hours"" WHERE id = @id";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			cmd.Parameters.AddWithValue("@id", id);




			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					EstimatingManHours obj = EstimatingManHours.FromDataReader(reader);
					if (obj.Id == null) {
						continue;
					}
					ret.Add(obj.Id.Value, obj);
				}
			}

			return ret;
		}

		public static Dictionary<Guid, EstimatingManHours> All(NpgsqlConnection connection) {

			Dictionary<Guid, EstimatingManHours> ret = new Dictionary<Guid, EstimatingManHours>();

			string sql = @"SELECT * from ""estimating-man-hours""";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					EstimatingManHours obj = EstimatingManHours.FromDataReader(reader);
					if (obj.Id == null) {
						continue;
					}
					ret.Add(obj.Id.Value, obj);
				}
			}

			return ret;
		}

		public static Dictionary<Guid, EstimatingManHours> ForIds(NpgsqlConnection connection, IEnumerable<Guid> ids) {

			Guid[] idsArr = ids.ToArray();

			Dictionary<Guid, EstimatingManHours> ret = new Dictionary<Guid, EstimatingManHours>();
			if (idsArr.Length == 0)
				return ret;

			List<string> valNames = new List<string>();
			for (int i = 0; i < idsArr.Length; i++) {
				valNames.Add($"@val{i}");
			}

			string sql = $"SELECT * from \"estimating-man-hours\" WHERE id IN ({string.Join(", ", valNames)})";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			for (int i = 0; i < valNames.Count; i++) {
				cmd.Parameters.AddWithValue(valNames[i], idsArr[i]);
			}

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					EstimatingManHours obj = EstimatingManHours.FromDataReader(reader);
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



			string sql = $"DELETE FROM \"estimating-man-hours\" WHERE \"id\" IN ({string.Join(", ", valNames)})";
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
			Dictionary<Guid, EstimatingManHours> updateObjects,
			out List<Guid> callerResponse,
			out Dictionary<Guid, EstimatingManHours> toSendToOthers,
			bool printDots = false
			) {

			callerResponse = new List<Guid>();
			toSendToOthers = new Dictionary<Guid, EstimatingManHours>();

			foreach (KeyValuePair<Guid, EstimatingManHours> kvp in updateObjects) {

				string sql = @"
					INSERT INTO
						public.""estimating-man-hours""
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


		public static EstimatingManHours FromDataReader(NpgsqlDataReader reader) {

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

			return new EstimatingManHours(
				Id: id,
				Json: json,
				SearchString: searchString,
				LastModifiedIso8601: lastModifiedIso8601
				);
		}


		[JsonIgnore]
		public string? Item
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root["item"];
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
		public decimal? ManHours
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root["manHours"];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				return tok.Value<decimal>();
			}
		}


		[JsonIgnore]
		public string? Measurement
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root["measurement"];
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
		public string GeneratedSearchString
		{
			get {
				return GenerateSearchString(Item, ManHours, Measurement);
			}
		}

		public static string GenerateSearchString(string? item, decimal? manhours, string? measurement) {
			return $"{item} {manhours} {measurement}";
		}




		public static void VerifyRepairTable(NpgsqlConnection dpDB, bool insertDefaultContents = false) {

			if (dpDB.TableExists("estimating-man-hours")) {
				Log.Debug($"----- Table \"estimating-man-hours\" exists.");
			} else {
				Log.Debug($"----- Table \"estimating-man-hours\" doesn't exist, creating.");

				using NpgsqlCommand cmd = new NpgsqlCommand(@"
					CREATE TABLE ""public"".""estimating-man-hours"" (
						""id"" uuid DEFAULT uuid_generate_v1() NOT NULL,
						""json"" json DEFAULT '{}' NOT NULL,
						""search-string"" character varying DEFAULT '',
						""last-modified-ISO8601"" character varying DEFAULT timestamp_iso8601(now(), 'utc') NOT NULL,
						CONSTRAINT ""estimating_man_hours_pk"" PRIMARY KEY(""id"")
					) WITH(oids = false);
					", dpDB);
				cmd.ExecuteNonQuery();
			}

			Console.Write("----- Table \"estimating-man-hours\": ");
			Dictionary<Guid, EstimatingManHours> updateObjects = new Dictionary<Guid, EstimatingManHours>();
			{
				Dictionary<Guid, EstimatingManHours> all = EstimatingManHours.All(dpDB);

				foreach (KeyValuePair<Guid, EstimatingManHours> kvp in all) {

					JObject? root = kvp.Value.JsonObject;
					if (null == root)
						continue;
					JToken? lastModifiedInJSONTok = root["lastModifiedISO8601"];

					EstimatingManHours obj = new EstimatingManHours(
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
			EstimatingManHours.Upsert(dpDB, updateObjects, out _, out _, true);

			Log.Debug(" done.");
		}








	}
}
