using Npgsql;
using System.Text;
using System.Data;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Serilog;

namespace SharedCode.DatabaseSchemas
{
	public record BillingCouponCodes(
		Guid? Uuid,
		decimal? Discount,
		string? DisplayName,
		int? Months,
		string? CouponCode,
		bool? ForbidNewApplications,
		string? Json
		)
	{
		public static Dictionary<Guid, BillingCouponCodes> ForId(NpgsqlConnection connection, Guid id) {

			Dictionary<Guid, BillingCouponCodes> ret = new Dictionary<Guid, BillingCouponCodes>();

			string sql = @"SELECT * from ""billing-coupon-codes"" WHERE uuid = @uuid";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			cmd.Parameters.AddWithValue("@uuid", id);




			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					BillingCouponCodes obj = BillingCouponCodes.FromDataReader(reader);
					if (obj.Uuid == null) {
						continue;
					}
					ret.Add(obj.Uuid.Value, obj);
				}
			}

			return ret;
		}

		public static Dictionary<Guid, BillingCouponCodes> ForIds(NpgsqlConnection connection, List<Guid> ids) {

			Guid[] idsArr = ids.ToArray();

			Dictionary<Guid, BillingCouponCodes> ret = new Dictionary<Guid, BillingCouponCodes>();
			if (idsArr.Length == 0)
				return ret;

			List<string> valNames = new List<string>();
			for (int i = 0; i < idsArr.Length; i++) {
				valNames.Add($"@val{i}");
			}

			string sql = $"SELECT * from \"billing-coupon-codes\" WHERE uuid IN ({string.Join(", ", valNames)})";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			for (int i = 0; i < valNames.Count; i++) {
				cmd.Parameters.AddWithValue(valNames[i], idsArr[i]);
			}

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					BillingCouponCodes obj = BillingCouponCodes.FromDataReader(reader);
					if (obj.Uuid == null) {
						continue;
					}
					ret.Add(obj.Uuid.Value, obj);
				}
			}

			return ret;


		}


		public static Dictionary<Guid, BillingCouponCodes> All(NpgsqlConnection connection) {

			Dictionary<Guid, BillingCouponCodes> ret = new Dictionary<Guid, BillingCouponCodes>();

			string sql = @"SELECT * from ""billing-coupon-codes""";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					BillingCouponCodes obj = BillingCouponCodes.FromDataReader(reader);
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



			string sql = $"DELETE FROM \"billing-coupon-codes\" WHERE \"uuid\" IN ({string.Join(", ", valNames)})";
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


		public static void Upsert(NpgsqlConnection connection, Dictionary<Guid, BillingCouponCodes> updateObjects, out List<Guid> callerResponse, out Dictionary<Guid, BillingCouponCodes> toSendToOthers) {

			callerResponse = new List<Guid>();
			toSendToOthers = new Dictionary<Guid, BillingCouponCodes>();

			foreach (KeyValuePair<Guid, BillingCouponCodes> kvp in updateObjects) {

				string sql = @"
					INSERT INTO
						""billing-coupon-codes""
						(
							""uuid"",
							""discount"",
							""display-name"",
							""months"",
							""coupon-code"",
							""forbid-new-applications"",
							""json""
						)
					VALUES
						(
							@uuid,
							@discount,
							@displayName,
							@months,
							@couponCode,
							@forbidNewApplications,
							CAST(@json AS json)
						)
					ON CONFLICT (""uuid"") DO UPDATE
						SET
							""discount"" = excluded.""discount"",
							""display-name"" = excluded.""display-name"",
							""months"" = excluded.""months"",
							""coupon-code"" = excluded.""coupon-code"",
							""forbid-new-applications"" = excluded.""forbid-new-applications"",
							""json"" = CAST(excluded.""json"" AS json)
					";

				using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
				cmd.Parameters.AddWithValue("@uuid", kvp.Key);
				cmd.Parameters.AddWithValue("@discount", null == kvp.Value.Discount ? (object)DBNull.Value : kvp.Value.Discount);
				cmd.Parameters.AddWithValue("@displayName", string.IsNullOrWhiteSpace(kvp.Value.DisplayName) ? (object)DBNull.Value : kvp.Value.DisplayName);
				cmd.Parameters.AddWithValue("@months", null == kvp.Value.Months ? (object)DBNull.Value : kvp.Value.Months);
				cmd.Parameters.AddWithValue("@couponCode", string.IsNullOrWhiteSpace(kvp.Value.CouponCode) ? (object)DBNull.Value : kvp.Value.CouponCode);
				cmd.Parameters.AddWithValue("@forbidNewApplications", kvp.Value.ForbidNewApplications == null ? (object)DBNull.Value : kvp.Value.ForbidNewApplications);
				cmd.Parameters.AddWithValue("@json", string.IsNullOrWhiteSpace(kvp.Value.Json) ? (object)DBNull.Value : kvp.Value.Json);

				int rowsAffected = cmd.ExecuteNonQuery();

				if (rowsAffected == 0) {
					continue;
				}

				toSendToOthers.Add(kvp.Key, kvp.Value);
				callerResponse.Add(kvp.Key);


			}



		}

		public static BillingCouponCodes FromDataReader(NpgsqlDataReader reader) {
			Guid? uuid = default;
			decimal? discount = default;
			string? displayName = default;
			int? months = default;
			string? couponCode = default;
			bool? forbidNewApplications = default;
			string? json = default;

			if (!reader.IsDBNull("uuid")) {
				uuid = reader.GetGuid("uuid");
			}
			if (!reader.IsDBNull("discount")) {
				discount = reader.GetDecimal("discount");
			}
			if (!reader.IsDBNull("display-name")) {
				displayName = reader.GetString("display-name");
			}
			if (!reader.IsDBNull("months")) {
				months = reader.GetInt32("months");
			}
			if (!reader.IsDBNull("coupon-code")) {
				couponCode = reader.GetString("coupon-code");
			}
			if (!reader.IsDBNull("forbid-new-applications")) {
				forbidNewApplications = reader.GetBoolean("forbid-new-applications");
			}
			if (!reader.IsDBNull("json")) {
				json = reader.GetString("json");
			}

			return new BillingCouponCodes(
				Uuid: uuid,
				Discount: discount,
				DisplayName: displayName,
				Months: months,
				CouponCode: couponCode,
				ForbidNewApplications: forbidNewApplications,
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

			if (db.TableExists("billing-coupon-codes")) {
				Log.Debug($"----- Table \"billing-coupon-codes\" exists.");
			} else {
				Log.Information($"----- Table \"billing-coupon-codes\" doesn't exist, creating.");

				using NpgsqlCommand cmd = new NpgsqlCommand(@"
					CREATE TABLE ""public"".""billing-coupon-codes"" (
						""uuid"" uuid DEFAULT public.uuid_generate_v1() NOT NULL,
						""discount"" numeric,
						""display-name"" character varying(255),
						""months"" integer,
						""coupon-code"" character varying(255),
						""forbid-new-applications"" boolean DEFAULT false NOT NULL,
						""json"" json DEFAULT '{}'::json NOT NULL,
						CONSTRAINT ""billing_coupon_codes_pk"" PRIMARY KEY(""uuid"")
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
