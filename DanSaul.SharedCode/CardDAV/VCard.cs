// (c) 2023 Dan Saul
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using JsonPropertyAttribute = Newtonsoft.Json.JsonPropertyAttribute;

namespace DanSaul.SharedCode.CardDav
{
	[BsonIgnoreExtraElements]
	public record VCard
	{
		[BsonId]
		[JsonIgnore]
		public ObjectId Id { get; init; } = ObjectId.GenerateNewId();

		[BsonIgnore]
		[JsonIgnore]
		IEnumerable<VCardAttribute> Attributes { get; init; } = new List<VCardAttribute>();
		public static VCard FromLinesAsync(IEnumerable<string> lines)
		{
			List<VCardAttribute?> attributes = new();
			foreach (string line in lines)
			{
				VCardAttribute? attribute = VCardAttribute.FromLine(line);
				attributes.Add(attribute);
			}
			

			VCardAttribute[] attributesFiltered = attributes.Select(e => e).Where(e => e != null).ToArray() as VCardAttribute[];

			VCard card = new() { Attributes = attributesFiltered };

			return card;
		}
		[BsonIgnore]
		[JsonIgnore]
		public IEnumerable<VCardAttributeVersion> VersionAttributes
		{
			get
			{
				return from VCardAttribute attribute in Attributes
					   where attribute is VCardAttributeVersion
					   select attribute as VCardAttributeVersion;
			}
		}
		[BsonElement]
		public string? Version
		{
			get
			{
				return VersionAttributes.FirstOrDefault()?.Value;
			}
			init
			{
				if (value == null)
					return;
				(Attributes as List<VCardAttribute>)?.Add(new VCardAttributeVersion { Value = value });
			}
		}
		[BsonIgnore]
		[JsonIgnore]
		public IEnumerable<VCardAttributeProdId> ProdIdAttributes
		{
			get
			{
				return from VCardAttribute attribute in Attributes
					   where attribute is VCardAttributeProdId
					   select attribute as VCardAttributeProdId;
			}
		}
		[BsonElement]
		public string? ProdId
		{
			get
			{
				return ProdIdAttributes.FirstOrDefault()?.Value;
			}
			init
			{
				if (value == null)
					return;


				(Attributes as List<VCardAttribute>)?.Add(new VCardAttributeProdId { Value = value });
			}
		}
		[BsonIgnore]
		[JsonIgnore]
		public IEnumerable<VCardAttributeUID> UIDAttributes
		{
			get
			{
				return from VCardAttribute attribute in Attributes
					   where attribute is VCardAttributeUID
					   select attribute as VCardAttributeUID;
			}
		}
		[BsonElement("UID")]
		public string? UID
		{
			get
			{
				return UIDAttributes.FirstOrDefault()?.Value;
			}
			init
			{
				if (value == null)
					return;
				(Attributes as List<VCardAttribute>)?.Add(new VCardAttributeUID { Value = value });
			}
		}
		[BsonIgnore]
		[JsonIgnore]
		public IEnumerable<VCardAttributeFullName> FullNameAttributes
		{
			get
			{
				return from VCardAttribute attribute in Attributes
					   where attribute is VCardAttributeFullName
					   select attribute as VCardAttributeFullName;
			}
		}
		[BsonElement]
		public string? FullName
		{
			get
			{
				return FullNameAttributes.FirstOrDefault()?.Value;
			}
			init
			{
				if (value == null)
					return;
				(Attributes as List<VCardAttribute>)?.Add(new VCardAttributeFullName { Value = value });
			}
		}
		[BsonIgnore]
		[JsonIgnore]
		public IEnumerable<VCardAttributeName> NameAttributes
		{
			get
			{
				return from VCardAttribute attribute in Attributes
					   where attribute is VCardAttributeName
					   select attribute as VCardAttributeName;
			}
		}
		[BsonElement]
		public IEnumerable<string> Names
		{
			get
			{
				return NameAttributes.FirstOrDefault()?.Names ?? Array.Empty<string>();
			}
			init
			{
				if (value == null)
					return;
				(Attributes as List<VCardAttribute>)?.Add(new VCardAttributeName { Names = value });
			}
		}
		[BsonIgnore]
		[JsonIgnore]
		public IEnumerable<VCardAttributeTelephone> TelephoneAttributes
		{
			get
			{
				return TelephoneNumbers;
			}
		}
		[BsonElement]
		public IEnumerable<VCardAttributeTelephone> TelephoneNumbers
		{
			get
			{
				return from VCardAttribute attribute in Attributes
					   where attribute is VCardAttributeTelephone
					   select attribute as VCardAttributeTelephone;
			}
			init
			{
				if (value == null)
					return;
				(Attributes as List<VCardAttribute>)?.AddRange(value);
			}
		}
		
		[BsonIgnore]
		[JsonIgnore]
		public IEnumerable<VCardAttributeEMail> EMailAttributes
		{
			get
			{
				return EMails;
			}
		}
		[BsonElement]
		public IEnumerable<VCardAttributeEMail> EMails
		{
			get
			{
				return from VCardAttribute attribute in Attributes
					   where attribute is VCardAttributeEMail
					   select attribute as VCardAttributeEMail;
			}
			init
			{
				if (value == null)
					return;
				(Attributes as List<VCardAttribute>)?.AddRange(value);
			}
		}
		[BsonIgnore]
		[JsonIgnore]
		public IEnumerable<VCardAttributeAddress> AddressAttributes
		{
			get
			{
				return Addresses;
			}
		}
		[BsonElement]
		public IEnumerable<VCardAttributeAddress> Addresses
		{
			get
			{
				return from VCardAttribute attribute in Attributes
					   where attribute is VCardAttributeAddress
					   select attribute as VCardAttributeAddress;
			}
			init
			{
				if (value == null)
					return;
				(Attributes as List<VCardAttribute>)?.AddRange(value);
			}
		}
		[BsonIgnore]
		[JsonIgnore]
		public IEnumerable<VCardAttributeBirthday> BirthdayAttributes
		{
			get
			{
				return from VCardAttribute attribute in Attributes
					   where attribute is VCardAttributeBirthday
					   select attribute as VCardAttributeBirthday;
			}
		}
		[BsonElement]
		public string? Birthday
		{
			get
			{
				return BirthdayAttributes.FirstOrDefault()?.Value;
			}
			init
			{
				(Attributes as List<VCardAttribute>)?.Add(new VCardAttributeBirthday { Value = value });
			}
		}
		[BsonIgnore]
		[JsonIgnore]
		public IEnumerable<VCardAttributePhoto> PhotoAttributes
		{
			get
			{
				return from VCardAttribute attribute in Attributes
					   where attribute is VCardAttributePhoto
					   select attribute as VCardAttributePhoto;
			}
		}
		[BsonIgnore]
		[JsonProperty]
		public string? PhotoURI
		{
			get
			{
				string? uid = UID;
				if (string.IsNullOrEmpty(uid) || null == Photo)
					return null;
				return $"/api/VCard/photo/{System.Web.HttpUtility.UrlEncode(uid)}";
			}
		}
		[BsonElement]
		[JsonIgnore] // This is different for json.
		public VCardAttributePhoto? Photo
		{
			get
			{
				return PhotoAttributes.FirstOrDefault();
			}
			init
			{
				if (null == value)
					return;
				(Attributes as List<VCardAttribute>)?.Add(value);
			}
		}
		[BsonIgnore]
		[JsonIgnore]
		public IEnumerable<VCardAttributeRevisionTime> RevisionTimeAttributes
		{
			get
			{
				return from VCardAttribute attribute in Attributes
					   where attribute is VCardAttributeRevisionTime
					   select attribute as VCardAttributeRevisionTime;
			}
		}
		[BsonElement]
		public string? RevisionTime
		{
			get
			{
				return RevisionTimeAttributes.FirstOrDefault()?.Value;
			}
			init
			{
				(Attributes as List<VCardAttribute>)?.Add(new VCardAttributeRevisionTime { Value = value });
			}
		}

	}
}
