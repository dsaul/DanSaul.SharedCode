using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;
using System.Text;
using System.Data;
using Serilog;
using DanSaul.SharedCode.Npgsql;

namespace SharedCode.DatabaseSchemas
{

	


	public record ScheduledTasks(
		Guid? Id,
		string? Json
		)
	{
		// Scheduled Task JSON
		public const string kJobsJsonKeyDescription = "Description";
		public const string kJobsJsonKeyCrontabExpression = "CrontabExpression";
		public const string kJobsJsonKeyLastJobDispatchedTimestampISO8601 = "LastJobDispatchedTimestampISO8601";
		public const string kJobsJsonKeyNewTaskJson = "NewTaskJson";
		public const string kJobsJsonKeyAllowMoreThanOneActive = "AllowMoreThanOneActive";
		public const string kJobsJsonKeyNewTaskJsonKeyJobType = "JobType";

		public static Dictionary<Guid, ScheduledTasks> ForId(NpgsqlConnection connection, Guid id) {

			Dictionary<Guid, ScheduledTasks> ret = new Dictionary<Guid, ScheduledTasks>();

			string sql = @"SELECT * from ""scheduled_tasks"" WHERE id = @id";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			cmd.Parameters.AddWithValue("@id", id);




			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					ScheduledTasks obj = ScheduledTasks.FromDataReader(reader);
					if (obj.Id == null) {
						continue;
					}
					ret.Add(obj.Id.Value, obj);
				}
			}

			return ret;
		}

		public static Dictionary<Guid, ScheduledTasks> All(NpgsqlConnection connection) {

			Dictionary<Guid, ScheduledTasks> ret = new Dictionary<Guid, ScheduledTasks>();

			string sql = @"SELECT * from ""scheduled_tasks""";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					ScheduledTasks obj = ScheduledTasks.FromDataReader(reader);
					if (obj.Id == null) {
						continue;
					}
					ret.Add(obj.Id.Value, obj);
				}
			}

			return ret;


		}


		public static Dictionary<Guid, ScheduledTasks> ForIds(NpgsqlConnection connection, IEnumerable<Guid> ids) {

			Guid[] idsArr = ids.ToArray();

			Dictionary<Guid, ScheduledTasks> ret = new Dictionary<Guid, ScheduledTasks>();
			if (idsArr.Length == 0)
				return ret;

			List<string> valNames = new List<string>();
			for (int i = 0; i < idsArr.Length; i++) {
				valNames.Add($"@val{i}");
			}

			string sql = $"SELECT * from \"scheduled_tasks\" WHERE id IN ({string.Join(", ", valNames)})";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			for (int i = 0; i < valNames.Count; i++) {
				cmd.Parameters.AddWithValue(valNames[i], idsArr[i]);
			}

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					ScheduledTasks obj = ScheduledTasks.FromDataReader(reader);
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



			string sql = $"DELETE FROM \"scheduled_tasks\" WHERE \"id\" IN ({string.Join(", ", valNames)})";
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
			Dictionary<Guid, ScheduledTasks> updateObjects,
			out List<Guid> callerResponse,
			out Dictionary<Guid, ScheduledTasks> toSendToOthers,
			bool printDots = false
			) {

			callerResponse = new List<Guid>();
			toSendToOthers = new Dictionary<Guid, ScheduledTasks>();

			foreach (KeyValuePair<Guid, ScheduledTasks> kvp in updateObjects) {

				string sql = @"
					INSERT INTO
						public.""scheduled_tasks""
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

		public static ScheduledTasks FromDataReader(NpgsqlDataReader reader) {

			Guid? id = default;
			string? json = default;


			if (!reader.IsDBNull("id")) {
				id = reader.GetGuid("id");
			}
			if (!reader.IsDBNull("json")) {
				json = reader.GetString("json");
			}

			return new ScheduledTasks(
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
		public JObject? NewTaskJsonObject
		{
			get {
				JObject? root = JsonObject;
				if (root == null)
					return default;

				return root[kJobsJsonKeyNewTaskJson] as JObject;
			}
		}



		[JsonIgnore]
		public string? Description
		{
			get {
				JObject? root = JsonObject;
				if (root == null)
					return default;
				return root.Value<string>(kJobsJsonKeyDescription);
			}
		}

		[JsonIgnore]
		public string? CrontabExpression
		{
			get {
				JObject? root = JsonObject;
				if (root == null)
					return default;
				return root.Value<string>(kJobsJsonKeyCrontabExpression);
			}
		}

		[JsonIgnore]
		public string? LastJobDispatchedTimestampISO8601
		{
			get {
				JObject? root = JsonObject;
				if (root == null)
					return default;
				return root.Value<string>(kJobsJsonKeyLastJobDispatchedTimestampISO8601);
			}
		}

		[JsonIgnore]
		public bool? AllowMoreThanOneActive
		{
			get {
				JObject? root = JsonObject;
				if (root == null)
					return default;
				if (false == root.ContainsKey(kJobsJsonKeyAllowMoreThanOneActive)) {
					return null;
				}

				return root.Value<bool>(kJobsJsonKeyAllowMoreThanOneActive);
			}
		}



		public static void VerifyRepairTable(NpgsqlConnection db, bool insertDefaultContents = false) {

			if (db.TableExists("scheduled_tasks")) {
				Log.Debug($"----- Table \"scheduled_tasks\" exists.");
			} else {
				Log.Information($"----- Table \"scheduled_tasks\" doesn't exist, creating.");

				using NpgsqlCommand cmd = new NpgsqlCommand(@"
					CREATE TABLE ""public"".""scheduled_tasks"" (
						""id"" uuid DEFAULT public.uuid_generate_v1() NOT NULL,
						""json"" jsonb DEFAULT '{}'::jsonb NOT NULL,
						CONSTRAINT ""scheduled_tasks_pk"" PRIMARY KEY(""id"")
					) WITH(oids = false);
					", db);
				cmd.ExecuteNonQuery();
			}


			if (insertDefaultContents) {
				Log.Information("Insert Default Contents");

				// Check database for consistency.
				Guid dbVerificationId = Guid.NewGuid();
				ScheduledTasks dbVerificationTask = new ScheduledTasks(
					Id: dbVerificationId,
					Json: new JObject {
						{ kJobsJsonKeyDescription, "Database Verification" },
						{ kJobsJsonKeyNewTaskJson, new JObject {
							{ kJobsJsonKeyNewTaskJsonKeyJobType, "DPDatabaseVerification" },
						} },
						{ kJobsJsonKeyCrontabExpression, "0 2 * * *" },
						{ kJobsJsonKeyAllowMoreThanOneActive, false },
						{ kJobsJsonKeyLastJobDispatchedTimestampISO8601, null },
					}.ToString(Formatting.Indented)
				);

				// Update the webcals every night.
				Guid updateWebCalId = Guid.NewGuid();
				ScheduledTasks updateWebCalTask = new ScheduledTasks(
					Id: updateWebCalId,
					Json: new JObject {
						{ kJobsJsonKeyDescription, "Update Web Cal Files" },
						{ kJobsJsonKeyNewTaskJson, new JObject {
							{ kJobsJsonKeyNewTaskJsonKeyJobType, "UpdateWebCalFiles" },
						} },
						{ kJobsJsonKeyCrontabExpression, "0 3 * * *" },
						{ kJobsJsonKeyAllowMoreThanOneActive, false },
						{ kJobsJsonKeyLastJobDispatchedTimestampISO8601, null },
					}.ToString(Formatting.Indented)
				);

				// Remove expired jobs
				Guid removeExpiredJobsId = Guid.NewGuid();
				ScheduledTasks removeExpiredJobsTask = new ScheduledTasks(
					Id: removeExpiredJobsId,
					Json: new JObject {
						{ kJobsJsonKeyDescription, "Remove Expired Jobs" },
						{ kJobsJsonKeyNewTaskJson, new JObject {
							{ kJobsJsonKeyNewTaskJsonKeyJobType, "JobRunnerRemoveExpiredJobs" },
						} },
						{ kJobsJsonKeyCrontabExpression, "*/5 * * * *" },
						{ kJobsJsonKeyAllowMoreThanOneActive, false },
						{ kJobsJsonKeyLastJobDispatchedTimestampISO8601, null },
					}.ToString(Formatting.Indented)
				);

				// Ensure each company has an S3 bucket.
				Guid ensureS3BucketsId = Guid.NewGuid();
				ScheduledTasks ensureS3BucketsTask = new ScheduledTasks(
					Id: ensureS3BucketsId,
					Json: new JObject {
						{ kJobsJsonKeyDescription, "Ensure Company S3 Buckets" },
						{ kJobsJsonKeyNewTaskJson, new JObject {
							{ kJobsJsonKeyNewTaskJsonKeyJobType, "EnsureCompanyS3Buckets" },
						} },
						{ kJobsJsonKeyCrontabExpression, "* * * * *" },
						{ kJobsJsonKeyAllowMoreThanOneActive, false },
						{ kJobsJsonKeyLastJobDispatchedTimestampISO8601, null },
					}.ToString(Formatting.Indented)
				);


				Upsert(db, new Dictionary<Guid, ScheduledTasks> {
					{dbVerificationId, dbVerificationTask},
					{updateWebCalId, updateWebCalTask},
					{removeExpiredJobsId, removeExpiredJobsTask},
					{ensureS3BucketsId, ensureS3BucketsTask},
				}, out _, out _);

			}





		}










	}
}
