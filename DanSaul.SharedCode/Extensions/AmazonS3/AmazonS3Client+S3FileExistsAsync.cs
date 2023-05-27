// (c) 2023 Dan Saul
using Amazon.S3;
using Amazon.S3.Model;

namespace DanSaul.SharedCode.Extensions.AmazonS3
{
	public static class AmazonS3Client_S3FileExistsAsync
    {
        public static async Task<bool> S3FileExistsAsync(this AmazonS3Client s3Client, string _bucket, string _key)
        {
            GetObjectMetadataRequest request = new()
            {
                BucketName = _bucket,
                Key = _key
            };

            try
            {
                await s3Client.GetObjectMetadataAsync(request);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
