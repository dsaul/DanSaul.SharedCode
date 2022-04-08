using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedCode.DatabaseSchemas
{
	public record EMail(Guid? id, string Label, string Value);
}
