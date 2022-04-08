using Amazon.Polly;
using Amazon.Polly.Model;
using Amazon;
using System.Text.RegularExpressions;
using Npgsql;
using Newtonsoft.Json.Linq;
using Amazon.S3;
using Amazon.S3.Transfer;
using Serilog;
using SharedCode.DatabaseSchemas;

namespace SharedCode
{
	public static class PollyText
	{


		


		public static Amazon.RegionEndpoint AwsRegion
		{
			get {
				return Amazon.RegionEndpoint.GetBySystemName("us-east-1");
			}
		}

		public static Cache? EnsureDatabaseEntry(string text, Engine engine, VoiceId voice, bool ssml = false) {

			//if (string.IsNullOrWhiteSpace(AWS_SHARED_CREDENTIALS_FILE)) {
			//	throw new Exception("ENV VARIABLE AWS_SHARED_CREDENTIALS_FILE NOT SET");
			//}
			if (string.IsNullOrWhiteSpace(EnvTTS.AWS_PROFILE)) {
				throw new Exception("ENV VARIABLE AWS_PROFILE NOT SET");
			}
			if (string.IsNullOrWhiteSpace(EnvTTS.FFMPEG_PATH)) {
				throw new Exception("ENV VARIABLE FFMPEG_PATH NOT SET");
			}
			if (string.IsNullOrWhiteSpace(EnvDatabases.NPGSQL_CONNECTION_STRING)) {
				throw new Exception("NPGSQL_CONNECTION_STRING_FILE NOT SET");
			}
			if (string.IsNullOrWhiteSpace(EnvDatabases.PGPASSFILE)) {
				throw new Exception("PGPASSFILE NOT SET");
			}
			if (string.IsNullOrWhiteSpace(EnvAmazonS3.S3_PBX_ACCESS_KEY_FILE)) {
				throw new Exception("S3_PBX_ACCESS_KEY_FILE NOT SET");
			}
			if (string.IsNullOrWhiteSpace(EnvAmazonS3.S3_PBX_SECRET_KEY_FILE)) {
				throw new Exception("S3_PBX_SECRET_KEY_FILE NOT SET");
			}
			if (string.IsNullOrWhiteSpace(EnvTTS.AWS_POLLY_ACCESS_KEY_ID_FILE)) {
				throw new Exception("AWS_POLLY_ACCESS_KEY_ID_FILE NOT SET");
			}
			if (string.IsNullOrWhiteSpace(EnvTTS.AWS_POLLY_SECRET_ACCESS_KEY_FILE)) {
				throw new Exception("AWS_POLLY_SECRET_ACCESS_KEY_FILE NOT SET");
			}



			//AWS_SHARED_CREDENTIALS_FILE: /run/secrets/aws-credentials-ari-dispatchpulse
			//FFMPEG_PATH: /usr/bin/ffmpeg
			//POLLY_TEXT_AUDIO_DIRECTORY_ARI: /srv/generated-voice/
			//AWS_PROFILE: ari-dispatchpulse
			// - "4573:4573"

			using NpgsqlConnection cacheDB = new NpgsqlConnection(EnvDatabases.DatabaseConnectionStringForDB(Cache.kTTSCacheDatabaseName));
			cacheDB.Open();

			string textModified = text.Trim();

			Cache? entry = null;
			var resCache = Cache.ForTextEngineVoice(cacheDB, textModified, engine, voice);
			if (resCache.Count > 0) {
				entry = resCache.FirstOrDefault().Value;
			}

			if (null != entry) {
				//Log.Debug("Entry already exists.");
				return entry;
			}

			Guid entryId = Guid.NewGuid();


			string tmpDirPath = Path.GetTempPath();
			string jobDir = Path.Join(tmpDirPath, entryId.ToString());

			if (!Directory.Exists(jobDir)) {
				Directory.CreateDirectory(jobDir);
			}




			var textSimplifiedForPath = Regex.Replace(textModified.ToLower(), "[^A-Za-z0-9]", "-");
			var textSimplifiedForPathTruncated = textSimplifiedForPath.Length <= 100 ? textSimplifiedForPath : textSimplifiedForPath.Substring(0, 100);

			string filenameMP3 = Path.Join(jobDir, $"{entryId}.mp3");
			string filenameWAV = Path.Join(jobDir, $"{entryId}.wav");
			string filenamePCM = Path.Join(jobDir, $"{entryId}.pcm");


			// Get a new generated audio file.

			AmazonPollyClient pc = new AmazonPollyClient(
				awsAccessKeyId: File.ReadAllText(EnvTTS.AWS_POLLY_ACCESS_KEY_ID_FILE),
				awsSecretAccessKey: File.ReadAllText(EnvTTS.AWS_POLLY_SECRET_ACCESS_KEY_FILE),
				AwsRegion);


			SynthesizeSpeechRequest sreq = new SynthesizeSpeechRequest {

				Text = textModified,
				OutputFormat = OutputFormat.Mp3,
				VoiceId = voice,
				Engine = engine
			};
			if (ssml) {
				sreq.TextType = TextType.Ssml;
			}



			SynthesizeSpeechResponse sres = pc.SynthesizeSpeechAsync(sreq).Result;

			using var fileStream = File.Create(filenameMP3);
			sres.AudioStream.CopyTo(fileStream);
			fileStream.Flush();
			fileStream.Close();




			// ffmpeg
			// .\ffmpeg.exe -i cf4f2f2e-dde7-4883-8e56-81b29a780b07-this-is-a-test.mp3 -ar 8000 -ac 1 -ab 64 cf4f2f2e-dde7-4883-8e56-81b29a780b07-this-is-a-test.wav -ar 8000 -ac 1 -ab 64 -f mulaw cf4f2f2e-dde7-4883-8e56-81b29a780b07-this-is-a-test.pcm -map 0:0 -map 0:0

			string strCmdText = $"-loglevel panic -hide_banner -nostats -i {filenameMP3} -ar 8000 -ac 1 -ab 64k {filenameWAV} -ar 8000 -ac 1 -ab 64k -f mulaw {filenamePCM} -map 0:0 -map 0:0";

			Log.Debug($"[EnsureDatabaseEntry()] FFMPEG PATH = {EnvTTS.FFMPEG_PATH}");
			Log.Debug($"[EnsureDatabaseEntry()] FFMPEG ARGUMENTS = {strCmdText}");

			var process =  System.Diagnostics.Process.Start(EnvTTS.FFMPEG_PATH, strCmdText);
			process.WaitForExit(5000);



			// Upload to S3
			var config = new AmazonS3Config
			{
				RegionEndpoint = RegionEndpoint.USWest1,
				ServiceURL = EnvAmazonS3.S3_DISPATCH_PULSE_SERVICE_URI,
				ForcePathStyle = true
			};
			var s3Client = new AmazonS3Client(File.ReadAllText(EnvAmazonS3.S3_PBX_ACCESS_KEY_FILE), File.ReadAllText(EnvAmazonS3.S3_PBX_SECRET_KEY_FILE), config);
			var fileTransferUtility = new TransferUtility(s3Client);

			string? uriMP3 = null;
			if (File.Exists(filenameMP3)) {
				try {
					fileTransferUtility.Upload(filenameMP3, Cache.kLaTeXBucketName, $"{engine}/{voice}/{entryId}/{entryId}.mp3");
					uriMP3 = $"{EnvAmazonS3.S3_DISPATCH_PULSE_SERVICE_URI}/{Cache.kLaTeXBucketName}/{engine}/{voice}/{entryId}/{entryId}.mp3";
				}
				catch (Exception e) {
					Log.Debug($"Error encountered on server. Message:'{e.Message}' when writing an object");
				}
			}

			string? uriWAV = null;
			if (File.Exists(filenameWAV)) {
				try {
					fileTransferUtility.Upload(filenameWAV, Cache.kLaTeXBucketName, $"{engine}/{voice}/{entryId}/{entryId}.wav");
					uriWAV = $"{EnvAmazonS3.S3_DISPATCH_PULSE_SERVICE_URI}/{Cache.kLaTeXBucketName}/{engine}/{voice}/{entryId}/{entryId}.wav";
				}
				catch (Exception e) {
					Log.Debug($"Error encountered on server. Message:'{e.Message}' when writing an object");
				}
			}

			string? uriPCM = null;
			if (File.Exists(filenamePCM)) {
				try {
					fileTransferUtility.Upload(filenamePCM, Cache.kLaTeXBucketName, $"{engine}/{voice}/{entryId}/{entryId}.pcm");
					uriPCM = $"{EnvAmazonS3.S3_DISPATCH_PULSE_SERVICE_URI}/{Cache.kLaTeXBucketName}/{engine}/{voice}/{entryId}/{entryId}.pcm";
				}
				catch (Exception e) {
					Log.Debug($"Error encountered on server. Message:'{e.Message}' when writing an object");
				}
			}





			entry = new Cache(entryId, new JObject {
				[Cache.kJsonKeyText] = textModified,
				[Cache.kJsonKeyEngine] = engine.ToString(),
				[Cache.kJsonKeyVoice] = voice.ToString(),
				[Cache.kJsonKeyS3URIMP3] = uriMP3,
				[Cache.kJsonKeyS3URIWAV] = uriWAV,
				[Cache.kJsonKeyS3URIPCM] = uriPCM,
			}.ToString());

			Log.Debug("Creating new entry.");

			Cache.Upsert(
				cacheDB,
				new Dictionary<Guid, Cache>
				{
					{ entryId, entry }
				},
				out _,
				out _
			);


			Directory.Delete(jobDir, true);



			return entry;
		}



	}
}
