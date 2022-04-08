using Npgsql;
using System.Text;
using System.Data;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Serilog;

namespace SharedCode.DatabaseSchemas
{
	public record BillingCurrency(
		Guid? Uuid,
		string? Currency,
		string? Json
		)
	{

		public static Dictionary<Guid, BillingCurrency> ForId(NpgsqlConnection connection, Guid id) {

			Dictionary<Guid, BillingCurrency> ret = new Dictionary<Guid, BillingCurrency>();

			string sql = @"SELECT * from ""billing-currency"" WHERE id = @uuid";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			cmd.Parameters.AddWithValue("@uuid", id);




			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					BillingCurrency obj = BillingCurrency.FromDataReader(reader);
					if (obj.Uuid == null) {
						continue;
					}
					ret.Add(obj.Uuid.Value, obj);
				}
			}

			return ret;
		}

		public static Dictionary<Guid, BillingCurrency> ForIds(NpgsqlConnection connection, IEnumerable<Guid> ids) {

			Guid[] idsArr = ids.ToArray();

			Dictionary<Guid, BillingCurrency> ret = new Dictionary<Guid, BillingCurrency>();
			if (idsArr.Length == 0)
				return ret;

			List<string> valNames = new List<string>();
			for (int i = 0; i < idsArr.Length; i++) {
				valNames.Add($"@val{i}");
			}

			string sql = $"SELECT * from \"billing-currency\" WHERE uuid IN ({string.Join(", ", valNames)})";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			for (int i = 0; i < valNames.Count; i++) {
				cmd.Parameters.AddWithValue(valNames[i], idsArr[i]);
			}

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					BillingCurrency obj = BillingCurrency.FromDataReader(reader);
					if (obj.Uuid == null) {
						continue;
					}
					ret.Add(obj.Uuid.Value, obj);
				}
			}

			return ret;


		}


		public static Dictionary<Guid, BillingCurrency> All(NpgsqlConnection connection) {

			Dictionary<Guid, BillingCurrency> ret = new Dictionary<Guid, BillingCurrency>();

			string sql = @"SELECT * from ""billing-currency""";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					BillingCurrency obj = BillingCurrency.FromDataReader(reader);
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



			string sql = $"DELETE FROM \"billing-currency\" WHERE \"uuid\" IN ({string.Join(", ", valNames)})";
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

		public static void Upsert(NpgsqlConnection connection, Dictionary<Guid, BillingCurrency> updateObjects, out List<Guid> callerResponse, out Dictionary<Guid, BillingCurrency> toSendToOthers) {

			callerResponse = new List<Guid>();
			toSendToOthers = new Dictionary<Guid, BillingCurrency>();

			foreach (KeyValuePair<Guid, BillingCurrency> kvp in updateObjects) {

				string sql = @"
					INSERT INTO
						""billing-currency""
						(
							""uuid"",
							""currency"",
							""json""
						)
					VALUES
						(
							@uuid,
							@currency,
							CAST(@json AS json)
						)
					ON CONFLICT (""uuid"") DO UPDATE
						SET
							""currency"" = excluded.""currency"",
							""json"" = CAST(excluded.""json"" AS json)
					";

				using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
				cmd.Parameters.AddWithValue("@uuid", kvp.Key);
				cmd.Parameters.AddWithValue("@currency", string.IsNullOrWhiteSpace(kvp.Value.Currency) ? (object)DBNull.Value : kvp.Value.Currency);
				cmd.Parameters.AddWithValue("@json", string.IsNullOrWhiteSpace(kvp.Value.Json) ? (object)DBNull.Value : kvp.Value.Json);

				int rowsAffected = cmd.ExecuteNonQuery();

				if (rowsAffected == 0) {
					continue;
				}

				toSendToOthers.Add(kvp.Key, kvp.Value);
				callerResponse.Add(kvp.Key);


			}



		}


		public static BillingCurrency FromDataReader(NpgsqlDataReader reader) {

			Guid? uuid = default;
			string? currency = default;
			string? json = default;

			if (!reader.IsDBNull("uuid")) {
				uuid = reader.GetGuid("uuid");
			}
			if (!reader.IsDBNull("currency")) {
				currency = reader.GetString("currency");
			}
			if (!reader.IsDBNull("json")) {
				json = reader.GetString("json");
			}


			return new BillingCurrency(
				Uuid: uuid,
				Currency: currency,
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

			if (db.TableExists("billing-currency")) {
				Log.Debug($"----- Table \"billing-currency\" exists.");
			} else {
				Log.Information($"----- Table \"billing-currency\" doesn't exist, creating.");

				using NpgsqlCommand cmd = new NpgsqlCommand(@"
					CREATE TABLE ""public"".""billing-currency"" (
						""uuid"" uuid DEFAULT public.uuid_generate_v1() NOT NULL,
						""currency"" character varying(255) NOT NULL,
						""json"" json DEFAULT '{}'::json NOT NULL,
						CONSTRAINT ""billing_currency_pk"" PRIMARY KEY(""uuid"")
					) WITH(oids = false);
					", db);
				cmd.ExecuteNonQuery();
			}


			if (insertDefaultContents) {
				// None
			}





		}

























	}
}
