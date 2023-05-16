using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using System.Text.RegularExpressions;

namespace DanSaul.SharedCode.Calendar
{
	public static class CalendarUtils
	{
		public static HashSet<CalendarEvent> EventsRightNow(Ical.Net.Calendar calendar) {
			HashSet<CalendarEvent> ret = new HashSet<CalendarEvent>();

			var now = DateTime.Now;

			// This will only get recurring events.
			var recurringEvents = calendar.GetOccurrences(now, now).OrderBy(o => o.Period.StartTime).ToList();
			foreach (Occurrence occ in recurringEvents) {
				CalendarEvent? e = occ.Source as CalendarEvent;
				if (null == e) {
					continue;
				}
				if (e.RecurrenceRules.Count == 0) {
					continue;
				}

				ret.Add(e);
			}

			// Get single events.
			foreach (CalendarEvent e in calendar.Events) {

				// Skip re-occuring.
				if (e.RecurrenceRules.Count > 0) {
					continue;
				}

				var eEnd = e.DtEnd.AsSystemLocal;
				if (eEnd < now) {
					continue;
				}

				var eStart = e.DtStart.AsSystemLocal;
				if (eStart > now) {
					continue;
				}

				ret.Add(e);
			}

			return ret;
		}

		public static HashSet<CalendarOnCallPhoneNumber> CalendarOnCallPhoneNumbersRightNow(Ical.Net.Calendar calendar) {

			HashSet<CalendarOnCallPhoneNumber> ret = new HashSet<CalendarOnCallPhoneNumber>();

			HashSet<CalendarEvent> events = EventsRightNow(calendar);

			foreach (CalendarEvent evt in events) {
				string summary = evt.Summary.Trim();
				string number = evt.Location.Trim();

				ret.Add(new CalendarOnCallPhoneNumber(summary, number));
			}

			return ret;
		}

		public static HashSet<string> CalendarOnCallEMailsRightNow(Ical.Net.Calendar calendar) {

			HashSet<string> ret = new HashSet<string>();

			HashSet<CalendarEvent> events = EventsRightNow(calendar);

			foreach (CalendarEvent evt in events) {
				string description = evt.Description;

				Regex regex = new Regex(@"(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*|""(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21\x23-\x5b\x5d-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])*"")@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?|\[(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?|[a-z0-9-]*[a-z0-9]:(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21-\x5a\x53-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])+)\])");
				foreach (Match match in regex.Matches(description)) {
					ret.Add(match.Value);
				}
			}

			return ret;
		}





		public static HashSet<Occurrence> OccurancesRoughlyAroundThisMonth(Ical.Net.Calendar calendar) {
			DateTime dtNow = DateTime.UtcNow;
			DateTime dtStart = new DateTime(dtNow.Year, dtNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
			dtStart = dtStart.AddDays(-1); // Add buffer for UTC to Localtime.

			DateTime dtEnd = new DateTime(dtNow.Year, dtNow.Month, DateTime.DaysInMonth(dtNow.Year, dtNow.Month), 23, 59, 59);
			dtEnd = dtEnd.AddDays(7);
			dtEnd = dtEnd.AddMonths(6);

			HashSet<Occurrence> occurances = new HashSet<Occurrence>();


			var recurringEvents = calendar.GetOccurrences(dtStart, dtEnd).OrderBy(o => o.Period.StartTime).ToList();
			foreach (Occurrence occ in recurringEvents) {
				CalendarEvent? e = occ.Source as CalendarEvent;
				if (null == e) {
					continue;
				}

				occurances.Add(occ);
			}

			return occurances;
		}




	}
}
