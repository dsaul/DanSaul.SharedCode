using Npgsql;
using System.Text;
using System.Data;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Square;
using Square.Models;
using Serilog;
using Square.Exceptions;
using DanSaul.SharedCode.Npgsql;
using DanSaul.SharedCode.StandardizedEnvironmentVariables;

namespace SharedCode.DatabaseSchemas
{
	public record BillingCompanies(
		Guid? Uuid,
		string? FullName,
		string? Abbreviation,
		string? Industry,
		string? MarketingCampaign,
		string? AddressCity,
		string? AddressCountry,
		string? AddressLine1,
		string? AddressLine2,
		string? AddressPostalCode,
		string? AddressProvince,
		string? StripeCustomerId,
		string? PaymentMethod,
		Guid? InvoiceContactId,
		string? PaymentFrequency,
		string? Json)
	{
		public const string kJsonKeyPhoneId = "phoneId";
		public const string kJsonKeyIANATimezone = "IANATimezone";
		public const string kJsonKeyS3BucketName = "S3BucketName";
		public const string kJsonKeySquareCustomerId = "SquareCustomerId";
		public const string kJsonKeyCurrency = "Currency";
		public const string kJsonKeySquareCardBrand = "SquareCardBrand";
		public const string kJsonKeySquareCardExpMonth = "SquareCardExpMonth";
		public const string kJsonKeySquareCardExpYear = "SquareCardExpYear";
		public const string kJsonKeySquareCardLast4 = "SquareCardLast4";
		public const string kJsonKeySquareCardId = "SquareCardId";
		public const string kJsonKeySquareCardAuthorizationStatus = "SquareCardAuthorizationStatus";
		public const string kJsonKeySquareCardAuthorizationS3ServiceURL = "SquareCardAuthorizationS3ServiceURL";
		public const string kJsonKeySquareCardAuthorizationS3BucketName = "SquareCardAuthorizationS3BucketName";
		public const string kJsonKeySquareCardAuthorizationS3Key = "SquareCardAuthorizationS3Key";
		public const string kPaymentMethodValueInvoice = "Invoice";
		public const string kPaymentMethodValueSquarePreAuthorized = "Square Pre-Authorized";
		public const string kPaymentFrequenciesValueMonthly = BillingPaymentFrequencies.kValueMonthly;
		public const string kPaymentFrequenciesValueQuarterly = BillingPaymentFrequencies.kValueQuarterly;
		public const string kPaymentFrequenciesValueAnnually = BillingPaymentFrequencies.kValueAnnually;

		public const string kJsonValueIANATimezoneDefault = "America/Winnipeg";

		static public string GroupNameForCompanyId(Guid companyId) {
			return $"company-{companyId}";
		}



		public static Dictionary<Guid, BillingCompanies> ForIds(NpgsqlConnection connection, Guid id) {

			Dictionary<Guid, BillingCompanies> ret = new();

			string sql = @"SELECT * from ""billing-companies"" WHERE uuid = @uuid";
			using NpgsqlCommand cmd = new(sql, connection);
			cmd.Parameters.AddWithValue("@uuid", id);




			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					BillingCompanies obj = FromDataReader(reader);
					if (obj.Uuid == null) {
						continue;
					}
					ret.Add(obj.Uuid.Value, obj);
				}
			}

			return ret;
		}

		public static Dictionary<Guid, BillingCompanies> ForSessionId(NpgsqlConnection connection, Guid sessionId) {
			Dictionary<Guid, BillingCompanies> ret = new();

			string sql = @"
				SELECT ""billing-companies"".*
				FROM ""billing-sessions""
				LEFT JOIN ""billing-contacts"" ON ""billing-sessions"".""contact-id"" = ""billing-contacts"".""uuid""
				LEFT JOIN ""billing-companies"" ON ""billing-contacts"".""company-id"" = ""billing-companies"".""uuid""
				WHERE ""billing-sessions"".""uuid"" = @sessionId
				";
			using NpgsqlCommand cmd = new(sql, connection);
			cmd.Parameters.AddWithValue("@sessionId", sessionId);




			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					BillingCompanies obj = FromDataReader(reader);
					if (obj.Uuid == null) {
						continue;
					}
					ret.Add(obj.Uuid.Value, obj);
				}
			}

			return ret;
		}

		public static Dictionary<Guid, BillingCompanies> ForIds(NpgsqlConnection connection, IEnumerable<Guid> ids) {

			Guid[] idsArr = ids.ToArray();

			Dictionary<Guid, BillingCompanies> ret = new();
			if (idsArr.Length == 0)
				return ret;

			List<string> valNames = new();
			for (int i = 0; i < idsArr.Length; i++) {
				valNames.Add($"@val{i}");
			}

			string sql = $"SELECT * from \"billing-companies\" WHERE uuid IN ({string.Join(", ", valNames)})";
			using NpgsqlCommand cmd = new(sql, connection);
			for (int i = 0; i < valNames.Count; i++) {
				cmd.Parameters.AddWithValue(valNames[i], idsArr[i]);
			}

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					BillingCompanies obj = FromDataReader(reader);
					if (obj.Uuid == null) {
						continue;
					}
					ret.Add(obj.Uuid.Value, obj);
				}
			}

			return ret;
		}


		public static Dictionary<Guid, BillingCompanies> ForAbbreviation(NpgsqlConnection connection, string abbr) {

			abbr = abbr.Trim();

			Dictionary<Guid, BillingCompanies> ret = new();

			string sql = @"SELECT * FROM ""billing-companies"" WHERE LOWER(""abbreviation"") = LOWER(@abbr)";
			using NpgsqlCommand cmd = new(sql, connection);
			cmd.Parameters.AddWithValue("@abbr", abbr);




			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					BillingCompanies obj = FromDataReader(reader);
					if (obj.Uuid == null) {
						continue;
					}
					ret.Add(obj.Uuid.Value, obj);
				}
			}

			return ret;
		}

		public static Dictionary<Guid, BillingCompanies> ForPaymentMethod(NpgsqlConnection connection, string paymentMethod) {

			paymentMethod = paymentMethod.Trim();

			Dictionary<Guid, BillingCompanies> ret = new();

			string sql = @"SELECT * FROM ""billing-companies"" WHERE LOWER(""payment-method"") = LOWER(@paymentMethod)";
			using NpgsqlCommand cmd = new(sql, connection);
			cmd.Parameters.AddWithValue("@paymentMethod", paymentMethod);




			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					BillingCompanies obj = FromDataReader(reader);
					if (obj.Uuid == null) {
						continue;
					}
					ret.Add(obj.Uuid.Value, obj);
				}
			}

			return ret;
		}


		public static Dictionary<Guid, BillingCompanies> ForPhoneId(NpgsqlConnection connection, string phoneId) {

			Dictionary<Guid, BillingCompanies> ret = new();

			string sql = @"SELECT * from ""billing-companies"" WHERE json->>'phoneId' = @phoneId";
			using NpgsqlCommand cmd = new(sql, connection);
			cmd.Parameters.AddWithValue("@phoneId", phoneId);

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					BillingCompanies obj = FromDataReader(reader);
					if (obj.Uuid == null) {
						continue;
					}
					ret.Add(obj.Uuid.Value, obj);
				}
			}

			return ret;


		}


		public static Dictionary<Guid, BillingCompanies> All(NpgsqlConnection connection) {

			Dictionary<Guid, BillingCompanies> ret = new();

			string sql = @"SELECT * from ""billing-companies""";
			using NpgsqlCommand cmd = new(sql, connection);

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					BillingCompanies obj = FromDataReader(reader);
					if (obj.Uuid == null) {
						continue;
					}
					ret.Add(obj.Uuid.Value, obj);
				}
			}

			return ret;


		}

		public static Dictionary<Guid, BillingCompanies> AllForProvisionOnCallAutoAttendants(
			NpgsqlConnection connection, 
			bool flag, 
			out Dictionary<Guid, string> databaseNames) {

			Dictionary<Guid, BillingCompanies> ret = new();
			databaseNames = new Dictionary<Guid, string>();

			string sql = @"			
				SELECT 
				DISTINCT ON (""billing-companies"".""uuid"") ""billing-companies"".uuid, 
				""billing-companies"".""full-name"",
				""billing-companies"".""abbreviation"",
				""billing-companies"".""industry"",
				""billing-companies"".""marketing-campaign"",
				""billing-companies"".""address-city"",
				""billing-companies"".""address-country"",
				""billing-companies"".""address-line-1"",
				""billing-companies"".""address-line-2"",
				""billing-companies"".""address-postal-code"",
				""billing-companies"".""address-province"",
				""billing-companies"".""stripe-customer-id"",
				""billing-companies"".""payment-method"",
				""billing-companies"".""invoice-contact-id"",
				""billing-companies"".""payment-frequency"",
				""billing-companies"".""json"",
				""billing-subscriptions"".""provisioned-database-name"" as ""x-provisioned-database-name""
				FROM ""billing-companies""
				LEFT JOIN ""billing-subscriptions"" ON ""billing-subscriptions"".""company-id"" = ""billing-companies"".""uuid""
				LEFT JOIN ""billing-packages"" ON ""billing-subscriptions"".""package-id"" = ""billing-packages"".""uuid""
				WHERE (""billing-packages"".json ->> 'ProvisionOnCallAutoAttendants')::bool = @flag
			";
			using NpgsqlCommand cmd = new(sql, connection);
			cmd.Parameters.AddWithValue("@flag", flag);

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					BillingCompanies obj = FromDataReader(reader);
					if (obj.Uuid == null) {
						continue;
					}

					ret.Add(obj.Uuid.Value, obj);

					if (!reader.IsDBNull("x-provisioned-database-name")) {
						string dbn = reader.GetString("x-provisioned-database-name");
						databaseNames.Add(obj.Uuid.Value, dbn);

					}
				}
			}

			return ret;


		}




		public static List<Guid> Delete(NpgsqlConnection connection, List<Guid> idsToDelete) {

			List<Guid> toSendToOthers = new();
			if (idsToDelete.Count == 0) {
				return toSendToOthers;
			}

			List<string> valNames = new();
			for (int i = 0; i < idsToDelete.Count; i++) {
				valNames.Add($"@val{i}");
			}



			string sql = $"DELETE FROM \"billing-companies\" WHERE \"uuid\" IN ({string.Join(", ", valNames)})";
			using NpgsqlCommand cmd = new(sql, connection);
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

		public static void Upsert(NpgsqlConnection connection, Dictionary<Guid, BillingCompanies> updateObjects, out List<Guid> callerResponse, out Dictionary<Guid, BillingCompanies> toSendToOthers) {

			callerResponse = new List<Guid>();
			toSendToOthers = new Dictionary<Guid, BillingCompanies>();

			foreach (KeyValuePair<Guid, BillingCompanies> kvp in updateObjects) {

				string sql = @"
					INSERT INTO
						""billing-companies""
						(
							""uuid"",
							""full-name"",
							""abbreviation"",
							""industry"",
							""marketing-campaign"",
							""address-city"",
							""address-country"",
							""address-line-1"",
							""address-line-2"",
							""address-postal-code"",
							""address-province"",
							""stripe-customer-id"",
							""payment-method"",
							""invoice-contact-id"",
							""payment-frequency"",
							""json""
						)
					VALUES
						(
							@uuid,
							@fullName,
							@abbreviation,
							@industry,
							@marketingCampaign,
							@addressCity,
							@addressCountry,
							@addressLine1,
							@addressLine2,
							@addressPostalCode,
							@addressProvince,
							@stripeCustomerId,
							@paymentMethod,
							@invoiceContactId,
							@paymentFrequency,
							CAST(@json AS json)
						)
					ON CONFLICT (""uuid"") DO UPDATE
						SET
							""full-name"" = excluded.""full-name"",
							""abbreviation"" = excluded.""abbreviation"",
							""industry"" = excluded.""industry"",
							""marketing-campaign"" = excluded.""marketing-campaign"",
							""address-city"" = excluded.""address-city"",
							""address-country"" = excluded.""address-country"",
							""address-line-1"" = excluded.""address-line-1"",
							""address-line-2"" = excluded.""address-line-2"",
							""address-postal-code"" = excluded.""address-postal-code"",
							""address-province"" = excluded.""address-province"",
							""stripe-customer-id"" = excluded.""stripe-customer-id"",
							""payment-method"" = excluded.""payment-method"",
							""invoice-contact-id"" = excluded.""invoice-contact-id"",
							""payment-frequency"" = excluded.""payment-frequency"",
							""json"" = CAST(excluded.""json"" AS json)
					";

				using NpgsqlCommand cmd = new(sql, connection);
				cmd.Parameters.AddWithValue("@uuid", kvp.Key);
				cmd.Parameters.AddWithValue("@fullName", string.IsNullOrWhiteSpace(kvp.Value.FullName) ? (object)DBNull.Value : kvp.Value.FullName);
				cmd.Parameters.AddWithValue("@abbreviation", string.IsNullOrWhiteSpace(kvp.Value.Abbreviation) ? (object)DBNull.Value : kvp.Value.Abbreviation);
				cmd.Parameters.AddWithValue("@industry", string.IsNullOrWhiteSpace(kvp.Value.Industry) ? (object)DBNull.Value : kvp.Value.Industry);
				cmd.Parameters.AddWithValue("@marketingCampaign", string.IsNullOrWhiteSpace(kvp.Value.MarketingCampaign) ? (object)DBNull.Value : kvp.Value.MarketingCampaign);
				cmd.Parameters.AddWithValue("@addressCity", string.IsNullOrWhiteSpace(kvp.Value.AddressCity) ? (object)DBNull.Value : kvp.Value.AddressCity);
				cmd.Parameters.AddWithValue("@addressCountry", string.IsNullOrWhiteSpace(kvp.Value.AddressCountry) ? (object)DBNull.Value : kvp.Value.AddressCountry);
				cmd.Parameters.AddWithValue("@addressLine1", string.IsNullOrWhiteSpace(kvp.Value.AddressLine1) ? (object)DBNull.Value : kvp.Value.AddressLine1);
				cmd.Parameters.AddWithValue("@addressLine2", string.IsNullOrWhiteSpace(kvp.Value.AddressLine2) ? (object)DBNull.Value : kvp.Value.AddressLine2);
				cmd.Parameters.AddWithValue("@addressPostalCode", string.IsNullOrWhiteSpace(kvp.Value.AddressPostalCode) ? (object)DBNull.Value : kvp.Value.AddressPostalCode);
				cmd.Parameters.AddWithValue("@addressProvince", string.IsNullOrWhiteSpace(kvp.Value.AddressProvince) ? (object)DBNull.Value : kvp.Value.AddressProvince);
				cmd.Parameters.AddWithValue("@stripeCustomerId", string.IsNullOrWhiteSpace(kvp.Value.StripeCustomerId) ? (object)DBNull.Value : kvp.Value.StripeCustomerId);
				cmd.Parameters.AddWithValue("@paymentMethod", string.IsNullOrWhiteSpace(kvp.Value.PaymentMethod) ? (object)DBNull.Value : kvp.Value.PaymentMethod);
				cmd.Parameters.AddWithValue("@invoiceContactId", null == kvp.Value.InvoiceContactId ? (object)DBNull.Value : kvp.Value.InvoiceContactId);
				cmd.Parameters.AddWithValue("@paymentFrequency", string.IsNullOrWhiteSpace(kvp.Value.PaymentFrequency) ? (object)DBNull.Value : kvp.Value.PaymentFrequency);
				cmd.Parameters.AddWithValue("@json", string.IsNullOrWhiteSpace(kvp.Value.Json) ? (object)DBNull.Value : kvp.Value.Json);

				int rowsAffected = cmd.ExecuteNonQuery();

				if (rowsAffected == 0) {
					continue;
				}

				toSendToOthers.Add(kvp.Key, kvp.Value);
				callerResponse.Add(kvp.Key);


			}



		}

		public static BillingCompanies FromDataReader(NpgsqlDataReader reader) {

			Guid? uuid = default;
			string? fullName = default;
			string? abbreviation = default;
			string? industry = default;
			string? marketingCampaign = default;
			string? addressCity = default;
			string? addressCountry = default;
			string? addressLine1 = default;
			string? addressLine2 = default;
			string? addressPostalCode = default;
			string? addressProvince = default;
			string? stripeCustomerId = default;
			string? paymentMethod = default;
			Guid? invoiceContactId = default;
			string? paymentFrequency = default;
			string? json = default;


			if (!reader.IsDBNull("uuid")) {
				uuid = reader.GetGuid("uuid");
			}
			if (!reader.IsDBNull("full-name")) {
				fullName = reader.GetString("full-name");
			}
			if (!reader.IsDBNull("abbreviation")) {
				abbreviation = reader.GetString("abbreviation");
			}
			if (!reader.IsDBNull("industry")) {
				industry = reader.GetString("industry");
			}
			if (!reader.IsDBNull("marketing-campaign")) {
				marketingCampaign = reader.GetString("marketing-campaign");
			}
			if (!reader.IsDBNull("address-city")) {
				addressCity = reader.GetString("address-city");
			}
			if (!reader.IsDBNull("address-country")) {
				addressCountry = reader.GetString("address-country");
			}
			if (!reader.IsDBNull("address-line-1")) {
				addressLine1 = reader.GetString("address-line-1");
			}
			if (!reader.IsDBNull("address-line-2")) {
				addressLine2 = reader.GetString("address-line-2");
			}
			if (!reader.IsDBNull("address-postal-code")) {
				addressPostalCode = reader.GetString("address-postal-code");
			}
			if (!reader.IsDBNull("address-province")) {
				addressProvince = reader.GetString("address-province");
			}
			if (!reader.IsDBNull("stripe-customer-id")) {
				stripeCustomerId = reader.GetString("stripe-customer-id");
			}
			if (!reader.IsDBNull("payment-method")) {
				paymentMethod = reader.GetString("payment-method");
			}
			if (!reader.IsDBNull("invoice-contact-id")) {
				invoiceContactId = reader.GetGuid("invoice-contact-id");
			}
			if (!reader.IsDBNull("payment-frequency")) {
				paymentFrequency = reader.GetString("payment-frequency");
			}
			if (!reader.IsDBNull("json")) {
				json = reader.GetString("json");
			}

			return new BillingCompanies(
				Uuid: uuid,
				FullName: fullName,
				Abbreviation: abbreviation,
				Industry: industry,
				MarketingCampaign: marketingCampaign,
				AddressCity: addressCity,
				AddressCountry: addressCountry,
				AddressLine1: addressLine1,
				AddressLine2: addressLine2,
				AddressPostalCode: addressPostalCode,
				AddressProvince: addressProvince,
				StripeCustomerId: stripeCustomerId,
				PaymentMethod: paymentMethod,
				InvoiceContactId: invoiceContactId,
				PaymentFrequency: paymentFrequency,
				Json: json
				);
		}


		[JsonIgnore]
		public JObject? JsonObject
		{
			get {
				if (Json == null) {
					return null;
				}
				return JsonConvert.DeserializeObject(Json, new JsonSerializerSettings() { DateParseHandling = DateParseHandling.None }) as JObject;
			}
		}


		

		[JsonIgnore]
		public string? IANATimezone
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyIANATimezone];
				if (null == tok) {
					return null;
				}

				if (tok.Type == JTokenType.Null) {
					return null;
				}

				string? str = tok.Value<string>();
				if (string.IsNullOrWhiteSpace(str)) {
					return null;
				}

				return str;
			}
			init {
				JObject? root = JsonObject;
				if (null == root) {
					root = new JObject();
				}
				root[kJsonKeyIANATimezone] = value;
				Json = root.ToString(Formatting.Indented);
			}
		}

		[JsonIgnore]
		public string? PhoneId
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyPhoneId];
				if (null == tok) {
					return null;
				}

				if (tok.Type == JTokenType.Null) {
					return null;
				}

				string? str = tok.Value<string>();
				if (string.IsNullOrWhiteSpace(str)) {
					return null;
				}

				return str;
			}
			init {
				JObject? root = JsonObject;
				if (null == root) {
					root = new JObject();
				}
				root[kJsonKeyPhoneId] = value;
				Json = root.ToString(Formatting.Indented);
			}
		}

		[JsonIgnore]
		public string? S3BucketName
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyS3BucketName];
				if (null == tok) {
					return null;
				}

				if (tok.Type == JTokenType.Null) {
					return null;
				}

				string? str = tok.Value<string>();
				if (string.IsNullOrWhiteSpace(str)) {
					return null;
				}

				return str;
			}
		}


		[JsonIgnore]
		public string? SquareCustomerId
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeySquareCustomerId];
				if (null == tok) {
					return null;
				}

				if (tok.Type == JTokenType.Null) {
					return null;
				}

				string? str = tok.Value<string>();
				if (string.IsNullOrWhiteSpace(str)) {
					return null;
				}

				return str;
			}
		}

		[JsonIgnore]
		public string? Currency
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyCurrency];
				if (null == tok) {
					return null;
				}

				if (tok.Type == JTokenType.Null) {
					return null;
				}

				string? str = tok.Value<string>();
				if (string.IsNullOrWhiteSpace(str)) {
					return null;
				}

				return str;
			}
		}

		[JsonIgnore]
		public string? SquareCardBrand
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeySquareCardBrand];
				if (null == tok) {
					return null;
				}

				if (tok.Type == JTokenType.Null) {
					return null;
				}

				string? str = tok.Value<string>();
				if (string.IsNullOrWhiteSpace(str)) {
					return null;
				}

				return str;
			}
		}


		[JsonIgnore]
		public int? SquareCardExpMonth
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeySquareCardExpMonth];
				if (null == tok) {
					return null;
				}

				if (tok.Type == JTokenType.Null) {
					return null;
				}


				return tok.Value<int>();
			}
		}

		[JsonIgnore]
		public int? SquareCardExpYear
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeySquareCardExpYear];
				if (null == tok) {
					return null;
				}

				if (tok.Type == JTokenType.Null) {
					return null;
				}


				return tok.Value<int>();
			}
		}

		[JsonIgnore]
		public string? SquareCardLast4
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeySquareCardLast4];
				if (null == tok) {
					return null;
				}

				if (tok.Type == JTokenType.Null) {
					return null;
				}

				string? str = tok.Value<string>();
				if (string.IsNullOrWhiteSpace(str)) {
					return null;
				}

				return str;
			}
		}

		[JsonIgnore]
		public string? SquareCardId
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeySquareCardId];
				if (null == tok) {
					return null;
				}

				if (tok.Type == JTokenType.Null) {
					return null;
				}

				string? str = tok.Value<string>();
				if (string.IsNullOrWhiteSpace(str)) {
					return null;
				}

				return str;
			}
		}

		public static async Task<BillingCompanies?> EnsureCompanyHasSquareAccount(SquareClient squareClient, BillingCompanies company) {

			BillingCompanies? res = company;

			if (null == company)
				throw new ArgumentNullException(nameof(company));
			if (null == company.JsonObject)
				throw new InvalidOperationException("null == company.JsonObject");
			if (null == company.Uuid)
				throw new InvalidOperationException("null == company.Uuid");

			string? squareCompanyId = company.SquareCustomerId;

			var customersApi = squareClient.CustomersApi;

			if (string.IsNullOrWhiteSpace(squareCompanyId)) {

				res = await CreateNewSquareCustomerForCompany(squareClient, company);

			} else {

				try {
					RetrieveCustomerResponse retrieveCustomerResponse = await customersApi.RetrieveCustomerAsync(squareCompanyId);
					if (null != retrieveCustomerResponse.Errors) {
						Log.Error("RetrieveCustomerResponse null != res.Errors {Errors}", retrieveCustomerResponse.Errors);
						throw new Exception("RetrieveCustomerResponse null != res.Errors");
					}

					Customer customer = retrieveCustomerResponse.Customer;

					Log.Information("Found Square Customer {Customer}", customer);


				}
				catch (ApiException e) {
					var errors = e.Errors;
					var statusCode = e.ResponseCode;
					var httpContext = e.HttpContext;

					if (statusCode == 404) {
						res = await CreateNewSquareCustomerForCompany(squareClient, company);
					} else {
						Log.Error(e, "Unhandled ApiException Exception while creating square customer. {Errors} {StatusCode} {HttpContext}", errors, statusCode, httpContext);
					}

				}
				catch (Exception e) {
					Log.Error(e, "Generic Exception while creating square customer.");
				}




			}



			return res;
		}


		public static async Task<BillingCompanies?> CreateNewSquareCustomerForCompany(SquareClient squareClient, BillingCompanies company) {

			BillingCompanies? res = company;


			if (null == company)
				throw new ArgumentNullException(nameof(company));
			if (null == company.JsonObject)
				throw new InvalidOperationException("null == company.JsonObject");
			if (null == company.Uuid)
				throw new InvalidOperationException("null == company.Uuid");


			// Get Billing Contact
			Guid? invoiceContactId = company.InvoiceContactId;
			if (null == invoiceContactId) {
				Log.Warning("No invoice contact id for {CompanyId} skipping.", company.Uuid);
				return null;
			}

			using NpgsqlConnection billingDB = new(EnvDatabases.KBillingDatabaseConnectionString);
			billingDB.Open();

			var resBC = BillingContacts.ForId(billingDB, invoiceContactId.Value);
			if (0 == resBC.Count) {
				Log.Warning("No invoice contact id for {CompanyId} skipping.", company.Uuid);
				return null;
			}

			BillingContacts billingContact = resBC.FirstOrDefault().Value;




			var customersApi = squareClient.CustomersApi;

			Square.Models.Address address = new Square.Models.Address.Builder()
					.AddressLine1(company.AddressLine1)
					.AddressLine2(company.AddressLine2)
					.Locality(company.AddressCity)
					//.Country(company.AddressCountry)
					.PostalCode(company.AddressPostalCode)
					.AdministrativeDistrictLevel1(company.AddressProvince)
					.Build();

			// Create a unique key(idempotency) for this creation operation so you don't accidentally
			// create the customer multiple times if you need to retry this operation.
			// For the purpose of example, we mark it as `unique_idempotency_key`
			CreateCustomerRequest createCustomerRequest = new CreateCustomerRequest.Builder()
					.IdempotencyKey(Guid.NewGuid().ToString())
					.EmailAddress(billingContact.Email)
					.Nickname(billingContact.FullName)
					.PhoneNumber(billingContact.Phone)
					.CompanyName(company.FullName)
					.Address(address)
					.Build();

			// Call createCustomer method to create a new customer in this Square account
			try {
				CreateCustomerResponse createCustomerResponse = await customersApi.CreateCustomerAsync(createCustomerRequest);
				if (null != createCustomerResponse.Errors) {
					Log.Error("CreateCustomerResponse null != createCustomerResponse.Errors {Errors}", createCustomerResponse.Errors);
					throw new Exception("CreateCustomerResponse null != createCustomerResponse.Errors");
				}


				Customer stripeCx = createCustomerResponse.Customer;
				string stripeCxId = stripeCx.Id;

				if (company.JsonObject.DeepClone() is not JObject copy)
					throw new InvalidOperationException("null == copy");

				copy[BillingCompanies.kJsonKeySquareCustomerId] = stripeCxId;

				BillingCompanies mod = company with
				{
					Json = copy.ToString(Formatting.Indented)
				};

				res = mod;

				BillingCompanies.Upsert(billingDB, new Dictionary<Guid, BillingCompanies> {
						{ company.Uuid.Value, mod }
					}, out _, out _);

			}
			catch (ApiException e) {
				var errors = e.Errors;
				var statusCode = e.ResponseCode;
				var httpContext = e.HttpContext;

				Log.Error(e, "ApiException Exception while creating square customer. {Errors} {StatusCode} {HttpContext}", errors, statusCode, httpContext);
			}
			catch (Exception e) {
				Log.Error(e, "Generic Exception while creating square customer.");
			}

			return res;
		}

		public static readonly Guid CECompanyId = Guid.Parse("20FCAAC7-FB81-4B68-9752-61444AE0531F");
		public static void VerifyRepairTable(NpgsqlConnection db, out Guid? companyId, bool insertDefaultContents = false) {

			if (db.TableExists("billing-companies")) {
				Log.Debug($"----- Table \"billing-companies\" exists.");
			} else {
				Log.Information($"----- Table \"billing-companies\" doesn't exist, creating.");

				using NpgsqlCommand cmd = new(@"
					CREATE TABLE ""public"".""billing-companies"" (
						""uuid"" uuid DEFAULT public.uuid_generate_v1() NOT NULL,
						""full-name"" character varying(255),
						""abbreviation"" character varying(255),
						""industry"" character varying(255),
						""marketing-campaign"" character varying(255),
						""address-city"" character varying(255),
						""address-country"" character varying(255),
						""address-line-1"" character varying(255),
						""address-line-2"" character varying(255),
						""address-postal-code"" character varying(255),
						""address-province"" character varying(255),
						""stripe-customer-id"" character varying(255),
						""payment-method"" character varying(255),
						""invoice-contact-id"" uuid,
						""is-deleted"" boolean DEFAULT false NOT NULL,
						""payment-frequency"" character varying(255),
						""json"" json DEFAULT '{}'::json NOT NULL,
						CONSTRAINT ""billing_companies_pk"" PRIMARY KEY(""uuid"")
					) WITH(oids = false);
					", db);
				cmd.ExecuteNonQuery();
			}


			if (insertDefaultContents) {
				Log.Information("Insert Default Contents");
				BillingCompanies bc = new(
					Uuid: CECompanyId,
					FullName: "Community Edition",
					Abbreviation: "CE",
					Industry: null,
					MarketingCampaign: null,
					AddressCity: null,
					AddressCountry: null,
					AddressLine1: null,
					AddressLine2: null,
					AddressPostalCode: null,
					AddressProvince: null,
					StripeCustomerId: null,
					PaymentMethod: null,
					InvoiceContactId: null,
					PaymentFrequency: null,
					Json: new JObject { }.ToString(Formatting.Indented)
					);

				Upsert(db, new Dictionary<Guid, BillingCompanies> {
					{CECompanyId, bc},
				}, out _, out _);

				companyId = CECompanyId;

			} else {
				companyId = null;
			}




			
		}
		
















	}
}
