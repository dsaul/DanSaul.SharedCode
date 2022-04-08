using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedCode
{
	public static class NodaTime_GetStartOfHour
	{
		public static ZonedDateTime GetStartOfHour(this ZonedDateTime zdt)
		{
			int minute = zdt.Minute;
			int second = zdt.Second;
			int millis = zdt.Millisecond;

			int totalMillis = millis + (second * 1000) + (minute * 60 * 1000);
			Duration span = Duration.FromMilliseconds(totalMillis);
			return zdt.Minus(span);
		}
	}
}
