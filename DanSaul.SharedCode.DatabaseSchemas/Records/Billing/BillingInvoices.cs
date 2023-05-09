using Npgsql;
using System.Text;
using System.Data;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Serilog;
using DanSaul.SharedCode.Npgsql;

namespace SharedCode.DatabaseSchemas
{
	public record BillingInvoices(
		Guid? Uuid,
		DateTime? TimestampStartUtc,
		DateTime? TimestampEndUtc,
		string? InvoiceNumber,
		string? Currency,
		decimal? AmountDue,
		decimal? AmountPaid,
		decimal? AmountRemaining,
		DateTime? TimestampCreatedUtc,
		DateTime? TimestampDueUtc,
		Guid? CompanyId,
		string? Json
		)
	{
		public static Dictionary<Guid, BillingInvoices> ForId(NpgsqlConnection connection, Guid id) {

			Dictionary<Guid, BillingInvoices> ret = new Dictionary<Guid, BillingInvoices>();

			string sql = @"SELECT * from ""billing-invoices"" WHERE uuid = @uuid";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			cmd.Parameters.AddWithValue("@uuid", id);




			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					BillingInvoices obj = BillingInvoices.FromDataReader(reader);
					if (obj.Uuid == null) {
						continue;
					}
					ret.Add(obj.Uuid.Value, obj);
				}
			}

			return ret;
		}

		public static Dictionary<Guid, BillingInvoices> ForIds(NpgsqlConnection connection, IEnumerable<Guid> ids) {

			Guid[] idsArr = ids.ToArray();

			Dictionary<Guid, BillingInvoices> ret = new Dictionary<Guid, BillingInvoices>();
			if (idsArr.Length == 0)
				return ret;

			List<string> valNames = new List<string>();
			for (int i = 0; i < idsArr.Length; i++) {
				valNames.Add($"@val{i}");
			}

			string sql = $"SELECT * from \"billing-invoices\" WHERE uuid IN ({string.Join(", ", valNames)})";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			for (int i = 0; i < valNames.Count; i++) {
				cmd.Parameters.AddWithValue(valNames[i], idsArr[i]);
			}

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					BillingInvoices obj = BillingInvoices.FromDataReader(reader);
					if (obj.Uuid == null) {
						continue;
					}
					ret.Add(obj.Uuid.Value, obj);
				}
			}

			return ret;


		}

		public static Dictionary<Guid, BillingInvoices> ForCompanyId(NpgsqlConnection connection, Guid companyId) {

			Dictionary<Guid, BillingInvoices> ret = new Dictionary<Guid, BillingInvoices>();

			string sql = @"SELECT * from ""billing-invoices"" WHERE ""company-id"" = @uuid";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			cmd.Parameters.AddWithValue("@uuid", companyId);

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					BillingInvoices obj = BillingInvoices.FromDataReader(reader);
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



			string sql = $"DELETE FROM \"billing-invoices\" WHERE \"uuid\" IN ({string.Join(", ", valNames)})";
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


		public static List<Guid> DeleteForCompanyId(NpgsqlConnection connection, List<Guid> idsToDelete) {

			List<Guid> toSendToOthers = new List<Guid>();
			if (idsToDelete.Count == 0) {
				return toSendToOthers;
			}

			List<string> valNames = new List<string>();
			for (int i = 0; i < idsToDelete.Count; i++) {
				valNames.Add($"@val{i}");
			}



			string sql = $"DELETE FROM \"billing-invoices\" WHERE \"company-id\" IN ({string.Join(", ", valNames)})";
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


		public static Dictionary<Guid, BillingInvoices> All(NpgsqlConnection connection) {

			Dictionary<Guid, BillingInvoices> ret = new Dictionary<Guid, BillingInvoices>();

			string sql = @"SELECT * from ""billing-invoices""";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					BillingInvoices obj = BillingInvoices.FromDataReader(reader);
					if (obj.Uuid == null) {
						continue;
					}
					ret.Add(obj.Uuid.Value, obj);
				}
			}

			return ret;


		}



		public static void Upsert(NpgsqlConnection connection, Dictionary<Guid, BillingInvoices> updateObjects, out List<Guid> callerResponse, out Dictionary<Guid, BillingInvoices> toSendToOthers) {

			callerResponse = new List<Guid>();
			toSendToOthers = new Dictionary<Guid, BillingInvoices>();

			foreach (KeyValuePair<Guid, BillingInvoices> kvp in updateObjects) {

				string sql = @"
					INSERT INTO
						""billing-invoices""
						(
							""uuid"",
							""timestamp-start-utc"",
							""timestamp-end-utc"",
							""invoice-number"",
							""currency"",
							""amount-due"",
							""amount-paid"",
							""amount-remaining"",
							""timestamp-created-utc"",
							""timestamp-due-utc"",
							""company-id"",
							""json""
						)
					VALUES
						(
							@uuid,
							@timestampStartUtc,
							@timestampEndUtc,
							@invoiceNumber,
							@currency,
							@amountDue,
							@amountPaid,
							@amountRemaining,
							@timestampCreatedUtc,
							@timestampDueUtc,
							@companyId
							CAST(@json AS json)
						)
					ON CONFLICT (""uuid"") DO UPDATE
						SET
							""uuid"" = excluded.""uuid"",
							""timestamp-start-utc"" = excluded.""timestamp-start-utc"",
							""timestamp-end-utc"" = excluded.""timestamp-end-utc"",
							""invoice-number"" = excluded.""invoice-number"",
							""currency"" = excluded.""currency"",
							""amount-due"" = excluded.""amount-due"",
							""amount-paid"" = excluded.""amount-paid"",
							""amount-remaining"" = excluded.""amount-remaining"",
							""timestamp-created-utc"" = excluded.""timestamp-created-utc"",
							""timestamp-due-utc"" = excluded.""timestamp-due-utc"",
							""company-id"" = excluded.""company-id"",
							""json"" = CAST(excluded.""json"" AS json)
					";

				using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
				cmd.Parameters.AddWithValue("@uuid", kvp.Key);
				cmd.Parameters.AddWithValue("@timestampStartUtc", null == kvp.Value.TimestampStartUtc ? (object)DBNull.Value : kvp.Value.TimestampStartUtc);
				cmd.Parameters.AddWithValue("@timestampEndUtc", null == kvp.Value.TimestampEndUtc ? (object)DBNull.Value : kvp.Value.TimestampEndUtc);
				cmd.Parameters.AddWithValue("@invoiceNumber", string.IsNullOrWhiteSpace(kvp.Value.InvoiceNumber) ? (object)DBNull.Value : kvp.Value.InvoiceNumber);
				cmd.Parameters.AddWithValue("@currency", string.IsNullOrWhiteSpace(kvp.Value.Currency) ? (object)DBNull.Value : kvp.Value.Currency);
				cmd.Parameters.AddWithValue("@amountDue", null == kvp.Value.AmountDue ? (object)DBNull.Value : kvp.Value.AmountDue);
				cmd.Parameters.AddWithValue("@amountPaid", null == kvp.Value.AmountPaid ? (object)DBNull.Value : kvp.Value.AmountPaid);
				cmd.Parameters.AddWithValue("@amountRemaining", null == kvp.Value.AmountRemaining ? (object)DBNull.Value : kvp.Value.AmountRemaining);
				cmd.Parameters.AddWithValue("@timestampCreatedUtc", null == kvp.Value.TimestampCreatedUtc ? (object)DBNull.Value : kvp.Value.TimestampCreatedUtc);
				cmd.Parameters.AddWithValue("@timestampDueUtc", null == kvp.Value.TimestampDueUtc ? (object)DBNull.Value : kvp.Value.TimestampDueUtc);
				cmd.Parameters.AddWithValue("@companyId", null == kvp.Value.CompanyId ? (object)DBNull.Value : kvp.Value.CompanyId);
				cmd.Parameters.AddWithValue("@json", string.IsNullOrWhiteSpace(kvp.Value.Json) ? (object)DBNull.Value : kvp.Value.Json);

				int rowsAffected = cmd.ExecuteNonQuery();

				if (rowsAffected == 0) {
					continue;
				}

				toSendToOthers.Add(kvp.Key, kvp.Value);
				callerResponse.Add(kvp.Key);


			}



		}



		public static BillingInvoices FromDataReader(NpgsqlDataReader reader) {

			Guid? uuid = default;
			DateTime? timestampStartUtc = default;
			DateTime? timestampEndUtc = default;
			string? invoiceNumber = default;
			string? currency = default;
			decimal? amountDue = default;
			decimal? amountPaid = default;
			decimal? amountRemaining = default;
			DateTime? timestampCreatedUtc = default;
			DateTime? timestampDueUtc = default;
			Guid? companyId = default;
			string? json = default;


			Dictionary<string, int> columns = new Dictionary<string, int>();
			for (int i = 0; i < reader.FieldCount; i++) {
				columns.Add(reader.GetName(i), i);
			}



			if (!reader.IsDBNull("uuid")) {
				uuid = reader.GetGuid("uuid");
			}
			if (!reader.IsDBNull("timestamp-start-utc")) {
				timestampStartUtc = reader.GetDateTime(columns["timestamp-start-utc"]);
			}
			if (!reader.IsDBNull("timestamp-end-utc")) {
				timestampEndUtc = reader.GetDateTime(columns["timestamp-end-utc"]);
			}
			if (!reader.IsDBNull("invoice-number")) {
				invoiceNumber = reader.GetString("invoice-number");
			}
			if (!reader.IsDBNull("currency")) {
				currency = reader.GetString("currency");
			}
			if (!reader.IsDBNull("amount-due")) {
				amountDue = reader.GetDecimal("amount-due");
			}
			if (!reader.IsDBNull("amount-paid")) {
				amountPaid = reader.GetDecimal("amount-paid");
			}
			if (!reader.IsDBNull("amount-remaining")) {
				amountRemaining = reader.GetDecimal("amount-remaining");
			}
			if (!reader.IsDBNull("timestamp-created-utc")) {
				timestampCreatedUtc = reader.GetDateTime(columns["timestamp-created-utc"]);
			}
			if (!reader.IsDBNull("timestamp-due-utc")) {
				timestampDueUtc = reader.GetDateTime(columns["timestamp-due-utc"]);
			}
			if (!reader.IsDBNull("company-id")) {
				companyId = reader.GetGuid("company-id");
			}
			if (!reader.IsDBNull("json")) {
				json = reader.GetString("json");
			}

			return new BillingInvoices(
				Uuid: uuid,
				TimestampStartUtc: timestampStartUtc,
				TimestampEndUtc: timestampEndUtc,
				InvoiceNumber: invoiceNumber,
				Currency: currency,
				AmountDue: amountDue,
				AmountPaid: amountPaid,
				AmountRemaining: amountRemaining,
				TimestampCreatedUtc: timestampCreatedUtc,
				TimestampDueUtc: timestampDueUtc,
				CompanyId: companyId,
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

			if (db.TableExists("billing-invoices")) {
				Log.Debug($"----- Table \"billing-invoices\" exists.");
			} else {
				Log.Information($"----- Table \"billing-invoices\" doesn't exist, creating.");

				using NpgsqlCommand cmd = new NpgsqlCommand(@"
					CREATE TABLE ""public"".""billing-invoices"" (
						""uuid"" uuid DEFAULT public.uuid_generate_v1() NOT NULL,
						""timestamp-start-utc"" timestamp without time zone NOT NULL,
						""timestamp-end-utc"" timestamp without time zone NOT NULL,
						""invoice-number"" character varying(255) NOT NULL,
						""currency"" character varying(255) NOT NULL,
						""amount-due"" numeric NOT NULL,
						""amount-paid"" numeric NOT NULL,
						""amount-remaining"" numeric NOT NULL,
						""timestamp-created-utc"" timestamp with time zone NOT NULL,
						""timestamp-due-utc"" timestamp with time zone NOT NULL,
						""company-id"" uuid NOT NULL,
						""json"" json DEFAULT '{}'::json NOT NULL,
						CONSTRAINT ""billing_invoices_pk"" PRIMARY KEY(""uuid"")
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
