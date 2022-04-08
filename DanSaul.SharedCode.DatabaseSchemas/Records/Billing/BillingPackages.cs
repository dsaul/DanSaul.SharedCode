using Npgsql;
using System.Text;
using System.Data;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Serilog;

namespace SharedCode.DatabaseSchemas
{
	public record BillingPackages(
		Guid? Uuid,
		string? PackageName,
		string? DisplayName,
		string? Currency,
		decimal? CostPerMonth,
		bool? ProvisionDispatchPulse,
		int? ProvisionDispatchPulseUsers,
		bool? AllowNewAssignment,
		string? Type,
		bool? ProvisionEmail,
		int? ProvisionEmailUsers,
		bool? ProvisionWebsites,
		int? ProvisionWebsitesStaticCount,
		bool? IsDemo,
		string? Json
		)
	{
		public const string kJsonKeyProvisionOnCallAutoAttendants = "ProvisionOnCallAutoAttendants";
		public const string kJsonKeyProvisionS3StorageMB = "ProvisionS3StorageMB";
		public const string kJsonKeyProvisionOnCallResponders = "ProvisionOnCallResponders";
		public const string kJsonKeyProvisionOnCallUsers = "ProvisionOnCallUsers";

		public static Dictionary<Guid, BillingPackages> ForPackageName(NpgsqlConnection connection, string packageName) {

			Dictionary<Guid, BillingPackages> ret = new Dictionary<Guid, BillingPackages>();

			string sql = @"SELECT * from ""billing-packages"" WHERE ""package-name"" = @packageName";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			cmd.Parameters.AddWithValue("@packageName", packageName);




			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					BillingPackages obj = BillingPackages.FromDataReader(reader);
					if (obj.Uuid == null) {
						continue;
					}
					ret.Add(obj.Uuid.Value, obj);
				}
			}

			return ret;
		}

		public static Dictionary<Guid, BillingPackages> ForProvisionDispatchPulse(NpgsqlConnection connection, bool provisionDispatchPulse) {

			Dictionary<Guid, BillingPackages> ret = new Dictionary<Guid, BillingPackages>();

			string sql = @"SELECT * from ""billing-packages"" WHERE ""provision-dispatch-pulse"" = @provisionDispatchPulse";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			cmd.Parameters.AddWithValue("@provisionDispatchPulse", provisionDispatchPulse);

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					BillingPackages obj = BillingPackages.FromDataReader(reader);
					if (obj.Uuid == null) {
						continue;
					}
					ret.Add(obj.Uuid.Value, obj);
				}
			}

			return ret;
		}

		public static Dictionary<Guid, BillingPackages> ForAllowNewAssignment(NpgsqlConnection connection, bool allowNewAssignment) {

			Dictionary<Guid, BillingPackages> ret = new Dictionary<Guid, BillingPackages>();

			string sql = @"SELECT * from ""billing-packages"" WHERE ""allow-new-assignment"" = @allowNewAssignment";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			cmd.Parameters.AddWithValue("@allowNewAssignment", allowNewAssignment);

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					BillingPackages obj = BillingPackages.FromDataReader(reader);
					if (obj.Uuid == null) {
						continue;
					}
					ret.Add(obj.Uuid.Value, obj);
				}
			}

			return ret;
		}

		public static Dictionary<Guid, BillingPackages> ForProvisionOnCallAutoAttendants(NpgsqlConnection connection, bool provisionOnCallAutoAttendants) {

			Dictionary<Guid, BillingPackages> ret = new Dictionary<Guid, BillingPackages>();

			string sql = @"SELECT * from ""billing-packages"" WHERE (""json""->>'ProvisionOnCallAutoAttendants')::boolean = @provisionOnCallAutoAttendants";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			cmd.Parameters.AddWithValue("@provisionOnCallAutoAttendants", provisionOnCallAutoAttendants);

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					BillingPackages obj = BillingPackages.FromDataReader(reader);
					if (obj.Uuid == null) {
						continue;
					}
					ret.Add(obj.Uuid.Value, obj);
				}
			}

			return ret;
		}


		public static Dictionary<Guid, BillingPackages> ForId(NpgsqlConnection connection, Guid id) {

			Dictionary<Guid, BillingPackages> ret = new Dictionary<Guid, BillingPackages>();

			string sql = @"SELECT * from ""billing-packages"" WHERE uuid = @uuid";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			cmd.Parameters.AddWithValue("@uuid", id);




			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					BillingPackages obj = BillingPackages.FromDataReader(reader);
					if (obj.Uuid == null) {
						continue;
					}
					ret.Add(obj.Uuid.Value, obj);
				}
			}

			return ret;
		}



		public static Dictionary<Guid, BillingPackages> ForIds(NpgsqlConnection connection, IEnumerable<Guid> ids) {

			Guid[] idsArr = ids.ToArray();

			Dictionary<Guid, BillingPackages> ret = new Dictionary<Guid, BillingPackages>();
			if (idsArr.Length == 0) {
				return ret;
			}


			List<string> valNames = new List<string>();
			for (int i = 0; i < idsArr.Length; i++) {
				valNames.Add($"@val{i}");
			}

			string sql = $"SELECT * FROM \"billing-packages\" WHERE \"uuid\" IN ({string.Join(", ", valNames)})";

			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			for (int i = 0; i < valNames.Count; i++) {
				cmd.Parameters.AddWithValue(valNames[i], idsArr[i]);
			}

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					BillingPackages obj = BillingPackages.FromDataReader(reader);
					if (obj.Uuid == null) {
						continue;
					}
					ret.Add(obj.Uuid.Value, obj);
				}
			}


			return ret;
		}


		public static Dictionary<Guid, BillingPackages> All(NpgsqlConnection connection) {

			Dictionary<Guid, BillingPackages> ret = new Dictionary<Guid, BillingPackages>();

			string sql = @"SELECT * from ""billing-packages""";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					BillingPackages obj = BillingPackages.FromDataReader(reader);
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



			string sql = $"DELETE FROM \"billing-packages\" WHERE \"uuid\" IN ({string.Join(", ", valNames)})";
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


		public static Dictionary<Guid, BillingPackages> AllThatProvisionDP(NpgsqlConnection connection) {

			Dictionary<Guid, BillingPackages> ret = new Dictionary<Guid, BillingPackages>();

			string sql = @"SELECT * from ""billing-packages"" WHERE ""provision-dispatch-pulse"" = true OR (""json""->>'ProvisionOnCallAutoAttendants')::boolean = true";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					BillingPackages obj = BillingPackages.FromDataReader(reader);
					if (obj.Uuid == null) {
						continue;
					}
					ret.Add(obj.Uuid.Value, obj);
				}
			}

			return ret;


		}


		public static void Upsert(NpgsqlConnection connection, Dictionary<Guid, BillingPackages> updateObjects, out List<Guid> callerResponse, out Dictionary<Guid, BillingPackages> toSendToOthers) {

			callerResponse = new List<Guid>();
			toSendToOthers = new Dictionary<Guid, BillingPackages>();

			foreach (KeyValuePair<Guid, BillingPackages> kvp in updateObjects) {

				string sql = @"
					INSERT INTO
						""billing-packages""
						(
							""uuid"",
							""package-name"",
							""display-name"",
							""currency"",
							""cost-per-month"",
							""provision-dispatch-pulse"",
							""provision-dispatch-pulse-users"",
							""allow-new-assignment"",
							""type"",
							""provision-email"",
							""provision-email-users"",
							""provision-websites"",
							""provision-websites-static-count"",
							""is-demo"",
							""json""
						)
					VALUES
						(
							@uuid,
							@packageName,
							@displayName,
							@currency,
							@costPerMonth,
							@provisionDispatchPulse,
							@provisionDispatchPulseUsers,
							@allowNewAssignment,
							@type,
							@provisionEmail,
							@provisionEmailUsers,
							@provisionWebsites,
							@provisionWebsitesStaticCount,
							@isDemo,
							CAST(@json AS json)
						)
					ON CONFLICT (""uuid"") DO UPDATE
						SET
							""package-name"" = excluded.""package-name"",
							""display-name"" = excluded.""display-name"",
							""currency"" = excluded.""currency"",
							""cost-per-month"" = excluded.""cost-per-month"",
							""provision-dispatch-pulse"" = excluded.""provision-dispatch-pulse"",
							""provision-dispatch-pulse-users"" = excluded.""provision-dispatch-pulse-users"",
							""allow-new-assignment"" = excluded.""allow-new-assignment"",
							""type"" = excluded.""type"",
							""provision-email"" = excluded.""provision-email"",
							""provision-email-users"" = excluded.""provision-email-users"",
							""provision-websites"" = excluded.""provision-websites"",
							""provision-websites-static-count"" = excluded.""provision-websites-static-count"",
							""is-demo"" = excluded.""is-demo"",
							""json"" = CAST(excluded.""json"" AS json)
					";

				using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
				cmd.Parameters.AddWithValue("@uuid", kvp.Key);
				cmd.Parameters.AddWithValue("@packageName", string.IsNullOrWhiteSpace(kvp.Value.PackageName) ? (object)DBNull.Value : kvp.Value.PackageName);
				cmd.Parameters.AddWithValue("@displayName", string.IsNullOrWhiteSpace(kvp.Value.DisplayName) ? (object)DBNull.Value : kvp.Value.DisplayName);
				cmd.Parameters.AddWithValue("@currency", string.IsNullOrWhiteSpace(kvp.Value.Currency) ? (object)DBNull.Value : kvp.Value.Currency);
				cmd.Parameters.AddWithValue("@costPerMonth", null == kvp.Value.CostPerMonth ? (object)DBNull.Value : kvp.Value.CostPerMonth);
				cmd.Parameters.AddWithValue("@provisionDispatchPulse", null == kvp.Value.ProvisionDispatchPulse ? (object)DBNull.Value : kvp.Value.ProvisionDispatchPulse);
				cmd.Parameters.AddWithValue("@provisionDispatchPulseUsers", null == kvp.Value.ProvisionDispatchPulseUsers ? (object)DBNull.Value : kvp.Value.ProvisionDispatchPulseUsers);
				cmd.Parameters.AddWithValue("@allowNewAssignment", null == kvp.Value.AllowNewAssignment ? (object)DBNull.Value : kvp.Value.AllowNewAssignment);
				cmd.Parameters.AddWithValue("@type", string.IsNullOrWhiteSpace(kvp.Value.Type) ? (object)DBNull.Value : kvp.Value.Type);
				cmd.Parameters.AddWithValue("@provisionEmail", null == kvp.Value.ProvisionEmail ? (object)DBNull.Value : kvp.Value.ProvisionEmail);
				cmd.Parameters.AddWithValue("@provisionEmailUsers", null == kvp.Value.ProvisionEmailUsers ? (object)DBNull.Value : kvp.Value.ProvisionEmailUsers);
				cmd.Parameters.AddWithValue("@provisionWebsites", null == kvp.Value.ProvisionWebsites ? (object)DBNull.Value : kvp.Value.ProvisionWebsites);
				cmd.Parameters.AddWithValue("@provisionWebsitesStaticCount", null == kvp.Value.ProvisionWebsitesStaticCount ? (object)DBNull.Value : kvp.Value.ProvisionWebsitesStaticCount);
				cmd.Parameters.AddWithValue("@isDemo", null == kvp.Value.IsDemo ? (object)DBNull.Value : kvp.Value.IsDemo);
				cmd.Parameters.AddWithValue("@json", string.IsNullOrWhiteSpace(kvp.Value.Json) ? (object)DBNull.Value : kvp.Value.Json);

				int rowsAffected = cmd.ExecuteNonQuery();

				if (rowsAffected == 0) {
					continue;
				}

				toSendToOthers.Add(kvp.Key, kvp.Value);
				callerResponse.Add(kvp.Key);


			}



		}



		public static BillingPackages FromDataReader(NpgsqlDataReader reader) {

			Guid? uuid = default;
			string? packageName = default;
			string? displayName = default;
			string? currency = default;
			decimal? costPerMonth = default;
			bool? provisionDispatchPulse = default;
			int? provisionDispatchPulseUsers = default;
			bool? allowNewAssignment = default;
			string? type = default;
			bool? provisionEmail = default;
			int? provisionEmailUsers = default;
			bool? provisionWebsites = default;
			int? provisionWebsitesStaticCount = default;
			bool? isDemo = default;
			string? json = default;

			if (!reader.IsDBNull("uuid")) {
				uuid = reader.GetGuid("uuid");
			}
			if (!reader.IsDBNull("package-name")) {
				packageName = reader.GetString("package-name");
			}
			if (!reader.IsDBNull("display-name")) {
				displayName = reader.GetString("display-name");
			}
			if (!reader.IsDBNull("currency")) {
				currency = reader.GetString("currency");
			}
			if (!reader.IsDBNull("cost-per-month")) {
				costPerMonth = reader.GetDecimal("cost-per-month");
			}
			if (!reader.IsDBNull("provision-dispatch-pulse")) {
				provisionDispatchPulse = reader.GetBoolean("provision-dispatch-pulse");
			}
			if (!reader.IsDBNull("provision-dispatch-pulse-users")) {
				provisionDispatchPulseUsers = reader.GetInt32("provision-dispatch-pulse-users");
			}
			if (!reader.IsDBNull("allow-new-assignment")) {
				allowNewAssignment = reader.GetBoolean("allow-new-assignment");
			}
			if (!reader.IsDBNull("type")) {
				type = reader.GetString("type");
			}
			if (!reader.IsDBNull("provision-email")) {
				provisionEmail = reader.GetBoolean("provision-email");
			}
			if (!reader.IsDBNull("provision-email-users")) {
				provisionEmailUsers = reader.GetInt32("provision-email-users");
			}
			if (!reader.IsDBNull("provision-websites")) {
				provisionWebsites = reader.GetBoolean("provision-websites");
			}
			if (!reader.IsDBNull("provision-websites-static-count")) {
				provisionWebsitesStaticCount = reader.GetInt32("provision-websites-static-count");
			}
			if (!reader.IsDBNull("is-demo")) {
				isDemo = reader.GetBoolean("is-demo");
			}
			if (!reader.IsDBNull("json")) {
				json = reader.GetString("json");
			}

			return new BillingPackages(
				Uuid: uuid,
				PackageName: packageName,
				DisplayName: displayName,
				Currency: currency,
				CostPerMonth: costPerMonth,
				ProvisionDispatchPulse: provisionDispatchPulse,
				ProvisionDispatchPulseUsers: provisionDispatchPulseUsers,
				AllowNewAssignment: allowNewAssignment,
				Type: type,
				ProvisionEmail: provisionEmail,
				ProvisionEmailUsers: provisionEmailUsers,
				ProvisionWebsites: provisionWebsites,
				ProvisionWebsitesStaticCount: provisionWebsitesStaticCount,
				IsDemo: isDemo,
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
		public bool? ProvisionOnCallAutoAttendants
		{
			get {
				JObject? root = JsonObject;
				if (root == null)
					return default;
				if (false == root.ContainsKey(kJsonKeyProvisionOnCallAutoAttendants)) {
					return null;
				}

				return root.Value<bool>(kJsonKeyProvisionOnCallAutoAttendants);
			}
		}

		[JsonIgnore]
		public int? ProvisionS3StorageMB
		{
			get {
				JObject? root = JsonObject;
				if (root == null)
					return default;
				if (false == root.ContainsKey(kJsonKeyProvisionS3StorageMB)) {
					return null;
				}

				return root.Value<int>(kJsonKeyProvisionS3StorageMB);
			}
		}


		[JsonIgnore]
		public int? ProvisionOnCallResponders
		{
			get {
				JObject? root = JsonObject;
				if (root == null)
					return default;
				if (false == root.ContainsKey(kJsonKeyProvisionOnCallResponders)) {
					return null;
				}

				return root.Value<int>(kJsonKeyProvisionOnCallResponders);
			}
		}

		[JsonIgnore]
		public int? ProvisionOnCallUsers
		{
			get {
				JObject? root = JsonObject;
				if (root == null)
					return default;
				if (false == root.ContainsKey(kJsonKeyProvisionOnCallUsers)) {
					return null;
				}

				return root.Value<int>(kJsonKeyProvisionOnCallUsers);
			}
		}


		public static void VerifyRepairTable(NpgsqlConnection db, out Guid? billingPackageCommunityEditionId, bool insertDefaultContents = false) {

			if (db.TableExists("billing-packages")) {
				Log.Debug($"----- Table \"billing-packages\" exists.");
			} else {
				Log.Information($"----- Table \"billing-packages\" doesn't exist, creating.");

				using NpgsqlCommand cmd = new NpgsqlCommand(@"
					CREATE TABLE ""public"".""billing-packages"" (
						""uuid"" uuid DEFAULT public.uuid_generate_v1() NOT NULL,
						""package-name"" character varying(255) NULL,
						""display-name"" character varying(255) NULL,
						""currency"" character varying(255) NULL,
						""cost-per-month"" numeric NULL,
						""provision-dispatch-pulse"" boolean NULL,
						""provision-dispatch-pulse-users"" integer NULL,
						""allow-new-assignment"" boolean NULL,
						""type"" character varying(255) NULL,
						""provision-email"" boolean NULL,
						""provision-email-users"" integer NULL,
						""provision-websites"" boolean NULL,
						""provision-websites-static-count"" integer NULL,
						""is-demo"" boolean DEFAULT false NULL,
						""json"" json DEFAULT '{}'::json NULL,
						CONSTRAINT ""billing_packages_pk"" PRIMARY KEY(""uuid"")
					) WITH(oids = false);
					", db);
				cmd.ExecuteNonQuery();
			}


			if (insertDefaultContents) {
				Log.Information("Insert Default Contents");
				Guid guid = Guid.NewGuid();
				BillingPackages bc = new BillingPackages(
					Uuid: guid,
					PackageName: "Community Edition",
					DisplayName: "Community Edition",
					Currency: null,
					CostPerMonth: 0,
					ProvisionDispatchPulse: true,
					ProvisionDispatchPulseUsers: 9999,
					AllowNewAssignment: true,
					Type: "Primary",
					ProvisionEmail: false,
					ProvisionEmailUsers: 0,
					ProvisionWebsites: false,
					ProvisionWebsitesStaticCount: 0,
					IsDemo: false,
					Json: new JObject {
						{ kJsonKeyProvisionOnCallAutoAttendants, true },
						{ kJsonKeyProvisionS3StorageMB, 9999999 },
						{ kJsonKeyProvisionOnCallResponders, true },
						{ kJsonKeyProvisionOnCallUsers, 9999999 }
					}.ToString(Formatting.Indented)
				);

				Upsert(db, new Dictionary<Guid, BillingPackages> {
					{guid, bc},
				}, out _, out _);

				billingPackageCommunityEditionId = guid;

			} else {
				billingPackageCommunityEditionId = null;
			}





		}















	}
}
