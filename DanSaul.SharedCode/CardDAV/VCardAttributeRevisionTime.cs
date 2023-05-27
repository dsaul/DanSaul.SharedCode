// (c) 2023 Dan Saul
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace DanSaul.SharedCode.CardDav
{
	[BsonIgnoreExtraElements]
	public record VCardAttributeRevisionTime : VCardAttribute
	{
		[BsonIgnore]
		[JsonIgnore]
		static Regex KRegExRemoveKey = new Regex(@"(?<=REV[;:]).*");
		[BsonElement]
		public string? Value { get; init; }
		[BsonIgnore]
		[JsonIgnore]
		public override string? Line
		{
			init
			{
				base.Line = value;

				string? val = null;

				if (value == null)
					goto setProps;

				MatchCollection matchRemoveKey = KRegExRemoveKey.Matches(value);

				if (matchRemoveKey.Count == 0)
					goto setProps;
				if (!matchRemoveKey[0].Success)
					goto setProps;
				string withoutKey = matchRemoveKey[0].Value;

				val = withoutKey;

			setProps:

				Value = val;

			}
		}
		

	}
}
