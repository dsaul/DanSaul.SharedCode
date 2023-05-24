using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DanSaul.SharedCode.Mongo
{
	[BsonIgnoreExtraElements]
	public class SettingsDocument
	{
		[BsonId]
		public ObjectId Id { get; set; }

		public string? SendFromE164 { get; set; }
	}
}
