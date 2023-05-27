// (c) 2023 Dan Saul
namespace DanSaul.SharedCode.Asterisk
{
	public record ParsedChannel(
		string? ChannelId,
		string? State,
		string? Time
		);
}
