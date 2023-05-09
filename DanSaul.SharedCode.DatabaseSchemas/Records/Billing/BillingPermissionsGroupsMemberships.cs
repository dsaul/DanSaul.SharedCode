using Npgsql;
using System.Text;
using System.Data;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Serilog;
using DanSaul.SharedCode.Npgsql;

namespace SharedCode.DatabaseSchemas
{
	public record BillingPermissionsGroupsMemberships(
		Guid? Id,
		Guid? GroupId,
		Guid? ContactId,
		string? Json
		)
	{

		public static Dictionary<Guid, BillingPermissionsGroupsMemberships> ForId(NpgsqlConnection connection, Guid id) {

			Dictionary<Guid, BillingPermissionsGroupsMemberships> ret = new Dictionary<Guid, BillingPermissionsGroupsMemberships>();

			string sql = @"SELECT * from ""billing-permissions-groups-memberships"" WHERE id = @id";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			cmd.Parameters.AddWithValue("@id", id);




			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					BillingPermissionsGroupsMemberships obj = BillingPermissionsGroupsMemberships.FromDataReader(reader);
					if (obj.Id == null) {
						continue;
					}
					ret.Add(obj.Id.Value, obj);
				}
			}

			return ret;
		}

		public static Dictionary<Guid, BillingPermissionsGroupsMemberships> ForIds(NpgsqlConnection connection, IEnumerable<Guid> ids) {

			Guid[] idsArr = ids.ToArray();

			Dictionary<Guid, BillingPermissionsGroupsMemberships> ret = new Dictionary<Guid, BillingPermissionsGroupsMemberships>();
			if (idsArr.Length == 0)
				return ret;

			List<string> valNames = new List<string>();
			for (int i = 0; i < idsArr.Length; i++) {
				valNames.Add($"@val{i}");
			}

			string sql = $"SELECT * from \"billing-permissions-groups-memberships\" WHERE id IN ({string.Join(", ", valNames)})";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			for (int i = 0; i < valNames.Count; i++) {
				cmd.Parameters.AddWithValue(valNames[i], idsArr[i]);
			}

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					BillingPermissionsGroupsMemberships obj = BillingPermissionsGroupsMemberships.FromDataReader(reader);
					if (obj.Id == null) {
						continue;
					}
					ret.Add(obj.Id.Value, obj);
				}
			}

			return ret;


		}

		public static Dictionary<Guid, BillingPermissionsGroupsMemberships> ForDeletionListForContact(NpgsqlConnection connection, Guid contactId, List<Guid> excludeGroupIds) {
			if (excludeGroupIds.Count == 0) {
				return ForDeletionListForContact_COUNT_0(connection, contactId);
			} else {
				return ForDeletionListForContact_COUNT_NOT_0(connection, contactId, excludeGroupIds);
			}
		}

		private static Dictionary<Guid, BillingPermissionsGroupsMemberships> ForDeletionListForContact_COUNT_0(NpgsqlConnection connection, Guid contactId) {

			Dictionary<Guid, BillingPermissionsGroupsMemberships> ret = new Dictionary<Guid, BillingPermissionsGroupsMemberships>();

			string sql = @"
				SELECT ""billing-permissions-groups-memberships"".*
				FROM ""billing-permissions-groups-memberships""
				LEFT JOIN ""billing-permissions-groups"" ON ""billing-permissions-groups-memberships"".""group-id"" = ""billing-permissions-groups"".""id""
				WHERE 
				""billing-permissions-groups-memberships"".""contact-id"" = @contactId
				AND
				(""billing-permissions-groups"".""json"" ->>'hidden')::boolean = false::boolean
			";

			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			cmd.Parameters.AddWithValue("@contactId", contactId);

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					BillingPermissionsGroupsMemberships obj = BillingPermissionsGroupsMemberships.FromDataReader(reader);
					if (obj.Id == null) {
						continue;
					}
					ret.Add(obj.Id.Value, obj);
				}
			}


			return ret;

		}

		private static Dictionary<Guid, BillingPermissionsGroupsMemberships> ForDeletionListForContact_COUNT_NOT_0(NpgsqlConnection connection, Guid contactId, List<Guid> excludeGroupIds) {

			Dictionary<Guid, BillingPermissionsGroupsMemberships> ret = new Dictionary<Guid, BillingPermissionsGroupsMemberships>();
			if (excludeGroupIds.Count == 0) {
				return ret;
			}

			List<string> valNames = new List<string>();
			for (int i = 0; i < excludeGroupIds.Count; i++) {
				valNames.Add($"@val{i}");
			}


			string sql = @"
				SELECT ""billing-permissions-groups-memberships"".*
				FROM ""billing-permissions-groups-memberships""
				LEFT JOIN ""billing-permissions-groups"" ON ""billing-permissions-groups-memberships"".""group-id"" = ""billing-permissions-groups"".""id""
				WHERE 
				""billing-permissions-groups-memberships"".""contact-id"" = @contactId
				AND
				(""billing-permissions-groups"".""json"" ->>'hidden')::boolean = false::boolean
				AND
				""billing-permissions-groups-memberships"".""group-id"" NOT IN (" + string.Join(", ", valNames) + @")
			";

			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			cmd.Parameters.AddWithValue("@contactId", contactId);
			for (int i = 0; i < valNames.Count; i++) {
				cmd.Parameters.AddWithValue(valNames[i], excludeGroupIds[i]);
			}


			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					BillingPermissionsGroupsMemberships obj = BillingPermissionsGroupsMemberships.FromDataReader(reader);
					if (obj.Id == null) {
						continue;
					}
					ret.Add(obj.Id.Value, obj);
				}
			}


			return ret;


		}










		public static Dictionary<Guid, BillingPermissionsGroupsMemberships> ForBillingContactIdAndGroupId(NpgsqlConnection connection, Guid contactId, List<Guid> groupIds) {

			Dictionary<Guid, BillingPermissionsGroupsMemberships> ret = new Dictionary<Guid, BillingPermissionsGroupsMemberships>();
			if (groupIds.Count == 0) {
				return ret;
			}


			List<string> valNames = new List<string>();
			for (int i = 0; i < groupIds.Count; i++) {
				valNames.Add($"@val{i}");
			}

			string sql = $"SELECT * FROM \"billing-permissions-groups-memberships\" WHERE \"contact-id\" = @contactId AND \"group-id\" IN ({string.Join(", ", valNames)})";

			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			cmd.Parameters.AddWithValue("@contactId", contactId);
			for (int i = 0; i < valNames.Count; i++) {
				cmd.Parameters.AddWithValue(valNames[i], groupIds[i]);
			}

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					BillingPermissionsGroupsMemberships obj = BillingPermissionsGroupsMemberships.FromDataReader(reader);
					if (obj.Id == null) {
						continue;
					}
					ret.Add(obj.Id.Value, obj);
				}
			}


			return ret;
		}

		public static Dictionary<Guid, BillingPermissionsGroupsMemberships> ForBillingContactId(NpgsqlConnection connection, Guid? contactId) {

			Dictionary<Guid, BillingPermissionsGroupsMemberships> ret = new Dictionary<Guid, BillingPermissionsGroupsMemberships>();

			if (null == contactId)
				return ret;

			string sql = @"SELECT * from ""billing-permissions-groups-memberships"" WHERE ""contact-id"" = @contactId";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			cmd.Parameters.AddWithValue("@contactId", contactId);

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					BillingPermissionsGroupsMemberships obj = BillingPermissionsGroupsMemberships.FromDataReader(reader);
					if (obj.Id == null) {
						continue;
					}
					ret.Add(obj.Id.Value, obj);
				}
			}

			return ret;
		}

		public static Dictionary<Guid, BillingPermissionsGroupsMemberships> ForBillingContactIds(NpgsqlConnection connection, List<Guid> contactIds) {

			Dictionary<Guid, BillingPermissionsGroupsMemberships> ret = new Dictionary<Guid, BillingPermissionsGroupsMemberships>();
			if (contactIds.Count == 0) {
				return ret;
			}


			List<string> valNames = new List<string>();
			for (int i = 0; i < contactIds.Count; i++) {
				valNames.Add($"@val{i}");
			}

			string sql = $"SELECT * FROM \"billing-permissions-groups-memberships\" WHERE \"contact-id\" IN ({string.Join(", ", valNames)})";

			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			for (int i = 0; i < valNames.Count; i++) {
				cmd.Parameters.AddWithValue(valNames[i], contactIds[i]);
			}

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					BillingPermissionsGroupsMemberships obj = BillingPermissionsGroupsMemberships.FromDataReader(reader);
					if (obj.Id == null) {
						continue;
					}
					ret.Add(obj.Id.Value, obj);
				}
			}


			return ret;
		}


		public static Dictionary<Guid, BillingPermissionsGroupsMemberships> All(NpgsqlConnection connection) {

			Dictionary<Guid, BillingPermissionsGroupsMemberships> ret = new Dictionary<Guid, BillingPermissionsGroupsMemberships>();

			string sql = @"SELECT * from ""billing-permissions-groups-memberships""";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					BillingPermissionsGroupsMemberships obj = BillingPermissionsGroupsMemberships.FromDataReader(reader);
					if (obj.Id == null) {
						continue;
					}
					ret.Add(obj.Id.Value, obj);
				}
			}

			return ret;


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



			string sql = $"DELETE FROM \"billing-permissions-groups-memberships\" WHERE \"contact-id\" IN ({string.Join(", ", valNames)})";
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


		public static List<Guid> Delete(NpgsqlConnection connection, IEnumerable<Guid> idsToDelete) {


			Guid[] ids = idsToDelete.ToArray();


			List<Guid> toSendToOthers = new List<Guid>();
			if (ids.Length == 0) {
				return toSendToOthers;
			}

			List<string> valNames = new List<string>();
			for (int i = 0; i < ids.Length; i++) {
				valNames.Add($"@val{i}");
			}



			string sql = $"DELETE FROM \"billing-permissions-groups-memberships\" WHERE \"id\" IN ({string.Join(", ", valNames)})";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			for (int i = 0; i < valNames.Count; i++) {
				cmd.Parameters.AddWithValue(valNames[i], ids[i]);
			}



			int rowsAffected = cmd.ExecuteNonQuery();
			if (rowsAffected == 0) {
				return toSendToOthers;
			}

			toSendToOthers.AddRange(ids);
			return toSendToOthers;



		}

















		public static void Upsert(NpgsqlConnection connection, Dictionary<Guid, BillingPermissionsGroupsMemberships> updateObjects, out List<Guid> callerResponse, out Dictionary<Guid, BillingPermissionsGroupsMemberships> toSendToOthers) {

			callerResponse = new List<Guid>();
			toSendToOthers = new Dictionary<Guid, BillingPermissionsGroupsMemberships>();

			foreach (KeyValuePair<Guid, BillingPermissionsGroupsMemberships> kvp in updateObjects) {

				string sql = @"
					INSERT INTO
						""billing-permissions-groups-memberships""
						(
							""id"",
							""group-id"",
							""contact-id"",
							""json""
						)
					VALUES
						(
							@id,
							@groupId,
							@contactId,
							CAST(@json AS json)
						)
					ON CONFLICT (""id"") DO UPDATE
						SET
							""group-id"" = excluded.""group-id"",
							""contact-id"" = excluded.""contact-id"",
							""json"" = CAST(excluded.""json"" AS json)
					";

				using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
				cmd.Parameters.AddWithValue("@id", kvp.Key);
				cmd.Parameters.AddWithValue("@groupId", null == kvp.Value.GroupId ? (object)DBNull.Value : kvp.Value.GroupId);
				cmd.Parameters.AddWithValue("@contactId", null == kvp.Value.ContactId ? (object)DBNull.Value : kvp.Value.ContactId);
				cmd.Parameters.AddWithValue("@json", string.IsNullOrWhiteSpace(kvp.Value.Json) ? (object)DBNull.Value : kvp.Value.Json);

				int rowsAffected = cmd.ExecuteNonQuery();

				if (rowsAffected == 0) {
					continue;
				}

				toSendToOthers.Add(kvp.Key, kvp.Value);
				callerResponse.Add(kvp.Key);


			}










		}



		public static BillingPermissionsGroupsMemberships FromDataReader(NpgsqlDataReader reader) {

			Guid? id = default;
			Guid? groupId = default;
			Guid? contactId = default;
			string? json = default;

			if (!reader.IsDBNull("id")) {
				id = reader.GetGuid("id");
			}
			if (!reader.IsDBNull("group-id")) {
				groupId = reader.GetGuid("group-id");
			}
			if (!reader.IsDBNull("contact-id")) {
				contactId = reader.GetGuid("contact-id");
			}
			if (!reader.IsDBNull("json")) {
				json = reader.GetString("json");
			}

			return new BillingPermissionsGroupsMemberships(
				Id: id,
				GroupId: groupId,
				ContactId: contactId,
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

			if (db.TableExists("billing-permissions-groups-memberships")) {
				Log.Debug($"----- Table \"billing-permissions-groups-memberships\" exists.");
			} else {
				Log.Information($"----- Table \"billing-permissions-groups-memberships\" doesn't exist, creating.");

				using NpgsqlCommand cmd = new NpgsqlCommand(@"
					CREATE TABLE ""public"".""billing-permissions-groups-memberships"" (
						""id"" uuid DEFAULT public.uuid_generate_v1() NOT NULL,
						""group-id"" uuid NOT NULL,
						""contact-id"" uuid NOT NULL,
						""json"" json DEFAULT '{}'::json NOT NULL,
						CONSTRAINT ""billing_permissions_groups_memberships_pk"" PRIMARY KEY(""id"")
					) WITH(oids = false);
					", db);
				cmd.ExecuteNonQuery();
			}

			

			if (insertDefaultContents) {
				// None

				Guid id = Guid.NewGuid();

				Log.Information("Insert Default Contents");
				BillingPermissionsGroupsMemberships m = new BillingPermissionsGroupsMemberships(
					Id: id,
					GroupId: Guid.Parse("456f60fe-774c-11ea-9784-02420a0000d8"),
					ContactId: BillingContacts.CEContactId,
					Json: new JObject { }.ToString(Formatting.Indented)
					);

				Upsert(db, new Dictionary<Guid, BillingPermissionsGroupsMemberships> {
					{id, m},
				}, out _, out _);


				










			}





		}




















	}
}
