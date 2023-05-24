using DanSaul.SharedCode.StandardizedEnvironmentVariables;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using MongoDB.Driver;

namespace DanSaul.SharedCode.CardDav
{
	[Route("api/[controller]")]
	[ApiController]
	public class VCardController : Controller
	{
		private IMongoClient MongoClient { get; set; }
		private IMongoDatabase Database { get; set; }
		private IMongoCollection<VCard> Contacts { get; set; }


		public VCardController(IMongoClient mongoClient)
		{
			MongoClient = mongoClient;
			Database = MongoClient.GetDatabase(EnvTextitude.kMongoDatabase);
			Contacts = Database.GetCollection<VCard>(EnvTextitude.kMongoCollectionCalDavContacts);
		}

		[HttpGet("photo/{uid}")]
		public async Task Details(
			[FromRoute] string uid
			)
		{
			var results = from sub in Contacts.AsQueryable()
						  where sub.UID == uid
						  select sub;

			if (!results.Any())
			{
				Response.StatusCode = 404;
				await Response.Body.FlushAsync();
				return;
			}

			VCardAttributePhoto? photo = results.FirstOrDefault()?.Photo;
			if (photo == null)
			{
				Response.StatusCode = 404;
				await Response.Body.FlushAsync();
				return;
			}

			byte[]? data = photo.Data;
			if (data == null)
			{
				Response.StatusCode = 404;
				await Response.Body.FlushAsync();
				return;
			}

			using MemoryStream stream = new MemoryStream(data);

			Response.StatusCode = 200;
			Response.Headers.Add(HeaderNames.ContentType, photo.MediaType);

			await stream.CopyToAsync(Response.Body);
			await Response.Body.FlushAsync();
		}

	}
}
