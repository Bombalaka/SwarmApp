using Amazon.S3;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;

namespace app.Storage;

public class S3ImageStorage : IImageStorage
{
   private readonly IAmazonS3 _s3;
    private readonly string _bucket;
    private readonly string _region;

    public S3ImageStorage(IAmazonS3 s3, IConfiguration config)
    {
        _s3     = s3;
        _bucket = config["S3:BucketName"] ?? Environment.GetEnvironmentVariable("S3_BUCKET");
        _region = config["AWS:Region"] ?? Environment.GetEnvironmentVariable("AWS_REGION");
    }

    public async Task<string> SaveAsync(IFormFile file, CancellationToken ct = default)
    {
        var ext = Path.GetExtension(file.FileName);
        var key = $"uploads/{Guid.NewGuid()}{ext}";

        using var stream = file.OpenReadStream();
        var tx = new TransferUtility(_s3);
        await tx.UploadAsync(stream, _bucket, key, ct);
        Console.WriteLine($"[IMG][S3] bucket={_bucket}, region={_region}");

        return $"https://{_bucket}.s3.{_region}.amazonaws.com/{key}";
        
    }
}
