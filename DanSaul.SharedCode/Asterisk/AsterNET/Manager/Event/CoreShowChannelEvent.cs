using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsterNET.Manager.Event
{
	/// <summary>
	///     Raised as part of the CoreShowChannelsAction action response list.<br/>
	///     See <see target="_blank"  href="https://wiki.asterisk.org/wiki/display/AST/Asterisk+13+ManagerEvent_CoreShowChannel">https://wiki.asterisk.org/wiki/display/AST/Asterisk+13+ManagerEvent_CoreShowChannel</see>
	/// </summary>
	public class CoreShowChannelEvent : AbstractChannelEvent
	{
		public string? Language { get; set; }
		public string? Context { get; set; }
		public string? Exten { get; set; }
		public string? Priority { get; set; }
		public string? Uniqueid { get; set; }
		public string? Linkedid { get; set; }
		public string? BridgeId { get; set; }
		public string? Application { get; set; }
		public string? ApplicationData { get; set; }
		public string? Duration { get; set; }

		/// <summary>
		///     Creates a new <see cref="CoreShowChannelEvent"/>.
		/// </summary>
		/// <param name="source"><see cref="ManagerConnection"/></param>
		public CoreShowChannelEvent(ManagerConnection source)
			: base(source)
		{
		}
	}
}
