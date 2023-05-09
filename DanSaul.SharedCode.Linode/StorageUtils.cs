using RestSharp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DanSaul.SharedCode.Linode
{
	public class StorageUtils
	{

		static RestClient RestClientLinodeAPI = new RestClient("https://api.linode.com");

		public record GetBucketInfoResponse(string Cluster, string Created, string Hostname, string Label, int Objects, int Size);

		public static async Task<GetBucketInfoResponse?> GetBucketInfo(string bearerToken, string bucketName, string clusterId = "us-east-1") {

			if (string.IsNullOrWhiteSpace(bearerToken)) {
				throw new Exception("string.IsNullOrWhiteSpace(bearerToken)");
			}
			if (string.IsNullOrWhiteSpace(bucketName)) {
				throw new Exception("string.IsNullOrWhiteSpace(bucketName)");
			}
			if (string.IsNullOrWhiteSpace(clusterId)) {
				throw new Exception("string.IsNullOrWhiteSpace(clusterId)");
			}

			// Authorization: Bearer $TOKEN
			// https://api.linode.com/v4/object-storage/buckets/us-east-1/example-bucket
			var request = new RestRequest($"/v4/object-storage/buckets/{clusterId}/{bucketName}", Method.Get);
			request.AddHeader("authorization", $"Bearer {bearerToken}");

			if (null == request) {
				throw new Exception("null == request");
			}

			var resp = await RestClientLinodeAPI.ExecuteAsync(request);

			if (null == resp) {
				return null;
			}


			string respContent = resp.Content;
			if (string.IsNullOrWhiteSpace(respContent)) {
				return null;
			}


			var ret = JsonConvert.DeserializeObject<GetBucketInfoResponse>(respContent, new JsonSerializerSettings() { DateParseHandling = DateParseHandling.None });
			if (null == ret) {
				return null;
			}


			return ret;
		}
	}
}
