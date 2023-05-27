// (c) 2023 Dan Saul
using Newtonsoft.Json;

namespace DanSaul.SharedCode.Hetzner
{
	[JsonObject(MemberSerialization.OptIn)]
	public record ZonesResponse
	{
		[JsonProperty("zones")]
		public List<Zone> Zones { get; init; } = new();
		
	}
}
