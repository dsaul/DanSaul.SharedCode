using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using FluentEmail.Core;
using FluentEmail.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using NodaTime;
using NodaTime.Text;
using Npgsql;
using Serilog;
using System.Data;
using System.Globalization;
using System.Text;

namespace SharedCode.DatabaseSchemas
{
	public record Voicemails(Guid? Id, string? Json, string? SearchString, string? LastModifiedIso8601) : JSONTable(Id, Json, SearchString, LastModifiedIso8601) {

		public const string kJsonKeyType = "type";
		public const string kJsonKeyOnCallAutoAttendantId = "onCallAutoAttendantId";
		public const string kJsonKeyMessageLeftAtISO8601 = "messageLeftAtISO8601";
		public const string kJsonKeyCallerIdNumber = "callerIdNumber";
		public const string kJsonKeyCallerIdName = "callerIdName";
		public const string kJsonKeyCallbackNumber = "callbackNumber";

		public const string kJsonKeyTimeline = "timeline";
		public const string kJsonKeyTimelineKeyType = "type";
		public const string kJsonKeyTimelineKeyTimestampISO8601 = "timestampISO8601";
		public const string kJsonKeyTimelineKeyDescription = "description";
		public const string kJsonKeyTimelineKeyColour = "colour";

		public const string kJsonKeyRecordingS3Bucket = "recordingsS3Bucket";
		public const string kJsonKeyRecordingS3Host = "recordingsS3Host";
		public const string kJsonKeyRecordingS3Key = "recordingsS3Key";
		public const string kJsonKeyRecordingS3HttpsURI = "recordingS3HttpsURI";
		public const string kJsonKeyRecordingS3CmdURI = "recordingS3CmdURI";

		public const string kJsonKeyOnCallAttemptsProgress = "onCallAttemptsProgress";
		public const string kJsonKeyOnCallAttemptsProgress_KeyCalendarId =  "calendarId";
		public const string kJsonKeyOnCallAttemptsProgress_KeyCallAttempts = "callAttempts";
		public const string kJsonKeyOnCallAttemptsProgress_KeyCallAttemptsMax = "callAttemptsMax";
		public const string kJsonKeyOnCallAttemptsProgress_KeySentMMS = "sentMMS";
		public const string kJsonKeyOnCallAttemptsProgress_KeySentEMail = "sentEMail";
		public const string kJsonKeyOnCallAttemptsProgress_KeyCallFiles = "callFiles";
		public const string kJsonKeyOnCallAttemptsProgress_KeyCallFiles_KeyFileName = "fileName";
		public const string kJsonKeyOnCallAttemptsProgress_KeyCallFiles_KeyIsPBXDone = "isPBXDone";
		public const string kJsonKeyOnCallAttemptsProgress_KeyCallFiles_KeyIsPBXError = "isPBXError";
		public const string kJsonKeyOnCallAttemptsProgress_KeyCallFiles_KeyOriginalCallFile = "originalCallFile";
		public const string kJsonKeyOnCallAttemptsProgress_KeyCallFiles_KeyArchivedCallFile = "archivedCallFile";

		public const string kJsonKeyOnCallAttemptsProgress_KeyGivenUp = "givenUp";

		public const string kJsonKeyTypeValueOnCall = "OnCall";

		public const string kJsonKeyOnCallAttemptsFinished = "onCallAttemptsFinished";
		public const string kJsonKeyIsMarkedHandled = "isMarkedHandled";
		public const string kJsonKeyMarkedHandledBy = "markedHandledBy";
		public const string kJsonKeyNoAgentResponseNotificationNumber = "noAgentResponseNotificationNumber";
		public const string kJsonKeyNoAgentResponseNotificationEMail = "noAgentResponseNotificationEMail";
		public const string kJsonKeyMarkedHandledNotificationEMail = "markedHandledNotificationEMail";
		public const string kJsonKeyNextAttemptAfterISO8601 = "nextAttemptAfterISO8601";
		public const string kJsonKeyMinutesBetweenCallAttempts = "minutesBetweenCallAttempts";
		

		public enum TimelineEntryTypeE {
			Text
		};
		public record TimelineEntry(TimelineEntryTypeE? Type, string? TimestampISO8601, string? Description);

		[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
		public record CallFile(
			string? FileName,
			bool? IsPBXDone,
			bool? IsPBXError,
			bool? HasSubmittedRetry,
			string? OriginalCallFile,
			string? ArchivedCallFile
			);

		[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
		public record CallAttemptsProgressEntry(
			Guid? CalendarId,
			decimal? CallAttempts,
			decimal? CallAttemptsMax,
			List<CallFile> CallFiles,
			bool? SentMMS,
			bool? SentEMail,
			bool? GivenUp
			);


		public Voicemails? GiveUpOnAttemptAtIndex(NpgsqlConnection connection, int index, string? reason) {

			if (null == JsonObject)
				return null;
			if (null == Id)
				return null;

			JObject? newJson = JsonObject.DeepClone() as JObject;
			if (null == newJson)
				return null;

			JArray? timelineArray = newJson[kJsonKeyTimeline] as JArray;
			if (null == timelineArray)
				return null;

			JObject timelineEntry = new JObject {
				[kJsonKeyTimelineKeyType] = "text",
				[kJsonKeyTimelineKeyTimestampISO8601] = DateTime.UtcNow.ToString("o", Culture.DevelopmentCulture),
				[kJsonKeyTimelineKeyColour] = "red",
				[kJsonKeyTimelineKeyDescription] = $"Giving up on calendar {index+1}. {reason}",
			};



			JArray? attemptsArray = newJson[kJsonKeyOnCallAttemptsProgress] as JArray;
			if (null == attemptsArray)
				return null;

			JObject? attempt = attemptsArray[index] as JObject;
			if (null == attempt)
				return null;

			attempt[kJsonKeyOnCallAttemptsProgress_KeyGivenUp] = true;

			Voicemails newVM = this with
			{
				Json = newJson.ToString(Formatting.Indented)
			};
			
			Upsert(connection, new Dictionary<Guid, Voicemails> {
				{ Id.Value, newVM }
			}, out _, out _);

			return newVM;
		}

		public Voicemails? SaveNewBackupCallFileAtIndex(NpgsqlConnection connection, int index, string? description, CallFile callFile) {


			if (null == JsonObject)
				return null;
			if (null == Id)
				return null;

			JObject? newJson = JsonObject.DeepClone() as JObject;
			if (null == newJson)
				return null;

			JArray? timelineArray = newJson[kJsonKeyTimeline] as JArray;
			if (null == timelineArray) {
				timelineArray = new JArray();
				newJson[kJsonKeyTimeline] = timelineArray;
			}

			JObject timelineEntry = new JObject {
				[kJsonKeyTimelineKeyType] = "text",
				[kJsonKeyTimelineKeyTimestampISO8601] = DateTime.UtcNow.ToString("o", Culture.DevelopmentCulture),
				[kJsonKeyTimelineKeyColour] = "orange",
				[kJsonKeyTimelineKeyDescription] = $"{description}",
			};
			timelineArray.Add(timelineEntry);


			JArray? attemptsArray = newJson[kJsonKeyOnCallAttemptsProgress] as JArray;
			if (null == attemptsArray) {
				attemptsArray = new JArray();
				newJson[kJsonKeyOnCallAttemptsProgress] = attemptsArray;
			}

			JObject? attempt = attemptsArray[index] as JObject;
			if (null == attempt)
				return null;

			// Add the call file.
			JArray? callFilesArray = attempt[kJsonKeyOnCallAttemptsProgress_KeyCallFiles] as JArray;
			if (null == callFilesArray) {
				callFilesArray = new JArray();
				attempt[kJsonKeyOnCallAttemptsProgress_KeyCallFiles] = callFilesArray;
			}

			JObject? callFileTok = JObject.FromObject(callFile);
			if (null != callFileTok) {
				callFilesArray.Add(callFileTok);
			}

			Voicemails newVM = this with
			{
				Json = newJson.ToString(Formatting.Indented)
			};

			Upsert(connection, new Dictionary<Guid, Voicemails> {
				{ Id.Value, newVM }
			}, out _, out _);

			return newVM;
		}


		public Voicemails? MarkANewAttemptAtIndex(NpgsqlConnection connection, int index, string? description, IEnumerable<CallFile> callFiles) {

			if (null == JsonObject)
				return null;
			if (null == Id)
				return null;

			JObject? newJson = JsonObject.DeepClone() as JObject;
			if (null == newJson)
				return null;

			JArray? timelineArray = newJson[kJsonKeyTimeline] as JArray;
			if (null == timelineArray) {
				timelineArray = new JArray();
				newJson[kJsonKeyTimeline] = timelineArray;
			}

			JObject timelineEntry = new JObject {
				[kJsonKeyTimelineKeyType] = "text",
				[kJsonKeyTimelineKeyTimestampISO8601] = DateTime.UtcNow.ToString("o", Culture.DevelopmentCulture),
				[kJsonKeyTimelineKeyColour] = "green",
				[kJsonKeyTimelineKeyDescription] = $"Attempted new outbound phone call to {description}",
			};
			timelineArray.Add(timelineEntry);


			JArray? attemptsArray = newJson[kJsonKeyOnCallAttemptsProgress] as JArray;
			if (null == attemptsArray) {
				attemptsArray = new JArray();
				newJson[kJsonKeyOnCallAttemptsProgress] = attemptsArray;
			}

			JObject? attempt = attemptsArray[index] as JObject;
			if (null == attempt)
				return null;


			// Increase the attempt count.
			JToken? attemptCountTok = attempt[kJsonKeyOnCallAttemptsProgress_KeyCallAttempts];
			if (null == attemptCountTok || attemptCountTok.Type == JTokenType.Null) {
				attempt[kJsonKeyOnCallAttemptsProgress_KeyCallAttempts] = 0;
				attemptCountTok = attempt[kJsonKeyOnCallAttemptsProgress_KeyCallAttempts];
			}

			if (null == attemptCountTok)
				throw new InvalidOperationException();

			decimal attemptCount = attemptCountTok.Value<decimal>();
			attemptCount += 1;

			attempt[kJsonKeyOnCallAttemptsProgress_KeyCallAttempts] = attemptCount;

			// Add the call file.
			JArray? callFilesArray = attempt[kJsonKeyOnCallAttemptsProgress_KeyCallFiles] as JArray;
			if (null == callFilesArray) {
				callFilesArray = new JArray();
				attempt[kJsonKeyOnCallAttemptsProgress_KeyCallFiles] = callFilesArray;
			}

			foreach (CallFile callFile in callFiles) {
				JObject? callFileTok = JObject.FromObject(callFile);
				if (null != callFileTok) {
					callFilesArray.Add(callFileTok);
				}
			}

			

			Voicemails newVM = this with
			{
				Json = newJson.ToString(Formatting.Indented)
			};

			Upsert(connection, new Dictionary<Guid, Voicemails> {
				{ Id.Value, newVM }
			}, out _, out _);

			return newVM;
		}


		public Voicemails? MarkSentMMSAtIndex(NpgsqlConnection connection, int index, string timelineDescription) {

			if (null == JsonObject)
				return null;
			if (null == Id)
				return null;

			JObject? newJson = JsonObject.DeepClone() as JObject;
			if (null == newJson)
				return null;

			JArray? timelineArray = newJson[kJsonKeyTimeline] as JArray;
			if (null == timelineArray) {
				timelineArray = new JArray();
				newJson[kJsonKeyTimeline] = timelineArray;
			}

			JObject timelineEntry = new JObject {
				[kJsonKeyTimelineKeyType] = "text",
				[kJsonKeyTimelineKeyTimestampISO8601] = DateTime.UtcNow.ToString("o", Culture.DevelopmentCulture),
				[kJsonKeyTimelineKeyColour] = "green",
				[kJsonKeyTimelineKeyDescription] = $"Sent MMS to {timelineDescription}",
			};
			timelineArray.Add(timelineEntry);


			JArray? attemptsArray = newJson[kJsonKeyOnCallAttemptsProgress] as JArray;
			if (null == attemptsArray)
				return null;

			JObject? attempt = attemptsArray[index] as JObject;
			if (null == attempt)
				return null;

			attempt[kJsonKeyOnCallAttemptsProgress_KeySentMMS] = true;


			Voicemails newVM = this with
			{
				Json = newJson.ToString(Formatting.Indented)
			};

			Upsert(connection, new Dictionary<Guid, Voicemails> {
				{ Id.Value, newVM }
			}, out _, out _);

			return newVM;
		}

		public Voicemails? UpdateVoicemailWithCompletedCallInformation(NpgsqlConnection dpDB, string fileName, string status, string callWasTo, string callFileContents) {

			if (null == JsonObject)
				return null;
			if (null == Id)
				return null;

			JObject? newJson = JsonObject.DeepClone() as JObject;
			if (null == newJson)
				return null;

			JArray? timelineArray = newJson[kJsonKeyTimeline] as JArray;
			if (null == timelineArray) {
				timelineArray = new JArray();
				newJson[kJsonKeyTimeline] = timelineArray;
			}

			JObject timelineEntry = new JObject {
				[kJsonKeyTimelineKeyType] = "text",
				[kJsonKeyTimelineKeyTimestampISO8601] = DateTime.UtcNow.ToString("o", Culture.DevelopmentCulture),
				[kJsonKeyTimelineKeyColour] = status == "Completed" ? "green" : "orange",
				[kJsonKeyTimelineKeyDescription] = status == "Completed" ? 
					$"Phone system succesfully called out to \"{callWasTo}\"." : 
					$"Phone system was not able to call to \"{callWasTo}\" status is \"{status}\".",
			};
			timelineArray.Add(timelineEntry);


			JArray? attemptsArray = newJson[kJsonKeyOnCallAttemptsProgress] as JArray;
			if (null == attemptsArray) {
				attemptsArray = new JArray();
				newJson[kJsonKeyOnCallAttemptsProgress] = attemptsArray;
			}

			foreach (JToken attemptArrayToken in attemptsArray) {
				JObject? attempt = attemptArrayToken as JObject;
				if (attempt == null)
					continue;

				JArray? callFilesArray = attempt[kJsonKeyOnCallAttemptsProgress_KeyCallFiles] as JArray;
				if (null == callFilesArray)
					continue;

				foreach (JToken callFilesArrayToken in callFilesArray) {
					JObject? callFile = callFilesArrayToken as JObject;
					if (callFile == null)
						continue;

					string dbFileName = callFile.Value<string>(kJsonKeyOnCallAttemptsProgress_KeyCallFiles_KeyFileName);

					if (fileName != dbFileName)
						continue;

					callFile[kJsonKeyOnCallAttemptsProgress_KeyCallFiles_KeyIsPBXDone] = true;
					callFile[kJsonKeyOnCallAttemptsProgress_KeyCallFiles_KeyIsPBXError] = status != "Completed";
					callFile[kJsonKeyOnCallAttemptsProgress_KeyCallFiles_KeyArchivedCallFile] = callFileContents;
				}
			}


			Voicemails newVM = this with
			{
				Json = newJson.ToString(Formatting.Indented)
			};

			Upsert(dpDB, new Dictionary<Guid, Voicemails> {
				{ Id.Value, newVM }
			}, out _, out _);

			return newVM;
		}



		public Voicemails? MarkSentEMailAtIndex(NpgsqlConnection connection, int index, string timelineDescription, bool failure) {

			if (null == JsonObject)
				return null;
			if (null == Id)
				return null;

			JObject? newJson = JsonObject.DeepClone() as JObject;
			if (null == newJson)
				return null;

			JArray? timelineArray = newJson[kJsonKeyTimeline] as JArray;
			if (null == timelineArray)
				return null;

			JObject timelineEntry = new JObject {
				[kJsonKeyTimelineKeyType] = "text",
				[kJsonKeyTimelineKeyTimestampISO8601] = DateTime.UtcNow.ToString("o", Culture.DevelopmentCulture),
				[kJsonKeyTimelineKeyColour] = failure ? "red" : "green",
				[kJsonKeyTimelineKeyDescription] = $"{timelineDescription}",
			};
			timelineArray.Add(timelineEntry);


			JArray? attemptsArray = newJson[kJsonKeyOnCallAttemptsProgress] as JArray;
			if (null == attemptsArray)
				return null;

			JObject? attempt = attemptsArray[index] as JObject;
			if (null == attempt)
				return null;

			attempt[kJsonKeyOnCallAttemptsProgress_KeySentEMail] = true;


			Voicemails newVM = this with
			{
				Json = newJson.ToString(Formatting.Indented)
			};

			Upsert(connection, new Dictionary<Guid, Voicemails> {
				{ Id.Value, newVM }
			}, out _, out _);

			return newVM;
		}



		public Voicemails? MarkNextAttemptTime(NpgsqlConnection connection) {

			if (null == JsonObject)
				return null;
			if (null == Id)
				return null;

			JObject? newJson = JsonObject.DeepClone() as JObject;
			if (null == newJson)
				return null;

			// Mark the next time we should try.
			decimal minutes = 5;
			JToken? minutesBetweenTok = newJson[kJsonKeyMinutesBetweenCallAttempts];
			if (null != minutesBetweenTok && minutesBetweenTok.Type != JTokenType.Null) {
				minutes = minutesBetweenTok.Value<decimal>();
			}

			newJson[kJsonKeyNextAttemptAfterISO8601] = DateTime.UtcNow.AddMinutes((double)minutes)
				.ToString("o", Culture.DevelopmentCulture);

			Voicemails newVM = this with
			{
				Json = newJson.ToString(Formatting.Indented)
			};

			Upsert(connection, new Dictionary<Guid, Voicemails> {
				{ Id.Value, newVM }
			}, out _, out _);

			return newVM;
		}

		public Voicemails? MarkFailed(NpgsqlConnection connection) {

			if (null == JsonObject)
				return null;
			if (null == Id)
				return null;

			JObject? newJson = JsonObject.DeepClone() as JObject;
			if (null == newJson)
				return null;

			JArray? timelineArray = newJson[kJsonKeyTimeline] as JArray;
			if (null == timelineArray)
				return null;

			JObject timelineEntry = new JObject {
				[kJsonKeyTimelineKeyType] = "text",
				[kJsonKeyTimelineKeyTimestampISO8601] = DateTime.UtcNow.ToString("o", Culture.DevelopmentCulture),
				[kJsonKeyTimelineKeyColour] = "red",
				[kJsonKeyTimelineKeyDescription] = $"After calling all calendar entries many times, we have been unable to contact any responders. We will dispatch a final call to the No Agent Response Notification Number of {NoAgentResponseNotificationNumber} and stop processing this message.",
			};
			timelineArray.Add(timelineEntry);

			newJson[kJsonKeyOnCallAttemptsFinished] = true;
			newJson[kJsonKeyMarkedHandledBy] = "Failed (Too Many Attempts)";

			Voicemails newVM = this with
			{
				Json = newJson.ToString(Formatting.Indented)
			};

			Upsert(connection, new Dictionary<Guid, Voicemails> {
				{ Id.Value, newVM }
			}, out _, out _);

			return newVM;
		}

		public Voicemails? MarkHandled(NpgsqlConnection connection, string who, string? emailTemplate = null, Guid? billingCompanyId = null, BillingCompanies? billingCompany = null) {

			if (null == JsonObject)
				return null;
			if (null == Id)
				return null;

			JObject? newJson = JsonObject.DeepClone() as JObject;
			if (null == newJson)
				return null;

			// Mark as handled.
			newJson[kJsonKeyIsMarkedHandled] = true;
			newJson[kJsonKeyMarkedHandledBy] = $"\"{who}\" marked this message as handled.";

			JArray? timelineArray = newJson[kJsonKeyTimeline] as JArray;
			if (null == timelineArray)
				return null;

			JObject timelineEntry = new JObject {
				[Voicemails.kJsonKeyTimelineKeyType] = "text",
				[Voicemails.kJsonKeyTimelineKeyTimestampISO8601] = DateTime.UtcNow.ToString("o", Culture.DevelopmentCulture),
				[Voicemails.kJsonKeyTimelineKeyDescription] = $"\"{who}\" marked this message as handled.",
				[Voicemails.kJsonKeyTimelineKeyColour] = "green",
			};
			timelineArray.Add(timelineEntry);

			newJson[kJsonKeyOnCallAttemptsFinished] = true;

			Voicemails newVM = this with
			{
				Json = newJson.ToString(Formatting.Indented)
			};

			Upsert(connection, new Dictionary<Guid, Voicemails> {
				{ Id.Value, newVM }
			}, out _, out _);



			if (
				!string.IsNullOrWhiteSpace(emailTemplate) && 
				!string.IsNullOrWhiteSpace(MarkedHandledNotificationEMail) &&
				null != billingCompanyId &&
				null != billingCompany
				) {
				// Get S3 File for attachment.

				string? key = EnvAmazonS3.S3_PBX_ACCESS_KEY;
				string? secret = EnvAmazonS3.S3_PBX_SECRET_KEY;

				using var s3Client = new AmazonS3Client(key, secret, new AmazonS3Config
						{
					RegionEndpoint = RegionEndpoint.USWest1,
					ServiceURL = EnvAmazonS3.S3_PBX_SERVICE_URI,
					ForcePathStyle = true
				});

				GetObjectRequest request = new GetObjectRequest
				{
					BucketName = RecordingS3Bucket,
					Key = RecordingS3Key,
				};

				using GetObjectResponse s3Response = s3Client.GetObjectAsync(request).Result;

				// Send notification email.

				JObject additionalData = new JObject();
				additionalData["description"] = MarkedHandledNotificationEMail;
				string additionalDataStr = additionalData.ToString();
				byte[] additionalDataBytes = Encoding.UTF8.GetBytes(additionalDataStr);
				string additionalDataBase64 = Convert.ToBase64String(additionalDataBytes);

				using Stream s3ResponseStream = s3Response.ResponseStream;

				string? dateString = null;

				if (null != MessageLeftAtISO8601) {
					ParseResult<Instant> result = InstantPattern.ExtendedIso.Parse(MessageLeftAtISO8601);
					if (result.Success) {
						Instant instant = result.Value;
						DateTimeZone? timeZone = null;
						if (!string.IsNullOrWhiteSpace(billingCompany.IANATimezone)) {
							timeZone = DateTimeZoneProviders.Tzdb[billingCompany.IANATimezone];
						}
						if (null == timeZone) {
							timeZone = DateTimeZoneProviders.Tzdb[BillingCompanies.kJsonValueIANATimezoneDefault];
						}
						ZonedDateTime dtWpg = instant.InZone(timeZone);
						dateString = dtWpg.ToString(@"h:mm tt \o\n dddd MMMM d yyyy", new CultureInfo("en-CA"));
					}
				}

				SendResponse? response = Email
					.From(EnvEmail.SMTP_USERNAME, "On Call Responder")
					.To(MarkedHandledNotificationEMail)
					.Subject("On Call Message (Marked Handled)")
					.UsingTemplate(emailTemplate, new {
						MessageId = Id,
						CallerIdName,
						CallerIdNumber,
						CallbackNumber,
						BillingCompanyId = billingCompanyId,
						BillingCompanyName = billingCompany.FullName,
						DateString = string.IsNullOrWhiteSpace(MessageLeftAtISO8601) ? null : DateTime.Parse(MessageLeftAtISO8601).ToString(@"h:mm tt \o\n dddd MMMM d yyyy"),
						AdditionalDataBase64 = additionalDataBase64,
						ON_CALL_RESPONDER_MESSAGE_ACCESS_BASE_URI = EnvOnCallResponder.ON_CALL_RESPONDER_MESSAGE_ACCESS_BASE_URI
					})
					.Attach(new Attachment{
						Data = s3ResponseStream,
						Filename = "Recording.wav",
						ContentType = "audio/vnd.wav"
					})
					.Send();

				Log.Information("{VoicemailId}, {BillingCompanyId}, {Database} Sent email to {EMailAddress}", Id, billingCompanyId, connection.Database, MarkedHandledNotificationEMail);
			} else {
				Log.Information("{VoicemailId}, {BillingCompanyId}, {Database} Not sending notification email as not enough supporting information provided.", Id, billingCompanyId, connection.Database);
			}

















			return newVM;
		}


		public static Dictionary<Guid, Voicemails> ForId(NpgsqlConnection connection, Guid id) {

			Dictionary<Guid, Voicemails> ret = new Dictionary<Guid, Voicemails>();

			string sql = @"SELECT * from ""voicemails"" WHERE id = @id";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			cmd.Parameters.AddWithValue("@id", id);




			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					Voicemails obj = Voicemails.FromDataReader(reader);
					if (obj.Id == null) {
						continue;
					}
					ret.Add(obj.Id.Value, obj);
				}
			}

			return ret;
		}

		public static Dictionary<Guid, Voicemails> ForOnCallAttemptsFinished(NpgsqlConnection connection, bool onCallAttemptsFinished) {

			Dictionary<Guid, Voicemails> ret = new Dictionary<Guid, Voicemails>();

			string sql = @"SELECT * from ""voicemails"" WHERE (json->>'onCallAttemptsFinished')::boolean = @onCallAttemptsFinished";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			cmd.Parameters.AddWithValue("@onCallAttemptsFinished", onCallAttemptsFinished);




			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					Voicemails obj = Voicemails.FromDataReader(reader);
					if (obj.Id == null) {
						continue;
					}
					ret.Add(obj.Id.Value, obj);
				}
			}

			return ret;
		}


		public static Dictionary<Guid, Voicemails> All(NpgsqlConnection connection) {

			Dictionary<Guid, Voicemails> ret = new Dictionary<Guid, Voicemails>();

			string sql = @"SELECT * from ""voicemails""";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					Voicemails obj = Voicemails.FromDataReader(reader);
					if (obj.Id == null) {
						continue;
					}
					ret.Add(obj.Id.Value, obj);
				}
			}

			return ret;


		}

		public static Dictionary<Guid, Voicemails> FromTheLast30Days(NpgsqlConnection connection) {

			Dictionary<Guid, Voicemails> ret = new Dictionary<Guid, Voicemails>();

			string sql = @"
				SELECT *
				FROM ""voicemails""
				WHERE(""json""->> 'messageLeftAtISO8601')::timestamptz BETWEEN NOW() -INTERVAL '30 DAYS' AND NOW()
				ORDER BY(""json""->> 'messageLeftAtISO8601')::timestamptz ASC
			";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					Voicemails obj = Voicemails.FromDataReader(reader);
					if (obj.Id == null) {
						continue;
					}
					ret.Add(obj.Id.Value, obj);
				}
			}

			return ret;


		}




		public static Dictionary<Guid, Voicemails> ForIds(NpgsqlConnection connection, IEnumerable<Guid> ids) {

			Guid[] idsArr = ids.ToArray();

			Dictionary<Guid, Voicemails> ret = new Dictionary<Guid, Voicemails>();
			if (idsArr.Length == 0)
				return ret;

			List<string> valNames = new List<string>();
			for (int i = 0; i < idsArr.Length; i++) {
				valNames.Add($"@val{i}");
			}

			string sql = $"SELECT * from \"voicemails\" WHERE id IN ({string.Join(", ", valNames)})";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			for (int i = 0; i < valNames.Count; i++) {
				cmd.Parameters.AddWithValue(valNames[i], idsArr[i]);
			}

			using NpgsqlDataReader reader = cmd.ExecuteReader();

			if (reader.HasRows) {
				while (reader.Read()) {
					Voicemails obj = Voicemails.FromDataReader(reader);
					if (obj.Id == null) {
						continue;
					}
					ret.Add(obj.Id.Value, obj);
				}
			}

			return ret;


		}

		public static List<Guid> Delete(NpgsqlConnection connection, List<Guid> idsToDelete) {


			List<Guid> toSendToOthers = new List<Guid>();
			if (idsToDelete.Count == 0) {
				return toSendToOthers;
			}


			List<string> valNames = new List<string>();
			for (int i = 0; i < idsToDelete.Count; i++) {
				valNames.Add($"@val{i}");
			}



			string sql = $"DELETE FROM \"voicemails\" WHERE \"id\" IN ({string.Join(", ", valNames)})";
			using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
			for (int i = 0; i < valNames.Count; i++) {
				cmd.Parameters.AddWithValue(valNames[i], idsToDelete[i]);
			}

			int rowsAffected = cmd.ExecuteNonQuery();
			if (rowsAffected == 0) {
				return toSendToOthers;
			}

			toSendToOthers.AddRange(idsToDelete);
			return toSendToOthers;



		}


		public static void Upsert(
			NpgsqlConnection connection,
			Dictionary<Guid, Voicemails> updateObjects,
			out List<Guid> callerResponse,
			out Dictionary<Guid, Voicemails> toSendToOthers,
			bool printDots = false
			) {

			callerResponse = new List<Guid>();
			toSendToOthers = new Dictionary<Guid, Voicemails>();

			foreach (KeyValuePair<Guid, Voicemails> kvp in updateObjects) {

				string sql = @"
					INSERT INTO
						public.""voicemails""
						(
							""id"",
							""json"",
							""search-string"",
							""last-modified-ISO8601""
						)
					VALUES
						(
							@id,
							CAST(@json AS json),
							@searchString, 
							@lastModifiedISO8601
						)
					ON CONFLICT (""id"") DO UPDATE
						SET
							""json"" = CAST(excluded.json AS json),
							""search-string"" = excluded.""search-string"",
							""last-modified-ISO8601"" = excluded.""last-modified-ISO8601""
					";

				using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
				cmd.Parameters.AddWithValue("@id", kvp.Key);
				cmd.Parameters.AddWithValue("@json", string.IsNullOrWhiteSpace(kvp.Value.Json) ? (object)DBNull.Value : kvp.Value.Json);
				cmd.Parameters.AddWithValue("@searchString", string.IsNullOrWhiteSpace(kvp.Value.SearchString) ? (object)DBNull.Value : kvp.Value.SearchString);
				cmd.Parameters.AddWithValue("@lastModifiedISO8601", string.IsNullOrWhiteSpace(kvp.Value.LastModifiedIso8601) ? (object)DBNull.Value : kvp.Value.LastModifiedIso8601);

				int rowsAffected = cmd.ExecuteNonQuery();

				if (rowsAffected == 0) {
					if (printDots)
						Console.Write("!");
					continue;
				}

				toSendToOthers.Add(kvp.Key, kvp.Value);
				callerResponse.Add(kvp.Key);

				if (printDots)
					Console.Write(".");
			}



		}

		public static Voicemails FromDataReader(NpgsqlDataReader reader) {

			Guid? id = default;
			string? json = default;
			string? searchString = default;
			string? lastModifiedIso8601 = default;


			if (!reader.IsDBNull("id")) {
				id = reader.GetGuid("id");
			}
			if (!reader.IsDBNull("json")) {
				json = reader.GetString("json");
			}
			if (!reader.IsDBNull("search-string")) {
				searchString = reader.GetString("search-string");
			}
			if (!reader.IsDBNull("last-modified-ISO8601")) {
				lastModifiedIso8601 = reader.GetString("last-modified-ISO8601");
			}

			return new Voicemails(
				Id: id,
				Json: json,
				SearchString: searchString,
				LastModifiedIso8601: lastModifiedIso8601
				);
		}










		[JsonIgnore]
		public string GeneratedSearchString
		{
			get {
				return GenerateSearchString();
			}
		}

		public static string GenerateSearchString() {
			return $"";
		}


		public static void VerifyRepairTable(NpgsqlConnection dpDB, bool insertDefaultContents = false) {

			if (dpDB.TableExists("voicemails")) {
				Log.Debug($"----- Table \"voicemails\" exists.");
			} else {
				Log.Debug($"----- Table \"voicemails\" doesn't exist, creating.");

				using NpgsqlCommand cmd = new NpgsqlCommand(@"
					CREATE TABLE ""public"".""voicemails"" (
						""id"" uuid DEFAULT uuid_generate_v1() NOT NULL,
						""json"" json DEFAULT '{}' NOT NULL,
						""search-string"" character varying DEFAULT '',
						""last-modified-ISO8601"" character varying DEFAULT timestamp_iso8601(now(), 'utc') NOT NULL,
						CONSTRAINT ""voicemails_pk"" PRIMARY KEY(""id"")
					) WITH(oids = false);
					", dpDB);
				cmd.ExecuteNonQuery();
			}

#warning TODO: Implement
		}


		public enum TypeE
		{
			OnCall
		}


		[JsonIgnore]
		public TypeE? Type
		{
			get {
				JObject? root = JsonObject;
				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyType];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				string str = tok.Value<string>();
				if (string.IsNullOrWhiteSpace(str)) {
					return null;
				}

				TypeE res;
				if (!Enum.TryParse<TypeE>(str, true, out res)) {
					return null;
				}

				return res;
			}
		}

		[JsonIgnore]
		public Guid? OnCallAutoAttendantId
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyOnCallAutoAttendantId];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				string str = tok.Value<string>();
				if (string.IsNullOrWhiteSpace(str)) {
					return null;
				}

				if (!Guid.TryParse(str, out Guid guid)) {
					return null;
				}

				return guid;
			}
		}



		[JsonIgnore]
		public bool? OnCallAttemptsFinished
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyOnCallAttemptsFinished];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				return tok.Value<bool>();
			}
		}


		[JsonIgnore]
		public bool? IsMarkedHandled
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyIsMarkedHandled];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				return tok.Value<bool>();
			}
		}

		[JsonIgnore]
		public decimal? MinutesBetweenCallAttempts
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyMinutesBetweenCallAttempts];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				return tok.Value<decimal>();
			}
		}

		[JsonIgnore]
		public string? MessageLeftAtISO8601
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyMessageLeftAtISO8601];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				string str = tok.Value<string>();
				if (string.IsNullOrWhiteSpace(str)) {
					return null;
				}

				return str;
			}
		}

		[JsonIgnore]
		public string? CallerIdNumber
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyCallerIdNumber];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				string str = tok.Value<string>();
				if (string.IsNullOrWhiteSpace(str)) {
					return null;
				}

				return str;
			}
		}

		[JsonIgnore]
		public string? CallerIdName
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyCallerIdName];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				string str = tok.Value<string>();
				if (string.IsNullOrWhiteSpace(str)) {
					return null;
				}

				return str;
			}
		}


		[JsonIgnore]
		public string? CallbackNumber
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyCallbackNumber];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				string str = tok.Value<string>();
				if (string.IsNullOrWhiteSpace(str)) {
					return null;
				}

				return str;
			}
		}

		[JsonIgnore]
		public string? NoAgentResponseNotificationNumber
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyNoAgentResponseNotificationNumber];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				string str = tok.Value<string>();
				if (string.IsNullOrWhiteSpace(str)) {
					return null;
				}

				return str;
			}
		}

		[JsonIgnore]
		public string? NoAgentResponseNotificationEMail
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyNoAgentResponseNotificationEMail];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				string str = tok.Value<string>();
				if (string.IsNullOrWhiteSpace(str)) {
					return null;
				}

				return str;
			}
		}

		[JsonIgnore]
		public string? MarkedHandledNotificationEMail
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyMarkedHandledNotificationEMail];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				string str = tok.Value<string>();
				if (string.IsNullOrWhiteSpace(str)) {
					return null;
				}

				return str;
			}
		}


		

		[JsonIgnore]
		public string? NextAttemptAfterISO8601
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyNextAttemptAfterISO8601];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				string str = tok.Value<string>();
				if (string.IsNullOrWhiteSpace(str)) {
					return null;
				}

				return str;
			}
		}

		[JsonIgnore]
		public List<TimelineEntry> Timeline
		{
			get {
				List<TimelineEntry> ret = new List<TimelineEntry>();

				JObject? root = JsonObject;

				if (null == root) {
					return ret;
				}

				JArray? arr = root[kJsonKeyTimeline] as JArray;
				if (null == arr || arr.Type == JTokenType.Null) {
					return ret;
				}

				foreach (JObject tok in arr) {

					//string? typeStr = tok.Value<string>(kJsonKeyTimelineKeyType);

					//TimelineEntryTypeE? type = null;
					//if (!Enum.TryParse<TimelineEntryTypeE>(typeStr, true, out TimelineEntryTypeE tmp)) {
					//	type = tmp;
					//}

					//string? timestampISO8601 = tok.Value<string>(kJsonKeyTimelineKeyTimestampISO8601);
					//string? description = tok.Value<string>(kJsonKeyTimelineKeyDescription);

					//ret.Add(new TimelineEntry(type, timestampISO8601, description));

					TimelineEntry? obj = tok.ToObject<TimelineEntry>();
					if (null != obj) {
						ret.Add(obj);
					}

				}

				return ret;
			}
		}

		[JsonIgnore]
		public List<CallAttemptsProgressEntry> OnCallAttemptsProgress
		{
			get {
				List<CallAttemptsProgressEntry> ret = new List<CallAttemptsProgressEntry>();

				JObject? root = JsonObject;

				if (null == root) {
					return ret;
				}

				JArray? arr = root[kJsonKeyOnCallAttemptsProgress] as JArray;
				if (null == arr || arr.Type == JTokenType.Null) {
					return ret;
				}

				foreach (JObject tok in arr) {

					//string? typeStr = tok.Value<string>(kJsonKeyTimelineKeyType);

					//TimelineEntryTypeE? type = null;
					//if (!Enum.TryParse<TimelineEntryTypeE>(typeStr, true, out TimelineEntryTypeE tmp)) {
					//	type = tmp;
					//}

					//string? timestampISO8601 = tok.Value<string>(kJsonKeyTimelineKeyTimestampISO8601);
					//string? description = tok.Value<string>(kJsonKeyTimelineKeyDescription);

					//ret.Add(new TimelineEntry(type, timestampISO8601, description));

					CallAttemptsProgressEntry? obj = tok.ToObject<CallAttemptsProgressEntry>();
					if (null != obj) {
						ret.Add(obj);
					}

				}

				return ret;
			}
		}

		[JsonIgnore]
		public string? RecordingS3Bucket
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyRecordingS3Bucket];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				string str = tok.Value<string>();
				if (string.IsNullOrWhiteSpace(str)) {
					return null;
				}

				return str;
			}
		}

		[JsonIgnore]
		public string? RecordingS3Host
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyRecordingS3Host];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				string str = tok.Value<string>();
				if (string.IsNullOrWhiteSpace(str)) {
					return null;
				}

				return str;
			}
		}

		[JsonIgnore]
		public string? RecordingS3Key
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyRecordingS3Key];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				string str = tok.Value<string>();
				if (string.IsNullOrWhiteSpace(str)) {
					return null;
				}

				return str;
			}
		}

		[JsonIgnore]
		public string? RecordingS3HttpsURI
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyRecordingS3HttpsURI];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				string str = tok.Value<string>();
				if (string.IsNullOrWhiteSpace(str)) {
					return null;
				}

				return str;
			}
		}

		[JsonIgnore]
		public string? RecordingS3CmdURI
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyRecordingS3CmdURI];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				string str = tok.Value<string>();
				if (string.IsNullOrWhiteSpace(str)) {
					return null;
				}

				return str;
			}
		}


		[JsonIgnore]
		public string? MarkedHandledBy
		{
			get {
				JObject? root = JsonObject;

				if (null == root) {
					return null;
				}

				JToken? tok = root[kJsonKeyMarkedHandledBy];
				if (null == tok || tok.Type == JTokenType.Null) {
					return null;
				}

				string str = tok.Value<string>();
				if (string.IsNullOrWhiteSpace(str)) {
					return null;
				}

				return str;
			}
		}







		


	}
}
