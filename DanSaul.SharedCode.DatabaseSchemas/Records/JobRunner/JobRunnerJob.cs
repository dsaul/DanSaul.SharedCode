using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;
using System.Text;
using System.Data;
using Serilog;

namespace SharedCode.DatabaseSchemas
{

	


	public record JobRunnerJob(
		Guid? Id,
		string? Json
		)
	{
		public const string kJobsDBName = "job_runner";

		public const string kJobsJsonKeyJobCreatedISO8601 = "JobCreatedISO8601";
		public const string kJobsJsonKeyJobType = "JobType";
		public const string kJobsJsonKeyRequestingBillingId = "RequestingBillingId";
		public const string kJobsJsonKeyDPDatabase = "DPDatabase";
		public const string kJobsJsonKeyTaskId = "TaskID";
		public const string kJobsJsonKeyTaskRunnerClaimedISO8601 = "TaskRunnerClaimedISO8601";
		public const string kJobsJsonKeyExpiresAtISO8601 = "ExpiresAtISO8601";
		public const string kJobsJsonKeyCompleted = "Completed";

		public const string kJobsJsonKeyAssignmentIds = "AssignmentIds";
		public const string kJobsJsonKeyCompanyIds = "CompanyIds";
		public const string kJobsJsonKeyContactIds = "ContactIds";
		public const string kJobsJsonKeyProjectIds = "ProjectIds";
		public const string kJobsJsonKeyProjectId = "ProjectId";
		public const string kJobsJsonKeyAgentId = "AgentId";
		public const string kJobsJsonKeyStartISO8601 = "StartISO8601";
		public const string kJobsJsonKeyEndISO8601 = "EndISO8601";
		public const string kJobsJsonKeyIncludeLabourForOtherProjectsWithMatchingAddresses = "IncludeLabourForOtherProjectsWithMatchingAddresses";

		public const string kJobsJsonKeyIncludeCompanies = "IncludeCompanies";
		public const string kJobsJsonKeyIncludeContacts = "IncludeContacts";
		public const string kJobsJsonKeyIncludeSchedule = "IncludeSchedule";
		public const string kJobsJsonKeyIncludeNotes = "IncludeNotes";
		public const string kJobsJsonKeyIncludeLabour = "IncludeLabour";
		public const string kJobsJsonKeyIncludeMaterials = "IncludeMaterials";

		public const string kJobsJsonKeyRunOnAllAssignments = "RunOnAllAssignments";
		public const string kJobsJsonKeyRunOnAllCompanies = "RunOnAllCompanies";
		public const string kJobsJsonKeyRunOnAllMaterials = "RunOnAllMaterials";
		public const string kJobsJsonKeyRunOnAllLabour = "RunOnAllLabour";
		public const string kJobsJsonKeyRunOnAllContacts = "RunOnAllContacts";

		public const string kJobTypeValueRunReportAssignment = "RunReportAssignment";
		public const string kJobTypeValueRunReportCompanies = "RunReportCompanies";
		public const string kJobTypeValueRunReportContacts = "RunReportContacts";
		public const string kJobTypeValueRunReportLabour = "RunReportLabour";
		public const string kJobTypeValueRunReportMaterials = "RunReportMaterials";
		public const string kJobTypeValueRunReportProjects = "RunReportProjects";
		public const string kJobTypeValueRunReportOnCallResponder30Days = "RunReportOnCallResponder30Days";
		public const string kJobTypeValuePDFLaTeX = "PDFLaTeX";
		public const string kJobTypeValueJobRunnerRemoveExpiredJobs = "JobRunnerRemoveExpiredJobs";
		public const string kJobTypeValueDPDatabaseVerification = "DPDatabaseVerification";
		public const string kJobTypeValueEnsureCompanyS3Buckets = "EnsureCompanyS3Buckets";
		public const string kJobTypeValueUpdateWebCalFiles = "UpdateWebCalFiles";

		public record JSON(
			string JobCreatedISO8601
			);


		public static Dictionary<Guid, JobRunnerJob> FreeJobsForJobType(NpgsqlConnection connection, string jobType) {

			Dictionary<Guid, JobRunnerJob> res = new Dictionary<Guid, JobRunnerJob>();

			string sql = $"SELECT * FROM \"jobs\" WHERE (json->> '{kJobsJsonKeyTaskRunnerClaimedISO8601}')::text IS NULL AND (json->> '{kJobsJsonKeyCompleted}')::boolean = false AND (json ->> '{kJobsJsonKeyJobType}') = @jobType";
			NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			cmd.Parameters.AddWithValue("@jobType", jobType);

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					JobRunnerJob obj = JobRunnerJob.FromDataReader(reader);
					if (obj.Id == null) {
						continue;
					}
					res.Add(obj.Id.Value, obj);
				}
			}

			return res;
		}


		public static JobRunnerJob? ClaimJobIfAvailable(NpgsqlConnection connection, string jobType) {

			Dictionary<Guid, JobRunnerJob> res = FreeJobsForJobType(connection, jobType);
			if (res.Count == 0)
				return null;

			

			JobRunnerJob job = res.FirstOrDefault().Value;
			JObject? root = job.JsonObject;
			if (root != null && job.Id != null) {
				
				root[kJobsJsonKeyTaskRunnerClaimedISO8601] = DateTime.UtcNow.ToString("o", Culture.DevelopmentCulture);

				JobRunnerJob mod = job with
				{
					Json = root.ToString()
				};

				using NpgsqlConnection jobsDB2 = new NpgsqlConnection(EnvDatabases.DatabaseConnectionStringForDB(JobRunnerJob.kJobsDBName));
				jobsDB2.Open();

				JobRunnerJob.Upsert(jobsDB2, new Dictionary<Guid, JobRunnerJob> {
					{
						mod.Id.Value, mod
					}
				}, out _, out _);


				job = mod;
			}
			

			return job;
		}





		public static Dictionary<Guid, JobRunnerJob> ForId(NpgsqlConnection connection, Guid id) {

			Dictionary<Guid, JobRunnerJob> ret = new Dictionary<Guid, JobRunnerJob>();

			string sql = @"SELECT * from ""jobs"" WHERE id = @id";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			cmd.Parameters.AddWithValue("@id", id);




			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					JobRunnerJob obj = JobRunnerJob.FromDataReader(reader);
					if (obj.Id == null) {
						continue;
					}
					ret.Add(obj.Id.Value, obj);
				}
			}

			return ret;
		}

		public static Dictionary<Guid, JobRunnerJob> All(NpgsqlConnection connection) {

			Dictionary<Guid, JobRunnerJob> ret = new Dictionary<Guid, JobRunnerJob>();

			string sql = @"SELECT * from ""jobs""";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					JobRunnerJob obj = JobRunnerJob.FromDataReader(reader);
					if (obj.Id == null) {
						continue;
					}
					ret.Add(obj.Id.Value, obj);
				}
			}

			return ret;


		}


		public static Dictionary<Guid, JobRunnerJob> ForIds(NpgsqlConnection connection, IEnumerable<Guid> ids) {

			Guid[] idsArr = ids.ToArray();

			Dictionary<Guid, JobRunnerJob> ret = new Dictionary<Guid, JobRunnerJob>();
			if (idsArr.Length == 0)
				return ret;

			List<string> valNames = new List<string>();
			for (int i = 0; i < idsArr.Length; i++) {
				valNames.Add($"@val{i}");
			}

			string sql = $"SELECT * from \"jobs\" WHERE id IN ({string.Join(", ", valNames)})";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			for (int i = 0; i < valNames.Count; i++) {
				cmd.Parameters.AddWithValue(valNames[i], idsArr[i]);
			}

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					JobRunnerJob obj = JobRunnerJob.FromDataReader(reader);
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



			string sql = $"DELETE FROM \"jobs\" WHERE \"id\" IN ({string.Join(", ", valNames)})";
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

		public static int DeleteExpiredJobs(NpgsqlConnection connection) {
			string sql = @"DELETE FROM ""jobs"" WHERE now() > (json->>'ExpiresAtISO8601')::timestamp";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);

			return cmd.ExecuteNonQuery();
		}


		public static void Upsert(
			NpgsqlConnection connection,
			Dictionary<Guid, JobRunnerJob> updateObjects,
			out List<Guid> callerResponse,
			out Dictionary<Guid, JobRunnerJob> toSendToOthers,
			bool printDots = false
			) {

			callerResponse = new List<Guid>();
			toSendToOthers = new Dictionary<Guid, JobRunnerJob>();

			foreach (KeyValuePair<Guid, JobRunnerJob> kvp in updateObjects) {

				string sql = @"
					INSERT INTO
						public.""jobs""
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

		public static JobRunnerJob FromDataReader(NpgsqlDataReader reader) {

			Guid? id = default;
			string? json = default;


			if (!reader.IsDBNull("id")) {
				id = reader.GetGuid("id");
			}
			if (!reader.IsDBNull("json")) {
				json = reader.GetString("json");
			}

			return new JobRunnerJob(
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
		public string? JobType
		{
			get {
				JObject? root = JsonObject;
				if (root == null)
					return default;
				return root.Value<string>(kJobsJsonKeyJobType);
			}
		}

		[JsonIgnore]
		public string? DPDatabase
		{
			get {
				JObject? root = JsonObject;
				if (root == null)
					return default;
				return root.Value<string>(kJobsJsonKeyDPDatabase);
			}
		}

		[JsonIgnore]
		public string? StartISO8601
		{
			get {
				JObject? root = JsonObject;
				if (root == null)
					return default;
				return root.Value<string>(kJobsJsonKeyStartISO8601);
			}
		}

		[JsonIgnore]
		public string? EndISO8601
		{
			get {
				JObject? root = JsonObject;
				if (root == null)
					return default;
				return root.Value<string>(kJobsJsonKeyEndISO8601);
			}
		}

		[JsonIgnore]
		public Guid? TaskId
		{
			get {
				JObject? root = JsonObject;
				if (root == null)
					return default;
				string str = root.Value<string>(kJobsJsonKeyTaskId);

				if (!Guid.TryParse(str, out Guid guid)) {
					return null;
				}

				return guid;
			}
		}

		[JsonIgnore]
		public Guid? ProjectId
		{
			get {
				JObject? root = JsonObject;
				if (root == null)
					return default;
				string str = root.Value<string>(kJobsJsonKeyProjectId);

				if (!Guid.TryParse(str, out Guid guid)) {
					return null;
				}

				return guid;
			}
		}

		[JsonIgnore]
		public Guid? AgentId
		{
			get {
				JObject? root = JsonObject;
				if (root == null)
					return default;
				string str = root.Value<string>(kJobsJsonKeyAgentId);

				if (!Guid.TryParse(str, out Guid guid)) {
					return null;
				}

				return guid;
			}
		}


		[JsonIgnore]
		public Guid? RequestingBillingId
		{
			get {
				JObject? root = JsonObject;
				if (root == null)
					return default;
				string str = root.Value<string>(kJobsJsonKeyRequestingBillingId);

				if (!Guid.TryParse(str, out Guid guid)) {
					return null;
				}

				return guid;
			}
		}

		[JsonIgnore]
		public List<Guid> AssignmentIds
		{
			get {
				List<Guid> ret = new List<Guid>();

				JObject? root = JsonObject;
				if (root == null)
					return ret;


				JArray? array = root[kJobsJsonKeyAssignmentIds] as JArray;

				if (null == array || array.Count == 0) {
					return ret;
				}

				

				foreach (JToken tok in array) {

					if (!Guid.TryParse(tok.Value<string>(), out Guid guid)) {
						continue;
					}

					ret.Add(guid);
				}

				return ret;
			}
		}

		[JsonIgnore]
		public List<Guid> CompanyIds
		{
			get {
				List<Guid> ret = new List<Guid>();

				JObject? root = JsonObject;
				if (root == null)
					return ret;


				JArray? array = root[kJobsJsonKeyCompanyIds] as JArray;

				if (null == array || array.Count == 0) {
					return ret;
				}



				foreach (JToken tok in array) {

					if (!Guid.TryParse(tok.Value<string>(), out Guid guid)) {
						continue;
					}

					ret.Add(guid);
				}

				return ret;
			}
		}

		[JsonIgnore]
		public List<Guid> ContactIds
		{
			get {
				List<Guid> ret = new List<Guid>();

				JObject? root = JsonObject;
				if (root == null)
					return ret;


				JArray? array = root[kJobsJsonKeyContactIds] as JArray;

				if (null == array || array.Count == 0) {
					return ret;
				}



				foreach (JToken tok in array) {

					if (!Guid.TryParse(tok.Value<string>(), out Guid guid)) {
						continue;
					}

					ret.Add(guid);
				}

				return ret;
			}
		}

		[JsonIgnore]
		public List<Guid> ProjectIds
		{
			get {
				List<Guid> ret = new List<Guid>();

				JObject? root = JsonObject;
				if (root == null)
					return ret;


				JArray? array = root[kJobsJsonKeyProjectIds] as JArray;

				if (null == array || array.Count == 0) {
					return ret;
				}



				foreach (JToken tok in array) {

					if (!Guid.TryParse(tok.Value<string>(), out Guid guid)) {
						continue;
					}

					ret.Add(guid);
				}

				return ret;
			}
		}

		

		[JsonIgnore]
		public bool? RunOnAllAssignments
		{
			get {
				JObject? root = JsonObject;
				if (root == null)
					return default;
				if (false == root.ContainsKey(kJobsJsonKeyRunOnAllAssignments)) {
					return null;
				}

				return root.Value<bool>(kJobsJsonKeyRunOnAllAssignments);
			}
		}

		[JsonIgnore]
		public bool? RunOnAllCompanies
		{
			get {
				JObject? root = JsonObject;
				if (root == null)
					return default;
				if (false == root.ContainsKey(kJobsJsonKeyRunOnAllCompanies)) {
					return null;
				}

				return root.Value<bool>(kJobsJsonKeyRunOnAllCompanies);
			}
		}

		[JsonIgnore]
		public bool? RunOnAllMaterials
		{
			get {
				JObject? root = JsonObject;
				if (root == null)
					return default;
				if (false == root.ContainsKey(kJobsJsonKeyRunOnAllMaterials)) {
					return null;
				}

				return root.Value<bool>(kJobsJsonKeyRunOnAllMaterials);
			}
		}

		[JsonIgnore]
		public bool? RunOnAllLabour
		{
			get {
				JObject? root = JsonObject;
				if (root == null)
					return default;
				if (false == root.ContainsKey(kJobsJsonKeyRunOnAllLabour)) {
					return null;
				}

				return root.Value<bool>(kJobsJsonKeyRunOnAllLabour);
			}
		}

		[JsonIgnore]
		public bool? RunOnAllContacts
		{
			get {
				JObject? root = JsonObject;
				if (root == null)
					return default;
				if (false == root.ContainsKey(kJobsJsonKeyRunOnAllContacts)) {
					return null;
				}

				return root.Value<bool>(kJobsJsonKeyRunOnAllContacts);
			}
		}


		[JsonIgnore]
		public string? TaskRunnerClaimedISO8601
		{
			get {
				JObject? root = JsonObject;
				if (root == null)
					return default;
				if (false == root.ContainsKey(kJobsJsonKeyTaskRunnerClaimedISO8601)) {
					return null;
				}

				return root.Value<string?>(kJobsJsonKeyTaskRunnerClaimedISO8601);
			}
		}

		[JsonIgnore]
		public bool? Completed
		{
			get {
				JObject? root = JsonObject;
				if (root == null)
					return default;
				if (false == root.ContainsKey(kJobsJsonKeyCompleted)) {
					return false;
				}

				return root.Value<bool>(kJobsJsonKeyCompleted);
			}
		}

		[JsonIgnore]
		public bool? IncludeLabourForOtherProjectsWithMatchingAddresses
		{
			get {
				JObject? root = JsonObject;
				if (root == null)
					return default;
				if (false == root.ContainsKey(kJobsJsonKeyIncludeLabourForOtherProjectsWithMatchingAddresses)) {
					return false;
				}

				return root.Value<bool>(kJobsJsonKeyIncludeLabourForOtherProjectsWithMatchingAddresses);
			}
		}



		[JsonIgnore]
		public bool? IncludeCompanies
		{
			get {
				JObject? root = JsonObject;
				if (root == null)
					return default;
				if (false == root.ContainsKey(kJobsJsonKeyIncludeCompanies)) {
					return null;
				}

				return root.Value<bool>(kJobsJsonKeyIncludeCompanies);
			}
		}


		[JsonIgnore]
		public bool? IncludeContacts
		{
			get {
				JObject? root = JsonObject;
				if (root == null)
					return default;
				if (false == root.ContainsKey(kJobsJsonKeyIncludeContacts)) {
					return null;
				}

				return root.Value<bool>(kJobsJsonKeyIncludeContacts);
			}
		}

		[JsonIgnore]
		public bool? IncludeSchedule
		{
			get {
				JObject? root = JsonObject;
				if (root == null)
					return default;
				if (false == root.ContainsKey(kJobsJsonKeyIncludeSchedule)) {
					return null;
				}

				return root.Value<bool>(kJobsJsonKeyIncludeSchedule);
			}
		}

		[JsonIgnore]
		public bool? IncludeNotes
		{
			get {
				JObject? root = JsonObject;
				if (root == null)
					return default;
				if (false == root.ContainsKey(kJobsJsonKeyIncludeNotes)) {
					return null;
				}

				return root.Value<bool>(kJobsJsonKeyIncludeNotes);
			}
		}

		[JsonIgnore]
		public bool? IncludeLabour
		{
			get {
				JObject? root = JsonObject;
				if (root == null)
					return default;
				if (false == root.ContainsKey(kJobsJsonKeyIncludeLabour)) {
					return null;
				}

				return root.Value<bool>(kJobsJsonKeyIncludeLabour);
			}
		}


		[JsonIgnore]
		public bool? IncludeMaterials
		{
			get {
				JObject? root = JsonObject;
				if (root == null)
					return default;
				if (false == root.ContainsKey(kJobsJsonKeyIncludeMaterials)) {
					return null;
				}

				return root.Value<bool>(kJobsJsonKeyIncludeMaterials);
			}
		}















		public static void VerifyRepairTable(NpgsqlConnection db, bool insertDefaultContents = false) {

			if (db.TableExists("jobs")) {
				Log.Debug($"----- Table \"jobs\" exists.");
			} else {
				Log.Information($"----- Table \"jobs\" doesn't exist, creating.");

				using NpgsqlCommand cmd = new NpgsqlCommand(@"
					CREATE TABLE ""public"".""jobs"" (
						""id"" uuid DEFAULT public.uuid_generate_v1() NOT NULL,
						""json"" jsonb DEFAULT '{}'::jsonb NOT NULL,
						CONSTRAINT ""jobs_pk"" PRIMARY KEY(""id"")
					) WITH(oids = false);
					", db);
				cmd.ExecuteNonQuery();
			}


			if (insertDefaultContents) {
				// No default jobs.

			}





		}















	}
}
