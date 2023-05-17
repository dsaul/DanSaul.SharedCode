using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsterNET.Manager.Event
{
	/// <summary>
	///     See <see target="_blank"  href="https://wiki.asterisk.org/wiki/display/AST/Asterisk+13+ManagerAction_CoreShowChannels">https://wiki.asterisk.org/wiki/display/AST/Asterisk+13+ManagerAction_CoreShowChannels</see>
	/// </summary>
	/// /// <seealso cref="ConfbridgeListEvent" />
	public class CoreShowChannelsCompleteEvent : ResponseEvent
	{
		/// <summary>
		///     Creates a new <see cref="CoreShowChannelsCompleteEvent"/>.
		/// </summary>
		/// <param name="source"><see cref="ManagerConnection"/></param>
		public CoreShowChannelsCompleteEvent(ManagerConnection source)
			: base(source)
		{
		}
	}
}

