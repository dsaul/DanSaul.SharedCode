using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Serilog;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SharedCode
{
	public class S3Utils
	{
		


		

		// Only works for max 1000 objects
		//public static async Task SpaceUsed(AmazonS3Client s3Client, string bucketName) {
		//	ListObjectsV2Request request = new ListObjectsV2Request {
		//		BucketName = bucketName
		//	};

		//	ListObjectsV2Response response = await s3Client.ListObjectsV2Async(request);
		//	long totalSize = 0;
		//	foreach (S3Object o in response.S3Objects) {
		//		totalSize += o.Size;
		//	}
		//	Log.Debug("Total Size of bucket " + bucketName + " is " +
		//		Math.Round(totalSize / 1024.0 / 1024.0, 2) + " MB");
		//}

		public static string GetPreSignedUrl(
			string? s3AccessKey, 
			string? s3SecretKey, 
			string? s3ServiceUri, 
			string? bucket,
			string? key,
			string? contentType,
			string? filename) {
			using var s3Client = new AmazonS3Client(s3AccessKey, s3SecretKey, new AmazonS3Config
				{
				RegionEndpoint = RegionEndpoint.USWest1,
				ServiceURL = s3ServiceUri,
				ForcePathStyle = true
			});


			// Create a GetPreSignedUrlRequest request
			GetPreSignedUrlRequest request = new() {
				BucketName = bucket,
				Key = key,
				Expires = DateTime.Now.AddMinutes(5),
				ResponseHeaderOverrides = new ResponseHeaderOverrides
					{
					ContentType = contentType,
					ContentDisposition = $"attachment; filename={filename}",
					CacheControl = "No-cache",
					Expires = "Thu, 01 Dec 1994 16:00:00 GMT",
				}
			};

			return s3Client.GetPreSignedURL(request);
		}
	}
}
