using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace DanSaul.SharedCode.CardDav
{
	[BsonIgnoreExtraElements]
	public record VCardAttributeBirthday : VCardAttribute
	{
		[BsonIgnore]
		[JsonIgnore]
		static readonly Regex KRegExRemoveKey = new Regex(@"(?<=PRODID[;:]).*");
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

				if (null == value)
					goto setProps;


				MatchCollection matchRemoveKey = KRegExRemoveKey.Matches(value);

				if (matchRemoveKey.Count == 0)
					return;
				if (!matchRemoveKey[0].Success)
					return;
				string withoutKey = matchRemoveKey[0].Value;

				val = withoutKey;


			setProps:

				Value = val;
				
			}
		}


	}
}
