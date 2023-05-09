using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DanSaul.SharedCode.SignalR
{
	public abstract class IdempotencyRequest : HubRequest
	{
		public Guid? SessionId { get; set; }
		public string? IdempotencyToken { get; set; }
		public string? RoundTripRequestId { get; set; }
	}
}
