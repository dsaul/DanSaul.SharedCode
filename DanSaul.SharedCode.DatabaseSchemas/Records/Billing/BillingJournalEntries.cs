using Npgsql;
using System.Text;
using System.Data;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Serilog;
using DanSaul.SharedCode.Npgsql;

namespace SharedCode.DatabaseSchemas
{
	public record BillingJournalEntries(
		Guid? Uuid,
		DateTime? Timestamp,
		string? Type,
		Guid? OtherEntryId,
		string? Description,
		decimal? Quantity,
		decimal? UnitPrice,
		string? Currency,
		decimal? TaxPercentage,
		decimal? TaxActual,
		decimal? Total,
		Guid? InvoiceId,
		Guid? PackageId,
		Guid? CompanyId,
		string? Json
		)
	{
		public const string kTypeValueBasic = "Basic";
		public const string kTypeValueReversePreviousEntry = "Reverse Previous Entry";
		public const string kTypeValuePromotionalDiscount = "Promotional Discount";
		public const string kTypeValueAccountCredit = "Account Credit";
		public const string kTypeValueWorkPerformed = "Work Performed";
		public const string kTypeValueChangeAdjustment = "Change Adjustment";
		public const string kTypeValuePayment = "Payment";

		public const string kJsonKeySquareCardBrand = "SquareCardBrand";
		public const string kJsonKeySquareCardType = "SquareCardType";
		public const string kJsonKeySquareCardholderName = "SquareCardholderName";
		public const string kJsonKeySquareExpMonth = "SquareExpMonth";
		public const string kJsonKeySquareExpYear = "SquareExpYear";
		public const string kJsonKeySquareFingerprint = "SquareFingerprint";
		public const string kJsonKeySquareLast4 = "SquareLast4";
		public const string kJsonKeySquareCardPaymentTimelineAuthorizedAt = "SquareCardPaymentTimelineAuthorizedAt";
		public const string kJsonKeySquareCardPaymentTimelineCapturedAt = "SquareCardPaymentTimelineCapturedAt";
		public const string kJsonKeySquareRecieptUrl = "SquareRecieptUrl";
		public const string kJsonKeySquarePaymentId = "SquarePaymentId";
		public const string kJsonKeySquareStatus = "SquareStatus";
		public const string kJsonKeyCreditCardFeeCredit = "CreditCardFeeCredit";

		public static Dictionary<Guid, BillingJournalEntries> ForId(NpgsqlConnection connection, Guid id) {

			Dictionary<Guid, BillingJournalEntries> ret = new Dictionary<Guid, BillingJournalEntries>();

			string sql = @"SELECT * from ""billing-journal-entries"" WHERE uuid = @uuid";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			cmd.Parameters.AddWithValue("@uuid", id);




			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					BillingJournalEntries obj = BillingJournalEntries.FromDataReader(reader);
					if (obj.Uuid == null) {
						continue;
					}
					ret.Add(obj.Uuid.Value, obj);
				}
			}

			return ret;
		}

		public static Dictionary<Guid, BillingJournalEntries> ForIds(NpgsqlConnection connection, IEnumerable<Guid> ids) {

			Guid[] idsArr = ids.ToArray();

			Dictionary<Guid, BillingJournalEntries> ret = new Dictionary<Guid, BillingJournalEntries>();
			if (idsArr.Length == 0)
				return ret;

			List<string> valNames = new List<string>();
			for (int i = 0; i < idsArr.Length; i++) {
				valNames.Add($"@val{i}");
			}

			string sql = $"SELECT * from \"billing-journal-entries\" WHERE uuid IN ({string.Join(", ", valNames)})";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			for (int i = 0; i < valNames.Count; i++) {
				cmd.Parameters.AddWithValue(valNames[i], idsArr[i]);
			}

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					BillingJournalEntries obj = BillingJournalEntries.FromDataReader(reader);
					if (obj.Uuid == null) {
						continue;
					}
					ret.Add(obj.Uuid.Value, obj);
				}
			}

			return ret;


		}


		public static Dictionary<Guid, BillingJournalEntries> ForCompanyId(NpgsqlConnection connection, Guid companyId) {

			Dictionary<Guid, BillingJournalEntries> ret = new Dictionary<Guid, BillingJournalEntries>();

			string sql = @"SELECT * from ""billing-journal-entries"" WHERE ""company-id"" = @uuid";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			cmd.Parameters.AddWithValue("@uuid", companyId);




			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					BillingJournalEntries obj = BillingJournalEntries.FromDataReader(reader);
					if (obj.Uuid == null) {
						continue;
					}
					ret.Add(obj.Uuid.Value, obj);
				}
			}

			return ret;
		}


		public static Dictionary<Guid, BillingJournalEntries> All(NpgsqlConnection connection) {

			Dictionary<Guid, BillingJournalEntries> ret = new Dictionary<Guid, BillingJournalEntries>();

			string sql = @"SELECT * from ""billing-journal-entries""";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					BillingJournalEntries obj = BillingJournalEntries.FromDataReader(reader);
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



			string sql = $"DELETE FROM \"billing-journal-entries\" WHERE \"uuid\" IN ({string.Join(", ", valNames)})";
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



			string sql = $"DELETE FROM \"billing-journal-entries\" WHERE \"company-id\" IN ({string.Join(", ", valNames)})";
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


		public static void Upsert(NpgsqlConnection connection, Dictionary<Guid, BillingJournalEntries> updateObjects, out List<Guid> callerResponse, out Dictionary<Guid, BillingJournalEntries> toSendToOthers) {

			callerResponse = new List<Guid>();
			toSendToOthers = new Dictionary<Guid, BillingJournalEntries>();

			foreach (KeyValuePair<Guid, BillingJournalEntries> kvp in updateObjects) {

				string sql = @"
					INSERT INTO
						""billing-journal-entries""
						(
							""uuid"",
							""timestamp"",
							""type"",
							""other-entry-id"",
							""description"",
							""quantity"",
							""unit-price"",
							""currency"",
							""tax-percentage"",
							""tax-actual"",
							""total"",
							""invoice-id"",
							""package-id"",
							""company-id"",
							""json""
						)
					VALUES
						(
							@uuid,
							@timestamp,
							@type,
							@otherEntryId,
							@description,
							@quantity,
							@unitPrice,
							@currency,
							@taxPercentage,
							@taxActual,
							@total,
							@invoiceId,
							@packageId,
							@companyId,
							CAST(@json AS json)
						)
					ON CONFLICT (""uuid"") DO UPDATE
						SET
							""timestamp"" = excluded.""timestamp"",
							""type"" = excluded.""type"",
							""other-entry-id"" = excluded.""other-entry-id"",
							""description"" = excluded.""description"",
							""quantity"" = excluded.""quantity"",
							""unit-price"" = excluded.""unit-price"",
							""currency"" = excluded.""currency"",
							""tax-percentage"" = excluded.""tax-percentage"",
							""tax-actual"" = excluded.""tax-actual"",
							""total"" = excluded.""total"",
							""invoice-id"" = excluded.""invoice-id"",
							""package-id"" = excluded.""package-id"",
							""company-id"" = excluded.""company-id"",
							""json"" = CAST(excluded.""json"" AS json)
					";

				using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
				cmd.Parameters.AddWithValue("@uuid", kvp.Key);
				cmd.Parameters.AddWithValue("@timestamp", null == kvp.Value.Timestamp ? (object)DBNull.Value : kvp.Value.Timestamp);
				cmd.Parameters.AddWithValue("@type", string.IsNullOrWhiteSpace(kvp.Value.Type) ? (object)DBNull.Value : kvp.Value.Type);
				cmd.Parameters.AddWithValue("@otherEntryId", null == kvp.Value.OtherEntryId ? (object)DBNull.Value : kvp.Value.OtherEntryId);
				cmd.Parameters.AddWithValue("@description", string.IsNullOrWhiteSpace(kvp.Value.Description) ? (object)DBNull.Value : kvp.Value.Description);
				cmd.Parameters.AddWithValue("@quantity", null == kvp.Value.Quantity ? (object)DBNull.Value : kvp.Value.Quantity);
				cmd.Parameters.AddWithValue("@unitPrice", null == kvp.Value.UnitPrice ? (object)DBNull.Value : kvp.Value.UnitPrice);
				cmd.Parameters.AddWithValue("@currency", string.IsNullOrWhiteSpace(kvp.Value.Currency) ? (object)DBNull.Value : kvp.Value.Currency);
				cmd.Parameters.AddWithValue("@taxPercentage", null == kvp.Value.TaxPercentage ? (object)DBNull.Value : kvp.Value.TaxPercentage);
				cmd.Parameters.AddWithValue("@taxActual", null == kvp.Value.TaxActual ? (object)DBNull.Value : kvp.Value.TaxActual);
				cmd.Parameters.AddWithValue("@total", null == kvp.Value.Total ? (object)DBNull.Value : kvp.Value.Total);
				cmd.Parameters.AddWithValue("@invoiceId", null == kvp.Value.InvoiceId ? (object)DBNull.Value : kvp.Value.InvoiceId);
				cmd.Parameters.AddWithValue("@packageId", null == kvp.Value.PackageId ? (object)DBNull.Value : kvp.Value.PackageId);
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










		public static BillingJournalEntries FromDataReader(NpgsqlDataReader reader) {

			Guid? uuid = default;
			DateTime? timestamp = default;
			string? type = default;
			Guid? otherEntryId = default;
			string? description = default;
			decimal? quantity = default;
			decimal? unitPrice = default;
			string? currency = default;
			decimal? taxPercentage = default;
			decimal? taxActual = default;
			decimal? total = default;
			Guid? invoiceId = default;
			Guid? packageId = default;
			Guid? companyId = default;
			string? json = default;


			Dictionary<string, int> columns = new Dictionary<string, int>();
			for (int i = 0; i < reader.FieldCount; i++) {
				columns.Add(reader.GetName(i), i);
			}


			if (!reader.IsDBNull("uuid")) {
				uuid = reader.GetGuid("uuid");
			}
			if (!reader.IsDBNull("timestamp")) {
				timestamp = reader.GetDateTime(columns["timestamp"]);
			}
			if (!reader.IsDBNull("type")) {
				type = reader.GetString("type");
			}
			if (!reader.IsDBNull("other-entry-id")) {
				otherEntryId = reader.GetGuid("other-entry-id");
			}
			if (!reader.IsDBNull("description")) {
				description = reader.GetString("description");
			}
			if (!reader.IsDBNull("quantity")) {
				quantity = reader.GetDecimal("quantity");
			}
			if (!reader.IsDBNull("unit-price")) {
				unitPrice = reader.GetDecimal("unit-price");
			}
			if (!reader.IsDBNull("currency")) {
				currency = reader.GetString("currency");
			}
			if (!reader.IsDBNull("tax-percentage")) {
				taxPercentage = reader.GetDecimal("tax-percentage");
			}
			if (!reader.IsDBNull("tax-actual")) {
				taxActual = reader.GetDecimal("tax-actual");
			}
			if (!reader.IsDBNull("total")) {
				total = reader.GetDecimal("total");
			}
			if (!reader.IsDBNull("invoice-id")) {
				invoiceId = reader.GetGuid("invoice-id");
			}
			if (!reader.IsDBNull("package-id")) {
				packageId = reader.GetGuid("package-id");
			}
			if (!reader.IsDBNull("company-id")) {
				companyId = reader.GetGuid("company-id");
			}
			if (!reader.IsDBNull("json")) {
				json = reader.GetString("json");
			}

			return new BillingJournalEntries(
				Uuid: uuid,
				Timestamp: timestamp,
				Type: type,
				OtherEntryId: otherEntryId,
				Description: description,
				Quantity: quantity,
				UnitPrice: unitPrice,
				Currency: currency,
				TaxPercentage: taxPercentage,
				TaxActual: taxActual,
				Total: total,
				InvoiceId: invoiceId,
				PackageId: packageId,
				CompanyId: companyId,
				Json: json
				);

		}

		[JsonIgnore]
		public JObject? JsonObject
		{
			get {
				if (null == Json)
					return null;
				return JsonConvert.DeserializeObject(Json, new JsonSerializerSettings() { DateParseHandling = DateParseHandling.None }) as JObject;
			}
		}

		public static void VerifyRepairTable(NpgsqlConnection db, bool insertDefaultContents = false) {

			if (db.TableExists("billing-journal-entries")) {
				Log.Debug($"----- Table \"billing-journal-entries\" exists.");
			} else {
				Log.Information($"----- Table \"billing-journal-entries\" doesn't exist, creating.");

				using NpgsqlCommand cmd = new NpgsqlCommand(@"
					CREATE TABLE ""public"".""billing-journal-entries"" (
						""uuid"" uuid DEFAULT public.uuid_generate_v1() NOT NULL,
						""timestamp"" timestamp without time zone DEFAULT now() NOT NULL,
						""type"" character varying NOT NULL,
						""other-entry-id"" uuid,
						""description"" character varying(255),
						""quantity"" numeric NOT NULL,
						""unit-price"" numeric NOT NULL,
						""currency"" character varying(255) NOT NULL,
						""tax-percentage"" numeric NOT NULL,
						""tax-actual"" numeric NOT NULL,
						""total"" numeric NOT NULL,
						""invoice-id"" uuid,
						""package-id"" uuid,
						""company-id"" uuid NOT NULL,
						json json DEFAULT '{}'::json NOT NULL,
						CONSTRAINT ""billing_journal_entries_pk"" PRIMARY KEY(""uuid"")
					) WITH(oids = false);
					", db);
				cmd.ExecuteNonQuery();
			}


			if (insertDefaultContents) {
				// none
			}





		}






	}
}
