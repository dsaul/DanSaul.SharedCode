using Amazon.S3;
using Amazon.S3.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DanSaul.SharedCode.Extensions.AmazonS3
{
	public static class AmazonS3Client_S3FileExistsAsync
    {
        public static async Task<bool> S3FileExistsAsync(this AmazonS3Client s3Client, string _bucket, string _key)
        {
            GetObjectMetadataRequest request = new GetObjectMetadataRequest()
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
