using System.Diagnostics.CodeAnalysis;
using System.Text;
using Amazon.Polly;
using AsterNET.FastAGI;
using SharedCode.DatabaseSchemas;
using Npgsql;
using Serilog;
using SharedCode;
using Amazon.S3;
using Amazon;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using DanSaul.SharedCode.Extensions.AmazonS3;
using DanSaul.SharedCode.StandardizedEnvironmentVariables;
using DanSaul.SharedCode.TTS;

namespace DanSaul.SharedCode.Asterisk
{
	public abstract class AGIScriptPlus : AGIScript
	{
		public const string kEscapeAllKeys = "0123456789*#";

		protected string? PromptDigitsPoundTerminated(
			IEnumerable<AudioPlaybackEvent> playbackEvents, 
			string escapeKeys, 
			int timeout = 5000
			)
		{
			char key = '\0';
			StringBuilder buffer = new();


			foreach (AudioPlaybackEvent e in playbackEvents)
			{

				bool stopPlayingAudio = false;

				switch (e.Type)
				{
					case AudioPlaybackEventType.Stream:

						if (string.IsNullOrWhiteSpace(e.StreamFile))
						{
							Log.Warning("e.StreamFile is null or empty");
							break;
						}

						key = StreamFile(e.StreamFile, escapeKeys);
						if (key != '\0')
						{
							buffer.Append(key);
						}
						if (key == '#')
						{
							stopPlayingAudio = true;
							break;
						}
						break;
					case AudioPlaybackEventType.SayAlpha:
						key = SayAlpha(e.Alpha, escapeKeys);
						if (key != '\0')
						{
							buffer.Append(key);
						}
						if (key == '#')
						{
							stopPlayingAudio = true;
							break;
						}
						break;
					case AudioPlaybackEventType.TTSText:
						key = PlayTTS(e.Text ?? "", escapeKeys, e.Engine, e.Voice);
						if (key != '\0')
						{
							buffer.Append(key);
						}
						if (key == '#')
						{
							stopPlayingAudio = true;
							break;
						}
						break;
					case AudioPlaybackEventType.Recording:
						if (null == e.DPDB)
						{
							Log.Error("null == e.DPDB");
							break;
						}
						if (null == e.RecordingId)
						{
							Log.Error("null == e.RecordingId");
							break;
						}
						key = PlayRecording(e.DPDB, e.RecordingId.Value, escapeKeys);
						if (key != '\0')
						{
							buffer.Append(key);
						}
						if (key == '#')
						{
							stopPlayingAudio = true;
							break;
						}
						break;
				}

				if (stopPlayingAudio)
				{
					break;
				}

			}

			while (key != '#')
			{
				// Wait 5 additional seconds for a digit.
				key = WaitForDigit(timeout);
				if (key == '\0')
					break;

				buffer.Append(key);
			}




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
					case AudioPlaybackEventType.Stream:
						key = StreamFile(e.StreamFile, "12");
						if (key == '1' || key == '2') {
							stopPlayingAudio = true;
							break;
						}
						break;
					case AudioPlaybackEventType.SayAlpha:
						key = SayAlpha(e.Alpha, "12");
						if (key == '1' || key == '2') {
							stopPlayingAudio = true;
							break;
						}
						break;
					case AudioPlaybackEventType.TTSText:
						key = PlayTTS(e.Text ?? "", "12", e.Engine, e.Voice);
						if (key == '1' || key == '2') {
							stopPlayingAudio = true;
							break;
						}
						break;
					case AudioPlaybackEventType.Recording:
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


		public static void PBXSyncTTSCacheSingle(Cache entry) {

			try {
				if (string.IsNullOrWhiteSpace(EnvAsterisk.PBX_LOCAL_TTS_CACHE_BUCKET_DIRECTORY)) {
					throw new Exception("[PBXSyncTTSCacheSingle()] ENV VARIABLE PBX_LOCAL_TTS_CACHE_BUCKET_DIRECTORY NOT SET");
				}

				string? path = entry.S3LocalPCMPath(EnvAsterisk.PBX_LOCAL_TTS_CACHE_BUCKET_DIRECTORY, '/', false);
				if (string.IsNullOrWhiteSpace(path)) {
					throw new Exception("[PBXSyncTTSCacheSingle()] PlayPollyText string.IsNullOrWhiteSpace(path)");
				}

				//Log.Debug("[PBXSyncTTSCacheSingle()] S3LocalPCMPath {path}", path);

				string? key = EnvAmazonS3.S3_PBX_ACCESS_KEY;
				string? secret = EnvAmazonS3.S3_PBX_SECRET_KEY;

				using var s3Client = new AmazonS3Client(key, secret, new AmazonS3Config
				{
					RegionEndpoint = RegionEndpoint.USWest1,
					ServiceURL = EnvAmazonS3.S3_PBX_SERVICE_URI,
					ForcePathStyle = true
				});

				S3Utils.DeconstructS3URI(entry.S3URIPCM, out string? s3Key, out string? s3Bucket);

				//Log.Debug("Deconstructed key {key} bucket {bucket}", s3Key, s3Bucket);

				GetObjectRequest request = new()
				{
					BucketName = s3Bucket,
					Key = s3Key,
				};

				string ttsCacheRoot = Path.Join("/srv/tts-cache", s3Key);
				//Log.Debug("ttsCacheRoot {ttsCacheRoot}", ttsCacheRoot);

				TransferUtility ftu = new(s3Client);
				ftu.Download(ttsCacheRoot, s3Bucket, s3Key);


			}
			catch (Exception e) {
				Log.Debug($"Exception: {e.Message}");
				throw;
			}
		}

		public char PlayRecording(NpgsqlConnection DPDB, Guid recordingId, string escapeKeys) {

			if (string.IsNullOrWhiteSpace(EnvAsterisk.PBX_LOCAL_CLIENT_RECORDINGS_DIRECTORY)) {
				throw new Exception("ENV VARIABLE PBX_LOCAL_TTS_CACHE_BUCKET_DIRECTORY NOT SET");
			}

			var resRec = Recordings.ForId(DPDB, recordingId);

			Recordings recording = resRec.FirstOrDefault().Value;

			if (null == recording)
				throw new Exception("null == recording");

			PBXSyncUserRecordingSingle(recording);


			string? path = recording.S3LocalPCMPath(EnvAsterisk.PBX_LOCAL_CLIENT_RECORDINGS_DIRECTORY, '/', true);
			if (string.IsNullOrWhiteSpace(path)) {
				throw new Exception("PBXSyncUserRecordingSingle string.IsNullOrWhiteSpace(path)");
			}


			return StreamFile(path, escapeKeys);
		}

		public static void PBXSyncUserRecordingSingle(Recordings recording) {

			try {
				if (recording == null)
					throw new ArgumentNullException(nameof(recording));
				if (string.IsNullOrWhiteSpace(EnvAsterisk.PBX_LOCAL_CLIENT_RECORDINGS_DIRECTORY)) {
					throw new Exception("ENV VARIABLE PBX_LOCAL_TTS_CACHE_BUCKET_DIRECTORY NOT SET");
				}

				string? path = recording.S3LocalPCMPath(EnvAsterisk.PBX_LOCAL_CLIENT_RECORDINGS_DIRECTORY, '/', false);
				if (string.IsNullOrWhiteSpace(path)) {
					throw new Exception("PBXSyncUserRecordingSingle string.IsNullOrWhiteSpace(path)");
				}

				AsyncProcess.StartProcess(
					"/bin/bash",
					$" -c \"if [ ! -f {path} ]; then s3cmd get {recording.S3CMDPCMURI} {path}; fi\"",
					null,
					1000 * 60 * 60, // 60 minutes
					Console.Out,
					Console.Out).Wait();
			}
			catch (Exception e) {
				Log.Error(e, $"Exception: {e.Message}");
				throw;
			}
		}

		public char PlayTTS(string text, string escapeKeys, Engine engine, VoiceId voice, bool ssml = false) {

			Log.Debug($"[PlayTTS()] PlayTTS {text}");

			if (string.IsNullOrWhiteSpace(EnvAsterisk.PBX_LOCAL_TTS_CACHE_BUCKET_DIRECTORY)) {
				throw new Exception("ENV VARIABLE PBX_LOCAL_TTS_CACHE_BUCKET_DIRECTORY NOT SET");
			}

			Cache? entry = PollyText.EnsureDatabaseEntry(text, engine, voice, ssml);
			if (entry == null) {
				Log.Debug("[PlayTTS()] PlayPollyText entry == null");
				return '\0';
			}

			Log.Debug("[PlayTTS()] Found Entry {entry}", entry);

			string? path = entry.S3LocalPCMPath(EnvAsterisk.PBX_LOCAL_TTS_CACHE_BUCKET_DIRECTORY, '/', true);
			if (string.IsNullOrWhiteSpace(path)) {
				Log.Debug("[PlayTTS()] string.IsNullOrWhiteSpace(path)");
				return '\0';
			}

			Log.Debug("[PlayTTS()] PBX Local PCM Path {path}", path);


			PBXSyncTTSCacheSingle(entry);


			return StreamFile(path, escapeKeys);
		}

		public char PlayS3File(string s3CmdUri, string escapeKeys) {

			



			string[] pathComponents = s3CmdUri.Split('/');
			string last = pathComponents.Last();
			//string[] lastSplit = last.Split(".");
			//string lastExtension = lastSplit.Last();
			string pbxTmpDir = "/tmp";
			string pbxTmpFile = $"{pbxTmpDir}/{last}";
			string filenameWithoutExtension = Path.GetFileNameWithoutExtension(pbxTmpFile);
			string pbxTmpFileWithoutExtension = $"{pbxTmpDir}/{filenameWithoutExtension}";

			// Tell the PBX To download the file.
			string? key = EnvAmazonS3.S3_PBX_ACCESS_KEY;
			string? secret = EnvAmazonS3.S3_PBX_SECRET_KEY;

			


			AsyncProcess.StartProcess(
					"/bin/bash",
					$" -c \"s3cmd get {s3CmdUri} {pbxTmpFile}\"",
					null,
					1000 * 60 * 60, // 60 minutes
					Console.Out,
					Console.Out).Wait();

			
			// Play the file now that it is downloaded.

			char ret = StreamFile(pbxTmpFileWithoutExtension, escapeKeys);

			
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
