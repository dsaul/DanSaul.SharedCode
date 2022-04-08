using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedCode.Asterisk
{
	public record ParsedAor(
		string? Name,
		int? MaxContacts
		);
}
