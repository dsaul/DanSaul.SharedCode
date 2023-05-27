using DanSaul.SharedCode.StandardizedEnvironmentVariables;
using Newtonsoft.Json;
using RestSharp;
using Serilog;
using System.Net;

namespace DanSaul.SharedCode.Hetzner
{
	public class DynamicDNSManager : IDisposable
	{
		HttpClient HttpClient { get; init; }
		RestClient RestClient { get; init; }
		IPAddress? IP { get; set; } = null;

		public DynamicDNSManager(HttpClient _HttpClient, RestClient _RestClient)
		{
			HttpClient = _HttpClient;
			RestClient = _RestClient;
		}

		public async Task<IPAddress?> GetIP()
		{
			string str = await HttpClient.GetStringAsync("https://api.ipify.org");
			if (!IPAddress.TryParse(str, out IPAddress? _parsed))
				return null;
			return _parsed;
		}

		public async Task Iterate()
		{
			try
			{
				IP = await GetIP();
				if (IP == null)
					throw new Exception("IP == null");

				RestRequest request = new("zones");
				RestResponse response = await RestClient.GetAsync(request);
				string? str = response.Content;
				if (string.IsNullOrWhiteSpace(str))
					throw new Exception("string.IsNullOrWhiteSpace(str)");

				ZonesResponse? zonesResponse = JsonConvert.DeserializeObject<ZonesResponse>(str);
				if (zonesResponse == null)
					throw new Exception("zonesResponse == null");

				var zonesE = from zone in zonesResponse.Zones
							 where zone.Name == EnvHetzner.HETZNER_TARGET_ZONE && zone != null
							 select zone;
				if (!zonesE.Any())
					return;

				foreach (Zone zone in zonesE)
					await ProcessZone(zone);
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Exception");
			}
		}

		async Task ProcessZone(Zone zone)
		{
			Log.Information("Processing {zone}", zone);

			RestRequest request = new($"records?zone_id={zone.Id}");
			RestResponse response = await RestClient.GetAsync(request);
			string? str = response.Content;
			if (string.IsNullOrWhiteSpace(str))
				throw new Exception("string.IsNullOrWhiteSpace(str)");

			RecordsResponse? recordsResponse = JsonConvert.DeserializeObject<RecordsResponse>(str);
			if (recordsResponse == null)
				throw new Exception("recordsResponse == null");

			var recordE = from record in recordsResponse.Records
						  where record.Name == EnvHetzner.HETZNER_TARGET_RECORD_NAME && 
						  record.Type == EnvHetzner.HETZNER_TARGET_RECORD_TYPE && record != null
						  select record;
			if (!recordE.Any())
				return;

			foreach (Record record in recordE)
				await UpdateRecord(record);

		}

		async Task UpdateRecord(Record record)
		{
			Log.Information("Processing {record}", record);

			if (IP == null)
				throw new Exception("IP == null");
			Record mod = record with { Value = IP.ToString() };

			

			RestRequest request = new($"records/{mod.Id}", Method.Put);

			string json = JsonConvert.SerializeObject(mod, Formatting.Indented);

			request.AddBody(json, ContentType.Json);

			RestResponse response = await RestClient.ExecuteAsync(request);
			if (response.StatusCode != HttpStatusCode.OK)
				throw new Exception("Error updating record.");
			string? str = response.Content;

			Log.Information("Updated {record} return {return}", mod, str);
		}



		public async Task Run()
		{
			while(true)
			{
				await Iterate();
				await Task.Delay(1000 * 60 * 10);
			}
		}

		#region IDisposable
		private bool disposedValue;

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					// TODO: dispose managed state (managed objects)
				}

				// TODO: free unmanaged resources (unmanaged objects) and override finalizer
				// TODO: set large fields to null
				disposedValue = true;
			}
		}

		// // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
		// ~DynamicDNSManager()
		// {
		//     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		//     Dispose(disposing: false);
		// }

		public void Dispose()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
