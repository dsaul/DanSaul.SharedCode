using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using SharedCode.Extensions;
using Serilog;

namespace SharedCode.DatabaseSchemas
{
	public record Materials(Guid? Id, string? Json, string? SearchString, string? LastModifiedIso8601) : JSONTable(Id, Json, SearchString, LastModifiedIso8601)
	{
		public static Dictionary<Guid, Materials> ForId(NpgsqlConnection connection, Guid id) {

			Dictionary<Guid, Materials> ret = new Dictionary<Guid, Materials>();

			string sql = @"SELECT * from ""materials"" WHERE id = @id";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			cmd.Parameters.AddWithValue("@id", id);




			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					Materials obj = Materials.FromDataReader(reader);
					if (obj.Id == null) {
						continue;
					}
					ret.Add(obj.Id.Value, obj);
				}
			}

			return ret;
		}


		public static Dictionary<Guid, Materials> All(NpgsqlConnection connection) {

			Dictionary<Guid, Materials> ret = new Dictionary<Guid, Materials>();

			string sql = @"SELECT * from ""materials""";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					Materials obj = Materials.FromDataReader(reader);
					if (obj.Id == null) {
						continue;
					}
					ret.Add(obj.Id.Value, obj);
				}
			}

			return ret;


		}

		public static Dictionary<Guid, Materials> ForIds(NpgsqlConnection connection, IEnumerable<Guid> ids) {

			Guid[] idsArr = ids.ToArray();

			Dictionary<Guid, Materials> ret = new Dictionary<Guid, Materials>();
			if (idsArr.Length == 0)
				return ret;

			List<string> valNames = new List<string>();
			for (int i = 0; i < idsArr.Length; i++) {
				valNames.Add($"@val{i}");
			}

			string sql = $"SELECT * from \"materials\" WHERE id IN ({string.Join(", ", valNames)})";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			for (int i = 0; i < valNames.Count; i++) {
				cmd.Parameters.AddWithValue(valNames[i], idsArr[i]);
			}

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					Materials obj = Materials.FromDataReader(reader);
					if (obj.Id == null) {
						continue;
					}
					ret.Add(obj.Id.Value, obj);
				}
			}

			return ret;


		}

		public static Dictionary<Guid, Materials> ForProjectId(NpgsqlConnection connection, Guid projectId) {

			Dictionary<Guid, Materials> ret = new Dictionary<Guid, Materials>();

			string sql = @"SELECT * from ""materials"" WHERE json ->> 'projectId' = @projectId";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			cmd.Parameters.AddWithValue("@projectId", projectId.ToString());




			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					Materials obj = Materials.FromDataReader(reader);
					if (obj.Id == null) {
						continue;
					}
					ret.Add(obj.Id.Value, obj);
				}
			}

			return ret;
		}

		public static Dictionary<Guid, Materials> ForProjects(NpgsqlConnection connection, IEnumerable<Projects> projects) {
			List<Guid> ids = new List<Guid>();
			foreach (Projects project in projects) {
				if (project == null || project.Id == null)
					continue;
				ids.Add(project.Id.Value);
			}
			return ForProjectIds(connection, ids);
		}

		public static Dictionary<Guid, Materials> ForProjectIds(NpgsqlConnection connection, IEnumerable<Guid> projectIds) {

			var needles = projectIds.ToArray();

			Dictionary<Guid, Materials> ret = new Dictionary<Guid, Materials>();
			if (needles.Length == 0)
				return ret;

			List<string> valNames = new List<string>();
			for (int i = 0; i < needles.Length; i++) {
				valNames.Add($"@val{i}");
			}

			string sql = $"SELECT * from \"materials\" WHERE (json ->> 'projectId')::uuid IN ({string.Join(", ", valNames)})";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			for (int i = 0; i < valNames.Count; i++) {
				cmd.Parameters.AddWithValue(valNames[i], needles[i]);
			}

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					Materials obj = Materials.FromDataReader(reader);
					if (obj.Id == null)
						continue;
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



			string sql = $"DELETE FROM \"materials\" WHERE \"id\" IN ({string.Join(", ", valNames)})";
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
			Dictionary<Guid, Materials> updateObjects,
			out List<Guid> callerResponse,
			out Dictionary<Guid, Materials> toSendToOthers,
			bool printDots = false
			) {

			callerResponse = new List<Guid>();
			toSendToOthers = new Dictionary<Guid, Materials>();

			foreach (KeyValuePair<Guid, Materials> kvp in updateObjects) {

				string sql = @"
					INSERT INTO
						public.""materials""
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


		public static Materials FromDataReader(NpgsqlDataReader reader) {

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

			return new Materials(
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
		public string? DateUsedISO8601
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root["dateUsedISO8601"];
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
		public decimal? Quantity
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root["quantity"];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				return tok.Value<decimal>();
			}
		}

		[JsonIgnore]
		public string? QuantityUnit
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root["quantityUnit"];
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
		public Guid? ProductId
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root["productId"];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				string str = tok.Value<string>();
				if (string.IsNullOrWhiteSpace(str)) {
					return null;
				}

				return !Guid.TryParse(str, out Guid guid) ? null : guid;
			}
		}


		[JsonIgnore]
		public bool? IsExtra
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root["isExtra"];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				return tok.Value<bool>();
			}
		}

		[JsonIgnore]
		public bool? IsBilled
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root["isBilled"];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				return tok.Value<bool>();
			}
		}

		[JsonIgnore]
		public string? Location
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root["location"];
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



		public string GeneratedSearchString(NpgsqlConnection connection) {
			string? projectStr = null;
			string? productStr = null;

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
				if (null == ProductId)
					break;

				var resProduct = Products.ForId(connection, ProductId.Value);
				if (0 == resProduct.Count)
					break;

				Products product = resProduct.FirstOrDefault().Value;

				productStr = product.Name;

			} while (false);


			string isExtraStr = (null != IsExtra && IsExtra.Value) ? "Is Extra" : "";
			string isBilledStr = (null != IsExtra && IsExtra.Value) ? "Is Billed" : "";



			return GenerateSearchString(
				projectStr,
				Quantity.ToString(),
				QuantityUnit,
				productStr,
				isExtraStr,
				isBilledStr,
				Location
				);
		}

		public static string GenerateSearchString(
			string? projectStr,
			string? quantityStr,
			string? quantityUnitStr,
			string? productStr,
			string? isExtraStr,
			string? isBilledStr,
			string? locationStr
			) {
			return $"{projectStr} {quantityStr} {quantityUnitStr} {productStr} {isExtraStr} {isBilledStr} {locationStr}";
		}

		












		public static void VerifyRepairTable(NpgsqlConnection dpDB, bool insertDefaultContents = false) {

			if (dpDB.TableExists("materials")) {
				Log.Debug($"----- Table \"materials\" exists.");
			} else {
				Log.Debug($"----- Table \"materials\" doesn't exist, creating.");

				using NpgsqlCommand cmd = new NpgsqlCommand(@"
					CREATE TABLE ""public"".""materials"" (
						""id"" uuid DEFAULT uuid_generate_v1() NOT NULL,
						""json"" json DEFAULT '{}' NOT NULL,
						""search-string"" character varying DEFAULT '',
						""last-modified-ISO8601"" character varying DEFAULT timestamp_iso8601(now(), 'utc') NOT NULL,
						CONSTRAINT ""materials_pk"" PRIMARY KEY(""id"")
					) WITH(oids = false);
					", dpDB);
				cmd.ExecuteNonQuery();
			}

			Console.Write("----- Table \"materials\": ");
			Dictionary<Guid, Materials> updateObjects = new Dictionary<Guid, Materials>();
			{
				Dictionary<Guid, Materials> all = Materials.All(dpDB);

				foreach (KeyValuePair<Guid, Materials> kvp in all) {

					JObject? root = kvp.Value.JsonObject;
					if (null == root)
						continue;
					JToken? lastModifiedInJSONTok = root["lastModifiedISO8601"];

					Materials obj = new Materials(
							Id: kvp.Key,
							Json: root.ToString(Formatting.Indented),
							LastModifiedIso8601: null == lastModifiedInJSONTok ? kvp.Value.LastModifiedIso8601 : lastModifiedInJSONTok.Value<string>(),
							SearchString: kvp.Value.GeneratedSearchString(dpDB)
							);

					Console.Write(".");

					updateObjects.Add(kvp.Key, obj);
				}
			}
			Console.Write(" saving");
			Materials.Upsert(dpDB, updateObjects, out _, out _, true);

			Log.Debug(" done.");
		}







	}
}
