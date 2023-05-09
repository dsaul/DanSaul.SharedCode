
namespace DanSaul.SharedCode.Asterisk
{
	public record ParsedContact(
		string? Name,
		string? Hash,
		string? Status,
		double? RTTms
		);
}
