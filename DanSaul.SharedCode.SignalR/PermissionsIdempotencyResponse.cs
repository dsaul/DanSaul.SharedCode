using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedCode
{
	public class PermissionsIdempotencyResponse : IdempotencyResponse
	{
		public bool IsPermissionsError { get; set; } = false;
	}
}
