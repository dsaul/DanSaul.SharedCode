
using DanSaul.SharedCode.CardDav;

namespace DanSaul.SharedCode.MQTT
{
	public record MQTTMessageContacts : MQTTMessage
	{
		public IEnumerable<VCard> Contacts { get; init; } = Array.Empty<VCard>();
	}
}
