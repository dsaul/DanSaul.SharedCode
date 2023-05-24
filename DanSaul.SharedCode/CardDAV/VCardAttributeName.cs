using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace DanSaul.SharedCode.CardDav
{
	[BsonIgnoreExtraElements]
	public record VCardAttributeName : VCardAttribute
	{
		[BsonIgnore]
		[JsonIgnore]
		static Regex KRegExRemoveKey = new Regex(@"(?<=N[;:]).*");
		[BsonElement]
		public IEnumerable<string>? Names { get; init; }
		[BsonIgnore]
		[JsonIgnore]
		public override string? Line
		{
			init
			{
				base.Line = value;

				string[]? names = null;

				if (value == null)
					goto setProps;

				MatchCollection matchRemoveKey = KRegExRemoveKey.Matches(value);
				if (matchRemoveKey.Count == 0)
					goto setProps;
				if (!matchRemoveKey[0].Success)
					goto setProps;
				string withoutKey = matchRemoveKey[0].Value;

				string[] namesUnfiltered = withoutKey.Split(';');
				names = namesUnfiltered.Where(e => !string.IsNullOrEmpty(e?.Trim())).Reverse().ToArray();

			setProps:

				Names = names;
			}
		}

		

	}
}
