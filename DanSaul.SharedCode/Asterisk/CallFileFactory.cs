using System.Text;

namespace DanSaul.SharedCode.Asterisk
{
	// http://the-asterisk-book.com/1.6/call-file.html
	public class CallFileFactory
	{
		/// <summary>
		/// Creates the content of an asterisk call file.
		/// </summary>
		/// <param name="channel">The channel upon which to initiate the call. Uses the same syntax as the Dial() command</param>
		/// <param name="callerId">The caller ID to be used for the call.</param>
		/// <param name="waitTime">Number of seconds the system waits for the call to be answered. If not specified, defaults to 45 seconds.</param>
		/// <param name="maxRetries">Maximum number of dial retries (if an attempt fails because the device is busy or not reachable). If not specified, defaults to 0 (only one attempt is made).</param>
		/// <param name="retryTime">Number of seconds to wait until the next dial attempt. If not specified, defaults to 300 seconds.</param>
		/// <param name="account">The account code for the CDR.</param>
		/// <param name="context">The destination context.</param>
		/// <param name="extension">The destination extension, in which dialplan execution begins if the device is answered.</param>
		/// <param name="priority">The destination priority. If not specified, defaults to 1.</param>
		/// <param name="setVar">var=value lets you set one or more channel variables.</param>
		/// <param name="archive">By default, call files are deleted immediately upon execution. If Archive: yes is set, they are copied into /var/spool/asterisk/outgoing_done/ instead. Asterisk adds a line to the call file which describes the result: Status: <Expired|Completed|Failed></param>
		/// <returns></returns>
		public static string Create(
			string? channel,
			string? callerId,
			string? waitTime,
			string? maxRetries,
			string? retryTime,
			string? account,
			string? context,
			string? extension,
			string? priority,
			IEnumerable<string>? setVar,
			string? archive
			) {

			StringBuilder sb = new StringBuilder();
			if (null != channel)
				sb.Append($"Channel: {channel}\n");
			if (null != callerId)
				sb.Append($"Callerid: {callerId}\n");
			if (null != waitTime)
				sb.Append($"WaitTime: {waitTime}\n");
			if (null != maxRetries)
				sb.Append($"MaxRetries: {maxRetries}\n");
			if (null != retryTime)
				sb.Append($"RetryTime: {retryTime}\n");
			if (null != account)
				sb.Append($"Account: {account}\n");
			if (null != context)
				sb.Append($"Context: {context}\n");
			if (null != extension)
				sb.Append($"Extension: {extension}\n");
			if (null != priority)
				sb.Append($"Priority: {priority}\n");
			if (null != archive)
				sb.Append($"Archive: {archive}\n");
			if (null != setVar) {
				foreach (string v in setVar) {
					sb.Append($"Setvar: {v}\n");
				}
			}
			return sb.ToString();
		}
	}
}
