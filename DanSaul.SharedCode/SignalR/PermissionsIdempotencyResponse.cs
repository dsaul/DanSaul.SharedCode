

namespace DanSaul.SharedCode.SignalR
{
	public class PermissionsIdempotencyResponse : IdempotencyResponse
	{
		public bool IsPermissionsError { get; set; } = false;
	}
}
