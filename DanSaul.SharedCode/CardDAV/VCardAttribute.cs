// (c) 2023 Dan Saul
using GraphQL.AspNet.Attributes;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using Serilog;

namespace DanSaul.SharedCode.CardDav
{
	[BsonIgnoreExtraElements]
	public abstract class VCardAttribute
	{
		[BsonIgnore]
		[JsonIgnore]
		[GraphSkip]
		const string kStartSentinelBegin = "BEGIN:VCARD";
		[BsonIgnore]
		[JsonIgnore]
		[GraphSkip]
		const string kStartSentinelVersion = "VERSION";
		[BsonIgnore]
		[JsonIgnore]
		[GraphSkip]
		const string kStartSentinelProdId = "PRODID";
		[BsonIgnore]
		[JsonIgnore]
		[GraphSkip]
		const string kStartSentinelUID = "UID";
		[BsonIgnore]
		[JsonIgnore]
		[GraphSkip]
		const string kStartSentinelFullName = "FN";
		[BsonIgnore]
		[JsonIgnore]
		[GraphSkip]
		const string kStartSentinelName = "N";
		[BsonIgnore]
		[JsonIgnore]
		[GraphSkip]
		const string kStartSentinelTelephone = "TEL";
		[BsonIgnore]
		[JsonIgnore]
		[GraphSkip]
		const string kStartSentinelEMail = "EMAIL";
		[BsonIgnore]
		[JsonIgnore]
		[GraphSkip]
		const string kStartSentinelAddress = "ADR";
		[BsonIgnore]
		[JsonIgnore]
		[GraphSkip]
		const string kStartSentinelBirthday = "BDAY";
		[BsonIgnore]
		[JsonIgnore]
		[GraphSkip]
		const string kStartSentinelPhoto = "PHOTO";
		[BsonIgnore]
		[JsonIgnore]
		[GraphSkip]
		const string kStartSentinelRevisionTime = "REV";
		[BsonIgnore]
		[JsonIgnore]
		[GraphSkip]
		const string kStartSentinelEnd = "END:VCARD";
		[BsonIgnore]
		[JsonIgnore]
		[GraphSkip]
		const string kStartSentinelIM = "IMPP";
		[BsonIgnore]
		[JsonIgnore]
		[GraphSkip]
		const string kStartSentinelCategories = "CATEGORIES";
		[BsonIgnore]
		[JsonIgnore]
		[GraphSkip]
		const string kStartSentinelOrganization = "ORG";

		// Some useless starts
		[BsonIgnore]
		[JsonIgnore]
		[GraphSkip]
		const string kStartSentinelXEVOLUTIONFILEAS = "X-EVOLUTION-FILE-AS";
		[BsonIgnore]
		[JsonIgnore]
		[GraphSkip]
		const string kStartSentinelXMOZILLAHTML = "X-MOZILLA-HTML";
		[BsonIgnore]
		[JsonIgnore]
		[GraphSkip]
		const string kStartSentinelITEM1 = "ITEM1";

		[JsonIgnore]
		[BsonIgnore]
		[GraphSkip]
		public virtual string? Line { get; init; }
		[GraphSkip]
		public static VCardAttribute? FromLine(string line)
		{
			switch (line.ToUpper())
			{
				case string x when x.StartsWith(kStartSentinelVersion):
					return new VCardAttributeVersion { Line = line };
				case string x when x.StartsWith(kStartSentinelProdId):
					return new VCardAttributeProdId { Line = line };
				case string x when x.StartsWith(kStartSentinelUID):
					return new VCardAttributeUID { Line = line };
				case string x when x.StartsWith(kStartSentinelFullName):
					return new VCardAttributeFullName { Line = line };
				case string x when x.StartsWith(kStartSentinelTelephone):
					return new VCardAttributeTelephone { Line = line };
				case string x when x.StartsWith(kStartSentinelEMail):
					return new VCardAttributeEMail { Line = line };
				case string x when x.StartsWith(kStartSentinelAddress):
					return new VCardAttributeAddress { Line = line };
				case string x when x.StartsWith(kStartSentinelBirthday):
					return new VCardAttributeBirthday { Line = line };
				case string x when x.StartsWith(kStartSentinelPhoto):
					return new VCardAttributePhoto { Line = line };
				case string x when x.StartsWith(kStartSentinelRevisionTime):
					return new VCardAttributeRevisionTime { Line = line };
				case string x when x.StartsWith(kStartSentinelIM):
					return new VCardAttributeIM { Line = line };
				case string x when x.StartsWith(kStartSentinelCategories):
					return new VCardAttributeCategories { Line = line };
				case string x when x.StartsWith(kStartSentinelOrganization):
					return new VCardAttributeOrganization { Line = line };
				case string x when (
					x.StartsWith(kStartSentinelXEVOLUTIONFILEAS) || 
					x.StartsWith(kStartSentinelXMOZILLAHTML) ||
					x.StartsWith(kStartSentinelBegin) ||
					x.StartsWith(kStartSentinelEnd) ||
					x.StartsWith(kStartSentinelITEM1)
					):
					return null;
				case string x when x.StartsWith(kStartSentinelName):
					return new VCardAttributeName { Line = line };
				default:
					Log.Error("VCardAttribute Unrecognized Line {line}", line);
					return null;
			}
		}
	}
}
