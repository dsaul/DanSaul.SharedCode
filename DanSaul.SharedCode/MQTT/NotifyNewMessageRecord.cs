using DanSaul.SharedCode.Mongo;

namespace DanSaul.SharedCode.MQTT
{
	public record NotifyNewMessageRecord(
		string? MessageId = null,
		string? MessageFrom = null,
		string? MessageBody = null,
		string? ISO8601 = null,
		List<MessageAttachment>? Attachments = null
	);
}
