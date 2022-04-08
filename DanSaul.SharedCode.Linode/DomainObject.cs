using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Serializers.NewtonsoftJson;
using Serilog;
using SharedCode;

namespace SharedCode
{
	

	public record DomainObject(
		int Id,
		string Type,
		string Domain,
		List<string> Tags,
		string Group,
		string Errors,
		string Description,
		[JsonProperty("soa_email")] string SoaEmail,
		[JsonProperty("retry_sec")] int RetrySec,
		[JsonProperty("master_ips")] List<string> MasterIps,
		[JsonProperty("axfr_ips")] List<string> AxfrIps,
		[JsonProperty("expire_sec")] int ExpireSec,
		[JsonProperty("refresh_sec")] int RefreshSec,
		[JsonProperty("ttl_sec")] int TtlSec,
		string Created,
		string Updated
		)
	{
		public static RestClient Client { get; set; } = new RestClient("https://api.linode.com")
		{
			Authenticator = new JwtAuthenticator(EnvLinode.LINODE_API_KEY!)
		}.UseNewtonsoftJson();

		public static async Task<DomainObject?> ForRootDomainName(string domainName)
		{
			// Get a list of all the domains so we can search by name rather than directly by id.
			var listDomainsRequest = new RestRequest("/v4/domains");
			DomainListResponse? listDomainsResponse = await Client.GetAsync<DomainListResponse>(listDomainsRequest);
			if (listDomainsResponse == null)
			{
				Log.Error("DomainListResponse No response from Linode.");
				return null;
			}

			var e = from obj in listDomainsResponse.Data
					where obj.Domain == domainName
					select obj;
			if (!e.Any())
			{
				return null;
			}

			return e.First();
		}


	}
}
