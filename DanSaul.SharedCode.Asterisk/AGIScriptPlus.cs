using System.Diagnostics.CodeAnalysis;
using System.Text;
using Amazon.Polly;
using AsterNET.FastAGI;
using SharedCode.DatabaseSchemas;
using Npgsql;
using Renci.SshNet;
using Serilog;

namespace ARI
{
	

	public abstract class AGIScriptPlus : AGIScript
	{
		public const string kEscapeAllKeys = "0123456789*#";

		
		




		public class AudioPlaybackEvent
		{
			public enum AudioPlaybackEventType
			{
				Unknown,
				Stream,
				SayAlpha,
				TTSText,
				Recording,
			}

			public AudioPlaybackEventType Type { get; set; } = AudioPlaybackEventType.Unknown;
			public string? StreamFile { get; set; } = null;
			public string? Alpha { get; set; } = null;
			public string? Text { get; set; } = null;
			public Engine Engine { get; set; } = Engine.Neural;
			public VoiceId Voice { get; set; } = VoiceId.Brian;
			public NpgsqlConnection? DPDB { get; set; } = null;
			public Guid? RecordingId { get; set; } = null;
		}

		protected string? PromptDigitsPoundTerminated(IEnumerable<AudioPlaybackEvent> playbackEvents, string escapeKeys, int timeout = 5000)
		{
			char key = '\0';
			StringBuilder buffer = new StringBuilder();

			
			do {



				foreach (AudioPlaybackEvent e in playbackEvents) {

					bool stopPlayingAudio = false;

					switch (e.Type) {
						case AudioPlaybackEvent.AudioPlaybackEventType.Stream:
							key = StreamFile(e.StreamFile, escapeKeys);
							if (key != '\0') {
								buffer.Append(key);
							}
							if (key == '#') {
								stopPlayingAudio = true;
								break;
							}
							break;
						case AudioPlaybackEvent.AudioPlaybackEventType.SayAlpha:
							key = SayAlpha(e.Alpha, escapeKeys);
							if (key != '\0') {
								buffer.Append(key);
							}
							if (key == '#') {
								stopPlayingAudio = true;
								break;
							}
							break;
						case AudioPlaybackEvent.AudioPlaybackEventType.TTSText:
							key = PlayTTS(e.Text ?? "", escapeKeys, e.Engine, e.Voice);
							if (key != '\0') {
								buffer.Append(key);
							}
							if (key == '#') {
								stopPlayingAudio = true;
								break;
							}
							break;
						case AudioPlaybackEvent.AudioPlaybackEventType.Recording:
							if (null == e.DPDB) {
								Log.Error("null == e.DPDB");
								break;
							}
							if (null == e.RecordingId) {
								Log.Error("null == e.RecordingId");
								break;
							}
							key = PlayRecording(e.DPDB, e.RecordingId.Value, escapeKeys);
							if (key != '\0') {
								buffer.Append(key);
							}
							if (key == '#') {
								stopPlayingAudio = true;
								break;
							}
							break;
					}

					if (stopPlayingAudio) {
						break;
					}

				}

				while (key != '#') {
					// Wait 5 additional seconds for a digit.
					key = WaitForDigit(timeout);
					if (key == '\0')
						break;

					buffer.Append(key);
				}


			} while (false);

			// Remove # from the end
			if (buffer.ToString().EndsWith('#'))
				buffer.Remove(buffer.Length - 1, 1);

			string? result = buffer.ToString();
			if (string.IsNullOrEmpty(result)) {
				return null;
			}

			return result;


		}


		protected bool? PromptBooleanQuestion(IEnumerable<AudioPlaybackEvent> playbackEvents, int timeout = 5000)
		{
			char key = '\0';

			foreach (AudioPlaybackEvent e in playbackEvents) {

				bool stopPlayingAudio = false;

				switch (e.Type) {
					case AudioPlaybackEvent.AudioPlaybackEventType.Stream:
						key = StreamFile(e.StreamFile, "12");
						if (key == '1' || key == '2') {
							stopPlayingAudio = true;
							break;
						}
						break;
					case AudioPlaybackEvent.AudioPlaybackEventType.SayAlpha:
						key = SayAlpha(e.Alpha, "12");
						if (key == '1' || key == '2') {
							stopPlayingAudio = true;
							break;
						}
						break;
					case AudioPlaybackEvent.AudioPlaybackEventType.TTSText:
						key = PlayTTS(e.Text ?? "", "12", e.Engine, e.Voice);
						if (key == '1' || key == '2') {
							stopPlayingAudio = true;
							break;
						}
						break;
					case AudioPlaybackEvent.AudioPlaybackEventType.Recording:
						if (null == e.DPDB) {
							Log.Error("null == e.DPDB");
							break;
						}
						if (null == e.RecordingId) {
							Log.Error("null == e.RecordingId");
							break;
						}
						key = PlayRecording(e.DPDB, e.RecordingId.Value, "12");
						if (key == '1' || key == '2') {
							stopPlayingAudio = true;
							break;
						}
						break;
				}

				if (stopPlayingAudio) {
					break;
				}

			}

			// After audio is done playing wait a bit longer.
			if (key == '\0') {
				key = WaitForDigit(timeout);
			}




			if (key == '1') {
				return true;
			} else if (key == '2') {
				return false;
			}


			//
			//SayAlpha(companyId);

			return null;
		}


		public static void PBXSyncTTSCacheFull() {

			string? ARI_TO_PBX_SSH_IDRSA_FILE = Environment.GetEnvironmentVariable("ARI_TO_PBX_SSH_IDRSA_FILE");
			if (string.IsNullOrWhiteSpace(ARI_TO_PBX_SSH_IDRSA_FILE)) {
				throw new Exception("ENV VARIABLE ARI_TO_PBX_SSH_IDRSA_FILE NOT SET");
			}

			if (null == SharedCode.ARI.Konstants.PBX_SSH_PORT) {
				throw new Exception("null == PBX_PORT");
			}

			if (null == SharedCode.ARI.Konstants.PBX_SSH_USER) {
				throw new Exception("null == PBX_SSH_USER");
			}


			var pk = new PrivateKeyFile(ARI_TO_PBX_SSH_IDRSA_FILE);
			var keyFiles = new[] { pk };

			var methods = new List<AuthenticationMethod>();
			methods.Add(new PrivateKeyAuthenticationMethod(SharedCode.ARI.Konstants.PBX_SSH_USER, keyFiles));

			var conn = new ConnectionInfo(SharedCode.ARI.Konstants.PBX_FQDN, SharedCode.ARI.Konstants.PBX_SSH_PORT.Value, SharedCode.ARI.Konstants.PBX_SSH_USER, methods.ToArray());

			var sshClient = new SshClient(conn);
			sshClient.Connect();
			SshCommand sc= sshClient.CreateCommand("s3cmd sync s3://tts-cache --skip-existing --delete-removed /srv/tts-cache");
			sc.Execute();
			string answer = sc.Result;
			Log.Debug($"SSH ANSWER {answer}");
		}

		public static void PBXSyncTTSCacheSingle(Cache entry) {

			try {
				if (string.IsNullOrWhiteSpace(SharedCode.ARI.Konstants.PBX_LOCAL_TTS_CACHE_BUCKET_DIRECTORY)) {
					throw new Exception("ENV VARIABLE PBX_LOCAL_TTS_CACHE_BUCKET_DIRECTORY NOT SET");
				}

				if (string.IsNullOrWhiteSpace(SharedCode.ARI.Konstants.ARI_TO_PBX_SSH_IDRSA_FILE)) {
					throw new Exception("ENV VARIABLE ARI_TO_PBX_SSH_IDRSA_FILE NOT SET");
				}

				string? path = entry.S3LocalPCMPath(SharedCode.ARI.Konstants.PBX_LOCAL_TTS_CACHE_BUCKET_DIRECTORY, '/', false);
				if (string.IsNullOrWhiteSpace(path)) {
					throw new Exception("PlayPollyText string.IsNullOrWhiteSpace(path)");
				}

				if (null == SharedCode.ARI.Konstants.PBX_SSH_PORT) {
					throw new Exception("null == PBX_PORT");
				}

				if (null == SharedCode.ARI.Konstants.PBX_SSH_USER) {
					throw new Exception("null == PBX_SSH_USER");
				}

				string cmd = $"bash -c \"if [ ! -f {path} ]; then s3cmd get {entry.S3CMDPCMPath()} {path}; fi\"";
				//Log.Debug(cmd);

				// Tell the PBX To download the file.

				var pk = new PrivateKeyFile(SharedCode.ARI.Konstants.ARI_TO_PBX_SSH_IDRSA_FILE);
				var keyFiles = new[] { pk };

				var methods = new List<AuthenticationMethod>();
				methods.Add(new PrivateKeyAuthenticationMethod(SharedCode.ARI.Konstants.PBX_SSH_USER, keyFiles));

				var conn = new ConnectionInfo(SharedCode.ARI.Konstants.PBX_FQDN, SharedCode.ARI.Konstants.PBX_SSH_PORT.Value, SharedCode.ARI.Konstants.PBX_SSH_USER, methods.ToArray());

				var sshClient = new SshClient(conn);
				sshClient.Connect();
				SshCommand sc= sshClient.CreateCommand(cmd);
				sc.Execute();
				//string answer = sc.Result;
				//Log.Debug($"SSH ANSWER {answer}");
			}
			catch (Exception e) {
				Log.Debug($"Exception: {e.Message}");
				throw;
			}
		}

		public char PlayRecording(NpgsqlConnection DPDB, Guid recordingId, string escapeKeys) {

			if (string.IsNullOrWhiteSpace(SharedCode.ARI.Konstants.PBX_LOCAL_CLIENT_RECORDINGS_DIRECTORY)) {
				throw new Exception("ENV VARIABLE PBX_LOCAL_TTS_CACHE_BUCKET_DIRECTORY NOT SET");
			}

			if (string.IsNullOrWhiteSpace(SharedCode.ARI.Konstants.ARI_TO_PBX_SSH_IDRSA_FILE)) {
				throw new Exception("ENV VARIABLE ARI_TO_PBX_SSH_IDRSA_FILE NOT SET");
			}

			var resRec = Recordings.ForId(DPDB, recordingId);

			Recordings recording = resRec.FirstOrDefault().Value;

			if (null == recording)
				throw new Exception("null == recording");

			PBXSyncUserRecordingSingle(recording);


			string? path = recording.S3LocalPCMPath(SharedCode.ARI.Konstants.PBX_LOCAL_CLIENT_RECORDINGS_DIRECTORY, '/', true);
			if (string.IsNullOrWhiteSpace(path)) {
				throw new Exception("PBXSyncUserRecordingSingle string.IsNullOrWhiteSpace(path)");
			}


			return StreamFile(path, escapeKeys);
		}

		public static void PBXSyncUserRecordingSingle(Recordings recording) {

			try {
				if (recording == null)
					throw new ArgumentNullException(nameof(recording));
				if (string.IsNullOrWhiteSpace(SharedCode.ARI.Konstants.PBX_LOCAL_CLIENT_RECORDINGS_DIRECTORY)) {
					throw new Exception("ENV VARIABLE PBX_LOCAL_TTS_CACHE_BUCKET_DIRECTORY NOT SET");
				}

				if (string.IsNullOrWhiteSpace(SharedCode.ARI.Konstants.ARI_TO_PBX_SSH_IDRSA_FILE)) {
					throw new Exception("ENV VARIABLE ARI_TO_PBX_SSH_IDRSA_FILE NOT SET");
				}



				string? path = recording.S3LocalPCMPath(SharedCode.ARI.Konstants.PBX_LOCAL_CLIENT_RECORDINGS_DIRECTORY, '/', false);
				if (string.IsNullOrWhiteSpace(path)) {
					throw new Exception("PBXSyncUserRecordingSingle string.IsNullOrWhiteSpace(path)");
				}

				if (null == SharedCode.ARI.Konstants.PBX_SSH_PORT) {
					throw new Exception("null == PBX_PORT");
				}

				if (null == SharedCode.ARI.Konstants.PBX_SSH_USER) {
					throw new Exception("null == PBX_SSH_USER");
				}




				string cmd = $"bash -c \"if [ ! -f {path} ]; then s3cmd get {recording.S3CMDPCMURI} {path}; fi\"";
				//Log.Debug(cmd);

				// Tell the PBX To download the file.

				var pk = new PrivateKeyFile(SharedCode.ARI.Konstants.ARI_TO_PBX_SSH_IDRSA_FILE);
				var keyFiles = new[] { pk };

				var methods = new List<AuthenticationMethod>();
				methods.Add(new PrivateKeyAuthenticationMethod(SharedCode.ARI.Konstants.PBX_SSH_USER, keyFiles));

				var conn = new ConnectionInfo(SharedCode.ARI.Konstants.PBX_FQDN, SharedCode.ARI.Konstants.PBX_SSH_PORT.Value, SharedCode.ARI.Konstants.PBX_SSH_USER, methods.ToArray());

				var sshClient = new SshClient(conn);
				sshClient.Connect();
				SshCommand sc= sshClient.CreateCommand(cmd);
				sc.Execute();
				string answer = sc.Result;
				Log.Debug($"SSH ANSWER {answer}");
			}
			catch (Exception e) {
				Log.Error(e, $"Exception: {e.Message}");
				throw;
			}
		}





		public char PlayTTS(string text, string escapeKeys, Engine engine, VoiceId voice, bool ssml = false) {

			//Log.Debug($"PlayTTS {text}");

			if (string.IsNullOrWhiteSpace(SharedCode.ARI.Konstants.PBX_LOCAL_TTS_CACHE_BUCKET_DIRECTORY)) {
				throw new Exception("ENV VARIABLE PBX_LOCAL_TTS_CACHE_BUCKET_DIRECTORY NOT SET");
			}

			if (string.IsNullOrWhiteSpace(SharedCode.ARI.Konstants.ARI_TO_PBX_SSH_IDRSA_FILE)) {
				throw new Exception("ENV VARIABLE ARI_TO_PBX_SSH_IDRSA_FILE NOT SET");
			}

			Cache? entry = PollyText.EnsureDatabaseEntry(text, engine, voice, ssml);
			if (entry == null) {
				Log.Debug("PlayPollyText entry == null");
				return '\0';
			}

			string? path = entry.S3LocalPCMPath(SharedCode.ARI.Konstants.PBX_LOCAL_TTS_CACHE_BUCKET_DIRECTORY, '/', true);
			if (string.IsNullOrWhiteSpace(path)) {
				Log.Debug("PlayPollyText string.IsNullOrWhiteSpace(path)");
				return '\0';
			}

			PBXSyncTTSCacheSingle(entry);


			return StreamFile(path, escapeKeys);
		}

		public char PlayS3File(string s3CmdUri, string escapeKeys) {

			if (string.IsNullOrWhiteSpace(SharedCode.ARI.Konstants.ARI_TO_PBX_SSH_IDRSA_FILE)) {
				throw new Exception("ENV VARIABLE ARI_TO_PBX_SSH_IDRSA_FILE NOT SET");
			}

			if (string.IsNullOrWhiteSpace(SharedCode.ARI.Konstants.PBX_SSH_USER)) {
				throw new Exception("ENV VARIABLE PBX_SSH_USER NOT SET");
			}

			if (string.IsNullOrWhiteSpace(SharedCode.ARI.Konstants.PBX_FQDN)) {
				throw new Exception("ENV VARIABLE PBX_FQDN NOT SET");
			}

			if (null == SharedCode.ARI.Konstants.PBX_SSH_PORT) {
				throw new Exception("ENV VARIABLE PBX_SSH_PORT NOT SET");
			}

			if (string.IsNullOrWhiteSpace(SharedCode.ARI.Konstants.PBX_SSH_USER)) {
				throw new Exception("ENV VARIABLE PBX_SSH_USER NOT SET");
			}

			string[] pathComponents = s3CmdUri.Split('/');
			string last = pathComponents.Last();
			//string[] lastSplit = last.Split(".");
			//string lastExtension = lastSplit.Last();
			string pbxTmpDir = "/tmp";
			string pbxTmpFile = $"{pbxTmpDir}/{last}";
			string filenameWithoutExtension = Path.GetFileNameWithoutExtension(pbxTmpFile);
			string pbxTmpFileWithoutExtension = $"{pbxTmpDir}/{filenameWithoutExtension}";

			string cmd = $"bash -c \"s3cmd get {s3CmdUri} {pbxTmpFile}\"";

			// Tell the PBX To download the file.

			var pk = new PrivateKeyFile(SharedCode.ARI.Konstants.ARI_TO_PBX_SSH_IDRSA_FILE);
			var keyFiles = new[] { pk };

			var methods = new List<AuthenticationMethod>();
			methods.Add(new PrivateKeyAuthenticationMethod(SharedCode.ARI.Konstants.PBX_SSH_USER, keyFiles));

			var conn = new ConnectionInfo(SharedCode.ARI.Konstants.PBX_FQDN, SharedCode.ARI.Konstants.PBX_SSH_PORT.Value, SharedCode.ARI.Konstants.PBX_SSH_USER, methods.ToArray());

			var sshClient = new SshClient(conn);
			sshClient.Connect();
			SshCommand sc= sshClient.CreateCommand(cmd);
			sc.Execute();

			string answer = sc.Result;
			//Log.Debug($"[{request.UniqueId}] S3CMD Upload output: {answer}");


			// Play the file now that it is downloaded.

			char ret = StreamFile(pbxTmpFileWithoutExtension, escapeKeys);

			// Now delete the audio file.

			using SftpClient sftp = new SftpClient(SharedCode.ARI.Konstants.PBX_FQDN, SharedCode.ARI.Konstants.PBX_SSH_PORT.Value, SharedCode.ARI.Konstants.PBX_SSH_USER, keyFiles);
			sftp.Connect();

			if (sftp.Exists(pbxTmpFile)) {

				try {
					sftp.DeleteFile(pbxTmpFile);
				}
				catch (Exception e) {
					Log.Error(e, "Exception deleting remote file.");
				}

			}


			return ret;
		}



		[DoesNotReturn]
		public void ThrowError(AGIRequest request, string code, string error) {
			Log.Information("[{AGIRequestUniqueId}][Code:{Code}] {Error}", request.UniqueId, code, error);
			PlayTTS($"System error, please try again later. Code {code}.", string.Empty, Engine.Neural, VoiceId.Brian);
			throw new PerformHangupException();
		}



	}
}
