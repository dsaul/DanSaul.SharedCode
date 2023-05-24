using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using Serilog;

namespace DanSaul.SharedCode.CardDav
{
	[BsonIgnoreExtraElements]
	public abstract record VCardAttribute
	{
		[BsonIgnore]
		[JsonIgnore]
		const string kStartSentinelBegin = "BEGIN:VCARD";
		[BsonIgnore]
		[JsonIgnore]
		const string kStartSentinelVersion = "VERSION";
		[BsonIgnore]
		[JsonIgnore]
		const string kStartSentinelProdId = "PRODID";
		[BsonIgnore]
		[JsonIgnore]
		const string kStartSentinelUID = "UID";
		[BsonIgnore]
		[JsonIgnore]
		const string kStartSentinelFullName = "FN";
		[BsonIgnore]
		[JsonIgnore]
		const string kStartSentinelName = "N";
		[BsonIgnore]
		[JsonIgnore]
		const string kStartSentinelTelephone = "TEL";
		[BsonIgnore]
		[JsonIgnore]
		const string kStartSentinelEMail = "EMAIL";
		[BsonIgnore]
		[JsonIgnore]
		const string kStartSentinelAddress = "ADR";
		[BsonIgnore]
		[JsonIgnore]
		const string kStartSentinelBirthday = "BDAY";
		[BsonIgnore]
		[JsonIgnore]
		const string kStartSentinelPhoto = "PHOTO";
		[BsonIgnore]
		[JsonIgnore]
		const string kStartSentinelRevisionTime = "REV";
		[BsonIgnore]
		[JsonIgnore]
		const string kStartSentinelEnd = "END:VCARD";
		[BsonIgnore]
		[JsonIgnore]
		const string kStartSentinelIM = "IMPP";
		[BsonIgnore]
		[JsonIgnore]
		const string kStartSentinelCategories = "CATEGORIES";
		[BsonIgnore]
		[JsonIgnore]
		const string kStartSentinelOrganization = "ORG";

		// Some useless starts
		[BsonIgnore]
		[JsonIgnore]
		const string kStartSentinelXEVOLUTIONFILEAS = "X-EVOLUTION-FILE-AS";
		[BsonIgnore]
		[JsonIgnore]
		const string kStartSentinelXMOZILLAHTML = "X-MOZILLA-HTML";
		[BsonIgnore]
		[JsonIgnore]
		const string kStartSentinelITEM1 = "ITEM1";

		[JsonIgnore]
		[BsonIgnore]
		public virtual string? Line { get; init; }

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
