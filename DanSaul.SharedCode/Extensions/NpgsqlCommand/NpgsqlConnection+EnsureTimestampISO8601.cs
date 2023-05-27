// (c) 2023 Dan Saul
using Npgsql;
using Serilog;

namespace DanSaul.SharedCode.Npgsql
{
	public static class NpgsqlConnection_EnsureTimestampISO8601
	{
		public static void EnsureTimestampISO8601(this NpgsqlConnection connection) {

			using NpgsqlCommand createUuidCommand = new(@"
CREATE OR REPLACE FUNCTION public.timestamp_iso8601(ts timestamp with time zone, tz text) RETURNS text
    LANGUAGE plpgsql
    AS $$

declare

  res text;

begin

  set datestyle = 'ISO';

  perform set_config('timezone', tz, true);

  res := ts::timestamptz(3)::text;

  reset datestyle;

  reset timezone;

  return replace(res, ' ', 'T') || ':00';

end;

$$;
					", connection);
			createUuidCommand.ExecuteNonQuery();

			Log.Debug("----- Done.");
		}
	}
}
