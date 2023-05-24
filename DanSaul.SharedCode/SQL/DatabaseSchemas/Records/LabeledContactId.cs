using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedCode.DatabaseSchemas
{
	public record LabeledContactId(Guid? id, string? Label, Guid? Value);
}
