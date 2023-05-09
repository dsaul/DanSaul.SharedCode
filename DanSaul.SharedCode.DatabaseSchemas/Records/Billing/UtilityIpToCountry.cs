using Npgsql;
using System.Text;
using System.Data;
using System.Net;
using Serilog;
using DanSaul.SharedCode.DatabaseSchemas.Properties;
using DanSaul.SharedCode.Npgsql;
using DanSaul.SharedCode;

namespace SharedCode.DatabaseSchemas
{
	public record UtilityIpToCountry(
		Guid? Uuid,
		int? IpFrom,
		int? IpTo,
		string? CountryCode,
		string? CountryName
		)
	{
		public static UtilityIpToCountry FromDataReader(NpgsqlDataReader reader) {

			Guid? uuid = default;
			int? ipFrom = default;
			int? ipTo = default;
			string? countryCode = default;
			string? countryName = default;

			if (!reader.IsDBNull("uuid")) {
				uuid = reader.GetGuid("uuid");
			}
			if (!reader.IsDBNull("ip-from")) {
				ipFrom = reader.GetInt32("ip-from");
			}
			if (!reader.IsDBNull("ip-to")) {
				ipTo = reader.GetInt32("ip-to");
			}
			if (!reader.IsDBNull("country-code")) {
				countryCode = reader.GetString("country-code");
			}
			if (!reader.IsDBNull("country-name")) {
				countryName = reader.GetString("country-name");
			}

			return new UtilityIpToCountry(
				Uuid: uuid,
				IpFrom: ipFrom,
				IpTo: ipTo,
				CountryCode: countryCode,
				CountryName: countryName
				);

		}

		public static Dictionary<Guid, UtilityIpToCountry> ForIPAddress(NpgsqlConnection connection, IPAddress ipObj) {

			Dictionary<Guid, UtilityIpToCountry> ret = new Dictionary<Guid, UtilityIpToCountry>();


			// Network Byte Order
			//int ipInt = (int)System.BitConverter.ToUInt32(ipObj.GetAddressBytes(), 0);

			// Host Byte Order
			int ipInt = IPAddress.NetworkToHostOrder(
					(int)System.BitConverter.ToUInt32(ipObj.GetAddressBytes(), 0));


			string sql = @"SELECT * from ""utility-ip-to-country"" WHERE @ipInt >= ""ip-from"" AND @ipInt <= ""ip-to"";";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			cmd.Parameters.AddWithValue("@ipInt", ipInt);

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					UtilityIpToCountry obj = UtilityIpToCountry.FromDataReader(reader);
					if (obj.Uuid == null) {
						continue;
					}
					ret.Add(obj.Uuid.Value, obj);
				}
			}

			return ret;
		}


		public static void VerifyRepairTable(NpgsqlConnection db, bool insertDefaultContents = false) {

			if (db.TableExists("utility-ip-to-country")) {
				Log.Debug($"----- Table \"utility-ip-to-country\" exists.");
			} else {
				Log.Information($"----- Table \"utility-ip-to-country\" doesn't exist, creating.");

				using NpgsqlCommand cmd = new NpgsqlCommand(@"
					CREATE TABLE ""public"".""utility-ip-to-country"" (
						""uuid"" uuid DEFAULT public.uuid_generate_v1() NOT NULL,
						""ip-from"" numeric,
						""ip-to"" numeric,
						""country-code"" character varying(255),
						""country-name"" character varying(255),
						CONSTRAINT ""utility_ip_to_country_pk"" PRIMARY KEY(""uuid"")
					) WITH(oids = false);
					", db);
				cmd.ExecuteNonQuery();
			}


			if (insertDefaultContents) {
				NpgsqlCommand command = new NpgsqlCommand(SQLUtility.RemoveCommentsFromSQLString(Resources.SQLUtilityIPToCountry, true), db);
				command.ExecuteNonQuery();
			}





		}







	}
}
