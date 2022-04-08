using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using SharedCode.Extensions;
using Serilog;
using SharedCode;
using SharedCode.Databases.Properties;

namespace SharedCode.DatabaseSchemas
{
	public record BillingPermissionsGroups(
		Guid? Id,
		string? Name,
		string? Json
		)
	{

		public static Dictionary<Guid, BillingPermissionsGroups> ForId(NpgsqlConnection connection, Guid id) {

			Dictionary<Guid, BillingPermissionsGroups> ret = new Dictionary<Guid, BillingPermissionsGroups>();

			string sql = @"SELECT * from ""billing-permissions-groups"" WHERE id = @id";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			cmd.Parameters.AddWithValue("@id", id);




			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					BillingPermissionsGroups obj = BillingPermissionsGroups.FromDataReader(reader);
					if (obj.Id == null) {
						continue;
					}
					ret.Add(obj.Id.Value, obj);
				}
			}

			return ret;
		}

		public static Dictionary<Guid, BillingPermissionsGroups> ForIds(NpgsqlConnection connection, IEnumerable<Guid> ids) {

			Guid[] idsArr = ids.ToArray();

			Dictionary<Guid, BillingPermissionsGroups> ret = new Dictionary<Guid, BillingPermissionsGroups>();
			if (idsArr.Length == 0)
				return ret;

			List<string> valNames = new List<string>();
			for (int i = 0; i < idsArr.Length; i++) {
				valNames.Add($"@val{i}");
			}

			string sql = $"SELECT * from \"billing-permissions-groups\" WHERE id IN ({string.Join(", ", valNames)})";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			for (int i = 0; i < valNames.Count; i++) {
				cmd.Parameters.AddWithValue(valNames[i], idsArr[i]);
			}

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					BillingPermissionsGroups obj = BillingPermissionsGroups.FromDataReader(reader);
					if (obj.Id == null) {
						continue;
					}
					ret.Add(obj.Id.Value, obj);
				}
			}

			return ret;


		}


		public static Dictionary<Guid, BillingPermissionsGroups> All(NpgsqlConnection connection) {

			Dictionary<Guid, BillingPermissionsGroups> ret = new Dictionary<Guid, BillingPermissionsGroups>();

			string sql = @"SELECT * from ""billing-permissions-groups""";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					BillingPermissionsGroups obj = BillingPermissionsGroups.FromDataReader(reader);
					if (obj.Id == null) {
						continue;
					}
					ret.Add(obj.Id.Value, obj);
				}
			}

			return ret;


		}

		public static Dictionary<Guid, BillingPermissionsGroups> AllMinusHidden(NpgsqlConnection connection) {

			Dictionary<Guid, BillingPermissionsGroups> ret = new Dictionary<Guid, BillingPermissionsGroups>();

			string sql = $"SELECT * FROM \"billing-permissions-groups\" WHERE(\"json\"->> 'hidden')::boolean != true";

			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			//cmd.Parameters.AddWithValue("@contactId", contact.Uuid);

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					BillingPermissionsGroups obj = BillingPermissionsGroups.FromDataReader(reader);
					if (obj.Id == null) {
						continue;
					}
					ret.Add(obj.Id.Value, obj);
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



			string sql = $"DELETE FROM \"billing-permissions-groups\" WHERE \"id\" IN ({string.Join(", ", valNames)})";
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


		public static void Upsert(NpgsqlConnection connection, Dictionary<Guid, BillingPermissionsGroups> updateObjects, out List<Guid> callerResponse, out Dictionary<Guid, BillingPermissionsGroups> toSendToOthers) {

			callerResponse = new List<Guid>();
			toSendToOthers = new Dictionary<Guid, BillingPermissionsGroups>();

			foreach (KeyValuePair<Guid, BillingPermissionsGroups> kvp in updateObjects) {

				string sql = @"
					INSERT INTO
						""billing-permissions-groups""
						(
							""id"",
							""name"",
							""json""
						)
					VALUES
						(
							@id,
							@name,
							CAST(@json AS json)
						)
					ON CONFLICT (""id"") DO UPDATE
						SET
							""name"" = excluded.""name"",
							""json"" = CAST(excluded.""json"" AS json)
					";

				using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
				cmd.Parameters.AddWithValue("@id", kvp.Key);
				cmd.Parameters.AddWithValue("@name", string.IsNullOrWhiteSpace(kvp.Value.Name) ? (object)DBNull.Value : kvp.Value.Name);
				cmd.Parameters.AddWithValue("@json", string.IsNullOrWhiteSpace(kvp.Value.Json) ? (object)DBNull.Value : kvp.Value.Json);

				int rowsAffected = cmd.ExecuteNonQuery();

				if (rowsAffected == 0) {
					continue;
				}

				toSendToOthers.Add(kvp.Key, kvp.Value);
				callerResponse.Add(kvp.Key);


			}



		}



		public static BillingPermissionsGroups FromDataReader(NpgsqlDataReader reader) {

			Guid? id = default;
			string? name = default;
			string? json = default;


			if (!reader.IsDBNull("id")) {
				id = reader.GetGuid("id");
			}
			if (!reader.IsDBNull("name")) {
				name = reader.GetString("name");
			}
			if (!reader.IsDBNull("json")) {
				json = reader.GetString("json");
			}


			return new BillingPermissionsGroups(
				Id: id,
				Name: name,
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

			if (db.TableExists("billing-permissions-groups")) {
				Log.Debug($"----- Table \"billing-permissions-groups\" exists.");
			} else {
				Log.Information($"----- Table \"billing-permissions-groups\" doesn't exist, creating.");

				using NpgsqlCommand cmd = new NpgsqlCommand(@"
					CREATE TABLE ""public"".""billing-permissions-groups"" (
						""id"" uuid DEFAULT public.uuid_generate_v1() NOT NULL,
						""name"" character varying,
						""json"" json DEFAULT '{}'::json NOT NULL,
						CONSTRAINT ""billing_permissions_groups_pk"" PRIMARY KEY(""id"")
					) WITH(oids = false);
					", db);
				cmd.ExecuteNonQuery();
			}


			if (insertDefaultContents) {
				NpgsqlCommand command = new NpgsqlCommand(SQLUtility.RemoveCommentsFromSQLString(Resources.SQLInsertDefaultBillingPermissionsGroups, true), db);
				command.ExecuteNonQuery();
			}





		}














	}
}
