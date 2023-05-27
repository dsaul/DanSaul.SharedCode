// (c) 2023 Dan Saul
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using NodaTime;
using NodaTime.Extensions;
using NodaTime.Text;

namespace DanSaul.SharedCode.Mongo
{
	[BsonIgnoreExtraElements]
	public class MessageDocument
	{
		public const string kBackendServiceTwilio = "Twilio";


		[BsonId]
		public ObjectId Id { get; set; }
		// [BsonElement("")]


		// Twilio
		[JsonProperty]
		public string? TwilioToCountry { get; set; }
		[JsonProperty]
		public string? TwilioToState { get; set; }
		[JsonProperty]
		public string? TwilioSmsMessageSid { get; set; }
		[JsonProperty]
		public string? TwilioNumMedia { get; set; }
		[JsonProperty]
		public string? TwilioToCity { get; set; }
		[JsonProperty]
		public string? TwilioFromZip { get; set; }
		[JsonProperty]
		public string? TwilioSmsSid { get; set; }
		[JsonProperty]
		public string? TwilioFromState { get; set; }
		[JsonProperty]
		public string? TwilioSmsStatus { get; set; }
		[JsonProperty]
		public string? TwilioFromCity { get; set; }
		[JsonProperty]
		public string? TwilioFromCountry { get; set; }
		[JsonProperty]
		public string? TwilioMessagingServiceSid { get; set; }
		[JsonProperty]
		public string? TwilioToZip { get; set; }
		[JsonProperty]
		public string? TwilioNumSegments { get; set; }
		[JsonProperty]
		public string? TwilioReferralNumMedia { get; set; }
		[JsonProperty]
		public string? TwilioMessageSid { get; set; }
		[JsonProperty]
		public string? TwilioAccountSid { get; set; }
		[JsonProperty]
		public string? TwilioURI { get; set; }
		[JsonProperty]
		public string? TwilioApiVersion { get; set; }
		[JsonProperty]
		public string? TwilioMediaUrl0 { get; set; }
		[JsonProperty]
		public string? TwilioMediaUrl1 { get; set; }
		[JsonProperty]
		public string? TwilioMediaUrl2 { get; set; }
		[JsonProperty]
		public string? TwilioMediaUrl3 { get; set; }
		[JsonProperty]
		public string? TwilioMediaUrl4 { get; set; }
		[JsonProperty]
		public string? TwilioMediaUrl5 { get; set; }
		[JsonProperty]
		public string? TwilioMediaUrl6 { get; set; }
		[JsonProperty]
		public string? TwilioMediaUrl7 { get; set; }
		[JsonProperty]
		public string? TwilioMediaUrl8 { get; set; }
		[JsonProperty]
		public string? TwilioMediaUrl9 { get; set; }

		// VoipMS
		[JsonProperty]
		public string? VoIPMSId { get; set; }

		// General
		[JsonProperty]
		public string? ISO8601Timestamp { get; set; } = OffsetDateTimePattern.GeneralIso.Format(SystemClock.Instance.InUtc().GetCurrentOffsetDateTime());
		public string? Price { get; set; }
		[JsonProperty]
		public string? From { get; set; }
		[JsonProperty]
		public string? ErrorMessage { get; set; }
		[JsonProperty]
		public bool? IsAIDetectedSpam { get; set; }
		[JsonProperty]
		public bool? IsHumanConfirmedSpam { get; set; }
		[JsonProperty]
		public string? BackendService { get; set; }
		[JsonProperty]
		public string? Body { get; set; }
		[JsonProperty]
		public string? To { get; set; }
		[JsonProperty]
		public bool IsRead { get; set; } = false;

		[JsonProperty]
		public List<MessageAttachment> Attachments { get; init; } = new List<MessageAttachment>();


		[BsonIgnore]
		[JsonProperty]
		public bool? IsSpam
		{
			get
			{
				if (null != IsHumanConfirmedSpam)
				{
					return IsHumanConfirmedSpam.Value;
				}
				else if (null != IsAIDetectedSpam)
				{
					return IsAIDetectedSpam.Value;
				}
				return null;
			}
		}

	}
}
