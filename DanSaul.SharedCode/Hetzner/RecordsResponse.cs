// (c) 2023 Dan Saul
using Newtonsoft.Json;

namespace DanSaul.SharedCode.Hetzner
{
	[JsonObject(MemberSerialization.OptIn)]
	public record RecordsResponse
	{
		[JsonProperty("records")]
		public List<Record> Records { get; init; } = new();
	}
}
