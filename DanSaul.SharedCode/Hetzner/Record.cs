using Newtonsoft.Json;

namespace DanSaul.SharedCode.Hetzner
{
	[JsonObject(MemberSerialization.OptIn)]
	public record Record
	{
		[JsonProperty("id")]
		public string? Id { get; init; }
		[JsonProperty("type")]
		public string? Type { get; init; }
		[JsonProperty("name")]
		public string? Name { get; init; }
		[JsonProperty("value")]
		public string? Value { get; init; }
		[JsonProperty("zone_id")]
		public string? ZoneId { get; init; }
		[JsonProperty("created")]
		public string? Created { get; init; }
		[JsonProperty("modified")]
		public string? Modified { get; init; }
	}
}
