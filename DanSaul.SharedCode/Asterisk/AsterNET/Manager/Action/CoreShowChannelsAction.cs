using AsterNET.Manager.Event;

namespace AsterNET.Manager.Action
{
    public class CoreShowChannelsAction : ManagerActionEvent
	{
        public override string Action
        {
            get { return "CoreShowChannels"; }
        }

		public override Type ActionCompleteEventClass()
		{
			return typeof(CoreShowChannelsCompleteEvent);
		}
	}
}