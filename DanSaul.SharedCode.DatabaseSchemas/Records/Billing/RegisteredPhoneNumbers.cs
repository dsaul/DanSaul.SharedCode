using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;
using Serilog;
using SharedCode.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedCode.DatabaseSchemas
{
	public record RegisteredPhoneNumbers(
		Guid? Id,
		string? Json
		)
	{

		public const string kJsonKeyPhoneNumber = "phoneNumber";
		public const string kJsonKeyBillingCompanyId = "billingCompanyId";


		public static Dictionary<Guid, RegisteredPhoneNumbers> ForPhoneNumber(NpgsqlConnection connection, string phoneNumber) {

			Dictionary<Guid, RegisteredPhoneNumbers> ret = new Dictionary<Guid, RegisteredPhoneNumbers>();

			string sql = @"SELECT * from ""registered-phone-numbers"" WHERE (json->>'phoneNumber')::text = @phoneNumber";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			cmd.Parameters.AddWithValue("@phoneNumber", phoneNumber);




			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					RegisteredPhoneNumbers obj = RegisteredPhoneNumbers.FromDataReader(reader);
					if (obj.Id == null) {
						continue;
					}
					ret.Add(obj.Id.Value, obj);
				}
			}

			return ret;
		}


		public static Dictionary<Guid, RegisteredPhoneNumbers> ForId(NpgsqlConnection connection, Guid id) {

			Dictionary<Guid, RegisteredPhoneNumbers> ret = new Dictionary<Guid, RegisteredPhoneNumbers>();

			string sql = @"SELECT * from ""registered-phone-numbers"" WHERE id = @id";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			cmd.Parameters.AddWithValue("@id", id);




			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					RegisteredPhoneNumbers obj = RegisteredPhoneNumbers.FromDataReader(reader);
					if (obj.Id == null) {
						continue;
					}
					ret.Add(obj.Id.Value, obj);
				}
			}

			return ret;
		}

		public static Dictionary<Guid, RegisteredPhoneNumbers> ForIds(NpgsqlConnection connection, IEnumerable<Guid> ids) {

			Guid[] idsArr = ids.ToArray();

			Dictionary<Guid, RegisteredPhoneNumbers> ret = new Dictionary<Guid, RegisteredPhoneNumbers>();
			if (idsArr.Length == 0)
				return ret;

			List<string> valNames = new List<string>();
			for (int i = 0; i < idsArr.Length; i++) {
				valNames.Add($"@val{i}");
			}

			string sql = $"SELECT * from \"registered-phone-numbers\" WHERE id IN ({string.Join(", ", valNames)})";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			for (int i = 0; i < valNames.Count; i++) {
				cmd.Parameters.AddWithValue(valNames[i], idsArr[i]);
			}

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					RegisteredPhoneNumbers obj = RegisteredPhoneNumbers.FromDataReader(reader);
					if (obj.Id == null) {
						continue;
					}
					ret.Add(obj.Id.Value, obj);
				}
			}

			return ret;


		}

		public static Dictionary<Guid, RegisteredPhoneNumbers> All(NpgsqlConnection connection) {

			Dictionary<Guid, RegisteredPhoneNumbers> ret = new Dictionary<Guid, RegisteredPhoneNumbers>();

			string sql = @"SELECT * from ""registered-phone-numbers""";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					RegisteredPhoneNumbers obj = RegisteredPhoneNumbers.FromDataReader(reader);
					if (obj.Id == null) {
						continue;
					}
					ret.Add(obj.Id.Value, obj);
				}
			}

			return ret;


		}



		public static List<Guid> Delete(NpgsqlConnection connection, IEnumerable<Guid> ids) {

			Guid[] idsArr = ids.ToArray();



			List<Guid> toSendToOthers = new List<Guid>();
			if (idsArr.Length == 0) {
				return toSendToOthers;
			}

			List<string> valNames = new List<string>();
			for (int i = 0; i < idsArr.Length; i++) {
				valNames.Add($"@val{i}");
			}



			string sql = $"DELETE FROM \"registered-phone-numbers\" WHERE \"id\" IN ({string.Join(", ", valNames)})";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			for (int i = 0; i < valNames.Count; i++) {
				cmd.Parameters.AddWithValue(valNames[i], idsArr[i]);
			}



			int rowsAffected = cmd.ExecuteNonQuery();
			if (rowsAffected == 0) {
				return toSendToOthers;
			}

			toSendToOthers.AddRange(idsArr);
			return toSendToOthers;



		}

















		public static void Upsert(NpgsqlConnection connection, Dictionary<Guid, RegisteredPhoneNumbers> updateObjects, out List<Guid> callerResponse, out Dictionary<Guid, RegisteredPhoneNumbers> toSendToOthers) {

			callerResponse = new List<Guid>();
			toSendToOthers = new Dictionary<Guid, RegisteredPhoneNumbers>();

			foreach (KeyValuePair<Guid, RegisteredPhoneNumbers> kvp in updateObjects) {

				string sql = @"
					INSERT INTO
						""registered-phone-numbers""
						(
							""id"",
							""json""
						)
					VALUES
						(
							@id,
							CAST(@json AS jsonb)
						)
					ON CONFLICT (""id"") DO UPDATE
						SET
							""json"" = CAST(excluded.""json"" AS jsonb)
					";

				using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
				cmd.Parameters.AddWithValue("@id", kvp.Key);
				cmd.Parameters.AddWithValue("@json", string.IsNullOrWhiteSpace(kvp.Value.Json) ? (object)DBNull.Value : kvp.Value.Json);

				int rowsAffected = cmd.ExecuteNonQuery();

				if (rowsAffected == 0) {
					continue;
				}

				toSendToOthers.Add(kvp.Key, kvp.Value);
				callerResponse.Add(kvp.Key);


			}










		}



		public static RegisteredPhoneNumbers FromDataReader(NpgsqlDataReader reader) {

			Guid? id = default;
			string? json = default;

			if (!reader.IsDBNull("id")) {
				id = reader.GetGuid("id");
			}
			if (!reader.IsDBNull("json")) {
				json = reader.GetString("json");
			}

			return new RegisteredPhoneNumbers(
				Id: id,
				Json: json
				);


		}

		[JsonIgnore]
		public JObject? JsonObject
		{
			get {
				if (string.IsNullOrWhiteSpace(Json)) {
					return null;
				}
				return JsonConvert.DeserializeObject(Json, new JsonSerializerSettings() { DateParseHandling = DateParseHandling.None }) as JObject;
			}
		}


		[JsonIgnore]
		public string? PhoneNumber
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyPhoneNumber];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				string str = tok.Value<string>();
				if (string.IsNullOrWhiteSpace(str)) {
					return null;
				}

				return str;
			}
		}

		[JsonIgnore]
		public Guid? BillingCompanyId
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyBillingCompanyId];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				string str = tok.Value<string>();
				if (string.IsNullOrWhiteSpace(str)) {
					return null;
				}

				if (!Guid.TryParse(str, out Guid guid)) {
					return null;
				}

				return guid;
			}
		}



		public static void VerifyRepairTable(NpgsqlConnection db, bool insertDefaultContents = false) {

			if (db.TableExists("registered-phone-numbers")) {
				Log.Debug($"----- Table \"registered-phone-numbers\" exists.");
			} else {
				Log.Information($"----- Table \"registered-phone-numbers\" doesn't exist, creating.");

				using NpgsqlCommand cmd = new NpgsqlCommand(@"
					CREATE TABLE ""public"".""registered-phone-numbers"" (
						""id"" uuid DEFAULT public.uuid_generate_v1() NOT NULL,
						""json"" jsonb DEFAULT '{}'::jsonb NOT NULL,
						CONSTRAINT ""registered_phone_numbers_pk"" PRIMARY KEY(""id"")
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
