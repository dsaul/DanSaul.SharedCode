// (c) 2023 Dan Saul
using Newtonsoft.Json;
using NodaTime;
using NodaTime.Extensions;
using NodaTime.Text;

namespace DanSaul.SharedCode.MQTT
{
	public record MQTTMessage
	{
		[JsonProperty]
		public string? ISO8601 { get; init; } = OffsetDateTimePattern.GeneralIso.Format(SystemClock.Instance.InUtc().GetCurrentOffsetDateTime());

	}
}
