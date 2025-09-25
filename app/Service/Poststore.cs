using System;
using System.Collections.Generic;
using app.Models;

namespace app.Service;

public class Poststore : IPoststore
{
   private readonly List<Post> _posts = new List<Post>();
     

    public Task<IEnumerable<Post>> AllAsync() => Task.FromResult(_posts.OrderByDescending(p => p.CreatedAt).AsEnumerable());
    public Task<Post> AddAsync(Post post)
    {
        
        _posts.Add(post);
        return Task.FromResult(post);
    }
    public Task<Post?> GetByIdAsync(string id) => Task.FromResult(_posts.FirstOrDefault(p => p.Id == id));
    public async Task<Post?> UpdateAsync(Post post)
    {
        var existingPost = await GetByIdAsync(post.Id);
        if (existingPost != null)
        {
            existingPost.Content = post.Content;
        }
        return existingPost;
    }
    public async Task<Post?> DeleteAsync(string id)
    {
        var post = await GetByIdAsync(id);
        if (post != null)
        {
            _posts.Remove(post);
        }
        return post;
    }
    public Task<bool> ExistsAsync(string id) => Task.FromResult(_posts.Any(p => p.Id == id));
}


