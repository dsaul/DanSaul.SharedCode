using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedCode.DatabaseSchemas
{
	public record LabeledCompanyId(Guid? id, string Label, Guid? Value);
}
