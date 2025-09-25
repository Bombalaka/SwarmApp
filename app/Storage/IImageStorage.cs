namespace app.Storage;

public interface IImageStorage
{
    Task<string> SaveAsync(IFormFile file, CancellationToken ct = default);
}