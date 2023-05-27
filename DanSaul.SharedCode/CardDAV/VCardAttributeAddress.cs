// (c) 2023 Dan Saul
using DanSaul.SharedCode.Extensions;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using Serilog;
using System.Text.RegularExpressions;

namespace DanSaul.SharedCode.CardDav
{
	[BsonIgnoreExtraElements]
	public record VCardAttributeAddress : VCardAttribute
	{
		[BsonIgnore]
		[JsonIgnore]
		static readonly Regex KRegExRemoveKey = new(@"(?<=ADR[;:]).*");
		[BsonIgnore]
		[JsonIgnore]
		static readonly Regex KRegexValueComponentsWithLabel = new(@"(?<type>(?<=TYPE=).*(?=;));LABEL=(?<label>.*):(?<address>.*)");
		[BsonIgnore]
		[JsonIgnore]
		static readonly Regex KRegexValueComponentsWithoutLabel = new(@"(?<type>(?<=TYPE=).*(?=:)):(?<address>.*)");
		[BsonElement]
		public string? Type { get; init; }
		[BsonElement]
		public string? Label { get; init; }
		[BsonElement]
		public IEnumerable<string>? AddressParts { get; init; }

		[BsonIgnore]
		[JsonIgnore]
		public override string? Line
		{
			init {
				base.Line = value;


				string? type = null;
				string? label = null;
				string[]? addressParts = null;

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
							case "label":
								label = $"{group.Value}".ToLower().UCWords().Trim();
								break;
							case "address":

								string raw = $"{group.Value}".Trim();
								string[] parts = raw.Split(';');

								addressParts = (from string part in parts
												where !string.IsNullOrEmpty(part)
												select part).ToArray();
								break;
						}
					}
				}

				{
					MatchCollection matchCollectionValueComponents = KRegexValueComponentsWithLabel.Matches(withoutKey);
					foreach (Match matchValueComponents in matchCollectionValueComponents)
						HandleMatch(matchValueComponents);

					if (matchCollectionValueComponents.Count != 0)
						goto setProps;
				}

				{
					MatchCollection matchCollectionValueComponents = KRegexValueComponentsWithoutLabel.Matches(withoutKey);
					foreach (Match matchValueComponents in matchCollectionValueComponents)
						HandleMatch(matchValueComponents);

					if (matchCollectionValueComponents.Count != 0)
						goto setProps;
				}

				if (withoutKey.Contains(';'))
				{
					string raw = $"{withoutKey}".Trim();
					string[] parts = raw.Split(';');

					addressParts = (from string part in parts
									where !string.IsNullOrEmpty(part)
									select part).ToArray();
					goto setProps;
				}

				Log.Error("VCardAttributeAddress no match for line {Line} {withoutKey}", value, withoutKey);

			setProps:

				Type = type;
				Label = label;
				AddressParts = addressParts;

			}
		}
		

	}
}
