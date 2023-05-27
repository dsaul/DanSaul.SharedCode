// (c) 2023 Dan Saul
namespace DanSaul.SharedCode.SignalR
{
	public abstract class IdempotencyRequest : HubRequest
	{
		public Guid? SessionId { get; set; }
		public string? IdempotencyToken { get; set; }
		public string? RoundTripRequestId { get; set; }
	}
}
