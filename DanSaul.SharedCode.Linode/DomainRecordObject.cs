using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Serializers.NewtonsoftJson;
using Serilog;
using SharedCode;

namespace DanSaul.SharedCode.Linode
{
	public record DomainRecordObject(
		[JsonProperty("id")] int Id,
		[JsonProperty("type")] string Type,
		[JsonProperty("name")] string Name,
		[JsonProperty("target")] string? Target,
		[JsonProperty("priority")] int Priority,
		[JsonProperty("weight")] int Weight,
		[JsonProperty("port")] int Port,
		[JsonProperty("service")] string? Service,
		[JsonProperty("protocol")] string? Protocol,
		[JsonProperty("ttl_sec")] int TTLSec,
		[JsonProperty("tag")] string? Tag,
		[JsonProperty("created")] string? Created,
		[JsonProperty("updated")] string? Updated
		)
	{
		public static RestClient Client { get; set; } = new RestClient("https://api.linode.com")
		{
			Authenticator = new JwtAuthenticator(EnvLinode.LINODE_API_KEY!)
		}.UseNewtonsoftJson();

		public static async Task<List<DomainRecordObject>> AllRecordsForDomain(int domainId)
		{
			List<DomainRecordObject> ret = new();

			int currentPage = 1;
			while (true)
			{
				// Get all the domain records for this domain.
				var listDomainRecordsRequest = new RestRequest($"/v4/domains/{domainId}/records")
					.AddQueryParameter("page", currentPage)
					.AddQueryParameter("page_size", 100);

				DomainRecordsListResponse? listDomainRecordsResponse = await Client.GetAsync<DomainRecordsListResponse>(listDomainRecordsRequest);
				if (listDomainRecordsResponse == null)
				{
					Log.Error("DomainRecordsListResponse No response from Linode.");
					break;
				}

				ret.AddRange(listDomainRecordsResponse.Data);


				if (listDomainRecordsResponse.Page >= listDomainRecordsResponse.Pages)
					break;

				await Task.Delay(100); // Make sure we don't hammer linode.
				currentPage++;
			}

			return ret;
		}

		public static async Task AddRecord(int domainId, DomainRecordObject obj)
		{
			// POST https://api.linode.com/v4/domains/{domainId}/records

			var updateRecordRequest = new RestRequest($"/v4/domains/{domainId}/records")
				.AddJsonBody(obj);
			await Client.PostAsync(updateRecordRequest);
		}

		public static async Task ModifyRecord(int domainId, DomainRecordObject obj)
		{
			// PUT https://api.linode.com/v4/domains/{domainId}/records/{recordId}

			var updateRecordRequest = new RestRequest($"/v4/domains/{domainId}/records/{obj.Id}")
									.AddJsonBody(obj);
			await Client.PutAsync(updateRecordRequest);
		}

		public static async Task DeleteRecord(int domainId, int recordId)
		{
			// DELETE https://api.linode.com/v4/domains/{domainId}/records/{recordId}

			var updateRecordRequest = new RestRequest($"/v4/domains/{domainId}/records/{recordId}");
			await Client.DeleteAsync(updateRecordRequest);
		}


	}
}
