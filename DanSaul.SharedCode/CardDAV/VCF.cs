using System.Net;
using System.Text;
using Serilog;
using WebDav;

namespace DanSaul.SharedCode.CardDav
{
	public record VCF
	{
		//static TimeLimiter RateLimit { get; } = TimeLimiter.GetFromMaxCountByInterval(5, TimeSpan.FromSeconds(1));

		public IEnumerable<VCard> Cards { get; init; } = Array.Empty<VCard>();


		public static async Task<VCF?> FromURIAsync(Uri uri, CredentialCache credentialCache)
		{
			//await RateLimit;
			HttpClient httpClient = new HttpClient(new HttpClientHandler { Credentials = credentialCache });
			HttpResponseMessage responseMessage = await httpClient.GetAsync(uri);
			if (responseMessage.StatusCode != HttpStatusCode.OK)
			{
				Log.Error("[VCF] {URI} FromURIAsync Error downloading", uri);
				return null;
			}

			string content = await responseMessage.Content.ReadAsStringAsync();

			return FromStringAsync(content);
		}

		public static async Task<VCF?> FromWebDav(WebDavClient client, WebDavResource res)
		{
			//await RateLimit;

			WebDavStreamResponse response = await client.GetRawFile(res.Uri);
			if (response.StatusCode != (int)HttpStatusCode.OK)
			{
                Log.Error("[VCF] {URI} FromWebDav Error downloading {Description}", res.Uri, response.Description);
                return null;
			}

			Log.Information("[VCF] {URI} downloaded", res.Uri);

			StreamReader reader = new StreamReader(response.Stream);
			string content = reader.ReadToEnd();

			return FromStringAsync(content);

		}

		public static VCF FromStringAsync(string payload)
		{
			payload = payload.Replace("\r\n", "\n");
			string[] linesRaw = payload.Split('\n');
			List<string> linesConverted = new();

			StringBuilder? sb = null;

			void ProcessLine(string? line)
			{
				if (line == null && sb != null)
				{
					linesConverted.Add(sb.ToString().Trim());
					sb = null;
					return;
				}

				if (line == null)
					return;

				if (line.StartsWith(' ') && sb == null)
					return;

				if (sb != null && !line.StartsWith(' '))
				{
					linesConverted.Add(sb.ToString().Trim());
					sb = null;
				}

				if (sb == null)
					sb = new();

				sb.Append(line.Trim());
			}

			foreach (string line in linesRaw)
				ProcessLine(line);
			if (sb != null)
				ProcessLine(null);



			List<List<string>> vcards = new List<List<string>>();

			List<string>? card = null;
			foreach (string line in linesConverted)
			{
				if (card == null)
				{
					if (!line.StartsWith("BEGIN:VCARD"))
						continue;

					card = new List<string>();
					card.Add(line);
					continue;
				}

				if (line.StartsWith("END:VCARD"))
				{
					card.Add(line);
					vcards.Add(card);
					card = null;
					continue;
				}

				card.Add(line);
			}
			if (card != null)
			{
				vcards.Add(card);
				card = null;
			}


			List<VCard> cards = new();
			foreach (List<string> vcard in vcards)
				cards.Add(VCard.FromLinesAsync(vcard));

			return new VCF { Cards = cards.ToArray() };
		}
	}
}
