using ARI;
using Renci.SshNet;
using Renci.SshNet.Common;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedCode.ARI
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

			if (string.IsNullOrWhiteSpace(SharedCode.ARI.Konstants.ARI_TO_PBX_SSH_IDRSA_FILE)) {
				Log.Error("string.IsNullOrWhiteSpace(SharedCode.ARI.Konstants.ARI_TO_PBX_SSH_IDRSA_FILE)");
				callFileName = null;
				callFileContents = null;
				return false;
			}
			if (string.IsNullOrWhiteSpace(SharedCode.ARI.Konstants.PBX_FQDN)) {
				Log.Error("string.IsNullOrWhiteSpace(SharedCode.ARI.Konstants.PBX_FQDN)");
				callFileName = null;
				callFileContents = null;
				return false;
			}
			if (null == SharedCode.ARI.Konstants.PBX_SSH_PORT) {
				Log.Error("null == SharedCode.ARI.Konstants.PBX_SSH_PORT");
				callFileName = null;
				callFileContents = null;
				return false;
			}
			if (string.IsNullOrWhiteSpace(SharedCode.ARI.Konstants.PBX_SSH_USER)) {
				Log.Error("string.IsNullOrWhiteSpace(SharedCode.ARI.Konstants.PBX_SSH_USER)");
				callFileName = null;
				callFileContents = null;
				return false;
			}
			if (string.IsNullOrWhiteSpace(SharedCode.ARI.Konstants.PBX_LOCAL_OUTGOING_SPOOL_DIRECTORY)) {
				Log.Error("string.IsNullOrWhiteSpace(SharedCode.ARI.Konstants.PBX_LOCAL_OUTGOING_SPOOL_DIRECTORY)");
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

			var pk = new PrivateKeyFile(SharedCode.ARI.Konstants.ARI_TO_PBX_SSH_IDRSA_FILE);
			var keyFiles = new[] { pk };

			using SftpClient sftp = new SftpClient(SharedCode.ARI.Konstants.PBX_FQDN, SharedCode.ARI.Konstants.PBX_SSH_PORT.Value, SharedCode.ARI.Konstants.PBX_SSH_USER, keyFiles);
			sftp.Connect();

			Guid guid = Guid.NewGuid();
			callFileName = $"{callFilePrefix}{callCategory}-callid-{guid}.call";
			string tmpPath = $"/tmp/{callFileName}";
			string spoolPath = $"{SharedCode.ARI.Konstants.PBX_LOCAL_OUTGOING_SPOOL_DIRECTORY}/{callFileName}";
			Log.Debug("callFileName:{callFileName}", callFileName);
			Log.Debug("tmpPath:{tmpPath}", tmpPath);
			Log.Debug("spoolPath:{tmpPath}", spoolPath);

			if (sftp.Exists(tmpPath)) {
				Log.Information($"The temporary call file already exists? Deleting!");

				try {
					sftp.DeleteFile(tmpPath);
				}
				catch (Exception e) {
					Log.Error(e, "Exception deleting remote file {tmpPath}.", tmpPath);
				}

			}

			try {
				sftp.WriteAllText(tmpPath, callFileContents);
			}
			catch (Exception e) {
				Log.Information(e, "Exception writing call file to {tmpPath}.", tmpPath);
			}

			if (sftp.Exists(tmpPath)) {
				try {
					sftp.ChangePermissions(tmpPath, 777);
					sftp.RenameFile(tmpPath, spoolPath);
				}
				catch (SshException e) {
					Log.Error(e, "Exception during RenameFile {tmpPath} to {spoolPath}. Likely " +
						"tried another call while an old one was ongoing. ", tmpPath, spoolPath);
					if (sftp.Exists(tmpPath)) {
						sftp.DeleteFile(tmpPath);
					}

					return false; // returning so that we don't try immediately.
				}
			} else {
				Log.Error("The temporary file is supposed to exist here but doesn't. {tmpPath}", tmpPath);
				return false;
			}

			return true;
		}
	}
}
