
namespace DanSaul.SharedCode.SignalR
{
	public class IdempotencyResponse : HubResponse
	{
		public string? IdempotencyToken { get; set; } = Guid.NewGuid().ToString();
		public string? RoundTripRequestId { get; set; }
	}
}
