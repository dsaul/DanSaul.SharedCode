// (c) 2023 Dan Saul
using Newtonsoft.Json;

namespace DanSaul.SharedCode.Mongo
{
	public record MessageAttachment
	{
		[JsonProperty]
		public string? Key { get; init; }
		[JsonProperty]
		public string? ServiceURI { get; init; }
		[JsonProperty]
		public string? Bucket { get; init; }
		[JsonProperty]
		public string? MediaType { get; init; }
		[JsonProperty]
		public int? ImageWidthPx { get; set; }
		[JsonProperty]
		public int? ImageHeightPx { get; set; }
		[JsonProperty]
		public string? URI
		{
			get
			{
				if (string.IsNullOrEmpty(ServiceURI) ||
					string.IsNullOrEmpty(Bucket) ||
					string.IsNullOrEmpty(Key)
					)
					return null;

				string serviceURI = System.Web.HttpUtility.UrlEncode(ServiceURI);
				string bucket = System.Web.HttpUtility.UrlEncode(Bucket);
				string key = System.Web.HttpUtility.UrlEncode(Key);
				return $"/api/Attachment/{serviceURI}/{bucket}/{key}";
			}
		}
	}
}
