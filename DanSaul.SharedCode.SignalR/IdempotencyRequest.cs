using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedCode
{
	public abstract class IdempotencyRequest : HubRequest
	{
		public Guid? SessionId { get; set; }
		public string? IdempotencyToken { get; set; }
		public string? RoundTripRequestId { get; set; }
	}
}
