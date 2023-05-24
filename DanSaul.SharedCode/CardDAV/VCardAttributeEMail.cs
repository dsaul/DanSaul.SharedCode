using DanSaul.SharedCode.Extensions;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using Serilog;
using System.Text.RegularExpressions;

namespace DanSaul.SharedCode.CardDav
{
	[BsonIgnoreExtraElements]
	public record VCardAttributeEMail : VCardAttribute
	{
		[BsonIgnore]
		[JsonIgnore]
		static readonly Regex KRegExRemoveKey = new Regex(@"(?<=EMAIL[;:]).*");
		[BsonIgnore]
		[JsonIgnore]
		static readonly Regex KRegexValueComponents = new Regex(@"(?<type>(?<=TYPE=).*(?=:)):(?<number>.*)");
		[BsonElement]
		public string? Type { get; init; }
		[BsonElement]
		public string? EMail { get; init; }
		[BsonIgnore]
		[JsonIgnore]
		public override string? Line
		{
			init
			{
				base.Line = value;


				string? type = null;
				string? email = null;

				if (value == null)
					goto setProps;

				MatchCollection matchRemoveKey = KRegExRemoveKey.Matches(value);

				if (matchRemoveKey.Count == 0)
					goto setProps;
				if (!matchRemoveKey[0].Success)
					goto setProps;
				string withoutKey = matchRemoveKey[0].Value;

				void HandleMatch(Match matchValueComponents)
				{
					GroupCollection groups = matchValueComponents.Groups;
					foreach (Group group in groups)
					{
						switch (group.Name)
						{
							case "type":
								type = $"{group.Value}".ToLower().UCWords().Trim();
								break;
							case "number":
								email = group.Value;
								break;
						}
					}
				}

				{
					MatchCollection matchCollectionValueComponents = KRegexValueComponents.Matches(withoutKey);
					foreach (Match matchValueComponents in matchCollectionValueComponents)
						HandleMatch(matchValueComponents);

					if (matchCollectionValueComponents.Count != 0)
						goto setProps;
				}

				Log.Error("VCardAttributeEMail no match for line {Line}", value);

			setProps:
				Type = type;
				EMail = email;

			}
		}

	}
}
