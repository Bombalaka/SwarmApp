using Microsoft.AspNetCore.Mvc;
using app.Models;
using app.Service;
using app.Repository;
using app.Storage;



namespace app.Controllers;

public class PostsController : Controller
{
    private readonly IPostRepository _postRepository;
    private readonly IImageStorage _imageStorage;

    public PostsController(IPostRepository postRepository, IImageStorage imageStorage)
    {
        _postRepository = postRepository;
        _imageStorage = imageStorage;
    }
    public async Task<IActionResult> Index()
    {
        var posts = await _postRepository.GetAllAsync();
        return View(posts);
    }

    [HttpGet]
    public IActionResult Create() => View(new Post());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Post model)
    {
        if (!ModelState.IsValid) return View(model);

        // save image via storage (S3 in prod, local in dev)
        string? imageUrl = null;
        if (model.ImageFile is { Length: > 0 })
        {
            imageUrl = await _imageStorage.SaveAsync(model.ImageFile);
            Console.WriteLine($"[IMG] Saved to: {imageUrl}");
        }

        var post = new Post
        {
            Title = model.Title,
            Content = model.Content,
            ImagePath = imageUrl
        };

        await _postRepository.CreateAsync(post);
        return RedirectToAction(nameof(Index));
    }

}
