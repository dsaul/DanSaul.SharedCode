// (c) 2023 Dan Saul
using HeyRed.Mime;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using Serilog;
using System.Text.RegularExpressions;

namespace DanSaul.SharedCode.CardDav
{
	[BsonIgnoreExtraElements]
	public record VCardAttributePhoto : VCardAttribute
	{
		[BsonIgnore]
		[JsonIgnore]
		static Regex KRegExRemoveKey = new Regex(@"(?<=PHOTO[;:]).*");
		[BsonIgnore]
		[JsonIgnore]
		static Regex KRegexValueComponents = new Regex(@"(?<encoding>(?<=ENCODING=).*(?=;)).*(?<=TYPE=)(?<type>.*)(?=:).*(?<=:)(?<data>.*)");
		[BsonIgnore]
		[JsonIgnore]
		static Regex KRegexValueComponentsWithoutType = new Regex(@"(?<encoding>(?<=ENCODING=).*(?=:)):(?<data>.*)");
		[BsonElement]
		public string? MediaType { get; init; }
		[BsonIgnore]
		[JsonIgnore]
		public byte[]? Data { get; init; }
		[BsonElement]
		public string? DataBase64
		{
			get
			{
				if (Data == null)
					return null;

				return Convert.ToBase64String(Data);
			}
			init
			{
				if (value == null)
				{
					Data = null;
					return;
				}
				Data = Convert.FromBase64String(value);
			}
		}
		[BsonIgnore]
		[JsonIgnore]
		public override string? Line
		{
			init
			{
				base.Line = value;


				string? mediaType = null;
				byte[]? data = null;

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
							case "encoding":
								// It's always base64
								//encoding = $"{group.Value}".ToLower().Trim();
								break;
							case "type":
								//type = $"{group.Value}".ToLower().Trim();
								// Ignore provided type, we'll magic the media type.
								break;
							case "data":
								data = Convert.FromBase64String(group.Value);

								if (data != null)
								{
									using MemoryStream stream = new MemoryStream(data);
									mediaType = MimeGuesser.GuessMimeType(stream);
								}

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

				{
					MatchCollection matchCollectionValueComponents = KRegexValueComponentsWithoutType.Matches(withoutKey);
					foreach (Match matchValueComponents in matchCollectionValueComponents)
						HandleMatch(matchValueComponents);

					if (matchCollectionValueComponents.Count != 0)
						goto setProps;
				}

				Log.Error("VCardAttributePhoto no match for line {Line}", value);

			setProps:
				MediaType = mediaType;
				Data = data;


			}
		}


	}
}
