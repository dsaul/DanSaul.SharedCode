using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedCode.Asterisk
{
	public record ParsedTransport(
		string? TransportId,
		string? Type,
		string? Cos,
		string? Tos,
		string? BindAddress
		);
}
