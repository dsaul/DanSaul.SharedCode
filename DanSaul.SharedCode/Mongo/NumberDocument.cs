// (c) 2023 Dan Saul
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace DanSaul.SharedCode.Mongo
{
	[BsonIgnoreExtraElements]
	public class NumberDocument
	{
		[BsonId]
		public ObjectId Id { get; set; }
		[JsonProperty]
		public bool? IsOwned { get; set; }
		[JsonProperty]
		public string? E164 { get; set; }
		[JsonProperty]
		public bool? CanSend { get; set; }
		[JsonProperty]
		public bool? IsFallback { get; set; }
	}
}
