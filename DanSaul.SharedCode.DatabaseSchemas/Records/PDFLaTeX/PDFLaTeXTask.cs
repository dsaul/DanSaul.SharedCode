using Npgsql;
using System.Text;
using System.Data;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace SharedCode.DatabaseSchemas
{
	public record PDFLaTeXTask(
		Guid? Id,
		string? Json
		)
	{
		public const string kPDFLaTeXDBName = "pdflatex";

		public const string kLaTeXBucketName = "pdflatex-compiled-pdfs";
		

		public const string kLaTeXJsonKeyTaskCreated = "TaskCreated";
		public const string kLaTeXJsonKeyRequestingBillingId = "RequestingBillingId";
		public const string kLaTeXJsonKeyReportType = "ReportType";
		public const string kLaTeXJsonKeyStatus = "Status";
		public const string kLaTeXJsonKeyLatexSource = "LaTeXSource";
		public const string kLaTeXJsonKeyErrorMessage = "ErrorMessage";
		public const string kLaTeXS3URITex = "S3URITex";
		public const string kLaTeXS3URIPdf = "S3URIPdf";
		public const string kLaTeXS3URIAux = "S3URIAux";
		public const string kLaTeXS3URILog = "S3URILog";
		public const string kLaTeXS3URIStdout = "S3URIStdout";
		public const string kLaTeXS3URIStderr = "S3URIStderr";

		public const string kLaTeXJsonStatusValueQueued = "Queued";
		public const string kLaTeXJsonStatusValueLatexGenerated = "Rendering";
		public const string kLaTeXJsonStatusValueError = "Error";
		public const string kLaTeXJsonStatusValueCompleted = "Completed";

		public const string kLaTeXJsonReportTypeValueAssignments = "Assignments";
		public const string kLaTeXJsonReportTypeValueCompanies = "Companies";
		public const string kLaTeXJsonReportTypeValueContacts = "Contacts";
		public const string kLaTeXJsonReportTypeValueLabour = "Labour";
		public const string kLaTeXJsonReportTypeValueMaterials = "Materials";
		public const string kLaTeXJsonReportTypeValueProjects = "Projects";
		public const string kLaTeXJsonReportTypeValueOnCallResponder30Days = "OnCallResponder30Days";

		public static Dictionary<Guid, PDFLaTeXTask> ForId(NpgsqlConnection connection, Guid id) {

			Dictionary<Guid, PDFLaTeXTask> ret = new Dictionary<Guid, PDFLaTeXTask>();

			string sql = @"SELECT * from ""tasks"" WHERE id = @id";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			cmd.Parameters.AddWithValue("@id", id);




			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					PDFLaTeXTask obj = PDFLaTeXTask.FromDataReader(reader);
					if (obj.Id == null) {
						continue;
					}
					ret.Add(obj.Id.Value, obj);
				}
			}

			return ret;
		}

		public static Dictionary<Guid, PDFLaTeXTask> All(NpgsqlConnection connection) {

			Dictionary<Guid, PDFLaTeXTask> ret = new Dictionary<Guid, PDFLaTeXTask>();

			string sql = @"SELECT * from ""tasks""";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					PDFLaTeXTask obj = PDFLaTeXTask.FromDataReader(reader);
					if (obj.Id == null) {
						continue;
					}
					ret.Add(obj.Id.Value, obj);
				}
			}

			return ret;


		}


		public static Dictionary<Guid, PDFLaTeXTask> ForIds(NpgsqlConnection connection, IEnumerable<Guid> ids) {

			Guid[] idsArr = ids.ToArray();

			Dictionary<Guid, PDFLaTeXTask> ret = new Dictionary<Guid, PDFLaTeXTask>();
			if (idsArr.Length == 0)
				return ret;

			List<string> valNames = new List<string>();
			for (int i = 0; i < idsArr.Length; i++) {
				valNames.Add($"@val{i}");
			}

			string sql = $"SELECT * from \"tasks\" WHERE id IN ({string.Join(", ", valNames)})";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			for (int i = 0; i < valNames.Count; i++) {
				cmd.Parameters.AddWithValue(valNames[i], idsArr[i]);
			}

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					PDFLaTeXTask obj = PDFLaTeXTask.FromDataReader(reader);
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



			string sql = $"DELETE FROM \"tasks\" WHERE \"id\" IN ({string.Join(", ", valNames)})";
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
			Dictionary<Guid, PDFLaTeXTask> updateObjects,
			out List<Guid> callerResponse,
			out Dictionary<Guid, PDFLaTeXTask> toSendToOthers,
			bool printDots = false
			) {

			callerResponse = new List<Guid>();
			toSendToOthers = new Dictionary<Guid, PDFLaTeXTask>();

			foreach (KeyValuePair<Guid, PDFLaTeXTask> kvp in updateObjects) {

				string sql = @"
					INSERT INTO
						public.""tasks""
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

		public static PDFLaTeXTask FromDataReader(NpgsqlDataReader reader) {

			Guid? id = default;
			string? json = default;


			if (!reader.IsDBNull("id")) {
				id = reader.GetGuid("id");
			}
			if (!reader.IsDBNull("json")) {
				json = reader.GetString("json");
			}

			return new PDFLaTeXTask(
				Id: id,
				Json: json
				);
		}

		[JsonIgnore]
		public JObject? JsonObject
		{
			get {
				if (string.IsNullOrWhiteSpace(Json))
					return null;
				return JsonConvert.DeserializeObject(Json, new JsonSerializerSettings() { DateParseHandling = DateParseHandling.None }) as JObject;
			}
		}

		[JsonIgnore]
		public Guid? RequestingBillingId
		{
			get {
				JObject? root = JsonObject;
				if (root == null)
					return default;
				string? str = root.Value<string>(kLaTeXJsonKeyRequestingBillingId);

				if (!Guid.TryParse(str, out Guid guid)) {
					return null;
				}

				return guid;
			}
		}


		[JsonIgnore]
		public string? ReportType
		{
			get {
				JObject? root = JsonObject;
				if (root == null)
					return default;
				return root.Value<string>(kLaTeXJsonKeyReportType);
			}
		}

		[JsonIgnore]
		public string? Status
		{
			get {
				JObject? root = JsonObject;
				if (root == null)
					return default;
				return root.Value<string>(kLaTeXJsonKeyStatus);
			}
		}


		[JsonIgnore]
		public string? LaTeXSource
		{
			get {
				JObject? root = JsonObject;
				if (root == null)
					return default;
				return root.Value<string>(kLaTeXJsonKeyLatexSource);
			}
		}

		[JsonIgnore]
		public string? ErrorMessage
		{
			get {
				JObject? root = JsonObject;
				if (root == null)
					return default;
				return root.Value<string>(kLaTeXJsonKeyErrorMessage);
			}
		}

		[JsonIgnore]
		public string? S3URITex
		{
			get {
				JObject? root = JsonObject;
				if (root == null)
					return default;
				return root.Value<string>(kLaTeXS3URITex);
			}
		}

		[JsonIgnore]
		public string? S3URIPdf
		{
			get {
				JObject? root = JsonObject;
				if (root == null)
					return default;
				return root.Value<string>(kLaTeXS3URIPdf);
			}
		}

		[JsonIgnore]
		public string? S3URIAux
		{
			get {
				JObject? root = JsonObject;
				if (root == null)
					return default;
				return root.Value<string>(kLaTeXS3URIAux);
			}
		}


		[JsonIgnore]
		public string? S3URILog
		{
			get {
				JObject? root = JsonObject;
				if (root == null)
					return default;
				return root.Value<string>(kLaTeXS3URILog);
			}
		}

		[JsonIgnore]
		public string? S3URIStdout
		{
			get {
				JObject? root = JsonObject;
				if (root == null)
					return default;
				return root.Value<string>(kLaTeXS3URIStdout);
			}
		}

		[JsonIgnore]
		public string? S3URIStderr
		{
			get {
				JObject? root = JsonObject;
				if (root == null)
					return default;
				return root.Value<string>(kLaTeXS3URIStderr);
			}
		}














	}
}
