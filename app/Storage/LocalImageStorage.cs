namespace app.Storage;

public class LocalImageStorage : IImageStorage
{

    private readonly string _uploadsRoot;
    public LocalImageStorage(IWebHostEnvironment env)
    {
        _uploadsRoot = Path.Combine(env.WebRootPath ?? "wwwroot", "uploads");
        Directory.CreateDirectory(_uploadsRoot);
    }

    public async Task<string> SaveAsync(IFormFile file, CancellationToken ct = default)
    {
        var ext = Path.GetExtension(file.FileName);
        var name = $"{Guid.NewGuid()}{ext}";
        var fullPath = Path.Combine(_uploadsRoot, name);

        using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream, ct);
        Console.WriteLine("[IMG][LOCAL] saving to /uploads");


        return $"/uploads/{name}";
    }

}
