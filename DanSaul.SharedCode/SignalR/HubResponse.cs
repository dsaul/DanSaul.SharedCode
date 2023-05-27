// (c) 2023 Dan Saul
using Microsoft.AspNetCore.SignalR.Protocol;

namespace DanSaul.SharedCode.SignalR
{
	public abstract class HubResponse : HubMessage
	{
		public bool? IsError { get; set; }
		public string? ErrorMessage { get; set; }
		//public bool? IsPermissionsError { get; set; }
		public bool ForceLogout { get; set; } = false;
	}
}
