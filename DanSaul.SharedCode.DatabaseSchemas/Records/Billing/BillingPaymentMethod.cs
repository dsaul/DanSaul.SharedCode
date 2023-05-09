using Npgsql;
using System.Text;
using System.Data;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Serilog;
using DanSaul.SharedCode.DatabaseSchemas.Properties;
using DanSaul.SharedCode;
using DanSaul.SharedCode.Npgsql;

namespace SharedCode.DatabaseSchemas
{
	public record BillingPaymentMethod(
		string? Value,
		Guid? Uuid,
		string? Json
		)
	{
		public const string kValueInvoice = "Invoice";
		public const string kValueSquarePreAuthorized = "Square Pre-Authorized";


		public static Dictionary<Guid, BillingPaymentMethod> ForId(NpgsqlConnection connection, Guid id) {

			Dictionary<Guid, BillingPaymentMethod> ret = new Dictionary<Guid, BillingPaymentMethod>();

			string sql = @"SELECT * from ""billing-payment-method"" WHERE uuid = @uuid";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			cmd.Parameters.AddWithValue("@uuid", id);




			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					BillingPaymentMethod obj = BillingPaymentMethod.FromDataReader(reader);
					if (obj.Uuid == null) {
						continue;
					}
					ret.Add(obj.Uuid.Value, obj);
				}
			}

			return ret;
		}

		public static Dictionary<Guid, BillingPaymentMethod> ForIds(NpgsqlConnection connection, IEnumerable<Guid> ids) {

			Guid[] idsArr = ids.ToArray();

			Dictionary<Guid, BillingPaymentMethod> ret = new Dictionary<Guid, BillingPaymentMethod>();
			if (idsArr.Length == 0)
				return ret;

			List<string> valNames = new List<string>();
			for (int i = 0; i < idsArr.Length; i++) {
				valNames.Add($"@val{i}");
			}

			string sql = $"SELECT * from \"billing-payment-method\" WHERE uuid IN ({string.Join(", ", valNames)})";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			for (int i = 0; i < valNames.Count; i++) {
				cmd.Parameters.AddWithValue(valNames[i], idsArr[i]);
			}

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					BillingPaymentMethod obj = BillingPaymentMethod.FromDataReader(reader);
					if (obj.Uuid == null) {
						continue;
					}
					ret.Add(obj.Uuid.Value, obj);
				}
			}

			return ret;


		}


		public static Dictionary<Guid, BillingPaymentMethod> All(NpgsqlConnection connection) {

			Dictionary<Guid, BillingPaymentMethod> ret = new Dictionary<Guid, BillingPaymentMethod>();

			string sql = @"SELECT * from ""billing-payment-method""";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					BillingPaymentMethod obj = BillingPaymentMethod.FromDataReader(reader);
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



			string sql = $"DELETE FROM \"billing-payment-method\" WHERE \"uuid\" IN ({string.Join(", ", valNames)})";
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


		public static void Upsert(NpgsqlConnection connection, Dictionary<Guid, BillingPaymentMethod> updateObjects, out List<Guid> callerResponse, out Dictionary<Guid, BillingPaymentMethod> toSendToOthers) {

			callerResponse = new List<Guid>();
			toSendToOthers = new Dictionary<Guid, BillingPaymentMethod>();

			foreach (KeyValuePair<Guid, BillingPaymentMethod> kvp in updateObjects) {

				string sql = @"
					INSERT INTO
						""billing-payment-method""
						(
							""uuid"",
							""value"",
							""json""
						)
					VALUES
						(
							@uuid,
							@value,
							CAST(@json AS json)
						)
					ON CONFLICT (""uuid"") DO UPDATE
						SET
							""value"" = excluded.""value"",
							""json"" = CAST(excluded.""json"" AS json)
					";

				using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
				cmd.Parameters.AddWithValue("@uuid", kvp.Key);
				cmd.Parameters.AddWithValue("@value", string.IsNullOrWhiteSpace(kvp.Value.Value) ? (object)DBNull.Value : kvp.Value.Value);
				cmd.Parameters.AddWithValue("@json", string.IsNullOrWhiteSpace(kvp.Value.Json) ? (object)DBNull.Value : kvp.Value.Json);

				int rowsAffected = cmd.ExecuteNonQuery();

				if (rowsAffected == 0) {
					continue;
				}

				toSendToOthers.Add(kvp.Key, kvp.Value);
				callerResponse.Add(kvp.Key);


			}



		}




		public static BillingPaymentMethod FromDataReader(NpgsqlDataReader reader) {

			string? value = default;
			Guid? uuid = default;
			string? json = default;

			if (!reader.IsDBNull("uuid")) {
				uuid = reader.GetGuid("uuid");
			}
			if (!reader.IsDBNull("value")) {
				value = reader.GetString("value");
			}
			if (!reader.IsDBNull("json")) {
				json = reader.GetString("json");
			}


			return new BillingPaymentMethod(
				Value: value,
				Uuid: uuid,
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

			if (db.TableExists("billing-payment-method")) {
				Log.Debug($"----- Table \"billing-payment-method\" exists.");
			} else {
				Log.Information($"----- Table \"billing-payment-method\" doesn't exist, creating.");

				using NpgsqlCommand cmd = new NpgsqlCommand(@"
					CREATE TABLE ""public"".""billing-payment-method"" (
						""value"" character varying(255) NOT NULL,
						""uuid"" uuid DEFAULT public.uuid_generate_v1() NOT NULL,
						""json"" json DEFAULT '{}'::json NOT NULL,
						CONSTRAINT ""billing_payment_method_pk"" PRIMARY KEY(""uuid"")
					) WITH(oids = false);
					", db);
				cmd.ExecuteNonQuery();
			}


			if (insertDefaultContents) {
				Log.Information("Insert Default Contents");

				NpgsqlCommand command = new NpgsqlCommand(SQLUtility.RemoveCommentsFromSQLString(Resources.SQLInsertDefaultPaymentMethod, true), db);
				command.ExecuteNonQuery();
			}





		}























	}
}
