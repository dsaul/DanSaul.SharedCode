using ARI;
using Mono.Unix;
using Renci.SshNet;
using Renci.SshNet.Common;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedCode.Asterisk
{
	public static class SpooledCall
	{
		public static bool Call(
			string astChannel,
			string context,
			string extension,
			string callFilePrefix,
			string callCategory,
			out string? callFileName,
			out string? callFileContents,
			int waitTime = 30,
			Dictionary<string, string>? variables = null
			) {

			if (string.IsNullOrWhiteSpace(EnvAsterisk.ARI_SPOOL_DIRECTORY))
			{
				Log.Error("string.IsNullOrWhiteSpace(EnvAsterisk.ARI_SPOOL_DIRECTORY)");
				callFileName = null;
				callFileContents = null;
				return false;
			}
			if (string.IsNullOrWhiteSpace(EnvAsterisk.PBX_LOCAL_OUTGOING_SPOOL_DIRECTORY)) {
				Log.Error("string.IsNullOrWhiteSpace(EnvAsterisk.PBX_LOCAL_OUTGOING_SPOOL_DIRECTORY)");
				callFileName = null;
				callFileContents = null;
				return false;
			}


			

			List<string> varList = new List<string>();
			if (null != variables) {
				foreach (KeyValuePair<string, string> kvp in variables) {
					varList.Add($"{kvp.Key}={kvp.Value}");
				}
			}

			// Create and upload the call file to the asterisk server.
			callFileContents = CallFileFactory.Create(
				channel: astChannel,
				callerId: null,
				waitTime: $"{waitTime}",
				maxRetries: "0",
				retryTime: null,
				account: null,
				context: context,
				extension: extension,
				priority: null,
				setVar: varList,
				archive: "yes"
			);






			// Send call file to asterisk
			// We have to write a temporary file first and then move it to the spool directory.

			Log.Debug("Placing Spooled Call");


			string tmpPath = Path.GetTempFileName();
			File.WriteAllText(tmpPath, callFileContents);

			var tmpPathFI = new UnixFileInfo(tmpPath);
			tmpPathFI.FileAccessPermissions = FileAccessPermissions.AllPermissions;


			Guid guid = Guid.NewGuid();
			callFileName = $"{callFilePrefix}{callCategory}-callid-{guid}.call";

			File.Move(tmpPath, Path.Join(EnvAsterisk.ARI_SPOOL_DIRECTORY, callFileName));


			Log.Debug("callFileName:{callFileName}", callFileName);
			Log.Debug("tmpPath:{tmpPath}", tmpPath);

			return true;
		}
	}
}
