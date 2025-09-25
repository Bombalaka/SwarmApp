using app.Models;
using app.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace app.Repository;

public class PostSqlRepository : IPostRepository
{
    private readonly AppDbContext _context;

    public PostSqlRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Post>> GetAllAsync()
    {
        return await _context.Set<Post>().ToListAsync();
    }

    public async Task<Post?> GetByIdAsync(string id)
    {
        return await _context.Set<Post>().FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Post> CreateAsync(Post post)
    {
        _context.Set<Post>().Add(post);
        await _context.SaveChangesAsync();
        return post;
    }

    public async Task<Post?> UpdateAsync(Post post)
    {
        var existingPost = await GetByIdAsync(post.Id);
        if (existingPost != null)
        {
            existingPost.Content = post.Content;
            existingPost.ImagePath = post.ImagePath;
            existingPost.CreatedAt = post.CreatedAt;
            await _context.SaveChangesAsync();
        }
        return existingPost;
    }

    public async Task<Post?> DeleteAsync(string id)
    {
        var post = await GetByIdAsync(id);
        if (post != null)
        {
            _context.Set<Post>().Remove(post);
            await _context.SaveChangesAsync();
        }
        return post;
    }
}
