using Microsoft.AspNetCore.SignalR.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
