using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedCode
{
	public static class NodaTime_GetEndOfHour
	{
		public static ZonedDateTime GetEndOfHour(this ZonedDateTime zdt)
		{
			int minute = 59 - zdt.Minute;
			int second = 59 - zdt.Second;
			int millis =  999 - zdt.Millisecond;

			int totalMillis = millis + (second * 1000) + (minute * 60 * 1000);
			Duration span = Duration.FromMilliseconds(totalMillis);

			return zdt.Plus(span);
		}
	}
}
