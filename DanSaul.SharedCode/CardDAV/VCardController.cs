// (c) 2023 Dan Saul
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
		IMongoCollection<VCard> VCards { get; set; }


		public VCardController(
			IMongoCollection<VCard> _VCards
			)
		{
			VCards = _VCards;
		}

		[HttpGet("photo/{uid}")]
		public async Task Details(
			[FromRoute] string uid
			)
		{
			var results = from sub in VCards.AsQueryable()
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
