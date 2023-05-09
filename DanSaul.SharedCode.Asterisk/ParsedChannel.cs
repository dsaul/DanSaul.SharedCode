using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DanSaul.SharedCode.Asterisk
{
	public record ParsedChannel(
		string? ChannelId,
		string? State,
		string? Time
		);
}
