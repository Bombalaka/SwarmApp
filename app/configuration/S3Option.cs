namespace app.Configuration;

public class S3Options
{
    public const string SectionName = "S3";

    public string BucketName { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
}