using Amazon.Polly;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;
using System.Data;
using System.Text;

namespace SharedCode.DatabaseSchemas
{
	public record Cache(
		Guid? Id,
		string? Json
		)
	{
		public const string kTTSCacheDatabaseName = "tts";
		public const string kLaTeXBucketName = "tts-cache";

		public const string kJsonKeyText = "text";
		public const string kJsonKeyVoice = "voice";
		public const string kJsonKeyEngine = "engine";
		public const string kJsonKeyS3URIMP3 = "S3URIMP3";
		public const string kJsonKeyS3URIWAV = "S3URIWAV";
		public const string kJsonKeyS3URIPCM = "S3URIPCM";

		public static Dictionary<Guid, Cache> ForId(NpgsqlConnection connection, Guid id) {

			Dictionary<Guid, Cache> ret = new Dictionary<Guid, Cache>();

			string sql = @"SELECT * from ""cache"" WHERE id = @id";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			cmd.Parameters.AddWithValue("@id", id);

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					Cache obj = Cache.FromDataReader(reader);
					if (obj.Id == null) {
						continue;
					}
					ret.Add(obj.Id.Value, obj);
				}
			}

			return ret;
		}

		public static Dictionary<Guid, Cache> ForTextEngineVoice(NpgsqlConnection connection, string text, Engine engine, VoiceId voice) {

			Dictionary<Guid, Cache> ret = new Dictionary<Guid, Cache>();

			string sql = @"SELECT * FROM ""cache"" WHERE TRIM(LOWER(json->>'text')) = TRIM(LOWER(@text)) AND json->>'engine' = @engine AND json->>'voice' = @voice";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			cmd.Parameters.AddWithValue("@text", text);
			cmd.Parameters.AddWithValue("@engine", engine.ToString());
			cmd.Parameters.AddWithValue("@voice", voice.ToString());

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					Cache obj = Cache.FromDataReader(reader);
					if (obj.Id == null) {
						continue;
					}
					ret.Add(obj.Id.Value, obj);
				}
			}

			return ret;
		}

		public static Dictionary<Guid, Cache> All(NpgsqlConnection connection) {

			Dictionary<Guid, Cache> ret = new Dictionary<Guid, Cache>();

			string sql = @"SELECT * from ""cache""";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					Cache obj = Cache.FromDataReader(reader);
					if (obj.Id == null) {
						continue;
					}
					ret.Add(obj.Id.Value, obj);
				}
			}

			return ret;


		}


		public static Dictionary<Guid, Cache> ForIds(NpgsqlConnection connection, IEnumerable<Guid> ids) {

			Guid[] idsArr = ids.ToArray();

			Dictionary<Guid, Cache> ret = new Dictionary<Guid, Cache>();
			if (idsArr.Length == 0)
				return ret;

			List<string> valNames = new List<string>();
			for (int i = 0; i < idsArr.Length; i++) {
				valNames.Add($"@val{i}");
			}

			string sql = $"SELECT * from \"cache\" WHERE id IN ({string.Join(", ", valNames)})";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			for (int i = 0; i < valNames.Count; i++) {
				cmd.Parameters.AddWithValue(valNames[i], idsArr[i]);
			}

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					Cache obj = Cache.FromDataReader(reader);
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



			string sql = $"DELETE FROM \"cache\" WHERE \"id\" IN ({string.Join(", ", valNames)})";
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
			Dictionary<Guid, Cache> updateObjects,
			out List<Guid> callerResponse,
			out Dictionary<Guid, Cache> toSendToOthers,
			bool printDots = false
			) {

			callerResponse = new List<Guid>();
			toSendToOthers = new Dictionary<Guid, Cache>();

			foreach (KeyValuePair<Guid, Cache> kvp in updateObjects) {

				string sql = @"
					INSERT INTO
						public.""cache""
						(
							""id"",
							""json""
						)
					VALUES
						(
							@id,
							CAST(@json AS jsonb)
						)
					ON CONFLICT (""id"") DO UPDATE
						SET
							""json"" = CAST(excluded.json AS jsonb)
					";

				using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
				cmd.Parameters.AddWithValue("@id", kvp.Key);
				cmd.Parameters.AddWithValue("@json", string.IsNullOrWhiteSpace(kvp.Value.Json) ? (object)DBNull.Value : kvp.Value.Json);

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

		public static Cache FromDataReader(NpgsqlDataReader reader) {

			Guid? id = default;
			string? json = default;


			if (!reader.IsDBNull("id")) {
				id = reader.GetGuid("id");
			}
			if (!reader.IsDBNull("json")) {
				json = reader.GetString("json");
			}

			return new Cache(
				Id: id,
				Json: json
				);
		}

		[JsonIgnore]
		public JObject? JsonObject
		{
			get {
				if (Json == null)
					return null;
				return JsonConvert.DeserializeObject(Json, new JsonSerializerSettings() { DateParseHandling = DateParseHandling.None }) as JObject;
			}
		}

		[JsonIgnore]
		public string? Text
		{
			get {
				JObject? root = JsonObject;
				if (root == null)
					return default;
				return root.Value<string>(kJsonKeyText);
			}
		}

		[JsonIgnore]
		public VoiceId? Voice
		{
			get {
				JObject? root = JsonObject;
				if (root == null)
					return default;

				string? str = root.Value<string>(kJsonKeyVoice);
				if (str == null)
					return default;

				return VoiceId.FindValue(str);
			}
		}

		[JsonIgnore]
		public Engine? Engine
		{
			get {
				JObject? root = JsonObject;
				if (root == null)
					return default;

				string? str = root.Value<string>(kJsonKeyEngine);
				if (str == null)
					return default;


				return Engine.FindValue(str);
			}
		}

		[JsonIgnore]
		public string? S3URIMP3
		{
			get {
				JObject? root = JsonObject;
				if (root == null)
					return default;
				return root.Value<string>(kJsonKeyS3URIMP3);
			}
		}

		[JsonIgnore]
		public string? S3URIWAV
		{
			get {
				JObject? root = JsonObject;
				if (root == null)
					return default;
				return root.Value<string>(kJsonKeyS3URIWAV);
			}
		}

		[JsonIgnore]
		public string? S3URIPCM
		{
			get {
				JObject? root = JsonObject;
				if (root == null)
					return default;
				return root.Value<string>(kJsonKeyS3URIPCM);
			}
		}

		public string? S3CMDPCMPath(char pathSeparator = '/') {

			if (string.IsNullOrWhiteSpace(S3URIPCM)) {
				return null;
			}

			Uri pdfUri = new Uri(S3URIPCM);
			

			string path = pdfUri.LocalPath;
			if (string.IsNullOrWhiteSpace(path)) {
				return null;
			}

			List<string> pathComponents = path.Split('/').ToList();

			// [0] should be blank.
			if (!string.IsNullOrWhiteSpace(pathComponents[0])) {
				return null;
			}

			// [1] should be the bucket name
			if (string.IsNullOrWhiteSpace(pathComponents[1])) {
				return null;
			}

			string bucketName = pathComponents[1];

			// Remove Empty Part
			pathComponents.RemoveAt(0);

			return $"s3://{string.Join(pathSeparator, pathComponents)}";
		}


		public string? S3LocalPCMPath(string localBucketPath, char pathSeparator = '/', bool stripExtension = false) {

			if (string.IsNullOrWhiteSpace(S3URIPCM)) {
				return null;
			}

			Uri pdfUri = new Uri(S3URIPCM);

			string path = pdfUri.LocalPath;
			if (string.IsNullOrWhiteSpace(path)) {
				return null;
			}

			List<string> pathComponents = path.Split('/').ToList();

			// [0] should be blank.
			if (!string.IsNullOrWhiteSpace(pathComponents[0])) {
				return null;
			}

			// [1] should be the bucket name
			if (string.IsNullOrWhiteSpace(pathComponents[1])) {
				return null;
			}

			string bucketName = pathComponents[1];

			// Remove non file components.
			pathComponents.RemoveRange(0, 2);

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
