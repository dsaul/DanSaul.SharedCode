// (c) 2023 Dan Saul
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using GraphQL.AspNet.Attributes;

namespace DanSaul.SharedCode.Mongo
{
	[BsonIgnoreExtraElements]
	public class CardDavSourceDocument
	{
		[BsonId]
		[GraphSkip]
		public ObjectId Id { get; set; }

		[GraphField("id")]
		public string ApolloId
		{
			get
			{
				return Id.ToString();
			}
		}

		public string? URI { get; set; }
		public string? AuthType { get; set; }
		public string? UserName { get; set; }
		public string? Password { get; set; }
	}
}
