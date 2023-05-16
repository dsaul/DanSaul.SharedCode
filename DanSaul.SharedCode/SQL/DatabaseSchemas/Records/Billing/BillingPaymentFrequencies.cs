using Npgsql;
using System.Text;
using System.Data;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Serilog;
using DanSaul.SharedCode.SQL.DatabaseSchemas.Properties;
using DanSaul.SharedCode.Npgsql;
using DanSaul.SharedCode.SQL;

namespace SharedCode.DatabaseSchemas
{
    public record BillingPaymentFrequencies(
		Guid? Uuid,
		string? Value,
		string? DisplayName,
		int? MonthsBetweenPayments,
		string? Json
		)
	{
		public const string kValueMonthly = "Monthly";
		public const string kValueQuarterly = "Quarterly";
		public const string kValueAnnually = "Annually";

		public static Dictionary<Guid, BillingPaymentFrequencies> ForId(NpgsqlConnection connection, Guid id) {

			Dictionary<Guid, BillingPaymentFrequencies> ret = new Dictionary<Guid, BillingPaymentFrequencies>();

			string sql = @"SELECT * from ""billing-payment-frequencies"" WHERE uuid = @uuid";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			cmd.Parameters.AddWithValue("@uuid", id);




			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					BillingPaymentFrequencies obj = BillingPaymentFrequencies.FromDataReader(reader);
					if (obj.Uuid == null) {
						continue;
					}
					ret.Add(obj.Uuid.Value, obj);
				}
			}

			return ret;
		}

		public static Dictionary<Guid, BillingPaymentFrequencies> ForIds(NpgsqlConnection connection, IEnumerable<Guid> ids) {

			Guid[] idsArr = ids.ToArray();

			Dictionary<Guid, BillingPaymentFrequencies> ret = new Dictionary<Guid, BillingPaymentFrequencies>();
			if (idsArr.Length == 0)
				return ret;

			List<string> valNames = new List<string>();
			for (int i = 0; i < idsArr.Length; i++) {
				valNames.Add($"@val{i}");
			}

			string sql = $"SELECT * from \"billing-payment-frequencies\" WHERE uuid IN ({string.Join(", ", valNames)})";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			for (int i = 0; i < valNames.Count; i++) {
				cmd.Parameters.AddWithValue(valNames[i], idsArr[i]);
			}

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					BillingPaymentFrequencies obj = BillingPaymentFrequencies.FromDataReader(reader);
					if (obj.Uuid == null) {
						continue;
					}
					ret.Add(obj.Uuid.Value, obj);
				}
			}

			return ret;


		}


		public static Dictionary<Guid, BillingPaymentFrequencies> All(NpgsqlConnection connection) {

			Dictionary<Guid, BillingPaymentFrequencies> ret = new Dictionary<Guid, BillingPaymentFrequencies>();

			string sql = @"SELECT * from ""billing-payment-frequencies""";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					BillingPaymentFrequencies obj = BillingPaymentFrequencies.FromDataReader(reader);
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



			string sql = $"DELETE FROM \"billing-payment-frequencies\" WHERE \"uuid\" IN ({string.Join(", ", valNames)})";
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

		public static void Upsert(NpgsqlConnection connection, Dictionary<Guid, BillingPaymentFrequencies> updateObjects, out List<Guid> callerResponse, out Dictionary<Guid, BillingPaymentFrequencies> toSendToOthers) {

			callerResponse = new List<Guid>();
			toSendToOthers = new Dictionary<Guid, BillingPaymentFrequencies>();

			foreach (KeyValuePair<Guid, BillingPaymentFrequencies> kvp in updateObjects) {

				string sql = @"
					INSERT INTO
						""billing-payment-frequencies""
						(
							""uuid"",
							""value"",
							""display-name"",
							""months-between-payments"",
							""json""
						)
					VALUES
						(
							@uuid,
							@value,
							@displayName,
							@monthsBetweenPayments,
							CAST(@json AS json),
						)
					ON CONFLICT (""uuid"") DO UPDATE
						SET
							""value"" = excluded.""value"",
							""displayName"" = excluded.""displayName"",
							""monthsBetweenPayments"" = excluded.""monthsBetweenPayments"",
							""json"" = CAST(excluded.""json"" AS json)
					";

				using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
				cmd.Parameters.AddWithValue("@uuid", kvp.Key);
				cmd.Parameters.AddWithValue("@value", string.IsNullOrWhiteSpace(kvp.Value.Value) ? (object)DBNull.Value : kvp.Value.Value);
				cmd.Parameters.AddWithValue("@displayName", string.IsNullOrWhiteSpace(kvp.Value.DisplayName) ? (object)DBNull.Value : kvp.Value.DisplayName);
				cmd.Parameters.AddWithValue("@monthsBetweenPayments", null == kvp.Value.MonthsBetweenPayments ? (object)DBNull.Value : kvp.Value.MonthsBetweenPayments);
				cmd.Parameters.AddWithValue("@json", string.IsNullOrWhiteSpace(kvp.Value.Json) ? (object)DBNull.Value : kvp.Value.Json);

				int rowsAffected = cmd.ExecuteNonQuery();

				if (rowsAffected == 0) {
					continue;
				}

				toSendToOthers.Add(kvp.Key, kvp.Value);
				callerResponse.Add(kvp.Key);


			}



		}


		public static BillingPaymentFrequencies FromDataReader(NpgsqlDataReader reader) {

			Guid? uuid = default;
			string? value = default;
			string? displayName = default;
			int? monthsBetweenPayments = default;
			string? json = default;


			if (!reader.IsDBNull("uuid")) {
				uuid = reader.GetGuid("uuid");
			}
			if (!reader.IsDBNull("value")) {
				value = reader.GetString("value");
			}
			if (!reader.IsDBNull("display-name")) {
				displayName = reader.GetString("display-name");
			}
			if (!reader.IsDBNull("months-between-payments")) {
				monthsBetweenPayments = reader.GetInt32("months-between-payments");
			}
			if (!reader.IsDBNull("json")) {
				json = reader.GetString("json");
			}


			return new BillingPaymentFrequencies(
				Uuid: uuid,
				Value: value,
				DisplayName: displayName,
				MonthsBetweenPayments: monthsBetweenPayments,
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



		public static void VerifyRepairTable(NpgsqlConnection db, bool insertDefaultContents = false) {

			if (db.TableExists("billing-payment-frequencies")) {
				Log.Debug($"----- Table \"billing-payment-frequencies\" exists.");
			} else {
				Log.Information($"----- Table \"billing-payment-frequencies\" doesn't exist, creating.");

				using NpgsqlCommand cmd = new NpgsqlCommand(@"
					CREATE TABLE ""public"".""billing-payment-frequencies"" (
						""uuid"" uuid DEFAULT public.uuid_generate_v1() NOT NULL,
						""value"" character varying(255) NOT NULL,
						""display-name"" character varying(255) NOT NULL,
						""months-between-payments"" integer NOT NULL,
						""json"" json DEFAULT '{}'::json NOT NULL,
						CONSTRAINT ""billing_payment_frequencies_pk"" PRIMARY KEY(""uuid"")
					) WITH(oids = false);
					", db);
				cmd.ExecuteNonQuery();
			}


			if (insertDefaultContents) {
				Log.Information("Insert Default Contents");
				
				NpgsqlCommand command = new NpgsqlCommand(SQLUtility.RemoveCommentsFromSQLString(Resources.SQLInsertDefaultBillingJournalEntriesType, true), db);
				command.ExecuteNonQuery();
			}





		}




















	}
}
