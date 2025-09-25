using app.Models;
using System.Collections.Concurrent;


namespace app.Repository;

public class InMemoryRepository : IPostRepository
{
    private readonly ConcurrentDictionary<string, Post> _posts = new();

    public Task<IEnumerable<Post>> GetAllAsync() => Task.FromResult(_posts.Values.AsEnumerable());
    public Task<Post?> GetByIdAsync(string id) => Task.FromResult(_posts.TryGetValue(id, out var post) ? post : null);
    public Task<Post> CreateAsync(Post post) => Task.FromResult(_posts.TryAdd(post.Id, post) ? post : throw new InvalidOperationException("Post already exists"));
    public Task<Post?> UpdateAsync(Post post) => Task.FromResult(_posts.TryUpdate(post.Id, post, _posts[post.Id]) ? post : null);
    public Task<Post?> DeleteAsync(string id) => Task.FromResult(_posts.TryRemove(id, out var post) ? post : null);
}