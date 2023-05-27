// (c) 2023 Dan Saul
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace DanSaul.SharedCode.Mongo
{
	[BsonIgnoreExtraElements]
	public class CardDavContactDocument
	{
		[BsonId]
		public ObjectId Id { get; set; }

		public string? CardDavUUID { get; set; }
		public string? Name { get; set; }
		public string? PhotoContents { get; set; }
		public string? PhotoExtension { get; set; }
		public bool? PhotoIsEmbedded { get; set; }

		public List<string> E164s { get; set; } = new();
	}
}
