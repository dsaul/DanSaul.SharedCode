// (c) 2023 Dan Saul
using DanSaul.SharedCode.Mongo;
using Newtonsoft.Json;

namespace DanSaul.SharedCode.MQTT
{
	public record MQTTMessageSMS : MQTTMessage
	{
		[JsonProperty]
		public MessageDocument? Message { get; init; }
	}
}
