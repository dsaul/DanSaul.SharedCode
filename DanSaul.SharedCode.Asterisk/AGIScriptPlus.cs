using System.Diagnostics.CodeAnalysis;
using System.Text;
using Amazon.Polly;
using AsterNET.FastAGI;
using SharedCode.DatabaseSchemas;
using Npgsql;
using Renci.SshNet;
using Serilog;
using SharedCode;

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

		protected async Task<string?> PromptDigitsPoundTerminated(IEnumerable<AudioPlaybackEvent> playbackEvents, string escapeKeys, int timeout = 5000)
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
							key = await PlayTTS(e.Text ?? "", escapeKeys, e.Engine, e.Voice);
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
							key = await PlayRecording(e.DPDB, e.RecordingId.Value, escapeKeys);
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


		protected async Task<bool?> PromptBooleanQuestion(IEnumerable<AudioPlaybackEvent> playbackEvents, int timeout = 5000)
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
						key = await PlayTTS(e.Text ?? "", "12", e.Engine, e.Voice);
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
						key = await PlayRecording(e.DPDB, e.RecordingId.Value, "12");
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


		public static async Task PBXSyncTTSCacheFull() {


			await AsyncProcess.StartProcess(
				"/usr/bin/s3cmd",
				$" sync s3://tts-cache --skip-existing --delete-removed /tts-cache",
				null,
				1000 * 60 * 60, // 60 minutes
				Console.Out,
				Console.Out);
		}

		public static async Task PBXSyncTTSCacheSingle(Cache entry) {

			try {
				if (string.IsNullOrWhiteSpace(SharedCode.Asterisk.Konstants.PBX_LOCAL_TTS_CACHE_BUCKET_DIRECTORY)) {
					throw new Exception("ENV VARIABLE PBX_LOCAL_TTS_CACHE_BUCKET_DIRECTORY NOT SET");
				}

				string? path = entry.S3LocalPCMPath(SharedCode.Asterisk.Konstants.PBX_LOCAL_TTS_CACHE_BUCKET_DIRECTORY, '/', false);
				if (string.IsNullOrWhiteSpace(path)) {
					throw new Exception("PlayPollyText string.IsNullOrWhiteSpace(path)");
				}

				await AsyncProcess.StartProcess(
					"/bin/bash",
					$" -c \"if [ ! -f {path} ]; then s3cmd get {entry.S3CMDPCMPath()} {path}; fi\"",
					null,
					1000 * 60 * 60, // 60 minutes
					Console.Out,
					Console.Out);

			}
			catch (Exception e) {
				Log.Debug($"Exception: {e.Message}");
				throw;
			}
		}

		public async Task<char> PlayRecording(NpgsqlConnection DPDB, Guid recordingId, string escapeKeys) {

			if (string.IsNullOrWhiteSpace(SharedCode.Asterisk.Konstants.PBX_LOCAL_CLIENT_RECORDINGS_DIRECTORY)) {
				throw new Exception("ENV VARIABLE PBX_LOCAL_TTS_CACHE_BUCKET_DIRECTORY NOT SET");
			}

			var resRec = Recordings.ForId(DPDB, recordingId);

			Recordings recording = resRec.FirstOrDefault().Value;

			if (null == recording)
				throw new Exception("null == recording");

			await PBXSyncUserRecordingSingle(recording);


			string? path = recording.S3LocalPCMPath(SharedCode.Asterisk.Konstants.PBX_LOCAL_CLIENT_RECORDINGS_DIRECTORY, '/', true);
			if (string.IsNullOrWhiteSpace(path)) {
				throw new Exception("PBXSyncUserRecordingSingle string.IsNullOrWhiteSpace(path)");
			}


			return StreamFile(path, escapeKeys);
		}

		public static async Task PBXSyncUserRecordingSingle(Recordings recording) {

			try {
				if (recording == null)
					throw new ArgumentNullException(nameof(recording));
				if (string.IsNullOrWhiteSpace(SharedCode.Asterisk.Konstants.PBX_LOCAL_CLIENT_RECORDINGS_DIRECTORY)) {
					throw new Exception("ENV VARIABLE PBX_LOCAL_TTS_CACHE_BUCKET_DIRECTORY NOT SET");
				}

				string? path = recording.S3LocalPCMPath(SharedCode.Asterisk.Konstants.PBX_LOCAL_CLIENT_RECORDINGS_DIRECTORY, '/', false);
				if (string.IsNullOrWhiteSpace(path)) {
					throw new Exception("PBXSyncUserRecordingSingle string.IsNullOrWhiteSpace(path)");
				}

				await AsyncProcess.StartProcess(
					"/bin/bash",
					$" -c \"if [ ! -f {path} ]; then s3cmd get {recording.S3CMDPCMURI} {path}; fi\"",
					null,
					1000 * 60 * 60, // 60 minutes
					Console.Out,
					Console.Out);
			}
			catch (Exception e) {
				Log.Error(e, $"Exception: {e.Message}");
				throw;
			}
		}

		public async Task<char> PlayTTS(string text, string escapeKeys, Engine engine, VoiceId voice, bool ssml = false) {

			//Log.Debug($"PlayTTS {text}");

			if (string.IsNullOrWhiteSpace(SharedCode.Asterisk.Konstants.PBX_LOCAL_TTS_CACHE_BUCKET_DIRECTORY)) {
				throw new Exception("ENV VARIABLE PBX_LOCAL_TTS_CACHE_BUCKET_DIRECTORY NOT SET");
			}

			Cache? entry = PollyText.EnsureDatabaseEntry(text, engine, voice, ssml);
			if (entry == null) {
				Log.Debug("PlayPollyText entry == null");
				return '\0';
			}

			string? path = entry.S3LocalPCMPath(SharedCode.Asterisk.Konstants.PBX_LOCAL_TTS_CACHE_BUCKET_DIRECTORY, '/', true);
			if (string.IsNullOrWhiteSpace(path)) {
				Log.Debug("PlayPollyText string.IsNullOrWhiteSpace(path)");
				return '\0';
			}

			await PBXSyncTTSCacheSingle(entry);


			return StreamFile(path, escapeKeys);
		}

		public async Task<char> PlayS3File(string s3CmdUri, string escapeKeys) {

			



			string[] pathComponents = s3CmdUri.Split('/');
			string last = pathComponents.Last();
			//string[] lastSplit = last.Split(".");
			//string lastExtension = lastSplit.Last();
			string pbxTmpDir = "/tmp";
			string pbxTmpFile = $"{pbxTmpDir}/{last}";
			string filenameWithoutExtension = Path.GetFileNameWithoutExtension(pbxTmpFile);
			string pbxTmpFileWithoutExtension = $"{pbxTmpDir}/{filenameWithoutExtension}";

			// Tell the PBX To download the file.


			await AsyncProcess.StartProcess(
					"/bin/bash",
					$" -c \"s3cmd get {s3CmdUri} {pbxTmpFile}\"",
					null,
					1000 * 60 * 60, // 60 minutes
					Console.Out,
					Console.Out);

			
			// Play the file now that it is downloaded.

			char ret = StreamFile(pbxTmpFileWithoutExtension, escapeKeys);

			
			return ret;
		}



		[DoesNotReturn]
		public async Task ThrowError(AGIRequest request, string code, string error) {
			Log.Information("[{AGIRequestUniqueId}][Code:{Code}] {Error}", request.UniqueId, code, error);
			await PlayTTS($"System error, please try again later. Code {code}.", string.Empty, Engine.Neural, VoiceId.Brian);
			throw new PerformHangupException();
		}



	}
}
