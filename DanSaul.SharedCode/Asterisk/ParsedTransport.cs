
namespace DanSaul.SharedCode.Asterisk
{
	public record ParsedTransport(
		string? TransportId,
		string? Type,
		string? Cos,
		string? Tos,
		string? BindAddress
		);
}
