// (c) 2023 Dan Saul
using DanSaul.SharedCode.Mongo;
using DanSaul.SharedCode.StandardizedEnvironmentVariables;
using MongoDB.Driver;
using Serilog;
using System.Net;
using WebDav;

namespace DanSaul.SharedCode.CardDav
{
	public class CardDAVController
	{
		IMongoCollection<CardDavSourceDocument> Sources { get; init; }
		IMongoCollection<VCard> VCards { get; init; }
		CredentialCache CredentialCache { get; init; }
		HttpClient HttpClient { get; init; }

		//private TimeLimiter RateLimit { get; init; } = TimeLimiter.GetFromMaxCountByInterval(5, TimeSpan.FromSeconds(1));

		public CardDAVController(
			IMongoCollection<CardDavSourceDocument> _Sources,
			IMongoCollection<VCard> _VCards,
			CredentialCache _CredentialCache,
			HttpClient _HttpClient
			)
		{
			Sources = _Sources;
			VCards = _VCards;
			CredentialCache = _CredentialCache;
			HttpClient = _HttpClient;

			ReloadCredentials();

			Task.Run(StartLoop);

		}
		public void ReloadCredentials()
		{
			ReloadDigestCredentials();
		}

		private void ReloadDigestCredentials()
		{
			var results = from sub in Sources.AsQueryable()
						  where sub.AuthType == "Digest"
						  select sub;

			foreach (CardDavSourceDocument source in results)
			{
				if (string.IsNullOrWhiteSpace(source.URI))
				{
					Log.Error("Skipping source as URI is null or whitespace");
					continue;
				}

				Uri uri = new(source.URI);


				CredentialCache.Add(
			new Uri(uri, "/"),
			"Digest",
				new NetworkCredential(source.UserName, source.Password)
				);
			}
		}

		public async Task StartLoop()
		{
			while (true)
			{
				List<string> syncedCardDavIds = new();
				await Iterate(syncedCardDavIds);

				await VCards.DeleteManyAsync(
					filter: Builders<VCard>.Filter.Where(x => !syncedCardDavIds.Contains(x.UID))
					);


				await Task.Delay(5 * 60 * 1000);
			}
		}

		public async Task Iterate(List<string> syncedCardDavIds)
		{
			Log.Information("[CallDav] Iterate()");

			//List<Task> tasks = new List<Task>();


			var results = (from sub in Sources.AsQueryable()
						  select sub).ToArray();

			Log.Information("[CallDav] {Count} Sources", results.Length);

			foreach (CardDavSourceDocument source in results)
			{
                Log.Information("[CallDav] Source URI {URI} Username {username}", source.URI, source.UserName);

                if (string.IsNullOrWhiteSpace(source.URI))
				{
					Log.Error("[CallDav] string.IsNullOrWhiteSpace(source.URI)");
                    continue;
                }

				Uri cardDavIndex = new(source.URI);

				//await RateLimit;


				WebDavClientParams clientParams = new()
				{
					BaseAddress = cardDavIndex,
					Credentials = new NetworkCredential(source.UserName, source.Password)
				};
				using WebDavClient client = new(clientParams);

				var result = await client.Propfind(source.URI);
				if (false == result.IsSuccessful)
				{
					Log.Error("[CallDav] PropFind {URI} Failed StatusCode {StatusCode} Description {Description}", 
						source.URI, result.StatusCode, result.Description);
					continue;
                }

                foreach (WebDavResource res in result.Resources)
                {
                    if (res.ContentLength == null || res.ContentLength == 0)
                    {
                        continue;
                    }


                    Log.Information("[CallDav] Found {URI} ", res);

					await DownloadAndProcessVCF(client, res, syncedCardDavIds);
                    //tasks.Add(DownloadAndProcessVCF(client, res, syncedCardDavIds));
                }

            }

			//await Task.WhenAll(tasks);

		}

        async Task DownloadAndProcessVCF(WebDavClient client, WebDavResource res, List<string> syncedCardDavIds)
		{
            Log.Information("[CallDav] {URI} Begin Processing  ", res.Uri);

            VCF? vcf = await VCF.FromWebDav(client, res);
            if (vcf == null)
            {
				Log.Error("[CallDav] {URI} No File Downloaded", res.Uri);
                return;
            }

			VCard[] cards = vcf.Cards.ToArray();
			if (cards.Length == 0)
			{
                Log.Error("[CallDav] {URI} No Cards in File!", res.Uri);
                return;
            }

			Log.Information("[CalDav] {URI} Found {Count} cards", res.Uri, cards.Length);

            foreach (VCard card in cards)
            {
                if (card.UID == null)
                {
                    Log.Warning("skipping vcard as it doesn't have a uid");
                    continue;
                }
                syncedCardDavIds.Add(card.UID);

                await VCards.DeleteManyAsync(
                    filter: Builders<VCard>.Filter.Where(x => x.UID == card.UID)
                    );

                await VCards.InsertOneAsync(card);

				Log.Information("[CallDav] {URI} Processed {UID} {FullName}", res.Uri, card.UID, card.FullName);
            }

            Log.Information("[CallDav] {URI} Finish Processing  ", res.Uri);
        }

	}
}
