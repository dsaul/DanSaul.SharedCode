using Npgsql;
using System.Text;
using System.Data;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Serilog;

namespace SharedCode.DatabaseSchemas
{
	public record BillingContacts(
		string? FullName,
		string? Email,
		string? PasswordHash,
		bool? EmailListMarketing,
		bool? EmailListTutorials,
		string? MarketingCampaign,
		string? Phone,
		Guid? Uuid,
		Guid? CompanyId,
		string? ApplicationData,
		string? Json)
	{

		public const string kApplicationDataKeyDispatchPulseAgentId = "dispatchPulseAgentId";
		public const string kApplicationDataKeyDispatchPulseContactId = "dispatchPulseContactId";
		public const string kJsonKeyLicenseAssignedProjectsSchedulingTime = "licenseAssignedProjectsSchedulingTime";
		public const string kJsonKeyLicenseAssignedOnCall = "licenseAssignedOnCall";

		static public string GroupNameForContactId(Guid userId) {
			return $"contact-{userId}";
		}

		static public string CompanyGroupNameForBillingContact(BillingContacts contact) {
			if (null == contact)
				throw new ArgumentNullException(nameof(contact));
			if (null == contact.CompanyId)
				throw new ArgumentNullException("contact.CompanyId");
			return BillingCompanies.GroupNameForCompanyId(contact.CompanyId.Value);
		}

		static public string UserGroupNameForBillingContact(BillingContacts contact) {
			if (null == contact)
				throw new ArgumentNullException(nameof(contact));
			if (null == contact.Uuid)
				throw new ArgumentNullException("contact.Uuid");
			return GroupNameForContactId(contact.Uuid.Value);
		}

		public static Dictionary<Guid, BillingContacts> ForEMailAndAbbreviation(NpgsqlConnection connection, string email, string abbreviation) {

			Dictionary<Guid, BillingContacts> ret = new Dictionary<Guid, BillingContacts>();


			string sql = @"
				SELECT 
					""billing-contacts"".""full-name"" as ""full-name"",
					""billing-contacts"".""email"" as ""email"",
					""billing-contacts"".""password-hash"" as ""password-hash"",
					""billing-contacts"".""email-list-marketing"" as ""email-list-marketing"",
					""billing-contacts"".""email-list-tutorials"" as ""email-list-tutorials"",
					""billing-contacts"".""marketing-campaign"" as ""marketing-campaign"",
					""billing-contacts"".""phone"" as ""phone"",
					""billing-contacts"".""uuid"" as ""uuid"",
					""billing-contacts"".""company-id"" as ""company-id"",
					""billing-contacts"".""application-data"" as ""application-data"",
					""billing-contacts"".""json"" as ""json""
				FROM 
					""billing-contacts""
				LEFT JOIN 
					""billing-companies""
				ON 
					""billing-contacts"".""company-id"" = ""billing-companies"".""uuid""
				WHERE
					LOWER(""billing-contacts"".""email"") = LOWER(@email)
					AND
					LOWER(""billing-companies"".""abbreviation"") = LOWER(@abbreviation)
				";


			
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			cmd.Parameters.AddWithValue("@email", email);
			cmd.Parameters.AddWithValue("@abbreviation", abbreviation);



			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					BillingContacts obj = BillingContacts.FromDataReader(reader);
					if (obj.Uuid == null) {
						continue;
					}
					ret.Add(obj.Uuid.Value, obj);
				}
			}

			return ret;
		}


		public static Dictionary<Guid, BillingContacts> ForId(NpgsqlConnection connection, Guid id) {

			Dictionary<Guid, BillingContacts> ret = new Dictionary<Guid, BillingContacts>();

			string sql = @"SELECT * from ""billing-contacts"" WHERE uuid = @uuid";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			cmd.Parameters.AddWithValue("@uuid", id);




			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					BillingContacts obj = BillingContacts.FromDataReader(reader);
					if (obj.Uuid == null) {
						continue;
					}
					ret.Add(obj.Uuid.Value, obj);
				}
			}

			return ret;
		}

		public static Dictionary<Guid, BillingContacts> ForIds(NpgsqlConnection connection, IEnumerable<Guid> ids) {

			Guid[] idsArr = ids.ToArray();

			Dictionary<Guid, BillingContacts> ret = new Dictionary<Guid, BillingContacts>();
			if (idsArr.Length == 0)
				return ret;

			List<string> valNames = new List<string>();
			for (int i = 0; i < idsArr.Length; i++) {
				valNames.Add($"@val{i}");
			}

			string sql = $"SELECT * from \"billing-contacts\" WHERE uuid IN ({string.Join(", ", valNames)})";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			for (int i = 0; i < valNames.Count; i++) {
				cmd.Parameters.AddWithValue(valNames[i], idsArr[i]);
			}

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					BillingContacts obj = BillingContacts.FromDataReader(reader);
					if (obj.Uuid == null) {
						continue;
					}
					ret.Add(obj.Uuid.Value, obj);
				}
			}

			return ret;


		}

		public static Dictionary<Guid, BillingContacts> ForCompany(NpgsqlConnection connection, Guid companyId) {

			Dictionary<Guid, BillingContacts> ret = new Dictionary<Guid, BillingContacts>();

			string sql = @"SELECT * from ""billing-contacts"" WHERE ""company-id"" = @uuid";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			cmd.Parameters.AddWithValue("@uuid", companyId);




			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					BillingContacts obj = BillingContacts.FromDataReader(reader);
					if (obj.Uuid == null) {
						continue;
					}
					ret.Add(obj.Uuid.Value, obj);
				}
			}

			return ret;
		}

		public static Dictionary<Guid, BillingContacts> All(NpgsqlConnection connection) {

			Dictionary<Guid, BillingContacts> ret = new Dictionary<Guid, BillingContacts>();

			string sql = @"SELECT * from ""billing-contacts""";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					BillingContacts obj = BillingContacts.FromDataReader(reader);
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

			// Delete billing permissions group memberships.
			{
				string sql = $"DELETE FROM \"billing-permissions-groups-memberships\" WHERE \"contact-id\" IN ({string.Join(", ", valNames)})";
				using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
				for (int i = 0; i < valNames.Count; i++) {
					cmd.Parameters.AddWithValue(valNames[i], idsToDelete[i]);
				}
				cmd.ExecuteNonQuery();
			}

			// Delete individual permissions.
			{
				string sql = $"DELETE FROM \"billing-permissions-bool\" WHERE \"contact-id\" IN ({string.Join(", ", valNames)})";
				using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
				for (int i = 0; i < valNames.Count; i++) {
					cmd.Parameters.AddWithValue(valNames[i], idsToDelete[i]);
				}
				cmd.ExecuteNonQuery();
			}


			// Delete sessions for the contact.
			{
				string sql = $"DELETE FROM \"billing-sessions\" WHERE \"contact-id\" IN ({string.Join(", ", valNames)})";
				using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
				for (int i = 0; i < valNames.Count; i++) {
					cmd.Parameters.AddWithValue(valNames[i], idsToDelete[i]);
				}

				cmd.ExecuteNonQuery();
			}


			// Delete actual billing contacts.
			{
				string sql = $"DELETE FROM \"billing-contacts\" WHERE \"uuid\" IN ({string.Join(", ", valNames)})";
				using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
				for (int i = 0; i < valNames.Count; i++) {
					cmd.Parameters.AddWithValue(valNames[i], idsToDelete[i]);
				}

				int rowsAffected = cmd.ExecuteNonQuery();
				if (rowsAffected == 0) {
					return toSendToOthers;
				}
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



			string sql = $"DELETE FROM \"billing-contacts\" WHERE \"company-id\" IN ({string.Join(", ", valNames)})";
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
			Dictionary<Guid, BillingContacts> updateObjects,
			out List<Guid> callerResponse,
			out Dictionary<Guid, BillingContacts> toSendToOthers
			) {

			callerResponse = new List<Guid>();
			toSendToOthers = new Dictionary<Guid, BillingContacts>();

			foreach (KeyValuePair<Guid, BillingContacts> kvp in updateObjects) {




				StringBuilder sb = new StringBuilder();
				sb.Append(@"
					INSERT INTO
						""billing-contacts""
						(
							""uuid"",
							""full-name"",
							""email"",
				");

				if (!string.IsNullOrWhiteSpace(kvp.Value.PasswordHash)) {
					sb.Append(@"
							""password-hash"",
					");
				}
				sb.Append(@"

							""email-list-marketing"",
							""email-list-tutorials"",
							""marketing-campaign"",
							""phone"",
							""company-id"",
							""application-data"",
							""json""
						)
					VALUES
						(
							@uuid,
							@fullName,
							@email,

				");
				if (!string.IsNullOrWhiteSpace(kvp.Value.PasswordHash)) {
					sb.Append(@"
							@passwordHash,
					");
				}
				sb.Append(@"

							@emailListMarketing,
							@emailListTutorials,
							@marketingCampaign,
							@phone,
							@companyId,
							CAST(@applicationData AS json),
							CAST(@json AS json)
						)
					ON CONFLICT (""uuid"") DO UPDATE
						SET
							""full-name"" = excluded.""full-name"",
							""email"" = excluded.""email"",

				");
				if (!string.IsNullOrWhiteSpace(kvp.Value.PasswordHash)) {
					sb.Append(@"
							""password-hash"" = excluded.""password-hash"",
					");
				}
				sb.Append(@"

							""email-list-marketing"" = excluded.""email-list-marketing"",
							""email-list-tutorials"" = excluded.""email-list-tutorials"",
							""marketing-campaign"" = excluded.""marketing-campaign"",
							""phone"" = excluded.""phone"",
							""company-id"" = excluded.""company-id"",
							""application-data"" = CAST(excluded.""application-data"" AS json),
							""json"" = CAST(excluded.""json"" AS json)

				");

				string sql = sb.ToString();


				using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);

				cmd.Parameters.AddWithValue("@fullName", string.IsNullOrWhiteSpace(kvp.Value.FullName) ? (object)DBNull.Value : kvp.Value.FullName);
				cmd.Parameters.AddWithValue("@email", string.IsNullOrWhiteSpace(kvp.Value.Email) ? (object)DBNull.Value : kvp.Value.Email);
				if (!string.IsNullOrWhiteSpace(kvp.Value.PasswordHash)) {
					cmd.Parameters.AddWithValue("@passwordHash", string.IsNullOrWhiteSpace(kvp.Value.PasswordHash) ? (object)DBNull.Value : kvp.Value.PasswordHash);
				}
				cmd.Parameters.AddWithValue("@emailListMarketing", null == kvp.Value.EmailListMarketing ? (object)DBNull.Value : kvp.Value.EmailListMarketing);
				cmd.Parameters.AddWithValue("@emailListTutorials", null == kvp.Value.EmailListTutorials ? (object)DBNull.Value : kvp.Value.EmailListTutorials);
				cmd.Parameters.AddWithValue("@marketingCampaign", string.IsNullOrWhiteSpace(kvp.Value.MarketingCampaign) ? (object)DBNull.Value : kvp.Value.MarketingCampaign);
				cmd.Parameters.AddWithValue("@phone", string.IsNullOrWhiteSpace(kvp.Value.Phone) ? (object)DBNull.Value : kvp.Value.Phone);
				cmd.Parameters.AddWithValue("@uuid", kvp.Key);
				cmd.Parameters.AddWithValue("@companyId", null == kvp.Value.CompanyId ? (object)DBNull.Value : kvp.Value.CompanyId);
				cmd.Parameters.AddWithValue("@applicationData", null == kvp.Value.ApplicationData ? (object)DBNull.Value : kvp.Value.ApplicationData);
				cmd.Parameters.AddWithValue("@json", string.IsNullOrWhiteSpace(kvp.Value.Json) ? (object)DBNull.Value : kvp.Value.Json);

				int rowsAffected = cmd.ExecuteNonQuery();

				if (rowsAffected == 0) {
					continue;
				}

				toSendToOthers.Add(kvp.Key, kvp.Value);
				callerResponse.Add(kvp.Key);


			}



		}


		public static BillingContacts FromDataReader(NpgsqlDataReader reader) {

			string? fullName = default;
			string? email = default;
			string? passwordHash = default;
			bool? emailListMarketing = default;
			bool? emailListTutorials = default;
			string? marketingCampaign = default;
			string? phone = default;
			Guid? uuid = default;
			Guid? companyId = default;
			string? applicationData = default;
			string? json = default;


			if (!reader.IsDBNull("full-name")) {
				fullName = reader.GetString("full-name");
			}
			if (!reader.IsDBNull("email")) {
				email = reader.GetString("email");
			}
			if (!reader.IsDBNull("password-hash")) {
				passwordHash = reader.GetString("password-hash");
			}
			if (!reader.IsDBNull("email-list-marketing")) {
				emailListMarketing = reader.GetBoolean("email-list-marketing");
			}
			if (!reader.IsDBNull("email-list-tutorials")) {
				emailListTutorials = reader.GetBoolean("email-list-tutorials");
			}
			if (!reader.IsDBNull("marketing-campaign")) {
				marketingCampaign = reader.GetString("marketing-campaign");
			}
			if (!reader.IsDBNull("phone")) {
				phone = reader.GetString("phone");
			}
			if (!reader.IsDBNull("uuid")) {
				uuid = reader.GetGuid("uuid");
			}
			if (!reader.IsDBNull("company-id")) {
				companyId = reader.GetGuid("company-id");
			}
			if (!reader.IsDBNull("application-data")) {
				applicationData = reader.GetString("application-data");
			}
			if (!reader.IsDBNull("json")) {
				json = reader.GetString("json");
			}


			return new BillingContacts(
				FullName: fullName,
				Email: email,
				PasswordHash: passwordHash,
				EmailListMarketing: emailListMarketing,
				EmailListTutorials: emailListTutorials,
				MarketingCampaign: marketingCampaign,
				Phone: phone,
				Uuid: uuid,
				CompanyId: companyId,
				ApplicationData: applicationData,
				Json: json
				);

		}

		//private string _PasswordHash = PasswordHash;

		//[JsonIgnore]
		//public string PasswordHash
		//{
		//	get {
		//		return _PasswordHash;
		//	}
		//	init { }
		//}


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
		public JObject? ApplicationDataObject
		{
			get {
				if (string.IsNullOrWhiteSpace(ApplicationData))
					return null;
				return JsonConvert.DeserializeObject(ApplicationData, new JsonSerializerSettings() { DateParseHandling = DateParseHandling.None }) as JObject;
			}
		}

		public bool ShouldSerializeDPAgentId() {
			return false;
		}

		[JsonIgnore]
		public Guid? DPAgentId
		{
			get {
				if (null == ApplicationDataObject)
					return null;

				JToken? sessionAgentIdTok = ApplicationDataObject[kApplicationDataKeyDispatchPulseAgentId];
				if (null == sessionAgentIdTok)
					return null;

				string sessionAgentIdStr = sessionAgentIdTok.Value<string>();
				if (string.IsNullOrWhiteSpace(sessionAgentIdStr))
					return null;

				if (!Guid.TryParse(sessionAgentIdStr, out Guid sessionAgentId)) {
					return null;
				}

				return sessionAgentId;
			}
			init {
				JObject? root = ApplicationDataObject;
				if (null == root) {
					root = new JObject();
				}
				root[kApplicationDataKeyDispatchPulseAgentId] = value;
				ApplicationData = root.ToString(Formatting.Indented);
			}
		}

		[JsonIgnore]
		public Guid? DPContactId
		{
			get {
				if (null == ApplicationDataObject)
					return null;

				JToken? sessionAgentIdTok = ApplicationDataObject[kApplicationDataKeyDispatchPulseContactId];
				if (null == sessionAgentIdTok)
					return null;

				string sessionAgentIdStr = sessionAgentIdTok.Value<string>();
				if (string.IsNullOrWhiteSpace(sessionAgentIdStr))
					return null;

				if (!Guid.TryParse(sessionAgentIdStr, out Guid sessionAgentId)) {
					return null;
				}

				return sessionAgentId;
			}
			init {
				JObject? root = ApplicationDataObject;
				if (null == root) {
					root = new JObject();
				}
				root[kApplicationDataKeyDispatchPulseContactId] = value;
				ApplicationData = root.ToString(Formatting.Indented);
			}
		}

		[JsonIgnore]
		public bool? LicenseAssignedProjectsSchedulingTime
		{
			get {
				JObject? root = JsonObject;
				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyLicenseAssignedProjectsSchedulingTime];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				bool b = tok.Value<bool>();
				return b;
			}
		}

		[JsonIgnore]
		public bool? LicenseAssignedOnCall
		{
			get {
				JObject? root = JsonObject;
				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyLicenseAssignedOnCall];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				bool b = tok.Value<bool>();
				return b;
			}
		}


		public static readonly Guid CEContactId = Guid.Parse("77977D09-BE3B-430D-97B0-6203A243C367");

		public static void VerifyRepairTable(NpgsqlConnection db, out Guid? billingContactId, bool insertDefaultContents = false, Guid? companyId = null) {

			if (db.TableExists("billing-contacts")) {
				Log.Debug($"----- Table \"billing-contacts\" exists.");
			} else {
				Log.Information($"----- Table \"billing-contacts\" doesn't exist, creating.");

				using NpgsqlCommand cmd = new NpgsqlCommand(@"
					CREATE TABLE ""public"".""billing-contacts"" (
						""full-name"" character varying(255),
						""email"" character varying(255),
						""password-hash"" character varying(255),
						""email-list-marketing"" boolean DEFAULT false,
						""email-list-tutorials"" boolean DEFAULT true,
						""marketing-campaign"" character varying(255),
						""phone"" character varying(255),
						""is-deleted"" boolean DEFAULT false NOT NULL,
						""uuid"" uuid DEFAULT public.uuid_generate_v1() NOT NULL,
						""company-id"" uuid,
						""application-data"" json,
						json json DEFAULT '{}'::json NOT NULL,
						CONSTRAINT ""billing_contacts_pk"" PRIMARY KEY(""uuid"")
					) WITH(oids = false);
					", db);
				cmd.ExecuteNonQuery();
			}


			if (insertDefaultContents) {
				Log.Information("Insert Default Contents");

				string hash = BCrypt.Net.BCrypt.HashPassword("CHANGEME");


				BillingContacts bc = new BillingContacts(
					FullName: "Example Contact",
					Email: "admin@example.com",
					PasswordHash: hash,
					EmailListMarketing: false,
					EmailListTutorials: false,
					MarketingCampaign: null,
					Phone: null,
					Uuid: CEContactId,
					CompanyId: companyId,
					ApplicationData: new JObject { }.ToString(Formatting.Indented),
					Json: new JObject {
						{ kJsonKeyLicenseAssignedProjectsSchedulingTime, true },
						{ kJsonKeyLicenseAssignedOnCall, true },
					}.ToString(Formatting.Indented)
					);

				Upsert(db, new Dictionary<Guid, BillingContacts> {
					{CEContactId, bc},
				}, out _, out _);

				billingContactId = CEContactId;

			} else {
				billingContactId = null;
			}





		}
















	}
}
