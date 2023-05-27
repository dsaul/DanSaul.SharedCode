// (c) 2023 Dan Saul
using System.Text.RegularExpressions;
using PhoneNumbers;
using Serilog;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using DanSaul.SharedCode.Extensions;

namespace DanSaul.SharedCode.CardDav
{
	[BsonIgnoreExtraElements]
	public record VCardAttributeTelephone : VCardAttribute
	{
		[BsonIgnore]
		[JsonIgnore]
		static Regex KRegExRemoveKey = new Regex(@"(?<=TEL[;:]).*");
		[BsonIgnore]
		[JsonIgnore]
		static Regex KRegexValueComponentsWithType = new Regex(@"(?<type>(?<=TYPE=).*(?=:)):(?<number>.*)");
		[BsonIgnore]
		[JsonIgnore]
		static PhoneNumberUtil PhoneNumberUtil = PhoneNumberUtil.GetInstance();
		[BsonElement]
		public string? Type { get; init; }
		[BsonElement]
		public string? E164 { get; init; }
		[BsonIgnore]
		[JsonIgnore]
		public override string? Line
		{
			init
			{
				base.Line = value;

				string? type = null;
				string? e164 = null;

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

								string numberRaw = group.Value;

								if (group.Value.Contains(','))
								{
									string[] numberParts = numberRaw.Split(',');
									PhoneNumber parsed = PhoneNumberUtil.Parse(numberParts[0], "US");
									numberParts[0] = $"+{parsed.CountryCode}{parsed.NationalNumber}";
									e164 = string.Join(',', numberParts);
								}
								else
								{
									PhoneNumber parsed = PhoneNumberUtil.Parse(numberRaw, "US");
									e164 = $"+{parsed.CountryCode}{parsed.NationalNumber}";
								}
								break;
						}
					}
				}

				if (!string.IsNullOrWhiteSpace(withoutKey))
				{

					if (withoutKey.StartsWith("TYPE="))
					{
                        MatchCollection matchCollectionValueComponents = KRegexValueComponentsWithType.Matches(withoutKey);

                        if (matchCollectionValueComponents.Count == 0)
                        {
                            goto setProps;
                        }

                        foreach (Match matchValueComponents in matchCollectionValueComponents)
                            HandleMatch(matchValueComponents);

                        goto setProps;
                    }

					// Some garbage that sometimes shows up.
					else if (withoutKey.StartsWith("VALUE=TEXT:"))
					{
						string part = withoutKey.Substring("VALUE=TEXT:".Length);
						PhoneNumber parsed = PhoneNumberUtil.Parse(part, "US");
						e164 = $"+{parsed.CountryCode}{parsed.NationalNumber}";

                        goto setProps;
                    }
					else
					{
						e164 = withoutKey;
                        goto setProps;

                    }
				}

				//TEL;VALUE=TEXT:311
				//TEL:(204) 287-7433
				//TEL:(647) 495 - 6994
				//TEL:(647) 490 - 4425
				//TEL:(647) 490 - 3601
				//TEL:(877) 677-2884
				//TEL:(833) 997-2884
				//TEL:(833) 326-7285
				//TEL:(236) 326-7285
				//TEL:(204) 890-0024
				Log.Error("VCardAttributeTelephone no match for line '{Line}'", value);

			setProps:
				Type = type;
				E164 = e164;



			}
		}


	}
}
