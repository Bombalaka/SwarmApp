

using app.Models;

namespace app.Service
{
    public interface IPoststore
    {
        Task<IEnumerable<Post>> AllAsync();
        Task<Post> AddAsync(Post post);
        Task<Post?> GetByIdAsync(string id);
        Task<Post?> UpdateAsync(Post post);
        Task<Post?> DeleteAsync(string id);
        Task<bool> ExistsAsync(string id);
    }
}