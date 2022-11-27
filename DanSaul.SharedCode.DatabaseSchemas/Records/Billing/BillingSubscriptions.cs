using Npgsql;
using System.Text;
using System.Data;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Serilog;

namespace SharedCode.DatabaseSchemas
{
	public record BillingSubscriptions(
		Guid? Uuid,
		Guid? CompanyId,
		Guid? PackageId,
		DateTime? TimestampAddedUtc,
		string? ProvisioningActual,
		string? ProvisioningDesired,
		string? ProvisionedDatabaseName,
		DateTime? TimestampLastSettingsPushUtc,
		string? Json
		)
	{
		public static Dictionary<Guid, BillingSubscriptions> ForId(NpgsqlConnection connection, Guid id) {

			Dictionary<Guid, BillingSubscriptions> ret = new Dictionary<Guid, BillingSubscriptions>();

			string sql = @"SELECT * from ""billing-subscriptions"" WHERE uuid = @uuid";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			cmd.Parameters.AddWithValue("@uuid", id);




			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					BillingSubscriptions obj = BillingSubscriptions.FromDataReader(reader);
					if (obj.Uuid == null) {
						continue;
					}
					ret.Add(obj.Uuid.Value, obj);
				}
			}

			return ret;
		}

		public static Dictionary<Guid, BillingSubscriptions> ForIds(NpgsqlConnection connection, IEnumerable<Guid> ids) {

			Guid[] idsArr = ids.ToArray();

			Dictionary<Guid, BillingSubscriptions> ret = new Dictionary<Guid, BillingSubscriptions>();
			if (idsArr.Length == 0) {
				return ret;
			}

			List<string> valNames = new List<string>();
			for (int i = 0; i < idsArr.Length; i++) {
				valNames.Add($"@val{i}");
			}

			string sql = $"SELECT * FROM \"billing-subscriptions\" WHERE \"uuid\" IN ({string.Join(", ", valNames)})";

			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			for (int i = 0; i < valNames.Count; i++) {
				cmd.Parameters.AddWithValue(valNames[i], idsArr[i]);
			}

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					BillingSubscriptions obj = BillingSubscriptions.FromDataReader(reader);
					if (obj.Uuid == null) {
						continue;
					}
					ret.Add(obj.Uuid.Value, obj);
				}
			}


			return ret;
		}

		public static Dictionary<Guid, BillingSubscriptions> ForPackageIdsAndHasDatabase(NpgsqlConnection connection, IEnumerable<Guid> packageIds) {

			Dictionary<Guid, BillingSubscriptions> ret = new Dictionary<Guid, BillingSubscriptions>();

			Guid[] packageIdsArr = packageIds.ToArray();

			List<string> valNames = new List<string>();
			for (int i = 0; i < packageIdsArr.Length; i++) {
				valNames.Add($"@val{i}");
			}


			string sql = @"
				SELECT 
					* 
				FROM 
					""billing-subscriptions""
				WHERE 
					""package-id"" IN ("+string.Join(", ", valNames)+@") 
					AND ""provisioned-database-name"" IS NOT NULL 
					AND ""provisioned-database-name"" <> ''
				";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);

			for (int i = 0; i < packageIdsArr.Length; i++) {
				cmd.Parameters.AddWithValue(valNames[i], packageIdsArr[i]);
			}


			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					BillingSubscriptions obj = BillingSubscriptions.FromDataReader(reader);
					if (obj.Uuid == null) {
						continue;
					}
					ret.Add(obj.Uuid.Value, obj);
				}
			}

			return ret;
		}



		public static Dictionary<Guid, BillingSubscriptions> ForCompanyIdPackageIdsAndHasDatabase(NpgsqlConnection connection, Guid companyId, IEnumerable<Guid> packageIds) {

			Dictionary<Guid, BillingSubscriptions> ret = new Dictionary<Guid, BillingSubscriptions>();

			Guid[] packageIdsArr = packageIds.ToArray();

			List<string> valNames = new List<string>();
			for (int i = 0; i < packageIdsArr.Length; i++) {
				valNames.Add($"@val{i}");
			}


			//string sql = @"SELECT * from ""billing-subscriptions"" WHERE ""company-id"" = @companyId AND ""package-id"" IN ('')";
			string sql = @"
				SELECT 
					* 
				FROM 
					""billing-subscriptions""
				WHERE 
					""company-id"" = @companyId 
					AND ""package-id"" IN ("+string.Join(", ", valNames)+@") 
					AND ""provisioned-database-name"" IS NOT NULL 
					AND ""provisioned-database-name"" <> ''
				";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			cmd.Parameters.AddWithValue("@companyId", companyId);

			for (int i = 0; i < packageIdsArr.Length; i++) {
				cmd.Parameters.AddWithValue(valNames[i], packageIdsArr[i]);
			}


			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					BillingSubscriptions obj = BillingSubscriptions.FromDataReader(reader);
					if (obj.Uuid == null) {
						continue;
					}
					ret.Add(obj.Uuid.Value, obj);
				}
			}

			return ret;
		}




		public static Dictionary<Guid, BillingSubscriptions> ForCompanyId(NpgsqlConnection connection, Guid companyId) {

			Dictionary<Guid, BillingSubscriptions> ret = new Dictionary<Guid, BillingSubscriptions>();

			string sql = @"SELECT * from ""billing-subscriptions"" WHERE ""company-id"" = @companyId";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			cmd.Parameters.AddWithValue("@companyId", companyId);




			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					BillingSubscriptions obj = BillingSubscriptions.FromDataReader(reader);
					if (obj.Uuid == null) {
						continue;
					}
					ret.Add(obj.Uuid.Value, obj);
				}
			}

			return ret;
		}


		public static Dictionary<Guid, BillingSubscriptions> All(NpgsqlConnection connection) {

			Dictionary<Guid, BillingSubscriptions> ret = new Dictionary<Guid, BillingSubscriptions>();

			string sql = @"SELECT * from ""billing-subscriptions""";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					BillingSubscriptions obj = BillingSubscriptions.FromDataReader(reader);
					if (obj.Uuid == null) {
						continue;
					}
					ret.Add(obj.Uuid.Value, obj);
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



			string sql = $"DELETE FROM \"billing-subscriptions\" WHERE \"uuid\" IN ({string.Join(", ", valNames)})";
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

		public static List<Guid> DeleteForCompanyIds(NpgsqlConnection connection, List<Guid> idsToDelete) {

			List<Guid> toSendToOthers = new List<Guid>();
			if (idsToDelete.Count == 0) {
				return toSendToOthers;
			}

			List<string> valNames = new List<string>();
			for (int i = 0; i < idsToDelete.Count; i++) {
				valNames.Add($"@val{i}");
			}



			string sql = $"DELETE FROM \"billing-subscriptions\" WHERE \"company-id\" IN ({string.Join(", ", valNames)})";
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


		public static void Upsert(NpgsqlConnection connection, Dictionary<Guid, BillingSubscriptions> updateObjects, out List<Guid> callerResponse, out Dictionary<Guid, BillingSubscriptions> toSendToOthers) {

			callerResponse = new List<Guid>();
			toSendToOthers = new Dictionary<Guid, BillingSubscriptions>();

			foreach (KeyValuePair<Guid, BillingSubscriptions> kvp in updateObjects) {

				string sql = @"
					INSERT INTO
						""billing-subscriptions""
						(
							""uuid"",
							""company-id"",
							""package-id"",
							""timestamp-added-utc"",
							""provisioning-actual"",
							""provisioning-desired"",
							""provisioned-database-name"",
							""timestamp-last-settings-push-utc"",
							""json""
						)
					VALUES
						(
							@uuid,
							@companyId,
							@packageId,
							@timestampAddedUtc,
							@provisioningActual,
							@provisioningDesired,
							@provisionedDatabaseName,
							@timestampLastSettingsPushUtc,
							CAST(@json AS json)
						)
					ON CONFLICT (""uuid"") DO UPDATE
						SET
							""company-id"" = excluded.""company-id"",
							""package-id"" = excluded.""package-id"",
							""timestamp-added-utc"" = excluded.""timestamp-added-utc"",
							""provisioning-actual"" = excluded.""provisioning-actual"",
							""provisioning-desired"" = excluded.""provisioning-desired"",
							""provisioned-database-name"" = excluded.""provisioned-database-name"",
							""timestamp-last-settings-push-utc"" = excluded.""timestamp-last-settings-push-utc"",
							""json"" = CAST(excluded.""json"" AS json)
					";

				using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
				cmd.Parameters.AddWithValue("@uuid", kvp.Key);
				cmd.Parameters.AddWithValue("@companyId", null == kvp.Value.CompanyId ? (object)DBNull.Value : kvp.Value.CompanyId);
				cmd.Parameters.AddWithValue("@packageId", null == kvp.Value.PackageId ? (object)DBNull.Value : kvp.Value.PackageId);
				cmd.Parameters.AddWithValue("@timestampAddedUtc", null == kvp.Value.TimestampAddedUtc? (object)DBNull.Value : kvp.Value.TimestampAddedUtc);
				cmd.Parameters.AddWithValue("@provisioningActual", string.IsNullOrWhiteSpace(kvp.Value.ProvisioningActual) ? (object)DBNull.Value : kvp.Value.ProvisioningActual);
				cmd.Parameters.AddWithValue("@provisioningDesired", string.IsNullOrWhiteSpace(kvp.Value.ProvisioningDesired) ? (object)DBNull.Value : kvp.Value.ProvisioningDesired);
				cmd.Parameters.AddWithValue("@provisionedDatabaseName", string.IsNullOrWhiteSpace(kvp.Value.ProvisionedDatabaseName) ? (object)DBNull.Value : kvp.Value.ProvisionedDatabaseName);
				cmd.Parameters.AddWithValue("@timestampLastSettingsPushUtc", null == kvp.Value.TimestampLastSettingsPushUtc ? (object)DBNull.Value : kvp.Value.TimestampLastSettingsPushUtc);
				cmd.Parameters.AddWithValue("@json", string.IsNullOrWhiteSpace(kvp.Value.Json) ? (object)DBNull.Value : kvp.Value.Json);

				int rowsAffected = cmd.ExecuteNonQuery();

				if (rowsAffected == 0) {
					continue;
				}

				toSendToOthers.Add(kvp.Key, kvp.Value);
				callerResponse.Add(kvp.Key);


			}



		}


		public static BillingSubscriptions FromDataReader(NpgsqlDataReader reader) {

			Guid? uuid = default;
			Guid? companyId = default;
			Guid? packageId = default;
			DateTime? timestampAddedUtc = default;
			string? provisioningActual = default;
			string? provisioningDesired = default;
			string? provisionedDatabaseName = default;
			DateTime? timestampLastSettingsPushUtc = default;
			string? json = default;


			Dictionary<string, int> columns = new Dictionary<string, int>();
			for (int i = 0; i < reader.FieldCount; i++) {
				columns.Add(reader.GetName(i), i);
			}


			if (!reader.IsDBNull("uuid")) {
				uuid = reader.GetGuid("uuid");
			}
			if (!reader.IsDBNull("company-id")) {
				companyId = reader.GetGuid("company-id");
			}
			if (!reader.IsDBNull("package-id")) {
				packageId = reader.GetGuid("package-id");
			}
			if (!reader.IsDBNull("timestamp-added-utc")) {
				timestampAddedUtc = reader.GetDateTime(columns["timestamp-added-utc"]);
			}
			if (!reader.IsDBNull("provisioning-actual")) {
				provisioningActual = reader.GetString("provisioning-actual");
			}
			if (!reader.IsDBNull("provisioning-desired")) {
				provisioningDesired = reader.GetString("provisioning-desired");
			}
			if (!reader.IsDBNull("provisioned-database-name")) {
				provisionedDatabaseName = reader.GetString("provisioned-database-name");
			}
			if (!reader.IsDBNull("timestamp-last-settings-push-utc")) {
				timestampLastSettingsPushUtc = reader.GetDateTime(columns["timestamp-last-settings-push-utc"]);
			}
			if (!reader.IsDBNull("json")) {
				json = reader.GetString("json");
			}



			return new BillingSubscriptions(
				Uuid: uuid,
				CompanyId: companyId,
				PackageId: packageId,
				TimestampAddedUtc: timestampAddedUtc,
				ProvisioningActual: provisioningActual,
				ProvisioningDesired: provisioningDesired,
				ProvisionedDatabaseName: provisionedDatabaseName,
				TimestampLastSettingsPushUtc: timestampLastSettingsPushUtc,
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


		public const string CEDatabaseName = "zclient_ce_0";



		public static void VerifyRepairTable(NpgsqlConnection db, bool insertDefaultContents = false) {

			if (db.TableExists("billing-subscriptions")) {
				Log.Debug($"----- Table \"billing-subscriptions\" exists.");
			} else {
				Log.Information($"----- Table \"billing-subscriptions\" doesn't exist, creating.");

				using NpgsqlCommand cmd = new NpgsqlCommand(@"
					CREATE TABLE ""public"".""billing-subscriptions"" (
						""uuid"" uuid DEFAULT public.uuid_generate_v1() NOT NULL,
						""company-id"" uuid,
						""package-id"" uuid,
						""timestamp-added-utc"" timestamp without time zone DEFAULT now() NOT NULL,
						""provisioning-actual"" character varying(255) NOT NULL,
						""provisioning-desired"" character varying(255) NOT NULL,
						""provisioned-database-name"" character varying(255),
						""timestamp-last-settings-push-utc"" timestamp without time zone,
						""json"" json DEFAULT '{}'::json NOT NULL,
						CONSTRAINT ""billing_subscriptions_pk"" PRIMARY KEY(""uuid"")
					) WITH(oids = false);
					", db);
				cmd.ExecuteNonQuery();
			}


			if (insertDefaultContents) {
				Log.Information("Insert Default Contents");
				Guid guid = Guid.NewGuid();
				BillingSubscriptions sub = new BillingSubscriptions(
					Uuid: guid,
					BillingCompanies.CECompanyId,
					BillingPackages.CEPackageId,
					DateTime.UtcNow,
					BillingSubscriptionsProvisioningStatus.ProvisionedName,
					BillingSubscriptionsProvisioningStatus.ProvisionedName,
					CEDatabaseName,
					null,
					Json: new JObject { }.ToString(Formatting.Indented)

					
					
					);

				Upsert(db, new Dictionary<Guid, BillingSubscriptions> {
					{guid, sub},
				}, out _, out _);

				
			}





		}




	}
}
