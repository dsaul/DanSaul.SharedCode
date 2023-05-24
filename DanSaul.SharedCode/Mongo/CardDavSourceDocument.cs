using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace DanSaul.SharedCode.Mongo
{
	[BsonIgnoreExtraElements]
	public class CardDavSourceDocument
	{
		[BsonId]
		public ObjectId Id { get; set; }

		public string? URI { get; set; }
		public string? AuthType { get; set; }
		public string? UserName { get; set; }
		public string? Password { get; set; }
	}
}
