using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;
using Serilog;
using System.Data;
using System.Text;

namespace SharedCode.DatabaseSchemas
{
	public record Recordings(Guid? Id, string? Json, string? SearchString, string? LastModifiedIso8601) : JSONTable(Id, Json, SearchString, LastModifiedIso8601)
	{
		public const string kJsonKeyName = "name";
		public const string kJsonKeyTmpMP3Path = "tmpMP3Path";
		public const string kJsonKeyTmpPCMPath = "tmpPCMPath";
		public const string kJsonKeyTmpWAVPath = "tmpWAVPath";

		public const string kJsonKeyS3Host = "S3Host";
		public const string kJsonKeyS3Bucket = "S3Bucket";

		public const string kJsonKeyS3MP3Key = "S3MP3Key";
		public const string kJsonKeyS3MP3HTTPSURI = "S3MP3HTTPSURI";
		public const string kJsonKeyS3CMDMP3URI = "S3CMDMP3URI";

		public const string kJsonKeyS3WAVKey = "S3WAVKey";
		public const string kJsonKeyS3WAVHTTPSURI = "S3WAVHTTPSURI";
		public const string kJsonKeyS3CMDWAVURI = "S3CMDWAVURI";

		public const string kJsonKeyS3PCMKey = "S3PCMKey";
		public const string kJsonKeyS3PCMHTTPSURI = "S3PCMHTTPSURI";
		public const string kJsonKeyS3CMDPCMURI = "S3CMDPCMURI";

		public static Dictionary<Guid, Recordings> ForId(NpgsqlConnection connection, Guid id) {

			Dictionary<Guid, Recordings> ret = new Dictionary<Guid, Recordings>();

			string sql = @"SELECT * from ""recordings"" WHERE id = @id";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			cmd.Parameters.AddWithValue("@id", id);




			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					Recordings obj = Recordings.FromDataReader(reader);
					if (obj.Id == null) {
						continue;
					}
					ret.Add(obj.Id.Value, obj);
				}
			}

			return ret;
		}


		public static Dictionary<Guid, Recordings> All(NpgsqlConnection connection) {

			Dictionary<Guid, Recordings> ret = new Dictionary<Guid, Recordings>();

			string sql = @"SELECT * from ""recordings""";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					Recordings obj = Recordings.FromDataReader(reader);
					if (obj.Id == null) {
						continue;
					}
					ret.Add(obj.Id.Value, obj);
				}
			}

			return ret;
		}

		public static Dictionary<Guid, Recordings> ForIds(NpgsqlConnection connection, IEnumerable<Guid> ids) {

			Guid[] idsArr = ids.ToArray();

			Dictionary<Guid, Recordings> ret = new Dictionary<Guid, Recordings>();
			if (idsArr.Length == 0)
				return ret;

			List<string> valNames = new List<string>();
			for (int i = 0; i < idsArr.Length; i++) {
				valNames.Add($"@val{i}");
			}

			string sql = $"SELECT * from \"recordings\" WHERE id IN ({string.Join(", ", valNames)})";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			for (int i = 0; i < valNames.Count; i++) {
				cmd.Parameters.AddWithValue(valNames[i], idsArr[i]);
			}

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					Recordings obj = Recordings.FromDataReader(reader);
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



			string sql = $"DELETE FROM \"recordings\" WHERE \"id\" IN ({string.Join(", ", valNames)})";
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
			Dictionary<Guid, Recordings> updateObjects,
			out List<Guid> callerResponse,
			out Dictionary<Guid, Recordings> toSendToOthers,
			bool printDots = false
			) {

			callerResponse = new List<Guid>();
			toSendToOthers = new Dictionary<Guid, Recordings>();

			foreach (KeyValuePair<Guid, Recordings> kvp in updateObjects) {

				string sql = @"
					INSERT INTO
						public.""recordings""
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

		public static Recordings FromDataReader(NpgsqlDataReader reader) {

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

			return new Recordings(
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
				return GenerateSearchString(
					Name
					);
			}
		}

		public static string GenerateSearchString(
			string? nameStr
			) {
			return $"{nameStr}";
		}

		public static void VerifyRepairTable(NpgsqlConnection dpDB, bool insertDefaultContents = false) {

			if (dpDB.TableExists("recordings")) {
				Log.Debug($"----- Table \"recordings\" exists.");
			} else {
				Log.Debug($"----- Table \"recordings\" doesn't exist, creating.");

				using NpgsqlCommand cmd = new NpgsqlCommand(@"
					CREATE TABLE ""public"".""recordings"" (
						""id"" uuid DEFAULT uuid_generate_v1() NOT NULL,
						""json"" json DEFAULT '{}' NOT NULL,
						""search-string"" character varying DEFAULT '',
						""last-modified-ISO8601"" character varying DEFAULT timestamp_iso8601(now(), 'utc') NOT NULL,
						CONSTRAINT ""recordings_pk"" PRIMARY KEY(""id"")
					) WITH(oids = false);
					", dpDB);
				cmd.ExecuteNonQuery();
			}

			Console.Write("----- Table \"recordings\": ");

			//if (insertDefaultContents) {
			//	Console.Write("------ Insert default contents. ");

			//	Dictionary<Guid, Recordings> updateObjects = new Dictionary<Guid, Recordings>();

			//	Console.Write(" saving");
			//	Recordings.Upsert(dpDB, updateObjects, out _, out _, true);
			//}


			{
				Console.Write("------ Repair Existing ");

				Dictionary<Guid, Recordings> updateObjects = new Dictionary<Guid, Recordings>();


				Dictionary<Guid, Recordings> all = Recordings.All(dpDB);

				foreach (KeyValuePair<Guid, Recordings> kvp in all) {

					JObject? root = kvp.Value.JsonObject;
					if (null == root)
						continue;
					JToken? lastModifiedInJSONTok = root["lastModifiedISO8601"];

					Recordings obj = new Recordings(
							Id: kvp.Key,
							Json: root.ToString(Newtonsoft.Json.Formatting.Indented),
							LastModifiedIso8601: null == lastModifiedInJSONTok ? kvp.Value.LastModifiedIso8601 : lastModifiedInJSONTok.Value<string>(),
							SearchString: kvp.Value.GeneratedSearchString
							);



					Console.Write(".");

					updateObjects.Add(kvp.Key, obj);
				}

				Console.Write(" saving");
				Recordings.Upsert(dpDB, updateObjects, out _, out _, true);
			}


			Log.Debug(" done.");
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
		public string? TmpMP3Path
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyTmpMP3Path];
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
		public string? TmpPCMPath
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyTmpPCMPath];
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
		public string? TmpWAVPath
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyTmpWAVPath];
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
		public string? S3Host
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyS3Host];
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
		public string? S3Bucket
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyS3Bucket];
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
		public string? S3MP3Key
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyS3MP3Key];
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
		public string? S3MP3HTTPSURI
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyS3MP3HTTPSURI];
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
		public string? S3CMDMP3URI
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyS3CMDMP3URI];
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
		public string? S3WAVKey
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyS3WAVKey];
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
		public string? S3WAVHTTPSURI
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyS3WAVHTTPSURI];
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
		public string? S3CMDWAVURI
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyS3CMDWAVURI];
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
		public string? S3PCMKey
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyS3PCMKey];
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
		public string? S3PCMHTTPSURI
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyS3PCMHTTPSURI];
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
		public string? S3CMDPCMURI
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyS3CMDPCMURI];
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



		public string? S3LocalPCMPath(string localBucketPath, char pathSeparator = '/', bool stripExtension = false) {

			if (string.IsNullOrWhiteSpace(S3CMDPCMURI)) {
				return null;
			}

			if (string.IsNullOrWhiteSpace(S3Bucket)) {
				return null;
			}

			Uri pdfUri = new Uri(S3CMDPCMURI);

			string path = pdfUri.LocalPath;
			if (string.IsNullOrWhiteSpace(path)) {
				return null;
			}

			List<string> pathComponents = path.Split('/').ToList();

			// [0] should be blank.
			if (!string.IsNullOrWhiteSpace(pathComponents[0])) {
				return null;
			}

			// Remove blank
			pathComponents.RemoveRange(0, 1);

			pathComponents.Insert(0, S3Bucket);
			

			if (pathComponents.Count == 0) {
				return null;
			}

			if (stripExtension) {
				pathComponents[pathComponents.Count - 1] = Path.GetFileNameWithoutExtension(pathComponents[pathComponents.Count - 1]);
			}


			string s3key = string.Join(pathSeparator, pathComponents);
			if (string.IsNullOrWhiteSpace(s3key)) {
				return null;
			}

			return Path.Join(localBucketPath, s3key);
		}














	}
}
