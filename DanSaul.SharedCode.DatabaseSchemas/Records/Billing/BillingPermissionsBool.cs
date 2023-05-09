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
	public record BillingPermissionsBool(
		Guid? Uuid,
		string? Key,
		bool? Value,
		Guid? ContactId,
		Guid? GroupId,
		string? Json
		)
	{
		public static Dictionary<Guid, BillingPermissionsBool> ForId(NpgsqlConnection connection, Guid id) {

			Dictionary<Guid, BillingPermissionsBool> ret = new Dictionary<Guid, BillingPermissionsBool>();

			string sql = @"SELECT * from ""billing-permissions-bool"" WHERE uuid = @uuid";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			cmd.Parameters.AddWithValue("@uuid", id);




			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					BillingPermissionsBool obj = BillingPermissionsBool.FromDataReader(reader);
					if (obj.Uuid == null) {
						continue;
					}
					ret.Add(obj.Uuid.Value, obj);
				}
			}

			return ret;
		}

		public static Dictionary<Guid, BillingPermissionsBool> ForIds(NpgsqlConnection connection, IEnumerable<Guid> ids) {

			Guid[] idsArr = ids.ToArray();

			Dictionary<Guid, BillingPermissionsBool> ret = new Dictionary<Guid, BillingPermissionsBool>();
			if (idsArr.Length == 0)
				return ret;

			List<string> valNames = new List<string>();
			for (int i = 0; i < idsArr.Length; i++) {
				valNames.Add($"@val{i}");
			}

			string sql = $"SELECT * from \"billing-permissions-bool\" WHERE uuid IN ({string.Join(", ", valNames)})";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			for (int i = 0; i < valNames.Count; i++) {
				cmd.Parameters.AddWithValue(valNames[i], idsArr[i]);
			}

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					BillingPermissionsBool obj = BillingPermissionsBool.FromDataReader(reader);
					if (obj.Uuid == null) {
						continue;
					}
					ret.Add(obj.Uuid.Value, obj);
				}
			}

			return ret;


		}


		public static Dictionary<Guid, BillingPermissionsBool> All(NpgsqlConnection connection) {

			Dictionary<Guid, BillingPermissionsBool> ret = new Dictionary<Guid, BillingPermissionsBool>();

			string sql = @"SELECT * from ""billing-permissions-bool""";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					BillingPermissionsBool obj = BillingPermissionsBool.FromDataReader(reader);
					if (obj.Uuid == null) {
						continue;
					}
					ret.Add(obj.Uuid.Value, obj);
				}
			}

			return ret;


		}



		public static Dictionary<Guid, BillingPermissionsBool> ForBillingContactsAndKeys(
			NpgsqlConnection connection,
			List<Guid> billingContactIds,
			List<string> keys
			) {
			Dictionary<Guid, BillingPermissionsBool> ret = new Dictionary<Guid, BillingPermissionsBool>();
			if (billingContactIds.Count == 0 && keys.Count == 0) {
				return ret;
			}


			List<string> valNamesBillingContact = new List<string>();
			for (int i = 0; i < billingContactIds.Count; i++) {
				valNamesBillingContact.Add($"@contactid{i}");
			}

			List<string> valNamesKeys = new List<string>();
			for (int i = 0; i < keys.Count; i++) {
				valNamesKeys.Add($"@keys{i}");
			}

			StringBuilder sb = new StringBuilder();
			sb.Append($"SELECT * FROM \"billing-permissions-bool\" WHERE ");

			if (billingContactIds.Count != 0) {
				sb.Append($" \"contact-id\" IN ({string.Join(", ", valNamesBillingContact)}) ");
			}
			if (billingContactIds.Count != 0 && keys.Count != 0) {
				sb.Append(" AND ");
			}
			if (keys.Count != 0) {
				sb.Append($" \"key\" IN ({string.Join(", ", valNamesKeys)}) ");
			}

			string sql = sb.ToString();

			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			if (billingContactIds.Count != 0) {
				for (int i = 0; i < valNamesBillingContact.Count; i++) {
					cmd.Parameters.AddWithValue(valNamesBillingContact[i], billingContactIds[i]);
				}
			}
			if (keys.Count != 0) {
				for (int i = 0; i < valNamesKeys.Count; i++) {
					cmd.Parameters.AddWithValue(valNamesKeys[i], keys[i]);
				}
			}


			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					BillingPermissionsBool obj = BillingPermissionsBool.FromDataReader(reader);
					if (obj.Uuid == null) {
						continue;
					}
					ret.Add(obj.Uuid.Value, obj);
				}
			}


			return ret;
		}
















		public static Dictionary<Guid, BillingPermissionsBool> ForBillingContactsOrGroups(
			NpgsqlConnection connection,
			List<Guid> billingContactIds,
			List<Guid> groupIds
			) {

			Dictionary<Guid, BillingPermissionsBool> ret = new Dictionary<Guid, BillingPermissionsBool>();
			if (billingContactIds.Count == 0 && groupIds.Count == 0) {
				return ret;
			}



			List<string> valNamesBillingContact = new List<string>();
			for (int i = 0; i < billingContactIds.Count; i++) {
				valNamesBillingContact.Add($"@contactid{i}");
			}

			List<string> valNamesGroupIds = new List<string>();
			for (int i = 0; i < groupIds.Count; i++) {
				valNamesGroupIds.Add($"@groupid{i}");
			}

			StringBuilder sb = new StringBuilder();
			sb.Append($"SELECT * FROM \"billing-permissions-bool\" WHERE ");

			if (billingContactIds.Count != 0) {
				sb.Append($" \"contact-id\" IN ({string.Join(", ", valNamesBillingContact)}) ");
			}
			if (billingContactIds.Count != 0 && groupIds.Count != 0) {
				sb.Append(" OR ");
			}
			if (groupIds.Count != 0) {
				sb.Append($" \"group-id\" IN ({string.Join(", ", valNamesGroupIds)}) ");
			}

			string sql = sb.ToString();

			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			if (billingContactIds.Count != 0) {
				for (int i = 0; i < valNamesBillingContact.Count; i++) {
					cmd.Parameters.AddWithValue(valNamesBillingContact[i], billingContactIds[i]);
				}
			}
			if (groupIds.Count != 0) {
				for (int i = 0; i < valNamesGroupIds.Count; i++) {
					cmd.Parameters.AddWithValue(valNamesGroupIds[i], groupIds[i]);
				}
			}


			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					BillingPermissionsBool obj = BillingPermissionsBool.FromDataReader(reader);
					if (obj.Uuid == null) {
						continue;
					}
					ret.Add(obj.Uuid.Value, obj);
				}
			}


			return ret;
		}






		public static HashSet<string> GrantedForBillingContact(NpgsqlConnection billingConnection, BillingContacts contact) {
			HashSet<string> ret = new HashSet<string>();

			if (contact.Uuid == null) {
				return ret;
			}

			// First we add the user's specific positive permissions.
			{
				string sql = $"SELECT * FROM \"billing-permissions-bool\" WHERE \"contact-id\" = @contactId AND \"value\" = TRUE";

				using NpgsqlCommand cmd = new NpgsqlCommand(sql, billingConnection);
				cmd.Parameters.AddWithValue("@contactId", contact.Uuid.Value);

				using NpgsqlDataReader reader = cmd.ExecuteReader();

				if (reader.HasRows) {
					while (reader.Read()) {
						BillingPermissionsBool obj = BillingPermissionsBool.FromDataReader(reader);
						if (obj.Key == null) {
							continue;
						}
						ret.Add(obj.Key);
					}
				}
			}

			// Next we find if they are in any groups.
			List<Guid> groupIds = new List<Guid>();
			{
				string sql = $"SELECT * FROM \"billing-permissions-groups-memberships\" WHERE \"contact-id\" = @contactId";

				using NpgsqlCommand cmd = new NpgsqlCommand(sql, billingConnection);
				cmd.Parameters.AddWithValue("@contactId", contact.Uuid);

				using NpgsqlDataReader reader = cmd.ExecuteReader();

				if (reader.HasRows) {
					while (reader.Read()) {
						BillingPermissionsGroupsMemberships obj = BillingPermissionsGroupsMemberships.FromDataReader(reader);
						if (obj.GroupId != null) {
							groupIds.Add(obj.GroupId.Value);
						}
						
					}
				}
			}

			if (groupIds.Count == 0) {
				return ret;
			}

			List<string> valNames = new List<string>();
			for (int i = 0; i < groupIds.Count; i++) {
				valNames.Add($"@val{i}");
			}

			// Next we add all of the groups' positive permissions.
			{
				string sql = $"SELECT * FROM \"billing-permissions-bool\" WHERE \"group-id\" IN ({string.Join(", ", valNames)}) AND \"value\" = TRUE";

				using NpgsqlCommand cmd = new NpgsqlCommand(sql, billingConnection);
				for (int i = 0; i < valNames.Count; i++) {
					cmd.Parameters.AddWithValue(valNames[i], groupIds[i]);
				}

				using NpgsqlDataReader reader = cmd.ExecuteReader();

				if (reader.HasRows) {
					while (reader.Read()) {
						BillingPermissionsBool obj = BillingPermissionsBool.FromDataReader(reader);
						if (obj.Key == null) {
							continue;
						}
						ret.Add(obj.Key);
					}
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



			string sql = $"DELETE FROM \"billing-permissions-bool\" WHERE \"uuid\" IN ({string.Join(", ", valNames)})";
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

		public static List<Guid> DeleteForContactIds(NpgsqlConnection connection, List<Guid> idsToDelete) {

			List<Guid> toSendToOthers = new List<Guid>();
			if (idsToDelete.Count == 0) {
				return toSendToOthers;
			}

			List<string> valNames = new List<string>();
			for (int i = 0; i < idsToDelete.Count; i++) {
				valNames.Add($"@val{i}");
			}



			string sql = $"DELETE FROM \"billing-permissions-bool\" WHERE \"contact-id\" IN ({string.Join(", ", valNames)})";
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


		public static void Upsert(NpgsqlConnection connection, Dictionary<Guid, BillingPermissionsBool> updateObjects, out List<Guid> callerResponse, out Dictionary<Guid, BillingPermissionsBool> toSendToOthers) {

			callerResponse = new List<Guid>();
			toSendToOthers = new Dictionary<Guid, BillingPermissionsBool>();

			foreach (KeyValuePair<Guid, BillingPermissionsBool> kvp in updateObjects) {

				string sql = @"
					INSERT INTO
						""billing-permissions-bool""
						(
							""uuid"",
							""key"",
							""value"",
							""contact-id"",
							""group-id"",
							""json""
						)
					VALUES
						(
							@uuid,
							@key,
							@value,
							@contactId,
							@groupId,
							CAST(@json AS json)
						)
					ON CONFLICT (""uuid"") DO UPDATE
						SET
							""key"" = excluded.""key"",
							""value"" = excluded.""value"",
							""contact-id"" = excluded.""contact-id"",
							""group-id"" = excluded.""group-id"",
							""json"" = CAST(excluded.""json"" AS json)
					";

				using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
				cmd.Parameters.AddWithValue("@uuid", kvp.Key);
				cmd.Parameters.AddWithValue("@key", string.IsNullOrWhiteSpace(kvp.Value.Key) ? (object)DBNull.Value : kvp.Value.Key);
				cmd.Parameters.AddWithValue("@value", null == kvp.Value.Value ? (object)DBNull.Value : kvp.Value.Value);
				cmd.Parameters.AddWithValue("@contactId", kvp.Value.ContactId == null ? (object)DBNull.Value : kvp.Value.ContactId);
				cmd.Parameters.AddWithValue("@groupId", kvp.Value.GroupId == null ? (object)DBNull.Value : kvp.Value.GroupId);
				cmd.Parameters.AddWithValue("@json", string.IsNullOrWhiteSpace(kvp.Value.Json) ? (object)DBNull.Value : kvp.Value.Json);

				int rowsAffected = cmd.ExecuteNonQuery();

				if (rowsAffected == 0) {
					continue;
				}

				toSendToOthers.Add(kvp.Key, kvp.Value);
				callerResponse.Add(kvp.Key);


			}



		}


		public static BillingPermissionsBool FromDataReader(NpgsqlDataReader reader) {

			Guid? uuid = default;
			string? key = default;
			bool? value = default;
			Guid? contactId = default;
			Guid? groupId = default;
			string? json = default;

			if (!reader.IsDBNull("uuid")) {
				uuid = reader.GetGuid("uuid");
			}
			if (!reader.IsDBNull("key")) {
				key = reader.GetString("key");
			}
			if (!reader.IsDBNull("value")) {
				value = reader.GetBoolean("value");
			}
			if (!reader.IsDBNull("contact-id")) {
				contactId = reader.GetGuid("contact-id");
			}
			if (!reader.IsDBNull("group-id")) {
				groupId = reader.GetGuid("group-id");
			}
			if (!reader.IsDBNull("json")) {
				json = reader.GetString("json");
			}


			return new BillingPermissionsBool(
				Uuid: uuid,
				Key: key,
				Value: value,
				ContactId: contactId,
				GroupId: groupId,
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

			if (db.TableExists("billing-permissions-bool")) {
				Log.Debug($"----- Table \"billing-permissions-bool\" exists.");
			} else {
				Log.Information($"----- Table \"billing-permissions-bool\" doesn't exist, creating.");

				using NpgsqlCommand cmd = new NpgsqlCommand(@"
					CREATE TABLE ""public"".""billing-permissions-bool"" (
						""uuid"" uuid DEFAULT public.uuid_generate_v1() NOT NULL,
						""key"" character varying(255) NOT NULL,
						""value"" boolean NOT NULL,
						""contact-id"" uuid,
						""group-id"" uuid,
						""json"" json DEFAULT '{}'::json NOT NULL,
						CONSTRAINT ""billing_permissions_bool_pk"" PRIMARY KEY(""uuid"")
					) WITH(oids = false);
					", db);
				cmd.ExecuteNonQuery();
			}


			if (insertDefaultContents) {
				Log.Information("Insert Default Contents");

				NpgsqlCommand command = new NpgsqlCommand(SQLUtility.RemoveCommentsFromSQLString(Resources.SQLInsertDefaultBillingPermissionsBool, true), db);
				command.ExecuteNonQuery();
			}





		}













	}
}
