// (c) 2023 Dan Saul
using Newtonsoft.Json;

namespace DanSaul.SharedCode.Hetzner
{
	[JsonObject(MemberSerialization.OptIn)]
	public record Zone
	{
		[JsonProperty("id")]
		public string? Id { get; init; }
		[JsonProperty("name")]
		public string? Name { get; init; }
	}
}
